using HMS.Models;
using Microsoft.EntityFrameworkCore;

namespace HMS.Data
{
    public class HMSContext : DbContext
    {
        public HMSContext (DbContextOptions<HMSContext> options)
            : base(options)
        { }
        public DbSet<User> Users => Set<User>();
        public DbSet<Role> Roles => Set<Role>();
        public DbSet<UserRoleMapping> UserRoleMappings => Set<UserRoleMapping>();
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // Optionally configure schema explicitly
            modelBuilder.HasDefaultSchema("hms");
        }
    }
}