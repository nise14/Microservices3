using Mango.Services.AuthAPI.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Mango.Services.AuthAPI.Data;

public class AppDbContext : IdentityDbContext<ApplicationUser>
{
    public DbSet<ApplicationUser> ApplicationUsers => Set<ApplicationUser>();

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
    }
}