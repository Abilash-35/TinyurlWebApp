using Microsoft.EntityFrameworkCore;
using TinyUrl.Api.Models;

namespace TinyUrl.Api.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<TinyUrls> TinyUrls => Set<TinyUrls>();
    }
}