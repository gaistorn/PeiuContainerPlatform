using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using PeiuPlatform.Model.Database;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace PeiuPlatform.App
{
    public class AccountEF : IdentityDbContext<UserAccountEF, 
        Role, 
        string, 
        UserClaim, 
        UserRole, 
        IdentityUserLogin<string>, 
        IdentityRoleClaim<string>, 
        IdentityUserToken<string>>,         IDesignTimeDbContextFactory<AccountEF>
    {
        public DbSet<AggregatorGroupEF> AggregatorGroups { get; set; }
        public DbSet<VwContractorsiteEF> VwContractorsites { get; set; }
        public DbSet<VwAggregatoruserEF> VwAggregatorusers { get; set; }
        public DbSet<UserLog> UserLogs { get; set; }

        public DbSet<SupervisorUserEF> SupervisorUsers { get; set; }
        //public DbSet<VwTemporarycontractoruser> VwTemporarycontractorusers { get; set; }
        public DbSet<VwContractoruserEF> VwContractorusers { get; set; }
        public DbSet<AggregatorUserEF> AggregatorUsers { get; set; }
        public DbSet<ContractorUserEF> ContractorUsers { get; set; }

        public DbSet<RegisterFileRepositaryEF> RegisterFileRepositaries { get; set; }
        public DbSet<ContractorSiteEF> ContractorSites { get; set; }
        public DbSet<ContractorAssetEF> ContractorAssets { get; set; }
        public DbSet<TemporaryContractorSiteEF> TemporaryContractorSites { get; set; }

        public DbSet<TemporaryContractorAssetEF> TemporaryContractorAssets { get; set; }

        public AccountEF() { }

        public AccountEF(DbContextOptions<AccountEF> options) : base(options)
        {
            
            //AccountRecordContext s;s.Find()
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Entity<UserAccountEF>().ToTable("UserAccounts");
            builder.Entity<IdentityRoleClaim<string>>().ToTable("RoleClaims");
            builder.Entity<IdentityUserToken<string>>().ToTable("UserTokens");
            builder.Entity<IdentityUserLogin<string>>().ToTable("UserLogins");
            builder.Entity<UserClaim>().ToTable("UserClaims");
            builder.Entity<UserRole>().ToTable("UserRoles");
            builder.Entity<Role>().ToTable("Roles");
            builder.Entity<UserAccountEF>()
                .HasOne<ContractorUserEF>(x => x.Contractor)
                .WithOne(x => x.User)
                .HasForeignKey<ContractorUserEF>(ad => ad.UserId);
            builder.Entity<UserAccountEF>()
                .HasOne<AggregatorUserEF>(x => x.Aggregator)
                .WithOne(x => x.User)
                .HasForeignKey<AggregatorUserEF>(ad => ad.UserId);
            builder.Entity<UserAccountEF>()
                .HasOne<SupervisorUserEF>(x => x.Supervisor)
                .WithOne(x => x.User)
                .HasForeignKey<SupervisorUserEF>(ad => ad.UserId);
        }

        public AccountEF CreateDbContext(string[] args)
        {
            IConfigurationRoot configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .Build();
            var builder = new DbContextOptionsBuilder<AccountEF>();
            var connectionString = configuration.GetConnectionString("peiu_account_connnectionstring");
            builder.UseMySql(connectionString);
            return new AccountEF(builder.Options);
        }

        //protected override void OnModelCreating(ModelBuilder builder)
        //{
        //    base.OnModelCreating(builder);
        //    builder.Entity<AssetDevices>().HasKey(x => x.PK);
        //    builder.Entity<AssetLocation>().HasKey(m => m.SiteId);
        //    builder.Entity<ReservedAssetLocation>().HasKey(x => x.ID);

        //    //builder.Entity<IdentityUser>().ToTable("MyUsers").Property(p => p.Id).HasColumnName("UserId");
        //    //builder.Entity<AccountModel>().ToTable("MyUsers").Property(p => p.Id).HasColumnName("UserId");
        //    //builder.Entity<IdentityUserRole<string>>().ToTable("MyUserRoles");
        //    //builder.Entity<IdentityUserLogin<string>>().ToTable("MyUserLogins");
        //    //builder.Entity<IdentityUserClaim<int>>().ToTable("MyUserClaims");
        //    //builder.Entity<IdentityRole>().ToTable("MyRoles");
        //    //// shadow properties
        //    //builder.Entity<EventLogData>().Property<DateTime>("UpdatedTimestamp");
        //    //builder.Entity<SourceInfo>().Property<DateTime>("UpdatedTimestamp");


        //}
    }
}
