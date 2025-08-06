using HMS.Models;
using Microsoft.EntityFrameworkCore;
using Models;

namespace HMS.Data
{
    public class HMSContext : DbContext
    {
        public HMSContext (DbContextOptions<HMSContext> options)
            : base(options)
        { }
        public DbSet<User> Users => Set<User>();
        public DbSet<Role> Roles => Set<Role>();
        public DbSet<ApiConfig> apiConfig => Set<ApiConfig>();
        public DbSet<UserRoleMapping> UserRoleMappings => Set<UserRoleMapping>();
        public DbSet<AgentMovementHistory> agentMovementHistory => Set<AgentMovementHistory>();
        public DbSet<AgentTerminationRequest> AgentTerminationRequest => Set<AgentTerminationRequest>();
        public DbSet<Agent> agent => Set<Agent>();
        public DbSet<AgentAuditTrail> AgentAuditTrail => Set<AgentAuditTrail>();
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // Optionally configure schema explicitly
            modelBuilder.HasDefaultSchema("hms");
        }
    }
}