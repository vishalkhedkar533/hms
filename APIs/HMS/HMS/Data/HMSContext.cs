using Microsoft.EntityFrameworkCore;

namespace HMS.Data
{
    public class HMSContext : DbContext
    {
        public HMSContext (DbContextOptions<HMSContext> options)
            : base(options)
        {
        }

        public DbSet<HMS.Models.User> User { get; set; } = default!;
    }
}
