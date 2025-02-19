using System.Text;
using Mango.Services.EmailAPI.Data;
using Mango.Services.EmailAPI.Models;
using Mango.Services.EmailAPI.Models.Dto;
using Microsoft.EntityFrameworkCore;

namespace Mango.Services.EmailAPI.Services;

public class EmailService : IEmailService
{
    private readonly DbContextOptions<AppDbContext> _dbOptions;

    public EmailService(DbContextOptions<AppDbContext> dbOptions)
    {
        _dbOptions = dbOptions;
    }
    public async Task EmailCartAndLog(CartDto cartDto)
    {
        var message = new StringBuilder();

        message.AppendLine("<br/>Cart Email Requested ");
        message.AppendLine($"<br/>Total {cartDto.CartHeader!.CartTotal}");
        message.AppendLine("<br/>");
        message.AppendLine("<ul>");

        foreach (var item in cartDto.CartDetails!)
        {
            message.AppendLine("<li>");
            message.AppendLine($"{item.Product!.Name} x {item.Count}");
            message.AppendLine("</li>");
        }

        message.AppendLine("</ul>");

        await LogAndEmail(message.ToString(), cartDto.CartHeader.Email!);
    }

    private async Task<bool> LogAndEmail(string message, string email)
    {
        try
        {
            EmailLogger emailLog = new()
            {
                Email = email,
                EmailSent = DateTime.Now,
                Message = message
            };

            await using var _db = new AppDbContext(_dbOptions);
            await _db.EmailLoggers.AddAsync(emailLog);
            await _db.SaveChangesAsync();

            return true;
        }
        catch
        {

            return false;
        }
    }
}