﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using PeiuPlatform.App;

namespace PeiuPlatform.App.Migrations
{
    [DbContext(typeof(AccountEF))]
    [Migration("20190903065317_InitializeCreate")]
    partial class InitializeCreate
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.1.11-servicing-32099")
                .HasAnnotation("Relational:MaxIdentifierLength", 64);

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("ClaimType");

                    b.Property<string>("ClaimValue");

                    b.Property<string>("RoleId")
                        .IsRequired();

                    b.HasKey("Id");

                    b.HasIndex("RoleId");

                    b.ToTable("RoleClaims");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<string>", b =>
                {
                    b.Property<string>("LoginProvider");

                    b.Property<string>("ProviderKey");

                    b.Property<string>("ProviderDisplayName");

                    b.Property<string>("UserId")
                        .IsRequired();

                    b.HasKey("LoginProvider", "ProviderKey");

                    b.HasIndex("UserId");

                    b.ToTable("UserLogins");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<string>", b =>
                {
                    b.Property<string>("UserId");

                    b.Property<string>("LoginProvider");

                    b.Property<string>("Name");

                    b.Property<string>("Value");

                    b.HasKey("UserId", "LoginProvider", "Name");

                    b.ToTable("UserTokens");
                });

            modelBuilder.Entity("PEIU.Models.Database.AggregatorGroup", b =>
                {
                    b.Property<string>("ID")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("AggName");

                    b.Property<string>("Representation");

                    b.HasKey("ID");

                    b.ToTable("AggregatorGroups");
                });

            modelBuilder.Entity("PEIU.Models.Database.AggregatorUser", b =>
                {
                    b.Property<string>("UserId");

                    b.Property<string>("AggGroupId");

                    b.HasKey("UserId");

                    b.HasIndex("AggGroupId");

                    b.ToTable("AggregatorUsers");
                });

            modelBuilder.Entity("PEIU.Models.Database.ContractorAsset", b =>
                {
                    b.Property<string>("UniqueId")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("AssetName");

                    b.Property<int>("AssetType");

                    b.Property<double>("CapacityKW");

                    b.Property<DateTime>("InstalDate");

                    b.Property<DateTime>("LastMaintenance");

                    b.Property<int>("SiteId");

                    b.HasKey("UniqueId");

                    b.HasIndex("SiteId");

                    b.ToTable("ContractorAssets");
                });

            modelBuilder.Entity("PEIU.Models.Database.ContractorSite", b =>
                {
                    b.Property<int>("SiteId")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Address1");

                    b.Property<string>("Address2");

                    b.Property<string>("Comment");

                    b.Property<string>("ContractUserId");

                    b.Property<int?>("DLNo");

                    b.Property<double>("Latitude");

                    b.Property<string>("LawFirstCode");

                    b.Property<string>("LawLastCode");

                    b.Property<string>("LawMiddleCode");

                    b.Property<double>("Longtidue");

                    b.Property<int>("RCC");

                    b.Property<DateTime>("RegisterTimestamp");

                    b.Property<string>("Represenation");

                    b.Property<int>("ServiceCode");

                    b.HasKey("SiteId");

                    b.HasIndex("ContractUserId");

                    b.ToTable("ContractorSites");
                });

            modelBuilder.Entity("PEIU.Models.Database.ContractorUser", b =>
                {
                    b.Property<string>("UserId");

                    b.Property<string>("AggGroupId");

                    b.Property<int>("ContractStatus");

                    b.HasKey("UserId");

                    b.HasIndex("AggGroupId");

                    b.ToTable("ContractorUsers");
                });

