using Microsoft.EntityFrameworkCore;
using Models.DB;

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

        public DbSet<HMSDashboard> HMSDashboard => Set<HMSDashboard>();
        public DbSet<ChannelDetails> ChannelDetails => Set<ChannelDetails>();
        public DbSet<StatusDetails> StatusDetails => Set<StatusDetails>();
        public DbSet<RoleMenuMapping> RoleMenuMapping => Set<RoleMenuMapping>();
        public DbSet<Agent> Agents { get; set; } = null!;
        public DbSet<AgentHierarchy> AgentHierarchies { get; set; }
        public DbSet<ErrorMaster> errorMaster => Set<ErrorMaster>();
        public DbSet<ChannelMaster> ChannelMaster => Set<ChannelMaster>();
        public DbSet<SubchannelMaster> SubchannelMaster => Set<SubchannelMaster>();
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
                entity.ToTable("agent_audit_trail", "hms");

                entity.HasOne(e => e.Agent)
                      .WithMany()
                      .HasForeignKey(e => e.AgentId)
                      .HasConstraintName("fk_agent")
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Agent>(entity =>
            {
                entity.ToTable("agent", "hms");

                entity.HasIndex(e => e.AgentCode).IsUnique();

                entity.HasOne(e => e.Supervisor)
                      .WithMany()
                      .HasForeignKey(e => e.SupervisorId)
                      .HasConstraintName("fk_supervisor")
                      .OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<RoleMenuMapping>(entity =>
            {
                entity.ToTable("role_menu_mapping", "hms");

                entity.HasOne(rmm => rmm.Role)
                      .WithMany()
                      .HasForeignKey(rmm => rmm.RoleId)
                      .HasConstraintName("fk_role")
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(rmm => rmm.Menu)
                      .WithMany()
                      .HasForeignKey(rmm => rmm.MenuId)
                      .HasConstraintName("fk_menu")
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.HasDefaultSchema("hms");

        }
    }
}