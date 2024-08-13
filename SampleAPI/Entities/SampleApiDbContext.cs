using Microsoft.EntityFrameworkCore;

namespace SampleAPI.Entities
{
    public class SampleApiDbContext : DbContext
    {
        public SampleApiDbContext() { }
        public SampleApiDbContext(DbContextOptions<SampleApiDbContext> options) :
            base(options)
        {
        }

        public virtual DbSet<Order> Orders { get; set; } = null!;
    }
}