            modelBuilder.Entity("PEIU.Models.Database.Role", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken();

                    b.Property<string>("Category")
                        .HasMaxLength(128);

                    b.Property<string>("Name")
                        .HasMaxLength(256);

                    b.Property<string>("NormalizedName")
                        .HasMaxLength(128);

                    b.HasKey("Id");

                    b.HasIndex("NormalizedName")
                        .IsUnique()
                        .HasName("RoleNameIndex");

                    b.ToTable("Roles");
                });

            modelBuilder.Entity("PEIU.Models.Database.SupervisorUser", b =>
                {
                    b.Property<string>("UserId");

                    b.HasKey("UserId");

                    b.ToTable("SupervisorUsers");
                });

            modelBuilder.Entity("PEIU.Models.Database.TemporaryContractorAsset", b =>
                {
                    b.Property<string>("UniqueId")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("AssetName");

                    b.Property<int>("AssetType");

                    b.Property<double>("CapacityKW");

                    b.Property<string>("SiteId");

                    b.HasKey("UniqueId");

                    b.HasIndex("SiteId");

                    b.ToTable("TemporaryContractorAssets");
                });

            modelBuilder.Entity("PEIU.Models.Database.TemporaryContractorSite", b =>
                {
                    b.Property<string>("UniqueId")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Address1");

                    b.Property<string>("Address2");

                    b.Property<string>("ContractUserId");

                    b.Property<double>("Latitude");

                    b.Property<string>("LawFirstCode");

                    b.Property<string>("LawLastCode");

                    b.Property<string>("LawMiddleCode");

                    b.Property<double>("Longtidue");

                    b.Property<DateTime>("RegisterTimestamp");

                    b.Property<int>("ServiceCode");

                    b.HasKey("UniqueId");

                    b.HasIndex("ContractUserId");

                    b.ToTable("TemporaryContractorSites");
                });

            modelBuilder.Entity("PEIU.Models.Database.UserAccount", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("AccessFailedCount");

                    b.Property<string>("Address");

                    b.Property<string>("CompanyName");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken();

                    b.Property<string>("Email")
                        .HasMaxLength(256);

                    b.Property<bool>("EmailConfirmed");

                    b.Property<DateTime>("Expire");

                    b.Property<string>("FirstName");

                    b.Property<string>("LastName");

                    b.Property<bool>("LockoutEnabled");

                    b.Property<DateTimeOffset?>("LockoutEnd");

                    b.Property<string>("NormalizedEmail")
                        .HasMaxLength(256);

                    b.Property<string>("NormalizedUserName")
                        .HasMaxLength(128);

                    b.Property<string>("PasswordHash");

                    b.Property<string>("PhoneNumber");

                    b.Property<bool>("PhoneNumberConfirmed");

                    b.Property<DateTime>("RegistDate");

                    b.Property<string>("RegistrationNumber");

                    b.Property<string>("SecurityStamp");

                    b.Property<bool>("SignInConfirm");

                    b.Property<bool>("TwoFactorEnabled");

                    b.Property<string>("UserName")
                        .HasMaxLength(256);

                    b.HasKey("Id");

                    b.HasIndex("NormalizedEmail")
                        .HasName("EmailIndex");

                    b.HasIndex("NormalizedUserName")
                        .IsUnique()
                        .HasName("UserNameIndex");

                    b.ToTable("UserAccounts");
                });

            modelBuilder.Entity("PEIU.Models.Database.UserClaim", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("ClaimType");

                    b.Property<string>("ClaimValue");

                    b.Property<string>("UserId")
                        .IsRequired();

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("UserClaims");
                });

            modelBuilder.Entity("PEIU.Models.Database.UserRole", b =>
                {
                    b.Property<string>("UserId");

                    b.Property<string>("RoleId");

                    b.HasKey("UserId", "RoleId");

                    b.HasIndex("RoleId");

                    b.ToTable("UserRoles");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>", b =>
                {
                    b.HasOne("PEIU.Models.Database.Role")
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<string>", b =>
                {
                    b.HasOne("PEIU.Models.Database.UserAccount")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<string>", b =>
                {
                    b.HasOne("PEIU.Models.Database.UserAccount")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("PEIU.Models.Database.AggregatorUser", b =>
                {
                    b.HasOne("PEIU.Models.Database.AggregatorGroup", "AggregatorGroup")
                        .WithMany("AggregatorUsers")
                        .HasForeignKey("AggGroupId");

                    b.HasOne("PEIU.Models.Database.UserAccount", "User")
                        .WithOne("Aggregator")
                        .HasForeignKey("PEIU.Models.Database.AggregatorUser", "UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("PEIU.Models.Database.ContractorAsset", b =>
                {
                    b.HasOne("PEIU.Models.Database.ContractorSite", "ContractorSite")
                        .WithMany("ContractorAssets")
                        .HasForeignKey("SiteId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("PEIU.Models.Database.ContractorSite", b =>
                {
                    b.HasOne("PEIU.Models.Database.ContractorUser", "ContractUser")
                        .WithMany("ContractorSite")
                        .HasForeignKey("ContractUserId");
                });

            modelBuilder.Entity("PEIU.Models.Database.ContractorUser", b =>
                {
                    b.HasOne("PEIU.Models.Database.AggregatorGroup", "AggregatorGroup")
                        .WithMany("ContractorUsers")
                        .HasForeignKey("AggGroupId");

                    b.HasOne("PEIU.Models.Database.UserAccount", "User")
                        .WithOne("Contractor")
                        .HasForeignKey("PEIU.Models.Database.ContractorUser", "UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("PEIU.Models.Database.SupervisorUser", b =>
                {
                    b.HasOne("PEIU.Models.Database.UserAccount", "User")
                        .WithOne("Supervisor")
                        .HasForeignKey("PEIU.Models.Database.SupervisorUser", "UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("PEIU.Models.Database.TemporaryContractorAsset", b =>
                {
                    b.HasOne("PEIU.Models.Database.TemporaryContractorSite", "ContractorSite")
                        .WithMany("ContractorAssets")
                        .HasForeignKey("SiteId");
                });

            modelBuilder.Entity("PEIU.Models.Database.TemporaryContractorSite", b =>
                {
                    b.HasOne("PEIU.Models.Database.ContractorUser", "ContractUser")
                        .WithMany()
                        .HasForeignKey("ContractUserId");
                });

            modelBuilder.Entity("PEIU.Models.Database.UserClaim", b =>
                {
                    b.HasOne("PEIU.Models.Database.UserAccount")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("PEIU.Models.Database.UserRole", b =>
                {
                    b.HasOne("PEIU.Models.Database.Role")
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("PEIU.Models.Database.UserAccount")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });
#pragma warning restore 612, 618
        }
    }
}
