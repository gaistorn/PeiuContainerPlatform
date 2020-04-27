using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PEIU.Models;
using Power21.PEIUEcosystem.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PEIU.Service.WebApiService
{
    public class AccountRecordContext : IdentityDbContext<AccountModel>
    {
        public DbSet<AssetDevices> DevicesInfos { get; set; }
        public DbSet<AssetLocation> AssetLocations { get; set; }

        public DbSet<ReservedAssetLocation> ReservedAssetLocations { get; set; }

        public AccountRecordContext(DbContextOptions<AccountRecordContext> options) : base(options)
        {
            
            //AccountRecordContext s;s.Find()
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Entity<AssetDevices>().HasKey(x => x.PK);
            builder.Entity<AssetLocation>().HasKey(m => m.SiteId);
            builder.Entity<ReservedAssetLocation>().HasKey(x => x.ID);

            //builder.Entity<IdentityUser>().ToTable("MyUsers").Property(p => p.Id).HasColumnName("UserId");
            //builder.Entity<AccountModel>().ToTable("MyUsers").Property(p => p.Id).HasColumnName("UserId");
            //builder.Entity<IdentityUserRole<string>>().ToTable("MyUserRoles");
            //builder.Entity<IdentityUserLogin<string>>().ToTable("MyUserLogins");
            //builder.Entity<IdentityUserClaim<int>>().ToTable("MyUserClaims");
            //builder.Entity<IdentityRole>().ToTable("MyRoles");
            //// shadow properties
            //builder.Entity<EventLogData>().Property<DateTime>("UpdatedTimestamp");
            //builder.Entity<SourceInfo>().Property<DateTime>("UpdatedTimestamp");


        }
    }

}
