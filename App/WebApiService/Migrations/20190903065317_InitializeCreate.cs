using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace PeiuPlatform.App.Migrations
{
    public partial class InitializeCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AggregatorGroups",
                columns: table => new
                {
                    ID = table.Column<string>(nullable: false),
                    AggName = table.Column<string>(nullable: true),
                    Representation = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AggregatorGroups", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    Name = table.Column<string>(maxLength: 256, nullable: true),
                    Category = table.Column<string>(maxLength: 128, nullable: false),
                    NormalizedName = table.Column<string>(maxLength: 128, nullable: true),
                    ConcurrencyStamp = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserAccounts",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    UserName = table.Column<string>(maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(maxLength: 128, nullable: true),
                    Email = table.Column<string>(maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(nullable: false),
                    PasswordHash = table.Column<string>(nullable: true),
                    SecurityStamp = table.Column<string>(nullable: true),
                    ConcurrencyStamp = table.Column<string>(nullable: true),
                    PhoneNumber = table.Column<string>(nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(nullable: false),
                    TwoFactorEnabled = table.Column<bool>(nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(nullable: true),
                    LockoutEnabled = table.Column<bool>(nullable: false),
                    AccessFailedCount = table.Column<int>(nullable: false),
                    FirstName = table.Column<string>(nullable: true),
                    LastName = table.Column<string>(nullable: true),
                    CompanyName = table.Column<string>(nullable: true),
                    RegistrationNumber = table.Column<string>(nullable: true),
                    Address = table.Column<string>(nullable: true),
                    RegistDate = table.Column<DateTime>(nullable: false),
                    Expire = table.Column<DateTime>(nullable: false),
                    SignInConfirm = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserAccounts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    RoleId = table.Column<string>(nullable: false),
                    ClaimType = table.Column<string>(nullable: true),
                    ClaimValue = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RoleClaims_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AggregatorUsers",
                columns: table => new
                {
                    UserId = table.Column<string>(nullable: false),
                    AggGroupId = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AggregatorUsers", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_AggregatorUsers_AggregatorGroups_AggGroupId",
                        column: x => x.AggGroupId,
                        principalTable: "AggregatorGroups",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AggregatorUsers_UserAccounts_UserId",
                        column: x => x.UserId,
                        principalTable: "UserAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ContractorUsers",
                columns: table => new
                {
                    UserId = table.Column<string>(nullable: false),
                    AggGroupId = table.Column<string>(nullable: true),
                    ContractStatus = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContractorUsers", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_ContractorUsers_AggregatorGroups_AggGroupId",
                        column: x => x.AggGroupId,
                        principalTable: "AggregatorGroups",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ContractorUsers_UserAccounts_UserId",
                        column: x => x.UserId,
                        principalTable: "UserAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SupervisorUsers",
                columns: table => new
                {
                    UserId = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupervisorUsers", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_SupervisorUsers_UserAccounts_UserId",
                        column: x => x.UserId,
                        principalTable: "UserAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UserId = table.Column<string>(nullable: false),
                    ClaimType = table.Column<string>(nullable: true),
                    ClaimValue = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserClaims_UserAccounts_UserId",
                        column: x => x.UserId,
                        principalTable: "UserAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(nullable: false),
                    ProviderKey = table.Column<string>(nullable: false),
                    ProviderDisplayName = table.Column<string>(nullable: true),
                    UserId = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_UserLogins_UserAccounts_UserId",
                        column: x => x.UserId,
                        principalTable: "UserAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(nullable: false),
                    RoleId = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_UserRoles_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserRoles_UserAccounts_UserId",
                        column: x => x.UserId,
                        principalTable: "UserAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(nullable: false),
                    LoginProvider = table.Column<string>(nullable: false),
                    Name = table.Column<string>(nullable: false),
                    Value = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_UserTokens_UserAccounts_UserId",
                        column: x => x.UserId,
                        principalTable: "UserAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ContractorSites",
                columns: table => new
                {
                    SiteId = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    RCC = table.Column<int>(nullable: false),
                    Longtidue = table.Column<double>(nullable: false),
                    Latitude = table.Column<double>(nullable: false),
                    LawFirstCode = table.Column<string>(nullable: true),
                    LawMiddleCode = table.Column<string>(nullable: true),
                    LawLastCode = table.Column<string>(nullable: true),
                    Address1 = table.Column<string>(nullable: true),
                    Address2 = table.Column<string>(nullable: true),
                    Represenation = table.Column<string>(nullable: true),
                    DLNo = table.Column<int>(nullable: true),
                    ContractUserId = table.Column<string>(nullable: true),
                    ServiceCode = table.Column<int>(nullable: false),
                    Comment = table.Column<string>(nullable: true),
                    RegisterTimestamp = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContractorSites", x => x.SiteId);
                    table.ForeignKey(
                        name: "FK_ContractorSites_ContractorUsers_ContractUserId",
                        column: x => x.ContractUserId,
                        principalTable: "ContractorUsers",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TemporaryContractorSites",
                columns: table => new
                {
                    UniqueId = table.Column<string>(nullable: false),
                    Longtidue = table.Column<double>(nullable: false),
                    Latitude = table.Column<double>(nullable: false),
                    LawFirstCode = table.Column<string>(nullable: true),
                    LawMiddleCode = table.Column<string>(nullable: true),
                    LawLastCode = table.Column<string>(nullable: true),
                    Address1 = table.Column<string>(nullable: true),
                    Address2 = table.Column<string>(nullable: true),
                    ContractUserId = table.Column<string>(nullable: true),
                    ServiceCode = table.Column<int>(nullable: false),
                    RegisterTimestamp = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TemporaryContractorSites", x => x.UniqueId);
                    table.ForeignKey(
                        name: "FK_TemporaryContractorSites_ContractorUsers_ContractUserId",
                        column: x => x.ContractUserId,
                        principalTable: "ContractorUsers",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ContractorAssets",
                columns: table => new
                {
                    UniqueId = table.Column<string>(nullable: false),
                    SiteId = table.Column<int>(nullable: false),
                    AssetType = table.Column<int>(nullable: false),
                    CapacityKW = table.Column<double>(nullable: false),
                    InstalDate = table.Column<DateTime>(nullable: false),
                    LastMaintenance = table.Column<DateTime>(nullable: false),
                    AssetName = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContractorAssets", x => x.UniqueId);
                    table.ForeignKey(
                        name: "FK_ContractorAssets_ContractorSites_SiteId",
                        column: x => x.SiteId,
                        principalTable: "ContractorSites",
                        principalColumn: "SiteId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TemporaryContractorAssets",
                columns: table => new
                {
                    UniqueId = table.Column<string>(nullable: false),
                    SiteId = table.Column<string>(nullable: true),
                    AssetType = table.Column<int>(nullable: false),
                    CapacityKW = table.Column<double>(nullable: false),
                    AssetName = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TemporaryContractorAssets", x => x.UniqueId);
                    table.ForeignKey(
                        name: "FK_TemporaryContractorAssets_TemporaryContractorSites_SiteId",
                        column: x => x.SiteId,
                        principalTable: "TemporaryContractorSites",
                        principalColumn: "UniqueId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AggregatorUsers_AggGroupId",
                table: "AggregatorUsers",
                column: "AggGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_ContractorAssets_SiteId",
                table: "ContractorAssets",
                column: "SiteId");

            migrationBuilder.CreateIndex(
                name: "IX_ContractorSites_ContractUserId",
                table: "ContractorSites",
                column: "ContractUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ContractorUsers_AggGroupId",
                table: "ContractorUsers",
                column: "AggGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_RoleClaims_RoleId",
                table: "RoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "Roles",
                column: "NormalizedName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TemporaryContractorAssets_SiteId",
                table: "TemporaryContractorAssets",
                column: "SiteId");

            migrationBuilder.CreateIndex(
                name: "IX_TemporaryContractorSites_ContractUserId",
                table: "TemporaryContractorSites",
                column: "ContractUserId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "UserAccounts",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "UserAccounts",
                column: "NormalizedUserName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserClaims_UserId",
                table: "UserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserLogins_UserId",
                table: "UserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserRoles_RoleId",
                table: "UserRoles",
                column: "RoleId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AggregatorUsers");

            migrationBuilder.DropTable(
                name: "ContractorAssets");

            migrationBuilder.DropTable(
                name: "RoleClaims");

            migrationBuilder.DropTable(
                name: "SupervisorUsers");

            migrationBuilder.DropTable(
                name: "TemporaryContractorAssets");

            migrationBuilder.DropTable(
                name: "UserClaims");

            migrationBuilder.DropTable(
                name: "UserLogins");

            migrationBuilder.DropTable(
                name: "UserRoles");

            migrationBuilder.DropTable(
                name: "UserTokens");

            migrationBuilder.DropTable(
                name: "ContractorSites");

            migrationBuilder.DropTable(
                name: "TemporaryContractorSites");

            migrationBuilder.DropTable(
                name: "Roles");

            migrationBuilder.DropTable(
                name: "ContractorUsers");

            migrationBuilder.DropTable(
                name: "AggregatorGroups");

            migrationBuilder.DropTable(
                name: "UserAccounts");
        }
    }
}
