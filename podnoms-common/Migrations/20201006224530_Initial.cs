using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace PodNoms.Common.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "admin");

            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FacebookId = table.Column<long>(type: "bigint", nullable: true),
                    TwitterHandle = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PictureUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DiskQuota = table.Column<long>(type: "bigint", nullable: true),
                    Slug = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    LastSeen = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "getdate()"),
                    IpAddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CountryCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CountryName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RegionCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RegionName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    City = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Zip = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Latitude = table.Column<double>(type: "float", nullable: true),
                    Longitude = table.Column<double>(type: "float", nullable: true),
                    EmailNotificationOptions = table.Column<int>(type: "int", nullable: false),
                    PlaylistAllowedEntryCount = table.Column<int>(type: "int", nullable: true),
                    UserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SecurityStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "bit", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BoilerPlates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Key = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "getdate()"),
                    UpdateDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "getdate()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BoilerPlates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "getdate()"),
                    UpdateDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "getdate()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ServerConfig",
                schema: "admin",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Key = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "getdate()"),
                    UpdateDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "getdate()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServerConfig", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SiteMessages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Url = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Type = table.Column<int>(type: "int", nullable: false),
                    PodcastCountThreshold = table.Column<int>(type: "int", nullable: true),
                    EntryCountThreshold = table.Column<int>(type: "int", nullable: true),
                    DiskSpaceThreshold = table.Column<long>(type: "bigint", nullable: true),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "getdate()"),
                    UpdateDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "getdate()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SiteMessages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
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
                });

            migrationBuilder.CreateTable(
                name: "AccountSubscriptions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AppUserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    WasSuccessful = table.Column<bool>(type: "bit", nullable: false),
                    Amount = table.Column<long>(type: "bigint", nullable: false),
                    TransactionId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Tier = table.Column<int>(type: "int", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "getdate()"),
                    UpdateDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "getdate()"),
                    ReceiptURL = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccountSubscriptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AccountSubscriptions_AspNetUsers_AppUserId",
                        column: x => x.AppUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ApplicationUserSlugRedirects",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ApplicationUserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    OldSlug = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "getdate()"),
                    UpdateDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "getdate()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplicationUserSlugRedirects", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ApplicationUserSlugRedirects_AspNetUsers_ApplicationUserId",
                        column: x => x.ApplicationUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
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
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderKey = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
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
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false)
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
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
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
                });

            migrationBuilder.CreateTable(
                name: "ChatMessages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FromUserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    ToUserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MessageSeen = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "getdate()"),
                    UpdateDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "getdate()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatMessages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChatMessages_AspNetUsers_FromUserId",
                        column: x => x.FromUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ChatMessages_AspNetUsers_ToUserId",
                        column: x => x.ToUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Donations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AppUserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Amount = table.Column<long>(type: "bigint", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "getdate()"),
                    UpdateDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "getdate()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Donations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Donations_AspNetUsers_AppUserId",
                        column: x => x.AppUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "IssuedApiKeys",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(25)", maxLength: 25, nullable: false),
                    Prefix = table.Column<string>(type: "nvarchar(7)", maxLength: 7, nullable: true),
                    Key = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Scopes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsValid = table.Column<bool>(type: "bit", nullable: false),
                    Expires = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IssuedToId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "getdate()"),
                    UpdateDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "getdate()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IssuedApiKeys", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IssuedApiKeys_AspNetUsers_IssuedToId",
                        column: x => x.IssuedToId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PatreonTokens",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AccessToken = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ExpiresIn = table.Column<long>(type: "bigint", nullable: false),
                    TokenType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FullName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RefreshToken = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Version = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AppUserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "getdate()"),
                    UpdateDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "getdate()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PatreonTokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PatreonTokens_AspNetUsers_AppUserId",
                        column: x => x.AppUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RefreshTokens",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Token = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Expires = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AppUserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    RemoteIpAddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "getdate()"),
                    UpdateDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "getdate()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RefreshTokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RefreshTokens_AspNetUsers_AppUserId",
                        column: x => x.AppUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserRequests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FromUserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    RequestText = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RequestType = table.Column<int>(type: "int", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "getdate()"),
                    UpdateDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "getdate()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserRequests_AspNetUsers_FromUserId",
                        column: x => x.FromUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Podcasts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AppUserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Slug = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    CustomDomain = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CustomRssDomain = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CategoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PublicTitle = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FacebookUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TwitterUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GoogleAnalyticsTrackingId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Private = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    AuthUserName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AuthPassword = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    AuthPasswordSalt = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "getdate()"),
                    UpdateDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "getdate()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Podcasts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Podcasts_AspNetUsers_AppUserId",
                        column: x => x.AppUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Podcasts_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Config = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PodcastId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "getdate()"),
                    UpdateDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "getdate()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Notifications_Podcasts_PodcastId",
                        column: x => x.PodcastId,
                        principalTable: "Podcasts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Playlists",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SourceUrl = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    PodcastId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "getdate()"),
                    UpdateDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "getdate()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Playlists", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Playlists_Podcasts_PodcastId",
                        column: x => x.PodcastId,
                        principalTable: "Podcasts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PodcastAggregators",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Url = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ImageUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PodcastId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "getdate()"),
                    UpdateDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "getdate()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PodcastAggregators", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PodcastAggregators_Podcasts_PodcastId",
                        column: x => x.PodcastId,
                        principalTable: "Podcasts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Subcategories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CategoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PodcastId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "getdate()"),
                    UpdateDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "getdate()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Subcategories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Subcategories_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Subcategories_Podcasts_PodcastId",
                        column: x => x.PodcastId,
                        principalTable: "Podcasts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "NotificationLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NotificationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Log = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Succeeded = table.Column<bool>(type: "bit", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "getdate()"),
                    UpdateDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "getdate()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificationLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NotificationLogs_Notifications_NotificationId",
                        column: x => x.NotificationId,
                        principalTable: "Notifications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PodcastEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Slug = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Author = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SourceUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AudioUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AudioLength = table.Column<float>(type: "real", nullable: false),
                    AudioFileSize = table.Column<long>(type: "bigint", nullable: false),
                    ImageUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ProcessingPayload = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ProcessingStatus = table.Column<int>(type: "int", nullable: false),
                    SourceCreateDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SourceItemId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MetadataStatus = table.Column<int>(type: "int", nullable: false),
                    ShareOptions = table.Column<int>(type: "int", nullable: false),
                    Processed = table.Column<bool>(type: "bit", nullable: false),
                    WaveformGenerated = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    PodcastId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PlaylistId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "getdate()"),
                    UpdateDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "getdate()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PodcastEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PodcastEntries_Playlists_PlaylistId",
                        column: x => x.PlaylistId,
                        principalTable: "Playlists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PodcastEntries_Podcasts_PodcastId",
                        column: x => x.PodcastId,
                        principalTable: "Podcasts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ActivityLogPodcastEntry",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PodcastEntryId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Referrer = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserAgent = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClientAddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ExtraInfo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CountryCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CountryName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RegionCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RegionName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    City = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Zip = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Latitude = table.Column<double>(type: "float", nullable: true),
                    Longitude = table.Column<double>(type: "float", nullable: true),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "getdate()"),
                    UpdateDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "getdate()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActivityLogPodcastEntry", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ActivityLogPodcastEntry_PodcastEntries_PodcastEntryId",
                        column: x => x.PodcastEntryId,
                        principalTable: "PodcastEntries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EntryComments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FromUser = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FromUserEmail = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Timestamp = table.Column<int>(type: "int", nullable: true),
                    PodcastEntryId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "getdate()"),
                    UpdateDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "getdate()"),
                    CommentText = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsSpam = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EntryComments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EntryComments_PodcastEntries_PodcastEntryId",
                        column: x => x.PodcastEntryId,
                        principalTable: "PodcastEntries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PodcastEntrySharingLinks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LinkIndex = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LinkId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    PodcastEntryId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ValidFrom = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ValidTo = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "getdate()"),
                    UpdateDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "getdate()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PodcastEntrySharingLinks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PodcastEntrySharingLinks_PodcastEntries_PodcastEntryId",
                        column: x => x.PodcastEntryId,
                        principalTable: "PodcastEntries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "94e79a7b-2ec7-46e2-b890-fcf0e5eaaf80", "35eb0496-26cf-4ce9-b106-03c16d4566fb", "client-admin", "CLIENT-ADMIN" },
                    { "f1c6e6a8-2461-48f2-ab7a-569a3b75b280", "75cd9bcf-91ef-4819-b66c-0c22918ba6e8", "website-admin", "WEBSITE-ADMIN" },
                    { "dba18578-271a-40de-8cb3-e21f97fcf159", "1d5a0bcd-6aff-4158-801d-d455b4ce6c25", "catastrophic-api-calls-allowed", "CATASTROPHIC-API-CALLS-ALLOWED" },
                    { "9517774b-a900-49ee-9ddf-c31b36938352", "ffbb9e22-e3a1-4631-ab85-239f26267de7", "god-mode", "GOD-MODE" }
                });

            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "Description" },
                values: new object[,]
                {
                    { new Guid("ad31686b-794b-4ebb-99a8-cdc812ca7e83"), "Education" },
                    { new Guid("b13cddb1-feff-42e1-9c80-cb8ad4a5f374"), "Games & Hobbies" },
                    { new Guid("5e023a7a-461d-46c6-bca8-c9049f6d2ec5"), "News & Politics" },
                    { new Guid("c4911d87-2b6e-42ea-b771-be910cb01624"), "Health" },
                    { new Guid("27fb7005-b75c-490b-ae13-bcc88525be65"), "Music" },
                    { new Guid("f177d65b-5eca-4137-b202-af672cd11d70"), "Society & Culture" },
                    { new Guid("97735523-d87a-4b5f-9dd1-ab8289af2ae6"), "Sports & Recreation" },
                    { new Guid("3219621b-8311-4b65-bb48-6f68fba4957c"), "Kids & Family" },
                    { new Guid("41b9ee87-a9ca-4305-8ed8-ee69a3dbcfc3"), "Arts" },
                    { new Guid("67d057a7-21b4-4462-a284-66ba62a6de1b"), "Religion & Spirituality" },
                    { new Guid("f0603194-6f45-4695-98c1-6288cffbfd94"), "Business" },
                    { new Guid("50495352-2339-4498-aad3-3f8c85f6ac69"), "Science & Medicine" },
                    { new Guid("db829bfe-a8fe-458e-9b67-2b00f4794750"), "Technology" },
                    { new Guid("29c0716a-94bc-4b79-bb7a-1acb2d872101"), "Comedy" },
                    { new Guid("a6aa8e20-8729-4698-a254-976012afdbf3"), "TV & Film" },
                    { new Guid("2e23f263-062a-43c3-9e27-fb7555fb8e76"), "Government & Organizations" }
                });

            migrationBuilder.InsertData(
                table: "Subcategories",
                columns: new[] { "Id", "CategoryId", "Description", "PodcastId" },
                values: new object[,]
                {
                    { new Guid("149f1619-eb42-4d9b-a4d6-da68ec0541d9"), new Guid("db829bfe-a8fe-458e-9b67-2b00f4794750"), "Podcasting", null },
                    { new Guid("4966ec0c-3f53-4710-9f21-afb2371ab3e2"), new Guid("c4911d87-2b6e-42ea-b771-be910cb01624"), "Sexuality", null },
                    { new Guid("cbcf3c5f-f7c9-44b3-9b2d-edc6523d8c3c"), new Guid("c4911d87-2b6e-42ea-b771-be910cb01624"), "Self-Help", null },
                    { new Guid("1f4b0b76-d7b8-404c-af32-3428fc488e30"), new Guid("b13cddb1-feff-42e1-9c80-cb8ad4a5f374"), "Video Games", null },
                    { new Guid("4f006295-e8ea-4a87-92e0-69e32f8d3ad9"), new Guid("b13cddb1-feff-42e1-9c80-cb8ad4a5f374"), "Aviation", null },
                    { new Guid("7174aa7c-8df2-4f9f-84aa-9bc1e9793eda"), new Guid("b13cddb1-feff-42e1-9c80-cb8ad4a5f374"), "Other Games", null },
                    { new Guid("23eb4b47-fc1d-45ac-9a21-e4ea5313f3d8"), new Guid("b13cddb1-feff-42e1-9c80-cb8ad4a5f374"), "Hobbies", null },
                    { new Guid("df0063a6-31ef-4f9a-be23-4f9178291bb3"), new Guid("ad31686b-794b-4ebb-99a8-cdc812ca7e83"), "Higher Education", null },
                    { new Guid("867ae49c-ade1-41b9-ba18-3f4801ec18ee"), new Guid("c4911d87-2b6e-42ea-b771-be910cb01624"), "Fitness & Nutrition", null },
                    { new Guid("c8829517-5a62-4bcc-82cc-ab4a75c6312c"), new Guid("ad31686b-794b-4ebb-99a8-cdc812ca7e83"), "K-12", null },
                    { new Guid("c546f84f-643a-4f44-9bdf-ce4b44926e0a"), new Guid("ad31686b-794b-4ebb-99a8-cdc812ca7e83"), "Training", null },
                    { new Guid("8114a581-24bb-451e-ba21-dcacd175bde4"), new Guid("ad31686b-794b-4ebb-99a8-cdc812ca7e83"), "Language Courses", null },
                    { new Guid("2488b878-124a-42f9-a761-53daa1eefde1"), new Guid("41b9ee87-a9ca-4305-8ed8-ee69a3dbcfc3"), "Literature", null },
                    { new Guid("3e3f77a4-4cd3-4644-ace1-7923e84d403d"), new Guid("41b9ee87-a9ca-4305-8ed8-ee69a3dbcfc3"), "Performing Arts", null },
                    { new Guid("7be75c87-d18a-45ee-92c3-ab6afce3e5db"), new Guid("41b9ee87-a9ca-4305-8ed8-ee69a3dbcfc3"), "Food", null },
                    { new Guid("7e371ab3-cc53-4f16-9de9-c2d10faa8938"), new Guid("41b9ee87-a9ca-4305-8ed8-ee69a3dbcfc3"), "Visual Arts", null },
                    { new Guid("35cf95e2-6506-47ad-bd5d-df3b5e389a8f"), new Guid("41b9ee87-a9ca-4305-8ed8-ee69a3dbcfc3"), "Fashion & Beauty", null },
                    { new Guid("d8c5c265-1fa1-4244-9d76-b181bd936846"), new Guid("ad31686b-794b-4ebb-99a8-cdc812ca7e83"), "Education Technology", null },
                    { new Guid("1d5212bc-b31c-4130-bcea-ae7c9df2985f"), new Guid("f177d65b-5eca-4137-b202-af672cd11d70"), "Philosophy", null },
                    { new Guid("5005a13d-45c9-40ea-b691-96ff5afa0e39"), new Guid("f177d65b-5eca-4137-b202-af672cd11d70"), "Personal Journals", null },
                    { new Guid("f9167edb-5d9c-4e34-ad26-7a2630528682"), new Guid("f177d65b-5eca-4137-b202-af672cd11d70"), "Places & Travel", null },
                    { new Guid("eb082ac8-856b-4bb0-a22f-f4062d4347d2"), new Guid("db829bfe-a8fe-458e-9b67-2b00f4794750"), "Software How-To", null },
                    { new Guid("8b2a590b-bb59-4f30-acb2-72fa01985e4b"), new Guid("50495352-2339-4498-aad3-3f8c85f6ac69"), "Social Sciences", null },
                    { new Guid("201fb42b-e241-4e16-bea5-5df768064402"), new Guid("f0603194-6f45-4695-98c1-6288cffbfd94"), "Investing", null },
                    { new Guid("375fa60d-8d6f-4684-a729-8a061ce2e062"), new Guid("f0603194-6f45-4695-98c1-6288cffbfd94"), "Management & Marketing", null },
                    { new Guid("797408d2-abbc-4d8d-92f4-9b74146d726d"), new Guid("f0603194-6f45-4695-98c1-6288cffbfd94"), "Business News", null },
                    { new Guid("c7465257-d5b6-46f3-8521-e7a8f91e17b8"), new Guid("f0603194-6f45-4695-98c1-6288cffbfd94"), "Shopping", null },
                    { new Guid("fb7c2755-4b09-4d9f-a457-eebb39abac3b"), new Guid("f0603194-6f45-4695-98c1-6288cffbfd94"), "Careers", null },
                    { new Guid("5c40fa5a-72df-475b-bfa8-561955145762"), new Guid("67d057a7-21b4-4462-a284-66ba62a6de1b"), "Other", null },
                    { new Guid("d9d8f925-cf6f-4d8d-9404-990609b312ce"), new Guid("67d057a7-21b4-4462-a284-66ba62a6de1b"), "Buddhism", null },
                    { new Guid("d5e38344-c701-406f-8762-9a793efb98d7"), new Guid("67d057a7-21b4-4462-a284-66ba62a6de1b"), "Christianity", null },
                    { new Guid("bd367623-40c3-48f2-9a59-9b526c643905"), new Guid("67d057a7-21b4-4462-a284-66ba62a6de1b"), "Hinduism", null },
                    { new Guid("37f4f907-7b02-4268-8e38-db8700c1dc87"), new Guid("67d057a7-21b4-4462-a284-66ba62a6de1b"), "Judaism", null },
                    { new Guid("8fb81212-5911-4190-9635-e478e2119c0c"), new Guid("67d057a7-21b4-4462-a284-66ba62a6de1b"), "Spirituality", null },
                    { new Guid("56a5dc3b-3dc5-46d2-9972-f846a4ca9909"), new Guid("67d057a7-21b4-4462-a284-66ba62a6de1b"), "Islam", null },
                    { new Guid("84ed0b38-616d-4926-90c2-3cc7ae2f8e4e"), new Guid("97735523-d87a-4b5f-9dd1-ab8289af2ae6"), "Professional", null },
                    { new Guid("c950eaac-f164-4f3c-8384-459679130aef"), new Guid("97735523-d87a-4b5f-9dd1-ab8289af2ae6"), "Outdoor", null },
                    { new Guid("4ecd4db0-0786-4594-a49b-86ab1362bc3c"), new Guid("97735523-d87a-4b5f-9dd1-ab8289af2ae6"), "Amateur", null },
                    { new Guid("b4284990-c542-48e4-a000-42d27202153b"), new Guid("2e23f263-062a-43c3-9e27-fb7555fb8e76"), "Non-Profit", null },
                    { new Guid("d69e2cc2-588f-49c5-933e-a6f963e79f32"), new Guid("2e23f263-062a-43c3-9e27-fb7555fb8e76"), "Local", null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_AccountSubscriptions_AppUserId",
                table: "AccountSubscriptions",
                column: "AppUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ActivityLogPodcastEntry_PodcastEntryId",
                table: "ActivityLogPodcastEntry",
                column: "PodcastEntryId");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationUserSlugRedirects_ApplicationUserId",
                table: "ApplicationUserSlugRedirects",
                column: "ApplicationUserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true,
                filter: "[NormalizedName] IS NOT NULL");

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
                name: "IX_AspNetUsers_Slug",
                table: "AspNetUsers",
                column: "Slug",
                unique: true,
                filter: "[Slug] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true,
                filter: "[NormalizedUserName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_BoilerPlates_Key",
                table: "BoilerPlates",
                column: "Key",
                unique: true,
                filter: "[Key] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ChatMessages_FromUserId",
                table: "ChatMessages",
                column: "FromUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ChatMessages_ToUserId",
                table: "ChatMessages",
                column: "ToUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Donations_AppUserId",
                table: "Donations",
                column: "AppUserId");

            migrationBuilder.CreateIndex(
                name: "IX_EntryComments_PodcastEntryId",
                table: "EntryComments",
                column: "PodcastEntryId");

            migrationBuilder.CreateIndex(
                name: "IX_IssuedApiKeys_IssuedToId",
                table: "IssuedApiKeys",
                column: "IssuedToId");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationLogs_NotificationId",
                table: "NotificationLogs",
                column: "NotificationId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_PodcastId",
                table: "Notifications",
                column: "PodcastId");

            migrationBuilder.CreateIndex(
                name: "IX_PatreonTokens_AppUserId",
                table: "PatreonTokens",
                column: "AppUserId",
                unique: true,
                filter: "[AppUserId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Playlists_PodcastId",
                table: "Playlists",
                column: "PodcastId");

            migrationBuilder.CreateIndex(
                name: "IX_Playlists_SourceUrl",
                table: "Playlists",
                column: "SourceUrl",
                unique: true,
                filter: "[SourceUrl] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_PodcastAggregators_PodcastId",
                table: "PodcastAggregators",
                column: "PodcastId");

            migrationBuilder.CreateIndex(
                name: "IX_PodcastEntries_PlaylistId",
                table: "PodcastEntries",
                column: "PlaylistId");

            migrationBuilder.CreateIndex(
                name: "IX_PodcastEntries_PodcastId",
                table: "PodcastEntries",
                column: "PodcastId");

            migrationBuilder.CreateIndex(
                name: "IX_PodcastEntrySharingLinks_LinkId",
                table: "PodcastEntrySharingLinks",
                column: "LinkId",
                unique: true,
                filter: "[LinkId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_PodcastEntrySharingLinks_LinkIndex",
                table: "PodcastEntrySharingLinks",
                column: "LinkIndex",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PodcastEntrySharingLinks_PodcastEntryId",
                table: "PodcastEntrySharingLinks",
                column: "PodcastEntryId");

            migrationBuilder.CreateIndex(
                name: "IX_Podcasts_AppUserId",
                table: "Podcasts",
                column: "AppUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Podcasts_CategoryId",
                table: "Podcasts",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Podcasts_Slug",
                table: "Podcasts",
                column: "Slug",
                unique: true,
                filter: "[Slug] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_AppUserId",
                table: "RefreshTokens",
                column: "AppUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Subcategories_CategoryId",
                table: "Subcategories",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Subcategories_PodcastId",
                table: "Subcategories",
                column: "PodcastId");

            migrationBuilder.CreateIndex(
                name: "IX_UserRequests_FromUserId",
                table: "UserRequests",
                column: "FromUserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AccountSubscriptions");

            migrationBuilder.DropTable(
                name: "ActivityLogPodcastEntry");

            migrationBuilder.DropTable(
                name: "ApplicationUserSlugRedirects");

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
                name: "BoilerPlates");

            migrationBuilder.DropTable(
                name: "ChatMessages");

            migrationBuilder.DropTable(
                name: "Donations");

            migrationBuilder.DropTable(
                name: "EntryComments");

            migrationBuilder.DropTable(
                name: "IssuedApiKeys");

            migrationBuilder.DropTable(
                name: "NotificationLogs");

            migrationBuilder.DropTable(
                name: "PatreonTokens");

            migrationBuilder.DropTable(
                name: "PodcastAggregators");

            migrationBuilder.DropTable(
                name: "PodcastEntrySharingLinks");

            migrationBuilder.DropTable(
                name: "RefreshTokens");

            migrationBuilder.DropTable(
                name: "ServerConfig",
                schema: "admin");

            migrationBuilder.DropTable(
                name: "SiteMessages");

            migrationBuilder.DropTable(
                name: "Subcategories");

            migrationBuilder.DropTable(
                name: "UserRequests");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "Notifications");

            migrationBuilder.DropTable(
                name: "PodcastEntries");

            migrationBuilder.DropTable(
                name: "Playlists");

            migrationBuilder.DropTable(
                name: "Podcasts");

            migrationBuilder.DropTable(
                name: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "Categories");
        }
    }
}
