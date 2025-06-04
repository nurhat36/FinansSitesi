using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using FinansSitesi.Models;
namespace FinansSitesi.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // İleride finans tablolarını da buraya eklersin
        // public DbSet<FinansModel> Finanslar { get; set; }
    }
}