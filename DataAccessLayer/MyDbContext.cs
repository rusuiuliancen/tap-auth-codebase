using DataAccessLayer.Models;
using Microsoft.EntityFrameworkCore;

namespace DataAccessLayer
{
    public class MyDbContext : DbContext
    {
        private readonly string _windowsConnectionString = @"Data Source=NBKR004513;Initial Catalog=AuthDatabase1;Integrated Security=True;TrustServerCertificate=True";
        public DbSet<TestModel> TestModels { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(_windowsConnectionString);
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
        }
    }
}
