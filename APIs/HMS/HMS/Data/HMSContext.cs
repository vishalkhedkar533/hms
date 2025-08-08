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

        public DbSet<HmsDashboard> hmsDashboard => Set<HmsDashboard>();
        //public DbSet<ChannelDetails> channelDetails => Set<ChannelDetails>();
        //public DbSet<StatusDetails> statusDetails => Set<StatusDetails>();
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // Optionally configure schema explicitly
            modelBuilder.Entity<AgentMovementHistory>(entity =>
            {
                entity.ToTable("AGENT_MOVEMENT_HISTORY", "hms");

                entity.HasOne(e => e.Agent)
                      .WithMany()
                      .HasForeignKey(e => e.AgentId)
                      .HasConstraintName("fk_agent")
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.OldSupervisor)
                      .WithMany()
                      .HasForeignKey(e => e.OldSupervisorCode)
                      .HasConstraintName("fk_old_supervisor")
                      .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(e => e.NewSupervisor)
                      .WithMany()
                      .HasForeignKey(e => e.NewSupervisorCode)
                      .HasConstraintName("fk_new_supervisor")
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<AgentAuditTrail>(entity =>
            {
                entity.ToTable("AGENT_AUDIT_TRAIL", "hms");

                entity.HasOne(e => e.Agent)
                      .WithMany()
                      .HasForeignKey(e => e.AgentId)
                      .HasConstraintName("fk_agent")
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Agent>(entity =>
            {
                entity.ToTable("AGENT", "hms");

                entity.HasIndex(e => e.AgentCode).IsUnique();

                entity.HasOne(e => e.Supervisor)
                      .WithMany()
                      .HasForeignKey(e => e.Supervisor_Id)
                      .HasConstraintName("fk_supervisor")
                      .OnDelete(DeleteBehavior.SetNull);
            });
            modelBuilder.HasDefaultSchema("hms");
        }
    }
}