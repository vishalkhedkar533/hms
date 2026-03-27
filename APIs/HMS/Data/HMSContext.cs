using Microsoft.EntityFrameworkCore;
using Models.DB;
using Models.DB.Tasks.Models;
using Models.DTO.CommissionMgmt;

namespace HMS.Data
{
    public class HMSContext : DbContext
    {
        public HMSContext (DbContextOptions<HMSContext> options)
            : base(options)
        { }
        public DbSet<User> Users => Set<User>();
        public DbSet<UserBranchMapping> UserBranchMappings => Set<UserBranchMapping>();
        public DbSet<Role> Roles => Set<Role>();
        //public DbSet<RoleMaster> RoleMasters => Set<RoleMaster>();
        public DbSet<MenuMaster> MenuMasters => Set<MenuMaster>();
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
        public DbSet<DesignationMaster> DesignationMaster => Set<DesignationMaster>();
        public DbSet<SubChannelMaster> SubchannelMaster => Set<SubChannelMaster>();
        public DbSet<BankAccount> BankAccount => Set<BankAccount>();
        public DbSet<Address> Address => Set<Address>();
        public DbSet<Nominee> Nominee => Set<Nominee>();
        public DbSet<PersonalInfo> PersonalInfo => Set<PersonalInfo>();
        public DbSet<Subscriber> Subscriber => Set<Subscriber>();
        public DbSet<Organisation> Organisation => Set<Organisation>();
        public DbSet<FileProcessingTask> FileProcessingTasks => Set<FileProcessingTask>();
        public DbSet<CommissionDashboard> CommissionMgmtDashboards => Set<CommissionDashboard>();
        public DbSet<CommissionCycle> CommissionCycles => Set<CommissionCycle>();
        public DbSet<EntityCommission> EntityCommissions => Set<EntityCommission>();
        public DbSet<PerformanceSnapshot> PerformanceSnapshots => Set<PerformanceSnapshot>();
        public DbSet<IndividualCommission> IndividualCommissions => Set<IndividualCommission>();
        public DbSet<AdhocCommission> AdhocCommissions => Set<AdhocCommission>();
        public DbSet<CurrentBusinessCycle> CurrentBusinessCycles => Set<CurrentBusinessCycle>();
        public DbSet<CommissionConfig> CommissionConfigs => Set<CommissionConfig>();
        public DbSet<JobConfig> JobConfigs => Set<JobConfig>();
        public DbSet<JobExeHist> JobExeHists => Set<JobExeHist>();
        public DbSet<JobExtns> JobExtns => Set<JobExtns>();
        public DbSet<LocationMaster> LocationMasters => Set<LocationMaster>();
        public DbSet<UiFieldsSetting>  uiFieldsSettings => Set<UiFieldsSetting>();
        public DbSet<UiField>  uiField => Set<UiField>();
        public DbSet<UiComponent> uiComponent => Set<UiComponent>();
        public DbSet<BranchMaster> BranchMaster => Set<BranchMaster>();
        public DbSet<ChannelBranchHeirarchy> ChannelBranchHeirarchies => Set<ChannelBranchHeirarchy>();
        public DbSet<Inbox> Inbox { get; set; }
        public DbSet<SrApprover> SrApprovers { get; set; }
        public DbSet<PartnerBranchHeirarchy> PartnerBranchHierarchies { get; set; }
        public DbSet<ApprovalSetting> ApprovalSettings { get; set; }
        public DbSet<RevenueComm> RevenueComms { get; set; }
        public DbSet<OrganizationPeriod> OrganizationPeriods { get; set; }
        public DbSet<AgentBranchMapping> AgentBranchMappings => Set<AgentBranchMapping>();
        public DbSet<UserAuditTrail> UserAuditTrails => Set<UserAuditTrail>();
        public DbSet<ProductMaster> productMasters => Set<ProductMaster>();
        public DbSet<CustomField> CustomFields => Set<CustomField>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 1. Explicitly define Primary Keys
            modelBuilder.Entity<LocationMaster>().HasKey(l => l.LocationMasterId);
            modelBuilder.Entity<BranchMaster>().HasKey(b => b.BranchId);

            // 2. Explicitly define the Relationship
            modelBuilder.Entity<BranchMaster>()
                .HasOne(b => b.Location)
                .WithMany() // No collection in LocationMaster
                .HasForeignKey(b => b.LocationMasterId)
                .HasPrincipalKey(l => l.LocationMasterId); // Points to the numeric PK

            // 3. Define the Unique Index for Branch
            modelBuilder.Entity<BranchMaster>()
                .HasIndex(b => new { b.OrgId, b.BranchCode, b.LocationMasterId })
                .IsUnique()
                .HasDatabaseName("branch_uq");

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

                // Self-referencing relationship for Supervisor
                entity.HasOne(a => a.Supervisor)
                      .WithMany()
                      .HasForeignKey(a => a.SupervisorId)
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

            modelBuilder.Entity<UserAuditTrail>(entity =>
            {
                entity.ToTable("user_audit_trail", "hms");
                entity.HasOne(x => x.User)
                    .WithMany()
                    .HasForeignKey(x => x.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<UserBranchMapping>(entity =>
            {
                entity.ToTable("user_branch_mapping", "hmsmaster");

                entity.HasOne(x => x.Organisation)
                    .WithMany()
                    .HasForeignKey(x => x.OrgId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.User)
                    .WithMany()
                    .HasForeignKey(x => x.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(x => x.CreatedByUser)
                    .WithMany()
                    .HasForeignKey(x => x.CreatedBy)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(x => x.Branch)
                    .WithMany()
                    .HasForeignKey(x => x.BranchId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(x => new { x.OrgId, x.UserId, x.BranchId })
                    .IsUnique()
                    .HasDatabaseName("uq_user_branch_mapping");
            });

            modelBuilder.Entity<Organisation>(entity =>
            {
                entity.ToTable("organisation", "app_subscription");
            });
            modelBuilder.Entity<DesignationMaster>()
                .Property(e => e.HierarchyPath)
                .HasColumnType("ltree");

            modelBuilder.Entity<LocationMaster>(entity =>
            {
                // Define the Unique Constraint (uq_loc)
                entity.HasIndex(e => new { e.OrgId, e.ChannelId, e.SubChannelId, e.LocationCode })
                      .IsUnique()
                      .HasDatabaseName("uq_loc");

                // Optional: Map to the specific schema if not using [Table] attribute
                entity.ToTable("location_master", "hmsmaster");
            });
            modelBuilder.Entity<PartnerBranchHeirarchy>()
                .Property(e => e.HierarchyPath)
                .HasColumnType("ltree");

            modelBuilder.Entity<ChannelBranchHeirarchy>()
                .Property(e => e.HierarchyPath)
                .HasColumnType("ltree");

            modelBuilder.Entity<AgentBranchMapping>(entity =>
            {
                entity.ToTable("agent_branch_mapping", "hmsmaster");

                entity.HasOne(x => x.Organisation)
                    .WithMany()
                    .HasForeignKey(x => x.OrgId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.Agent)
                    .WithMany()
                    .HasForeignKey(x => x.AgentId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.Branch)
                    .WithMany()
                    .HasForeignKey(x => x.BranchId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(x => new { x.OrgId, x.AgentId, x.BranchId })
                    .IsUnique()
                    .HasDatabaseName("uq_agent_branch_mapping");
            });

            modelBuilder.HasDefaultSchema("hms");
        }
    }
}