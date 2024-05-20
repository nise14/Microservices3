using AutoMapper;
using Mango.MessageBus;
using Mango.Services.ShoppingCartAPI.Data;
using Mango.Services.ShoppingCartAPI.Models;
using Mango.Services.ShoppingCartAPI.Models.Dto;
using Mango.Services.ShoppingCartAPI.Service.IService;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Mango.Services.ShoppingCartAPI.Controllers;

[ApiController]
[Route("api/cart")]
public class ShoppingCartAPIController : ControllerBase
{
    private readonly ResponseDto _response;
    private readonly IMapper _mapper;
    private readonly AppDbContext _context;
    private readonly IProductService _productService;
    private readonly ICouponService _couponService;
    private readonly IMessageBus _messageBus;
    private readonly IConfiguration _configuration;

    public ShoppingCartAPIController(AppDbContext context, IMapper mapper, IProductService productService, ICouponService couponService, IMessageBus messageBus, IConfiguration configuration)
    {
        _context = context;
        _mapper = mapper;
        _response = new ResponseDto();
        _productService = productService;
        _couponService = couponService;
        _messageBus = messageBus;
        _configuration = configuration;
    }

    [HttpGet("GetCart/{userId}")]
    public async Task<ResponseDto> GetCart(string userId)
    {
        try
        {
            CartDto cart = new()
            {
                CartHeader = _mapper.Map<CartHeaderDto>(await _context.CartHeaders.FirstAsync(u => u.UserId == userId))
            };

            cart.CartDetails = _mapper.Map<IEnumerable<CartDetailDto>>(_context.CartDetails
                .Where(u => u.CartHeaderId == cart.CartHeader.CartHeaderId));

            IEnumerable<ProductDto> productDtos = await _productService.GetProducts();

            foreach (var item in cart.CartDetails)
            {
                item.Product = productDtos.FirstOrDefault(u => u.ProductId == item.ProductId);
                cart.CartHeader.CartTotal += item.Count * item.Product.Price;
            }

            if (!string.IsNullOrWhiteSpace(cart.CartHeader.CouponCode))
            {
                CouponDto coupon = await _couponService.GetCoupon(cart.CartHeader.CouponCode);

                if (coupon != null && cart.CartHeader.CartTotal > coupon.MinAmount)
                {
                    cart.CartHeader.CartTotal -= coupon.DiscountAmount;
                    cart.CartHeader.Discount = coupon.DiscountAmount;
                }
            }

            _response.Result = cart;
        }
        catch (Exception ex)
        {
            _response.IsSuccess = false;
            _response.Message = ex.Message;
        }

        return _response;
    }

    [HttpPost("ApplyCoupon")]
    public async Task<object> ApplyCoupon([FromBody] CartDto cartDto)
    {
        try
        {
            var cartFromDB = await _context.CartHeaders.FirstAsync(u => u.UserId == cartDto.CartHeader.UserId);
            cartFromDB.CouponCode = cartDto.CartHeader.CouponCode;
            _context.CartHeaders.Update(cartFromDB);
            await _context.SaveChangesAsync();
            _response.Result = true;
        }
        catch (Exception ex)
        {
            _response.IsSuccess = false;
            _response.Message = ex.ToString();
        }
        return _response;
    }

    [HttpPost("EmailCartRequest")]
    public async Task<object> EmailCartRequest([FromBody] CartDto cartDto)
    {
        try
        {
            await _messageBus.PublishMessage(cartDto, _configuration.GetValue<string>("TopicAndQueueNames:EmailShoppingCartQueue"));
            _response.Result = true;
        }
        catch (Exception ex)
        {
            _response.IsSuccess = false;
            _response.Message = ex.ToString();
        }
        return _response;
    }

    [HttpPost("CartUpsert")]
    public async Task<ResponseDto> CartUpsert(CartDto cartDto)
    {
        try
        {
            var cartHeaderFromDb = await _context.CartHeaders.AsNoTracking()
                .FirstOrDefaultAsync(u => u.UserId == cartDto.CartHeader.UserId);
            if (cartHeaderFromDb == null)
            {
                //create header and details
                CartHeader cartHeader = _mapper.Map<CartHeader>(cartDto.CartHeader);
                _context.CartHeaders.Add(cartHeader);
                await _context.SaveChangesAsync();
                cartDto.CartDetails.First().CartHeaderId = cartHeader.CartHeaderId;
                _context.CartDetails.Add(_mapper.Map<CartDetail>(cartDto.CartDetails.First()));
                await _context.SaveChangesAsync();
            }
            else
            {
                //if header is not null
                //check if details has same product
                var cartDetailsFromDb = await _context.CartDetails.AsNoTracking().FirstOrDefaultAsync(
                    u => u.ProductId == cartDto.CartDetails.First().ProductId &&
                    u.CartHeaderId == cartHeaderFromDb.CartHeaderId);
                if (cartDetailsFromDb == null)
                {
                    //create cartdetails
                    cartDto.CartDetails.First().CartHeaderId = cartHeaderFromDb.CartHeaderId;
                    _context.CartDetails.Add(_mapper.Map<CartDetail>(cartDto.CartDetails.First()));
                    await _context.SaveChangesAsync();
                }
                else
                {
                    //update count in cart details
                    cartDto.CartDetails.First().Count += cartDetailsFromDb.Count;
                    cartDto.CartDetails.First().CartHeaderId = cartDetailsFromDb.CartHeaderId;
                    cartDto.CartDetails.First().CartDetailId = cartDetailsFromDb.CartDetailId;
                    _context.CartDetails.Update(_mapper.Map<CartDetail>(cartDto.CartDetails.First()));
                    await _context.SaveChangesAsync();
                }
            }
            _response.Result = cartDto;
        }
        catch (Exception ex)
        {
            _response.Message = ex.Message.ToString();
            _response.IsSuccess = false;
        }
        return _response;
    }

    [HttpPost("RemoveCart")]
    public async Task<ResponseDto> RemoveCart([FromBody] int cartDetailId)
    {
        try
        {
            CartDetail cartDetail = await _context.CartDetails.FirstAsync(u => u.CartDetailId == cartDetailId);

            int totalCountOfCartItems = _context.CartDetails.Where(u => u.CartHeaderId == cartDetail.CartHeaderId).Count();

            _context.CartDetails.Remove(cartDetail);

            if (totalCountOfCartItems == 1)
            {
                var cartHeaderToRemove = await _context.CartHeaders
                    .FirstOrDefaultAsync(u => u.CartHeaderId == cartDetail.CartHeaderId);

                _context.CartHeaders.Remove(cartHeaderToRemove);
            }

            await _context.SaveChangesAsync();

            _response.Result = true;
        }
        catch (Exception ex)
        {

            _response.Message = ex.Message;
            _response.IsSuccess = false;
        }

        return _response;
    }
}