using System;
using Microsoft.EntityFrameworkCore.Migrations;
using MySql.EntityFrameworkCore.Metadata;

#nullable disable

namespace NLOIQ.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ContentArchitecture : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    Name = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "longtext", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    DisplayName = table.Column<string>(type: "varchar(160)", maxLength: 160, nullable: false),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    UserName = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    PasswordHash = table.Column<string>(type: "longtext", nullable: true),
                    SecurityStamp = table.Column<string>(type: "longtext", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "longtext", nullable: true),
                    PhoneNumber = table.Column<string>(type: "longtext", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "datetime", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "MediaAssets",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    StorageProvider = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: false),
                    StorageKey = table.Column<string>(type: "varchar(300)", maxLength: 300, nullable: false, collation: "utf8mb4_bin"),
                    OriginalFileName = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false),
                    ContentType = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    ByteSize = table.Column<long>(type: "bigint", nullable: false),
                    Width = table.Column<int>(type: "int", nullable: true),
                    Height = table.Column<int>(type: "int", nullable: true),
                    Sha256 = table.Column<string>(type: "char(64)", nullable: false),
                    IsDemo = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CreatedUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MediaAssets", x => x.Id);
                    table.CheckConstraint("CK_MediaAssets_Bytes", "`ByteSize` > 0");
                    table.CheckConstraint("CK_MediaAssets_Dimensions", "(`Width` IS NULL OR `Width` > 0) AND (`Height` IS NULL OR `Height` > 0)");
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    RoleId = table.Column<Guid>(type: "char(36)", nullable: false),
                    ClaimType = table.Column<string>(type: "longtext", nullable: true),
                    ClaimValue = table.Column<string>(type: "longtext", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    UserId = table.Column<Guid>(type: "char(36)", nullable: false),
                    ClaimType = table.Column<string>(type: "longtext", nullable: true),
                    ClaimValue = table.Column<string>(type: "longtext", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "varchar(255)", nullable: false),
                    ProviderKey = table.Column<string>(type: "varchar(255)", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "longtext", nullable: true),
                    UserId = table.Column<Guid>(type: "char(36)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "char(36)", nullable: false),
                    RoleId = table.Column<Guid>(type: "char(36)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "char(36)", nullable: false),
                    LoginProvider = table.Column<string>(type: "varchar(255)", nullable: false),
                    Name = table.Column<string>(type: "varchar(255)", nullable: false),
                    Value = table.Column<string>(type: "longtext", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "MediaAssetTranslations",
                columns: table => new
                {
                    MediaAssetId = table.Column<long>(type: "bigint", nullable: false),
                    Culture = table.Column<string>(type: "varchar(2)", maxLength: 2, nullable: false),
                    AltText = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MediaAssetTranslations", x => new { x.MediaAssetId, x.Culture });
                    table.CheckConstraint("CK_MediaAssetTranslations_Culture", "`Culture` IN ('en','ar')");
                    table.ForeignKey(
                        name: "FK_MediaAssetTranslations_MediaAssets_MediaAssetId",
                        column: x => x.MediaAssetId,
                        principalTable: "MediaAssets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "AuditLogs",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    OccurredUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    ActorUserId = table.Column<Guid>(type: "char(36)", nullable: true),
                    ActorLabel = table.Column<string>(type: "varchar(160)", maxLength: 160, nullable: false),
                    Action = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: false),
                    ContentItemId = table.Column<long>(type: "bigint", nullable: true),
                    RevisionId = table.Column<long>(type: "bigint", nullable: true),
                    DetailsJson = table.Column<string>(type: "longtext", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AuditLogs_AspNetUsers_ActorUserId",
                        column: x => x.ActorUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "BusinessHours",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    RevisionId = table.Column<long>(type: "bigint", nullable: false),
                    DayOfWeek = table.Column<int>(type: "int", nullable: false),
                    IsClosed = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    OpensAt = table.Column<TimeOnly>(type: "time", nullable: true),
                    ClosesAt = table.Column<TimeOnly>(type: "time", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BusinessHours", x => x.Id);
                    table.CheckConstraint("CK_BusinessHours_Day", "`DayOfWeek` BETWEEN 0 AND 6");
                    table.CheckConstraint("CK_BusinessHours_Times", "(`IsClosed` = 1 AND `OpensAt` IS NULL AND `ClosesAt` IS NULL) OR (`IsClosed` = 0 AND `OpensAt` IS NOT NULL AND `ClosesAt` IS NOT NULL)");
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ContactDetails",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    RevisionId = table.Column<long>(type: "bigint", nullable: false),
                    Kind = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false),
                    Value = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContactDetails", x => x.Id);
                    table.UniqueConstraint("AK_ContactDetails_RevisionId_Id", x => new { x.RevisionId, x.Id });
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ContactSettings",
                columns: table => new
                {
                    RevisionId = table.Column<long>(type: "bigint", nullable: false),
                    TimeZoneId = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContactSettings", x => x.RevisionId);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ContactTranslations",
                columns: table => new
                {
                    RevisionId = table.Column<long>(type: "bigint", nullable: false),
                    ContactDetailId = table.Column<long>(type: "bigint", nullable: false),
                    Culture = table.Column<string>(type: "varchar(2)", maxLength: 2, nullable: false),
                    Label = table.Column<string>(type: "varchar(120)", maxLength: 120, nullable: false),
                    LocalizedValue = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContactTranslations", x => new { x.RevisionId, x.ContactDetailId, x.Culture });
                    table.CheckConstraint("CK_ContactTranslations_Culture", "`Culture` IN ('en','ar')");
                    table.ForeignKey(
                        name: "FK_ContactTranslations_ContactDetails_RevisionId_ContactDetailId",
                        columns: x => new { x.RevisionId, x.ContactDetailId },
                        principalTable: "ContactDetails",
                        principalColumns: new[] { "RevisionId", "Id" },
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ContentItems",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    Kind = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: false),
                    Key = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: false, collation: "utf8mb4_bin"),
                    State = table.Column<string>(type: "varchar(16)", maxLength: 16, nullable: false),
                    IsDemo = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Version = table.Column<long>(type: "bigint", nullable: false),
                    CreatedUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    DraftRevisionId = table.Column<long>(type: "bigint", nullable: true),
                    PublishedRevisionId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContentItems", x => x.Id);
                    table.CheckConstraint("CK_ContentItems_PublishedPointer", "`State` <> 'Published' OR `PublishedRevisionId` IS NOT NULL");
                    table.CheckConstraint("CK_ContentItems_State", "`State` IN ('Draft','Published','Archived')");
                    table.CheckConstraint("CK_ContentItems_Version", "`Version` > 0");
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ContentRevisions",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    ContentItemId = table.Column<long>(type: "bigint", nullable: false),
                    RevisionNumber = table.Column<int>(type: "int", nullable: false),
                    SchemaVersion = table.Column<int>(type: "int", nullable: false),
                    CreatedUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "char(36)", nullable: true),
                    SupersedesRevisionId = table.Column<long>(type: "bigint", nullable: true),
                    ChangeNote = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContentRevisions", x => x.Id);
                    table.UniqueConstraint("AK_ContentRevisions_ContentItemId_Id", x => new { x.ContentItemId, x.Id });
                    table.CheckConstraint("CK_ContentRevisions_Number", "`RevisionNumber` > 0 AND `SchemaVersion` > 0");
                    table.ForeignKey(
                        name: "FK_ContentRevisions_AspNetUsers_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ContentRevisions_ContentItems_ContentItemId",
                        column: x => x.ContentItemId,
                        principalTable: "ContentItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ContentRevisions_ContentRevisions_ContentItemId_SupersedesRe~",
                        columns: x => new { x.ContentItemId, x.SupersedesRevisionId },
                        principalTable: "ContentRevisions",
                        principalColumns: new[] { "ContentItemId", "Id" },
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ContentTranslations",
                columns: table => new
                {
                    RevisionId = table.Column<long>(type: "bigint", nullable: false),
                    Culture = table.Column<string>(type: "varchar(2)", maxLength: 2, nullable: false),
                    Title = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false),
                    Summary = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: false),
                    Body = table.Column<string>(type: "longtext", maxLength: 16000, nullable: false),
                    Slug = table.Column<string>(type: "varchar(160)", maxLength: 160, nullable: true, collation: "utf8mb4_bin"),
                    PrimaryButtonLabel = table.Column<string>(type: "varchar(120)", maxLength: 120, nullable: true),
                    SecondaryButtonLabel = table.Column<string>(type: "varchar(120)", maxLength: 120, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContentTranslations", x => new { x.RevisionId, x.Culture });
                    table.CheckConstraint("CK_ContentTranslations_Culture", "`Culture` IN ('en','ar')");
                    table.ForeignKey(
                        name: "FK_ContentTranslations_ContentRevisions_RevisionId",
                        column: x => x.RevisionId,
                        principalTable: "ContentRevisions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "FooterSettings",
                columns: table => new
                {
                    RevisionId = table.Column<long>(type: "bigint", nullable: false),
                    ShowCopyright = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FooterSettings", x => x.RevisionId);
                    table.ForeignKey(
                        name: "FK_FooterSettings_ContentRevisions_RevisionId",
                        column: x => x.RevisionId,
                        principalTable: "ContentRevisions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "NavigationSettings",
                columns: table => new
                {
                    RevisionId = table.Column<long>(type: "bigint", nullable: false),
                    Location = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NavigationSettings", x => x.RevisionId);
                    table.ForeignKey(
                        name: "FK_NavigationSettings_ContentRevisions_RevisionId",
                        column: x => x.RevisionId,
                        principalTable: "ContentRevisions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Pages",
                columns: table => new
                {
                    RevisionId = table.Column<long>(type: "bigint", nullable: false),
                    LayoutKey = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pages", x => x.RevisionId);
                    table.ForeignKey(
                        name: "FK_Pages_ContentRevisions_RevisionId",
                        column: x => x.RevisionId,
                        principalTable: "ContentRevisions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ProjectCategories",
                columns: table => new
                {
                    RevisionId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectCategories", x => x.RevisionId);
                    table.ForeignKey(
                        name: "FK_ProjectCategories_ContentRevisions_RevisionId",
                        column: x => x.RevisionId,
                        principalTable: "ContentRevisions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "PublicationSchedules",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    ContentItemId = table.Column<long>(type: "bigint", nullable: false),
                    RevisionId = table.Column<long>(type: "bigint", nullable: false),
                    ScheduledUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    State = table.Column<string>(type: "varchar(16)", maxLength: 16, nullable: false),
                    CompletedUtc = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    Version = table.Column<long>(type: "bigint", nullable: false),
                    FailureCode = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PublicationSchedules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PublicationSchedules_ContentRevisions_ContentItemId_Revision~",
                        columns: x => new { x.ContentItemId, x.RevisionId },
                        principalTable: "ContentRevisions",
                        principalColumns: new[] { "ContentItemId", "Id" },
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "PublishedRoutes",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    Culture = table.Column<string>(type: "varchar(2)", maxLength: 2, nullable: false),
                    Scope = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: false, collation: "utf8mb4_bin"),
                    Slug = table.Column<string>(type: "varchar(160)", maxLength: 160, nullable: false, collation: "utf8mb4_bin"),
                    ContentItemId = table.Column<long>(type: "bigint", nullable: false),
                    RevisionId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PublishedRoutes", x => x.Id);
                    table.CheckConstraint("CK_PublishedRoutes_Culture", "`Culture` IN ('en','ar')");
                    table.ForeignKey(
                        name: "FK_PublishedRoutes_ContentRevisions_ContentItemId_RevisionId",
                        columns: x => new { x.ContentItemId, x.RevisionId },
                        principalTable: "ContentRevisions",
                        principalColumns: new[] { "ContentItemId", "Id" },
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "RevisionMedia",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    RevisionId = table.Column<long>(type: "bigint", nullable: false),
                    MediaAssetId = table.Column<long>(type: "bigint", nullable: false),
                    UsageKey = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RevisionMedia", x => x.Id);
                    table.UniqueConstraint("AK_RevisionMedia_RevisionId_Id", x => new { x.RevisionId, x.Id });
                    table.ForeignKey(
                        name: "FK_RevisionMedia_ContentRevisions_RevisionId",
                        column: x => x.RevisionId,
                        principalTable: "ContentRevisions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RevisionMedia_MediaAssets_MediaAssetId",
                        column: x => x.MediaAssetId,
                        principalTable: "MediaAssets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Sections",
                columns: table => new
                {
                    RevisionId = table.Column<long>(type: "bigint", nullable: false),
                    SectionKind = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false),
                    LayoutKey = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: false),
                    Background = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false),
                    IsVisible = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    ParallaxIntensity = table.Column<decimal>(type: "decimal(3,2)", precision: 3, scale: 2, nullable: false),
                    PrimaryButtonTarget = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true),
                    SecondaryButtonTarget = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sections", x => x.RevisionId);
                    table.CheckConstraint("CK_Sections_Parallax", "`ParallaxIntensity` BETWEEN 0 AND 1");
                    table.ForeignKey(
                        name: "FK_Sections_ContentRevisions_RevisionId",
                        column: x => x.RevisionId,
                        principalTable: "ContentRevisions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "SocialLinks",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    RevisionId = table.Column<long>(type: "bigint", nullable: false),
                    Platform = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: false),
                    Url = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SocialLinks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SocialLinks_ContentRevisions_RevisionId",
                        column: x => x.RevisionId,
                        principalTable: "ContentRevisions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ThemeSettings",
                columns: table => new
                {
                    RevisionId = table.Column<long>(type: "bigint", nullable: false),
                    Evergreen = table.Column<string>(type: "char(7)", nullable: false),
                    Forest = table.Column<string>(type: "char(7)", nullable: false),
                    Moss = table.Column<string>(type: "char(7)", nullable: false),
                    BurntOrange = table.Column<string>(type: "char(7)", nullable: false),
                    Copper = table.Column<string>(type: "char(7)", nullable: false),
                    Gold = table.Column<string>(type: "char(7)", nullable: false),
                    Ivory = table.Column<string>(type: "char(7)", nullable: false),
                    Charcoal = table.Column<string>(type: "char(7)", nullable: false),
                    LightAction = table.Column<string>(type: "char(7)", nullable: false),
                    SignalLight = table.Column<string>(type: "char(7)", nullable: false),
                    SignalDark = table.Column<string>(type: "char(7)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ThemeSettings", x => x.RevisionId);
                    table.ForeignKey(
                        name: "FK_ThemeSettings_ContentRevisions_RevisionId",
                        column: x => x.RevisionId,
                        principalTable: "ContentRevisions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "NavigationItems",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    RevisionId = table.Column<long>(type: "bigint", nullable: false),
                    ParentItemId = table.Column<long>(type: "bigint", nullable: true),
                    TargetContentItemId = table.Column<long>(type: "bigint", nullable: true),
                    Href = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsVisible = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NavigationItems", x => x.Id);
                    table.UniqueConstraint("AK_NavigationItems_RevisionId_Id", x => new { x.RevisionId, x.Id });
                    table.ForeignKey(
                        name: "FK_NavigationItems_ContentItems_TargetContentItemId",
                        column: x => x.TargetContentItemId,
                        principalTable: "ContentItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_NavigationItems_ContentRevisions_RevisionId",
                        column: x => x.RevisionId,
                        principalTable: "ContentRevisions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_NavigationItems_NavigationItems_RevisionId_ParentItemId",
                        columns: x => new { x.RevisionId, x.ParentItemId },
                        principalTable: "NavigationItems",
                        principalColumns: new[] { "RevisionId", "Id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_NavigationItems_NavigationSettings_RevisionId",
                        column: x => x.RevisionId,
                        principalTable: "NavigationSettings",
                        principalColumn: "RevisionId",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Projects",
                columns: table => new
                {
                    RevisionId = table.Column<long>(type: "bigint", nullable: false),
                    YouTubeVideoId = table.Column<string>(type: "varchar(11)", maxLength: 11, nullable: true),
                    IsFeatured = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    CoverMediaUsageId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Projects", x => x.RevisionId);
                    table.CheckConstraint("CK_Projects_Order", "`DisplayOrder` >= 0");
                    table.ForeignKey(
                        name: "FK_Projects_ContentRevisions_RevisionId",
                        column: x => x.RevisionId,
                        principalTable: "ContentRevisions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Projects_RevisionMedia_RevisionId_CoverMediaUsageId",
                        columns: x => new { x.RevisionId, x.CoverMediaUsageId },
                        principalTable: "RevisionMedia",
                        principalColumns: new[] { "RevisionId", "Id" },
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "RevisionMediaTranslations",
                columns: table => new
                {
                    RevisionId = table.Column<long>(type: "bigint", nullable: false),
                    RevisionMediaId = table.Column<long>(type: "bigint", nullable: false),
                    Culture = table.Column<string>(type: "varchar(2)", maxLength: 2, nullable: false),
                    AltText = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false),
                    Caption = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RevisionMediaTranslations", x => new { x.RevisionId, x.RevisionMediaId, x.Culture });
                    table.CheckConstraint("CK_RevisionMediaTranslations_Culture", "`Culture` IN ('en','ar')");
                    table.ForeignKey(
                        name: "FK_RevisionMediaTranslations_ContentRevisions_RevisionId",
                        column: x => x.RevisionId,
                        principalTable: "ContentRevisions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RevisionMediaTranslations_RevisionMedia_RevisionId_RevisionM~",
                        columns: x => new { x.RevisionId, x.RevisionMediaId },
                        principalTable: "RevisionMedia",
                        principalColumns: new[] { "RevisionId", "Id" },
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "SeoMetadata",
                columns: table => new
                {
                    RevisionId = table.Column<long>(type: "bigint", nullable: false),
                    Culture = table.Column<string>(type: "varchar(2)", maxLength: 2, nullable: false),
                    Title = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "varchar(320)", maxLength: 320, nullable: false),
                    OpenGraphTitle = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true),
                    OpenGraphDescription = table.Column<string>(type: "varchar(320)", maxLength: 320, nullable: true),
                    OpenGraphMediaUsageId = table.Column<long>(type: "bigint", nullable: true),
                    CanonicalPath = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true),
                    NoIndex = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SeoMetadata", x => new { x.RevisionId, x.Culture });
                    table.CheckConstraint("CK_SeoMetadata_Culture", "`Culture` IN ('en','ar')");
                    table.ForeignKey(
                        name: "FK_SeoMetadata_ContentRevisions_RevisionId",
                        column: x => x.RevisionId,
                        principalTable: "ContentRevisions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SeoMetadata_ContentTranslations_RevisionId_Culture",
                        columns: x => new { x.RevisionId, x.Culture },
                        principalTable: "ContentTranslations",
                        principalColumns: new[] { "RevisionId", "Culture" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SeoMetadata_RevisionMedia_RevisionId_OpenGraphMediaUsageId",
                        columns: x => new { x.RevisionId, x.OpenGraphMediaUsageId },
                        principalTable: "RevisionMedia",
                        principalColumns: new[] { "RevisionId", "Id" },
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Services",
                columns: table => new
                {
                    RevisionId = table.Column<long>(type: "bigint", nullable: false),
                    IconKey = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsVisible = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    MediaUsageId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Services", x => x.RevisionId);
                    table.CheckConstraint("CK_Services_Order", "`DisplayOrder` >= 0");
                    table.ForeignKey(
                        name: "FK_Services_ContentRevisions_RevisionId",
                        column: x => x.RevisionId,
                        principalTable: "ContentRevisions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Services_RevisionMedia_RevisionId_MediaUsageId",
                        columns: x => new { x.RevisionId, x.MediaUsageId },
                        principalTable: "RevisionMedia",
                        principalColumns: new[] { "RevisionId", "Id" },
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "SiteSettings",
                columns: table => new
                {
                    RevisionId = table.Column<long>(type: "bigint", nullable: false),
                    SiteName = table.Column<string>(type: "varchar(160)", maxLength: 160, nullable: false),
                    DefaultCulture = table.Column<string>(type: "varchar(2)", maxLength: 2, nullable: false),
                    DefaultTheme = table.Column<string>(type: "varchar(8)", maxLength: 8, nullable: false),
                    PublicBaseUrl = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true),
                    LogoMediaUsageId = table.Column<long>(type: "bigint", nullable: true),
                    FaviconMediaUsageId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SiteSettings", x => x.RevisionId);
                    table.CheckConstraint("CK_SiteSettings_Culture", "`DefaultCulture` IN ('en','ar')");
                    table.CheckConstraint("CK_SiteSettings_Theme", "`DefaultTheme` IN ('light','dark','system')");
                    table.ForeignKey(
                        name: "FK_SiteSettings_ContentRevisions_RevisionId",
                        column: x => x.RevisionId,
                        principalTable: "ContentRevisions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SiteSettings_RevisionMedia_RevisionId_FaviconMediaUsageId",
                        columns: x => new { x.RevisionId, x.FaviconMediaUsageId },
                        principalTable: "RevisionMedia",
                        principalColumns: new[] { "RevisionId", "Id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SiteSettings_RevisionMedia_RevisionId_LogoMediaUsageId",
                        columns: x => new { x.RevisionId, x.LogoMediaUsageId },
                        principalTable: "RevisionMedia",
                        principalColumns: new[] { "RevisionId", "Id" },
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "PageSections",
                columns: table => new
                {
                    RevisionId = table.Column<long>(type: "bigint", nullable: false),
                    SectionRevisionId = table.Column<long>(type: "bigint", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PageSections", x => new { x.RevisionId, x.SectionRevisionId });
                    table.CheckConstraint("CK_PageSections_Order", "`DisplayOrder` >= 0");
                    table.ForeignKey(
                        name: "FK_PageSections_ContentRevisions_RevisionId",
                        column: x => x.RevisionId,
                        principalTable: "ContentRevisions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PageSections_Pages_RevisionId",
                        column: x => x.RevisionId,
                        principalTable: "Pages",
                        principalColumn: "RevisionId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PageSections_Sections_SectionRevisionId",
                        column: x => x.SectionRevisionId,
                        principalTable: "Sections",
                        principalColumn: "RevisionId",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "NavigationTranslations",
                columns: table => new
                {
                    RevisionId = table.Column<long>(type: "bigint", nullable: false),
                    NavigationItemId = table.Column<long>(type: "bigint", nullable: false),
                    Culture = table.Column<string>(type: "varchar(2)", maxLength: 2, nullable: false),
                    Label = table.Column<string>(type: "varchar(120)", maxLength: 120, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NavigationTranslations", x => new { x.RevisionId, x.NavigationItemId, x.Culture });
                    table.CheckConstraint("CK_NavigationTranslations_Culture", "`Culture` IN ('en','ar')");
                    table.ForeignKey(
                        name: "FK_NavigationTranslations_ContentRevisions_RevisionId",
                        column: x => x.RevisionId,
                        principalTable: "ContentRevisions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_NavigationTranslations_NavigationItems_RevisionId_Navigation~",
                        columns: x => new { x.RevisionId, x.NavigationItemId },
                        principalTable: "NavigationItems",
                        principalColumns: new[] { "RevisionId", "Id" },
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ProjectCategoryAssignments",
                columns: table => new
                {
                    RevisionId = table.Column<long>(type: "bigint", nullable: false),
                    CategoryRevisionId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectCategoryAssignments", x => new { x.RevisionId, x.CategoryRevisionId });
                    table.ForeignKey(
                        name: "FK_ProjectCategoryAssignments_ContentRevisions_RevisionId",
                        column: x => x.RevisionId,
                        principalTable: "ContentRevisions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProjectCategoryAssignments_ProjectCategories_CategoryRevisio~",
                        column: x => x.CategoryRevisionId,
                        principalTable: "ProjectCategories",
                        principalColumn: "RevisionId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProjectCategoryAssignments_Projects_RevisionId",
                        column: x => x.RevisionId,
                        principalTable: "Projects",
                        principalColumn: "RevisionId",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ProjectMedia",
                columns: table => new
                {
                    RevisionId = table.Column<long>(type: "bigint", nullable: false),
                    MediaUsageId = table.Column<long>(type: "bigint", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectMedia", x => new { x.RevisionId, x.MediaUsageId });
                    table.ForeignKey(
                        name: "FK_ProjectMedia_ContentRevisions_RevisionId",
                        column: x => x.RevisionId,
                        principalTable: "ContentRevisions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProjectMedia_Projects_RevisionId",
                        column: x => x.RevisionId,
                        principalTable: "Projects",
                        principalColumn: "RevisionId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProjectMedia_RevisionMedia_RevisionId_MediaUsageId",
                        columns: x => new { x.RevisionId, x.MediaUsageId },
                        principalTable: "RevisionMedia",
                        principalColumns: new[] { "RevisionId", "Id" },
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_ActorUserId_OccurredUtc",
                table: "AuditLogs",
                columns: new[] { "ActorUserId", "OccurredUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_ContentItemId_OccurredUtc",
                table: "AuditLogs",
                columns: new[] { "ContentItemId", "OccurredUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_ContentItemId_RevisionId",
                table: "AuditLogs",
                columns: new[] { "ContentItemId", "RevisionId" });

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_OccurredUtc",
                table: "AuditLogs",
                column: "OccurredUtc");

            migrationBuilder.CreateIndex(
                name: "IX_BusinessHours_RevisionId_DayOfWeek",
                table: "BusinessHours",
                columns: new[] { "RevisionId", "DayOfWeek" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ContactDetails_RevisionId_DisplayOrder",
                table: "ContactDetails",
                columns: new[] { "RevisionId", "DisplayOrder" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ContentItems_Id_DraftRevisionId",
                table: "ContentItems",
                columns: new[] { "Id", "DraftRevisionId" });

            migrationBuilder.CreateIndex(
                name: "IX_ContentItems_Id_PublishedRevisionId",
                table: "ContentItems",
                columns: new[] { "Id", "PublishedRevisionId" });

            migrationBuilder.CreateIndex(
                name: "IX_ContentItems_Kind_Key",
                table: "ContentItems",
                columns: new[] { "Kind", "Key" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ContentItems_Kind_State",
                table: "ContentItems",
                columns: new[] { "Kind", "State" });

            migrationBuilder.CreateIndex(
                name: "IX_ContentRevisions_ContentItemId_RevisionNumber",
                table: "ContentRevisions",
                columns: new[] { "ContentItemId", "RevisionNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ContentRevisions_ContentItemId_SupersedesRevisionId",
                table: "ContentRevisions",
                columns: new[] { "ContentItemId", "SupersedesRevisionId" });

            migrationBuilder.CreateIndex(
                name: "IX_ContentRevisions_CreatedByUserId",
                table: "ContentRevisions",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_MediaAssets_Sha256",
                table: "MediaAssets",
                column: "Sha256");

            migrationBuilder.CreateIndex(
                name: "IX_MediaAssets_StorageProvider_StorageKey",
                table: "MediaAssets",
                columns: new[] { "StorageProvider", "StorageKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_NavigationItems_RevisionId_DisplayOrder",
                table: "NavigationItems",
                columns: new[] { "RevisionId", "DisplayOrder" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_NavigationItems_RevisionId_ParentItemId",
                table: "NavigationItems",
                columns: new[] { "RevisionId", "ParentItemId" });

            migrationBuilder.CreateIndex(
                name: "IX_NavigationItems_TargetContentItemId",
                table: "NavigationItems",
                column: "TargetContentItemId");

            migrationBuilder.CreateIndex(
                name: "IX_PageSections_RevisionId_DisplayOrder",
                table: "PageSections",
                columns: new[] { "RevisionId", "DisplayOrder" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PageSections_SectionRevisionId",
                table: "PageSections",
                column: "SectionRevisionId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectCategoryAssignments_CategoryRevisionId",
                table: "ProjectCategoryAssignments",
                column: "CategoryRevisionId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectMedia_RevisionId_DisplayOrder",
                table: "ProjectMedia",
                columns: new[] { "RevisionId", "DisplayOrder" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Projects_IsFeatured_DisplayOrder",
                table: "Projects",
                columns: new[] { "IsFeatured", "DisplayOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_Projects_RevisionId_CoverMediaUsageId",
                table: "Projects",
                columns: new[] { "RevisionId", "CoverMediaUsageId" });

            migrationBuilder.CreateIndex(
                name: "IX_PublicationSchedules_ContentItemId_RevisionId",
                table: "PublicationSchedules",
                columns: new[] { "ContentItemId", "RevisionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PublicationSchedules_State_ScheduledUtc",
                table: "PublicationSchedules",
                columns: new[] { "State", "ScheduledUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_PublishedRoutes_ContentItemId_Culture",
                table: "PublishedRoutes",
                columns: new[] { "ContentItemId", "Culture" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PublishedRoutes_ContentItemId_RevisionId",
                table: "PublishedRoutes",
                columns: new[] { "ContentItemId", "RevisionId" });

            migrationBuilder.CreateIndex(
                name: "IX_PublishedRoutes_Culture_Scope_Slug",
                table: "PublishedRoutes",
                columns: new[] { "Culture", "Scope", "Slug" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RevisionMedia_MediaAssetId",
                table: "RevisionMedia",
                column: "MediaAssetId");

            migrationBuilder.CreateIndex(
                name: "IX_RevisionMedia_RevisionId_UsageKey",
                table: "RevisionMedia",
                columns: new[] { "RevisionId", "UsageKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SeoMetadata_RevisionId_OpenGraphMediaUsageId",
                table: "SeoMetadata",
                columns: new[] { "RevisionId", "OpenGraphMediaUsageId" });

            migrationBuilder.CreateIndex(
                name: "IX_Services_DisplayOrder",
                table: "Services",
                column: "DisplayOrder");

            migrationBuilder.CreateIndex(
                name: "IX_Services_RevisionId_MediaUsageId",
                table: "Services",
                columns: new[] { "RevisionId", "MediaUsageId" });

            migrationBuilder.CreateIndex(
                name: "IX_SiteSettings_RevisionId_FaviconMediaUsageId",
                table: "SiteSettings",
                columns: new[] { "RevisionId", "FaviconMediaUsageId" });

            migrationBuilder.CreateIndex(
                name: "IX_SiteSettings_RevisionId_LogoMediaUsageId",
                table: "SiteSettings",
                columns: new[] { "RevisionId", "LogoMediaUsageId" });

            migrationBuilder.CreateIndex(
                name: "IX_SocialLinks_RevisionId_Platform",
                table: "SocialLinks",
                columns: new[] { "RevisionId", "Platform" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_AuditLogs_ContentItems_ContentItemId",
                table: "AuditLogs",
                column: "ContentItemId",
                principalTable: "ContentItems",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AuditLogs_ContentRevisions_ContentItemId_RevisionId",
                table: "AuditLogs",
                columns: new[] { "ContentItemId", "RevisionId" },
                principalTable: "ContentRevisions",
                principalColumns: new[] { "ContentItemId", "Id" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_BusinessHours_ContactSettings_RevisionId",
                table: "BusinessHours",
                column: "RevisionId",
                principalTable: "ContactSettings",
                principalColumn: "RevisionId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_BusinessHours_ContentRevisions_RevisionId",
                table: "BusinessHours",
                column: "RevisionId",
                principalTable: "ContentRevisions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ContactDetails_ContactSettings_RevisionId",
                table: "ContactDetails",
                column: "RevisionId",
                principalTable: "ContactSettings",
                principalColumn: "RevisionId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ContactDetails_ContentRevisions_RevisionId",
                table: "ContactDetails",
                column: "RevisionId",
                principalTable: "ContentRevisions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ContactSettings_ContentRevisions_RevisionId",
                table: "ContactSettings",
                column: "RevisionId",
                principalTable: "ContentRevisions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ContactTranslations_ContentRevisions_RevisionId",
                table: "ContactTranslations",
                column: "RevisionId",
                principalTable: "ContentRevisions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ContentItems_ContentRevisions_Id_DraftRevisionId",
                table: "ContentItems",
                columns: new[] { "Id", "DraftRevisionId" },
                principalTable: "ContentRevisions",
                principalColumns: new[] { "ContentItemId", "Id" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ContentItems_ContentRevisions_Id_PublishedRevisionId",
                table: "ContentItems",
                columns: new[] { "Id", "PublishedRevisionId" },
                principalTable: "ContentRevisions",
                principalColumns: new[] { "ContentItemId", "Id" },
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ContentRevisions_AspNetUsers_CreatedByUserId",
                table: "ContentRevisions");

            migrationBuilder.DropForeignKey(
                name: "FK_ContentRevisions_ContentItems_ContentItemId",
                table: "ContentRevisions");

            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "AuditLogs");

            migrationBuilder.DropTable(
                name: "BusinessHours");

            migrationBuilder.DropTable(
                name: "ContactTranslations");

            migrationBuilder.DropTable(
                name: "FooterSettings");

            migrationBuilder.DropTable(
                name: "MediaAssetTranslations");

            migrationBuilder.DropTable(
                name: "NavigationTranslations");

            migrationBuilder.DropTable(
                name: "PageSections");

            migrationBuilder.DropTable(
                name: "ProjectCategoryAssignments");

            migrationBuilder.DropTable(
                name: "ProjectMedia");

            migrationBuilder.DropTable(
                name: "PublicationSchedules");

            migrationBuilder.DropTable(
                name: "PublishedRoutes");

            migrationBuilder.DropTable(
                name: "RevisionMediaTranslations");

            migrationBuilder.DropTable(
                name: "SeoMetadata");

            migrationBuilder.DropTable(
                name: "Services");

            migrationBuilder.DropTable(
                name: "SiteSettings");

            migrationBuilder.DropTable(
                name: "SocialLinks");

            migrationBuilder.DropTable(
                name: "ThemeSettings");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "ContactDetails");

            migrationBuilder.DropTable(
                name: "NavigationItems");

            migrationBuilder.DropTable(
                name: "Pages");

            migrationBuilder.DropTable(
                name: "Sections");

            migrationBuilder.DropTable(
                name: "ProjectCategories");

            migrationBuilder.DropTable(
                name: "Projects");

            migrationBuilder.DropTable(
                name: "ContentTranslations");

            migrationBuilder.DropTable(
                name: "ContactSettings");

            migrationBuilder.DropTable(
                name: "NavigationSettings");

            migrationBuilder.DropTable(
                name: "RevisionMedia");

            migrationBuilder.DropTable(
                name: "MediaAssets");

            migrationBuilder.DropTable(
                name: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "ContentItems");

            migrationBuilder.DropTable(
                name: "ContentRevisions");
        }
    }
}
