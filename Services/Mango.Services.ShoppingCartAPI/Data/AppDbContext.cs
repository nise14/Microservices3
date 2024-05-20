using Mango.Services.ShoppingCartAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace Mango.Services.ShoppingCartAPI.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<CartHeader> CartHeaders => Set<CartHeader>();
    public DbSet<CartDetail> CartDetails => Set<CartDetail>();
}