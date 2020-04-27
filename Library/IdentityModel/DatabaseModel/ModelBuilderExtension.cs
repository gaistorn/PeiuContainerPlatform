using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace PeiuPlatform.Model.Database
{
    public static class ModelBuilderExtension
    {

        public static void ModelBuildUp(this ModelBuilder builder)
        {
            builder.Entity<AggregatorGroupEF>()
                .HasMany(x => x.AggregatorUsers)
                .WithOne(x => x.AggregatorGroup)
                .OnDelete(DeleteBehavior.SetNull)
                .IsRequired();
            builder.Entity<AggregatorGroupEF>()
                .HasMany(x => x.ContractorUsers)
                .WithOne(x => x.AggregatorGroup)
                .OnDelete(DeleteBehavior.SetNull)
                .IsRequired();

            builder.Entity<ContractorUserEF>()
                .HasMany(x => x.ContractorSite)
                .WithOne(x => x.ContractUser)
                .OnDelete(DeleteBehavior.SetNull)
                .IsRequired();

            builder.Entity<ContractorSiteEF>()
                .HasMany(x => x.ContractorAssets)
                .WithOne(x => x.ContractorSite)
                .OnDelete(DeleteBehavior.SetNull)
                .IsRequired();
        }
    }
}
