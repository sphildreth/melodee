using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NodaTime;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Melodee.Common.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Artists",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MediaUniqueId = table.Column<long>(type: "bigint", nullable: false),
                    RealName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    Roles = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    AlbumCount = table.Column<int>(type: "integer", nullable: false),
                    SongCount = table.Column<int>(type: "integer", nullable: false),
                    Biography = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    MetaDataStatus = table.Column<int>(type: "integer", nullable: false),
                    IsLocked = table.Column<bool>(type: "boolean", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    ApiKey = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<Instant>(type: "timestamp with time zone", nullable: false),
                    LastUpdatedAt = table.Column<Instant>(type: "timestamp with time zone", nullable: true),
                    Tags = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Notes = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    Description = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    AlternateNames = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    LastPlayedAt = table.Column<Instant>(type: "timestamp with time zone", nullable: true),
                    LastMetaDataUpdatedAt = table.Column<Instant>(type: "timestamp with time zone", nullable: true),
                    PlayedCount = table.Column<int>(type: "integer", nullable: false),
                    ItunesId = table.Column<string>(type: "text", nullable: true),
                    AmgId = table.Column<string>(type: "text", nullable: true),
                    DiscogsId = table.Column<string>(type: "text", nullable: true),
                    MusicBrainzId = table.Column<string>(type: "text", nullable: true),
                    LastFmId = table.Column<string>(type: "text", nullable: true),
                    SpotifyId = table.Column<string>(type: "text", nullable: true),
                    CalculatedRating = table.Column<decimal>(type: "numeric", nullable: false),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    NameNormalized = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    SortName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Artists", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ClientApplications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ClientApplicationStatus = table.Column<int>(type: "integer", nullable: false),
                    IsLocked = table.Column<bool>(type: "boolean", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    ApiKey = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<Instant>(type: "timestamp with time zone", nullable: false),
                    LastUpdatedAt = table.Column<Instant>(type: "timestamp with time zone", nullable: true),
                    Tags = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Notes = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    Description = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClientApplications", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Libraries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Path = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    LastScanAt = table.Column<Instant>(type: "timestamp with time zone", nullable: true),
                    IsLocked = table.Column<bool>(type: "boolean", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    ApiKey = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<Instant>(type: "timestamp with time zone", nullable: false),
                    LastUpdatedAt = table.Column<Instant>(type: "timestamp with time zone", nullable: true),
                    Tags = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Notes = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    Description = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Libraries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RadioStations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    StreamUrl = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    HomePageUrl = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    IsLocked = table.Column<bool>(type: "boolean", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    ApiKey = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<Instant>(type: "timestamp with time zone", nullable: false),
                    LastUpdatedAt = table.Column<Instant>(type: "timestamp with time zone", nullable: true),
                    Tags = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Notes = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    Description = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RadioStations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Settings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Key = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    Comment = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    Category = table.Column<int>(type: "integer", nullable: true),
                    Value = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    IsLocked = table.Column<bool>(type: "boolean", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    ApiKey = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<Instant>(type: "timestamp with time zone", nullable: false),
                    LastUpdatedAt = table.Column<Instant>(type: "timestamp with time zone", nullable: true),
                    Tags = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Notes = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    Description = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Settings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    UserNameNormalized = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    EmailNormalized = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    PublicKey = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    PasswordEncrypted = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    LastLoginAt = table.Column<Instant>(type: "timestamp with time zone", nullable: true),
                    LastActivityAt = table.Column<Instant>(type: "timestamp with time zone", nullable: true),
                    IsAdmin = table.Column<bool>(type: "boolean", nullable: false),
                    HasSettingsRole = table.Column<bool>(type: "boolean", nullable: false),
                    HasDownloadRole = table.Column<bool>(type: "boolean", nullable: false),
                    HasUploadRole = table.Column<bool>(type: "boolean", nullable: false),
                    HasPlaylistRole = table.Column<bool>(type: "boolean", nullable: false),
                    HasCoverArtRole = table.Column<bool>(type: "boolean", nullable: false),
                    HasCommentRole = table.Column<bool>(type: "boolean", nullable: false),
                    HasPodcastRole = table.Column<bool>(type: "boolean", nullable: false),
                    HasStreamRole = table.Column<bool>(type: "boolean", nullable: false),
                    HasJukeboxRole = table.Column<bool>(type: "boolean", nullable: false),
                    HasShareRole = table.Column<bool>(type: "boolean", nullable: false),
                    IsScrobblingEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    IsLocked = table.Column<bool>(type: "boolean", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    ApiKey = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<Instant>(type: "timestamp with time zone", nullable: false),
                    LastUpdatedAt = table.Column<Instant>(type: "timestamp with time zone", nullable: true),
                    Tags = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Notes = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    Description = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Albums",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ArtistId = table.Column<int>(type: "integer", nullable: false),
                    MediaUniqueId = table.Column<long>(type: "bigint", nullable: false),
                    AlbumStatus = table.Column<short>(type: "smallint", nullable: false),
                    MetaDataStatus = table.Column<int>(type: "integer", nullable: false),
                    AlbumType = table.Column<short>(type: "smallint", nullable: false),
                    OriginalReleaseDate = table.Column<LocalDate>(type: "date", nullable: true),
                    ReleaseDate = table.Column<LocalDate>(type: "date", nullable: false),
                    IsCompilation = table.Column<bool>(type: "boolean", nullable: false),
                    SongCount = table.Column<short>(type: "smallint", nullable: true),
                    DiscCount = table.Column<short>(type: "smallint", nullable: true),
                    Duration = table.Column<double>(type: "double precision", nullable: false),
                    Genres = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Directory = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    IsLocked = table.Column<bool>(type: "boolean", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    ApiKey = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<Instant>(type: "timestamp with time zone", nullable: false),
                    LastUpdatedAt = table.Column<Instant>(type: "timestamp with time zone", nullable: true),
                    Tags = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Notes = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    Description = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    AlternateNames = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    LastPlayedAt = table.Column<Instant>(type: "timestamp with time zone", nullable: true),
                    LastMetaDataUpdatedAt = table.Column<Instant>(type: "timestamp with time zone", nullable: true),
                    PlayedCount = table.Column<int>(type: "integer", nullable: false),
                    ItunesId = table.Column<string>(type: "text", nullable: true),
                    AmgId = table.Column<string>(type: "text", nullable: true),
                    DiscogsId = table.Column<string>(type: "text", nullable: true),
                    MusicBrainzId = table.Column<string>(type: "text", nullable: true),
                    LastFmId = table.Column<string>(type: "text", nullable: true),
                    SpotifyId = table.Column<string>(type: "text", nullable: true),
                    CalculatedRating = table.Column<decimal>(type: "numeric", nullable: false),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    NameNormalized = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    SortName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Albums", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Albums_Artists_ArtistId",
                        column: x => x.ArtistId,
                        principalTable: "Artists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LibraryScanHistories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CreatedAt = table.Column<Instant>(type: "timestamp with time zone", nullable: false),
                    LibraryId = table.Column<int>(type: "integer", nullable: false),
                    ForArtistId = table.Column<int>(type: "integer", nullable: true),
                    ForAlbumId = table.Column<int>(type: "integer", nullable: true),
                    FoundArtistsCount = table.Column<int>(type: "integer", nullable: false),
                    FoundAlbumsCount = table.Column<int>(type: "integer", nullable: false),
                    FoundSongsCount = table.Column<int>(type: "integer", nullable: false),
                    DurationInMs = table.Column<double>(type: "double precision", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LibraryScanHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LibraryScanHistories_Libraries_LibraryId",
                        column: x => x.LibraryId,
                        principalTable: "Libraries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Players",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    UserAgent = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    Client = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    IpAddress = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    LastSeenAt = table.Column<Instant>(type: "timestamp with time zone", nullable: false),
                    MaxBitRate = table.Column<int>(type: "integer", nullable: true),
                    ScrobbleEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    IsLocked = table.Column<bool>(type: "boolean", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    ApiKey = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<Instant>(type: "timestamp with time zone", nullable: false),
                    LastUpdatedAt = table.Column<Instant>(type: "timestamp with time zone", nullable: true),
                    Tags = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Notes = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    Description = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Players", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Players_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Shares",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    SongIds = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    ExpiresAt = table.Column<Instant>(type: "timestamp with time zone", nullable: false),
                    IsDownloadable = table.Column<bool>(type: "boolean", nullable: false),
                    LastVisitedAt = table.Column<Instant>(type: "timestamp with time zone", nullable: true),
                    VisitCount = table.Column<int>(type: "integer", nullable: false),
                    IsLocked = table.Column<bool>(type: "boolean", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    ApiKey = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<Instant>(type: "timestamp with time zone", nullable: false),
                    LastUpdatedAt = table.Column<Instant>(type: "timestamp with time zone", nullable: true),
                    Tags = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Notes = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    Description = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Shares", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Shares_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserArtists",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    ArtistId = table.Column<int>(type: "integer", nullable: false),
                    IsStarred = table.Column<bool>(type: "boolean", nullable: false),
                    Rating = table.Column<int>(type: "integer", nullable: false),
                    IsLocked = table.Column<bool>(type: "boolean", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    ApiKey = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<Instant>(type: "timestamp with time zone", nullable: false),
                    LastUpdatedAt = table.Column<Instant>(type: "timestamp with time zone", nullable: true),
                    Tags = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Notes = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    Description = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserArtists", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserArtists_Artists_ArtistId",
                        column: x => x.ArtistId,
                        principalTable: "Artists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserArtists_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AlbumDiscs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AlbumId = table.Column<int>(type: "integer", nullable: false),
                    DiscNumber = table.Column<short>(type: "smallint", nullable: false),
                    SongCount = table.Column<short>(type: "smallint", nullable: true),
                    Title = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AlbumDiscs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AlbumDiscs_Albums_AlbumId",
                        column: x => x.AlbumId,
                        principalTable: "Albums",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserAlbums",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    AlbumId = table.Column<int>(type: "integer", nullable: false),
                    PlayedCount = table.Column<int>(type: "integer", nullable: false),
                    LastPlayedAt = table.Column<Instant>(type: "timestamp with time zone", nullable: false),
                    IsStarred = table.Column<bool>(type: "boolean", nullable: false),
                    Rating = table.Column<int>(type: "integer", nullable: false),
                    IsLocked = table.Column<bool>(type: "boolean", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    ApiKey = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<Instant>(type: "timestamp with time zone", nullable: false),
                    LastUpdatedAt = table.Column<Instant>(type: "timestamp with time zone", nullable: true),
                    Tags = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Notes = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    Description = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserAlbums", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserAlbums_Albums_AlbumId",
                        column: x => x.AlbumId,
                        principalTable: "Albums",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserAlbums_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Songs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AlbumDiscId = table.Column<int>(type: "integer", nullable: false),
                    LibraryId = table.Column<int>(type: "integer", nullable: false),
                    MediaUniqueId = table.Column<long>(type: "bigint", nullable: false),
                    Title = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    TitleSort = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    TitleNormalized = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    SongNumber = table.Column<int>(type: "integer", nullable: false),
                    FileName = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    Lyrics = table.Column<string>(type: "character varying(62000)", maxLength: 62000, nullable: true),
                    FileSize = table.Column<long>(type: "bigint", nullable: false),
                    FileHash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    PartTitles = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Duration = table.Column<double>(type: "double precision", nullable: false),
                    SamplingRate = table.Column<int>(type: "integer", nullable: false),
                    BitRate = table.Column<int>(type: "integer", nullable: false),
                    BitDepth = table.Column<int>(type: "integer", nullable: false),
                    BPM = table.Column<int>(type: "integer", nullable: false),
                    ChannelCount = table.Column<int>(type: "integer", nullable: true),
                    IsLocked = table.Column<bool>(type: "boolean", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    ApiKey = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<Instant>(type: "timestamp with time zone", nullable: false),
                    LastUpdatedAt = table.Column<Instant>(type: "timestamp with time zone", nullable: true),
                    Tags = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Notes = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    Description = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    AlternateNames = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    LastPlayedAt = table.Column<Instant>(type: "timestamp with time zone", nullable: true),
                    LastMetaDataUpdatedAt = table.Column<Instant>(type: "timestamp with time zone", nullable: true),
                    PlayedCount = table.Column<int>(type: "integer", nullable: false),
                    ItunesId = table.Column<string>(type: "text", nullable: true),
                    AmgId = table.Column<string>(type: "text", nullable: true),
                    DiscogsId = table.Column<string>(type: "text", nullable: true),
                    MusicBrainzId = table.Column<string>(type: "text", nullable: true),
                    LastFmId = table.Column<string>(type: "text", nullable: true),
                    SpotifyId = table.Column<string>(type: "text", nullable: true),
                    CalculatedRating = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Songs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Songs_AlbumDiscs_AlbumDiscId",
                        column: x => x.AlbumDiscId,
                        principalTable: "AlbumDiscs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Songs_Libraries_LibraryId",
                        column: x => x.LibraryId,
                        principalTable: "Libraries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Bookmarks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    SongId = table.Column<int>(type: "integer", nullable: false),
                    Comment = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Position = table.Column<int>(type: "integer", nullable: false),
                    IsLocked = table.Column<bool>(type: "boolean", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    ApiKey = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<Instant>(type: "timestamp with time zone", nullable: false),
                    LastUpdatedAt = table.Column<Instant>(type: "timestamp with time zone", nullable: true),
                    Tags = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Notes = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    Description = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    AlternateNames = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    LastPlayedAt = table.Column<Instant>(type: "timestamp with time zone", nullable: true),
                    LastMetaDataUpdatedAt = table.Column<Instant>(type: "timestamp with time zone", nullable: true),
                    PlayedCount = table.Column<int>(type: "integer", nullable: false),
                    ItunesId = table.Column<string>(type: "text", nullable: true),
                    AmgId = table.Column<string>(type: "text", nullable: true),
                    DiscogsId = table.Column<string>(type: "text", nullable: true),
                    MusicBrainzId = table.Column<string>(type: "text", nullable: true),
                    LastFmId = table.Column<string>(type: "text", nullable: true),
                    SpotifyId = table.Column<string>(type: "text", nullable: true),
                    CalculatedRating = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bookmarks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Bookmarks_Songs_SongId",
                        column: x => x.SongId,
                        principalTable: "Songs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Bookmarks_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Contributors",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Role = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    SubRole = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    ArtistId = table.Column<int>(type: "integer", nullable: false),
                    SongId = table.Column<int>(type: "integer", nullable: true),
                    AlbumId = table.Column<int>(type: "integer", nullable: false),
                    IsLocked = table.Column<bool>(type: "boolean", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    ApiKey = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<Instant>(type: "timestamp with time zone", nullable: false),
                    LastUpdatedAt = table.Column<Instant>(type: "timestamp with time zone", nullable: true),
                    Tags = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Notes = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    Description = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Contributors", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Contributors_Albums_AlbumId",
                        column: x => x.AlbumId,
                        principalTable: "Albums",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Contributors_Artists_ArtistId",
                        column: x => x.ArtistId,
                        principalTable: "Artists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Contributors_Songs_SongId",
                        column: x => x.SongId,
                        principalTable: "Songs",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Playlists",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Comment = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    IsPublic = table.Column<bool>(type: "boolean", nullable: false),
                    SongCount = table.Column<short>(type: "smallint", nullable: true),
                    Duration = table.Column<double>(type: "double precision", nullable: false),
                    AllowedUserIds = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    SongId = table.Column<int>(type: "integer", nullable: true),
                    IsLocked = table.Column<bool>(type: "boolean", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    ApiKey = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<Instant>(type: "timestamp with time zone", nullable: false),
                    LastUpdatedAt = table.Column<Instant>(type: "timestamp with time zone", nullable: true),
                    Tags = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Notes = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    Description = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Playlists", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Playlists_Songs_SongId",
                        column: x => x.SongId,
                        principalTable: "Songs",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Playlists_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PlayQues",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    SongId = table.Column<int>(type: "integer", nullable: false),
                    SongApiKey = table.Column<Guid>(type: "uuid", nullable: false),
                    Position = table.Column<decimal>(type: "numeric", nullable: false),
                    IsLocked = table.Column<bool>(type: "boolean", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    ApiKey = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<Instant>(type: "timestamp with time zone", nullable: false),
                    LastUpdatedAt = table.Column<Instant>(type: "timestamp with time zone", nullable: true),
                    Tags = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Notes = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    Description = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayQues", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlayQues_Songs_SongId",
                        column: x => x.SongId,
                        principalTable: "Songs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PlayQues_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Scrobbles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    ServiceUrl = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    SongId = table.Column<int>(type: "integer", nullable: false),
                    PlayTimeInMs = table.Column<long>(type: "bigint", nullable: false),
                    EnqueueTime = table.Column<Instant>(type: "timestamp with time zone", nullable: false),
                    IsLocked = table.Column<bool>(type: "boolean", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    ApiKey = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<Instant>(type: "timestamp with time zone", nullable: false),
                    LastUpdatedAt = table.Column<Instant>(type: "timestamp with time zone", nullable: true),
                    Tags = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Notes = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    Description = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Scrobbles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Scrobbles_Songs_SongId",
                        column: x => x.SongId,
                        principalTable: "Songs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Scrobbles_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserSongs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    SongId = table.Column<int>(type: "integer", nullable: false),
                    PlayedCount = table.Column<int>(type: "integer", nullable: false),
                    LastPlayedAt = table.Column<Instant>(type: "timestamp with time zone", nullable: false),
                    IsStarred = table.Column<bool>(type: "boolean", nullable: false),
                    Rating = table.Column<int>(type: "integer", nullable: false),
                    IsLocked = table.Column<bool>(type: "boolean", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    ApiKey = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<Instant>(type: "timestamp with time zone", nullable: false),
                    LastUpdatedAt = table.Column<Instant>(type: "timestamp with time zone", nullable: true),
                    Tags = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Notes = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    Description = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserSongs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserSongs_Songs_SongId",
                        column: x => x.SongId,
                        principalTable: "Songs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserSongs_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PlaylistSong",
                columns: table => new
                {
                    SongId = table.Column<int>(type: "integer", nullable: false),
                    PlaylistId = table.Column<int>(type: "integer", nullable: false),
                    SongApiKey = table.Column<Guid>(type: "uuid", nullable: false),
                    PlaylistOrder = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlaylistSong", x => new { x.SongId, x.PlaylistId });
                    table.ForeignKey(
                        name: "FK_PlaylistSong_Playlists_PlaylistId",
                        column: x => x.PlaylistId,
                        principalTable: "Playlists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PlaylistSong_Songs_SongId",
                        column: x => x.SongId,
                        principalTable: "Songs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Libraries",
                columns: new[] { "Id", "ApiKey", "CreatedAt", "Description", "IsLocked", "LastScanAt", "LastUpdatedAt", "Name", "Notes", "Path", "SortOrder", "Tags", "Type" },
                values: new object[,]
                {
                    { 1, new Guid("33626ccd-d8b9-4c72-b511-fc29482444f0"), NodaTime.Instant.FromUnixTimeTicks(17313017283652584L), "Files in this directory are scanned and Album information is gathered via processing.", false, null, null, "Inbound", null, "/storage/inbound", 0, null, 1 },
                    { 2, new Guid("942b60f5-4fa4-409f-aeb2-1569e6731fde"), NodaTime.Instant.FromUnixTimeTicks(17313017283652584L), "The staging directory to place processed files into (Inbound -> Staging -> Library).", false, null, null, "Staging", null, "/storage/staging", 0, null, 2 },
                    { 3, new Guid("9358d119-82bd-4e5a-9bc5-374b0512437a"), NodaTime.Instant.FromUnixTimeTicks(17313017283652584L), "The library directory to place processed, reviewed and ready to use music files into.", false, null, null, "Library", null, "/storage/library", 0, null, 3 }
                });

            migrationBuilder.InsertData(
                table: "Settings",
                columns: new[] { "Id", "ApiKey", "Category", "Comment", "CreatedAt", "Description", "IsLocked", "Key", "LastUpdatedAt", "Notes", "SortOrder", "Tags", "Value" },
                values: new object[,]
                {
                    { 1, new Guid("852982fd-85a8-4a80-87ad-0076af038b29"), null, "Add a default filter to show only albums with this or less number of songs.", NodaTime.Instant.FromUnixTimeTicks(17313017283652584L), null, false, "filtering.lessThanSongCount", null, null, 0, null, "3" },
                    { 2, new Guid("d034a6eb-0251-47a4-a1b7-3eaa2c8e2181"), null, "Add a default filter to show only albums with this or less duration.", NodaTime.Instant.FromUnixTimeTicks(17313017283652584L), null, false, "filtering.lessThanDuration", null, null, 0, null, "720000" },
                    { 3, new Guid("f3c4ec56-9208-4ff7-9255-0e19b1265628"), null, "Maximum number of albums to scan when processing inbound directory.", NodaTime.Instant.FromUnixTimeTicks(17313017283652584L), null, false, "processing.stagingDirectoryScanLimit", null, null, 0, null, "250" },
                    { 4, new Guid("797f7256-81bc-47cd-aec0-307e9c7cf0dc"), null, "Default page size when view including pagination.", NodaTime.Instant.FromUnixTimeTicks(17313017283652584L), null, false, "defaults.pagesize", null, null, 0, null, "100" },
                    { 5, new Guid("edbe7828-bea1-4ffe-9081-6c3764a821a9"), null, "When true then move the Melodee.json data file, created during staging, when moving Albums into Library - otherwise delete.", NodaTime.Instant.FromUnixTimeTicks(17313017283652584L), null, false, "processing.moveMelodeeJsonDataFileToLibrary", null, null, 0, null, "true" },
                    { 6, new Guid("f600075d-cf3f-40bb-9d2b-858a307c039c"), null, "Amount of time to display a Toast then auto-close (in milliseconds.)", NodaTime.Instant.FromUnixTimeTicks(17313017283652584L), null, false, "userinterface.toastAutoCloseTime", null, null, 0, null, "2000" },
                    { 7, new Guid("e5dfb195-4e4a-461e-8acb-44413f1b68a2"), null, "Short Format to use when displaying full dates.", NodaTime.Instant.FromUnixTimeTicks(17313017283652584L), null, false, "formatting.dateTimeDisplayFormatShort", null, null, 0, null, "yyyyMMdd HH:mm" },
                    { 8, new Guid("bfae3103-56e5-4ec0-9cb9-51e73133ff8a"), null, "Format to use when displaying activity related dates (e.g. processing messages)", NodaTime.Instant.FromUnixTimeTicks(17313017283652584L), null, false, "formatting.dateTimeDisplayActivityFormat", null, null, 0, null, "HH:mm:ss.fff" },
                    { 9, new Guid("cac573bc-35c7-4c4d-b0e0-f008a7a14cc8"), null, "List of ignored articles when scanning media (pipe delimited).", NodaTime.Instant.FromUnixTimeTicks(17313017283652584L), null, false, "processing.ignoredArticles", null, null, 0, null, "THE|EL|LA|LOS|LAS|LE|LES|OS|AS|O|A" },
                    { 10, new Guid("2cc64d35-7483-4c6c-bb22-641c996049d6"), null, "Is Magic processing enabled.", NodaTime.Instant.FromUnixTimeTicks(17313017283652584L), null, false, "magic.enabled", null, null, 0, null, "true" },
                    { 11, new Guid("78044f22-efc4-46d7-8bc6-73af2eb2323f"), null, "Renumber songs when doing magic processing.", NodaTime.Instant.FromUnixTimeTicks(17313017283652584L), null, false, "magic.doRenumberSongs", null, null, 0, null, "true" },
                    { 12, new Guid("91c25ca8-db4b-4c3d-a56f-bf8aedae7ba5"), null, "Remove featured artists from song artist when doing magic.", NodaTime.Instant.FromUnixTimeTicks(17313017283652584L), null, false, "magic.doRemoveFeaturingArtistFromSongArtist", null, null, 0, null, "true" },
                    { 13, new Guid("7710ea0f-96a3-4b54-8f15-b7e16bc8438f"), null, "Remove featured artists from song title when doing magic.", NodaTime.Instant.FromUnixTimeTicks(17313017283652584L), null, false, "magic.doRemoveFeaturingArtistFromSongTitle", null, null, 0, null, "true" },
                    { 14, new Guid("12ed3882-66e9-4b58-a4ac-ac1bf4aef040"), null, "Replace song artist separators with standard ID3 separator ('/') when doing magic.", NodaTime.Instant.FromUnixTimeTicks(17313017283652584L), null, false, "magic.doReplaceSongsArtistSeparators", null, null, 0, null, "true" },
                    { 15, new Guid("69a05cb1-8d4f-4bbe-9d11-e54e2fdba8b4"), null, "Set the song year to current year if invalid or missing when doing magic.", NodaTime.Instant.FromUnixTimeTicks(17313017283652584L), null, false, "magic.doSetYearToCurrentIfInvalid", null, null, 0, null, "true" },
                    { 16, new Guid("441f2334-1bd3-4eb2-801a-12a3fda5b653"), null, "Remove unwanted text from album title when doing magic.", NodaTime.Instant.FromUnixTimeTicks(17313017283652584L), null, false, "magic.doRemoveUnwantedTextFromAlbumTitle", null, null, 0, null, "true" },
                    { 17, new Guid("f59a3df2-f8b1-4641-a9a2-a78d808beafa"), null, "Remove unwanted text from song titles when doing magic.", NodaTime.Instant.FromUnixTimeTicks(17313017283652584L), null, false, "magic.doRemoveUnwantedTextFromSongTitles", null, null, 0, null, "true" },
                    { 18, new Guid("b353cfe2-090e-4732-820f-db1d2f0dbf05"), null, "Enable Melodee to convert non-mp3 media files during processing.", NodaTime.Instant.FromUnixTimeTicks(17313017283652584L), null, false, "conversion.enabled", null, null, 0, null, "true" },
                    { 19, new Guid("5df6976c-6274-4115-9566-924f1e4c5d0d"), null, "Bitrate to convert non-mp3 media files during processing.", NodaTime.Instant.FromUnixTimeTicks(17313017283652584L), null, false, "conversion.bitrate", null, null, 0, null, "384" },
                    { 20, new Guid("fcefba32-2a94-4516-99d0-22d9b46eff0c"), null, "Vbr to convert non-mp3 media files during processing.", NodaTime.Instant.FromUnixTimeTicks(17313017283652584L), null, false, "conversion.vbrLevel", null, null, 0, null, "4" },
                    { 21, new Guid("ad8b0473-6733-4850-9c07-9477b32d6f0c"), null, "Sampling rate to convert non-mp3 media files during processing.", NodaTime.Instant.FromUnixTimeTicks(17313017283652584L), null, false, "conversion.samplingRate", null, null, 0, null, "48000" },
                    { 22, new Guid("1d89fdce-5241-4038-8948-8bfd5c3c0ec9"), null, "Process of CueSheet files during processing.", NodaTime.Instant.FromUnixTimeTicks(17313017283652584L), null, false, "plugin.cueSheet.enabled", null, null, 0, null, "true" },
                    { 23, new Guid("77780752-2c55-4434-b6f0-59d0934fb58d"), null, "Process of M3U files during processing.", NodaTime.Instant.FromUnixTimeTicks(17313017283652584L), null, false, "plugin.m3u.enabled", null, null, 0, null, "true" },
                    { 24, new Guid("0ad372fb-76ae-47a4-bdfa-e491a984211c"), null, "Process of NFO files during processing.", NodaTime.Instant.FromUnixTimeTicks(17313017283652584L), null, false, "plugin.nfo.enabled", null, null, 0, null, "true" },
                    { 25, new Guid("8d140a7f-2bba-4f88-a6d5-d8540a93c558"), null, "Process of Simple File Verification (SFV) files during processing.", NodaTime.Instant.FromUnixTimeTicks(17313017283652584L), null, false, "plugin.simpleFileVerification.enabled", null, null, 0, null, "true" },
                    { 26, new Guid("cc983268-b0b5-4880-9fe3-57801a64f70f"), null, "Fragments of artist names to replace (JSON Dictionary).", NodaTime.Instant.FromUnixTimeTicks(17313017283652584L), null, false, "processing.artistNameReplacements", null, null, 0, null, "{'AC/DC': ['AC; DC', 'AC;DC', 'AC/ DC', 'AC DC'] , 'Love/Hate': ['Love; Hate', 'Love;Hate', 'Love/ Hate', 'Love Hate'] }" },
                    { 27, new Guid("f5da88c0-6fef-4111-93fd-9d06de0893ea"), null, "If OrigAlbumYear [TOR, TORY, TDOR] value is invalid use current year.", NodaTime.Instant.FromUnixTimeTicks(17313017283652584L), null, false, "processing.doUseCurrentYearAsDefaultOrigAlbumYearValue", null, null, 0, null, "true" },
                    { 28, new Guid("00810ebb-711b-4730-ba2c-20cf1dd8b495"), null, "Delete original files when processing. When false a copy if made, else original is deleted after processed.", NodaTime.Instant.FromUnixTimeTicks(17313017283652584L), null, false, "processing.doDeleteOriginal", null, null, 0, null, "true" },
                    { 29, new Guid("b2935710-2b40-4ac6-bc05-ee30bcaebd42"), null, "Extension to add to file when converted, leave blank to disable.", NodaTime.Instant.FromUnixTimeTicks(17313017283652584L), null, false, "processing.convertedExtension", null, null, 0, null, "_converted" },
                    { 30, new Guid("c5c1dbcd-44c7-4763-ad43-dee4b255c5c3"), null, "Extension to add to file when processed, leave blank to disable.", NodaTime.Instant.FromUnixTimeTicks(17313017283652584L), null, false, "processing.processedExtension", null, null, 0, null, "_processed" },
                    { 31, new Guid("14d479d3-3125-47c1-be43-86b1f35cfb87"), null, "Extension to add to file to indicate other files in the same category where processed and this file was skipped during processing, leave blank to disable.", NodaTime.Instant.FromUnixTimeTicks(17313017283652584L), null, false, "processing.skippedExtension", null, null, 0, null, "_skipped" },
                    { 32, new Guid("d4e2b896-a3e1-4f4a-b340-15aacb05c99f"), null, "When processing over write any existing Melodee data files, otherwise skip and leave in place.", NodaTime.Instant.FromUnixTimeTicks(17313017283652584L), null, false, "processing.doOverrideExistingMelodeeDataFiles", null, null, 0, null, "true" },
                    { 33, new Guid("272281ff-9962-4f17-82eb-66c35efbd681"), null, "Include any embedded images from media files into the Melodee data file.", NodaTime.Instant.FromUnixTimeTicks(17313017283652584L), null, false, "processing.doLoadEmbeddedImages", null, null, 0, null, "true" },
                    { 34, new Guid("3e42892c-eef2-472c-a90e-f787c4ec4f3e"), null, "The maximum number of files to process, set to zero for infinite.", NodaTime.Instant.FromUnixTimeTicks(17313017283652584L), null, false, "processing.maximumProcessingCount", null, null, 0, null, "0" },
                    { 35, new Guid("9441a749-7ad9-4333-ba5a-6aed98295a2e"), null, "Maximum allowed length of album directory name.", NodaTime.Instant.FromUnixTimeTicks(17313017283652584L), null, false, "processing.maximumAlbumDirectoryNameLength", null, null, 0, null, "255" },
                    { 36, new Guid("9c66806e-7046-48da-a6da-923bc52acd80"), null, "Maximum allowed length of artist directory name.", NodaTime.Instant.FromUnixTimeTicks(17313017283652584L), null, false, "processing.maximumArtistDirectoryNameLength", null, null, 0, null, "255" },
                    { 37, new Guid("8488b30d-afe1-4528-8211-ca174fe50b28"), null, "Fragments to remove from album titles (JSON array).", NodaTime.Instant.FromUnixTimeTicks(17313017283652584L), null, false, "processing.albumTitleRemovals", null, null, 0, null, "['^', '~', '#']" },
                    { 38, new Guid("7fdf6d25-9fc7-472a-b882-88c636e7e439"), null, "Fragments to remove from song titles (JSON array).", NodaTime.Instant.FromUnixTimeTicks(17313017283652584L), null, false, "processing.songTitleRemovals", null, null, 0, null, "[';', '(Remaster)', 'Remaster']" },
                    { 39, new Guid("7d34c0a8-9167-459e-9e3b-a760306ead8e"), null, "Continue processing if an error is encountered.", NodaTime.Instant.FromUnixTimeTicks(17313017283652584L), null, false, "processing.doContinueOnDirectoryProcessingErrors", null, null, 0, null, "true" },
                    { 40, new Guid("848ed2cf-5d67-4b4d-a031-39935dc2fa5d"), null, "When true then move Album Melodee json files to the Staging directory.", NodaTime.Instant.FromUnixTimeTicks(17313017283652584L), null, false, "processing.doMoveMelodeeDataFileToStagingDirectory", null, null, 0, null, "true" },
                    { 41, new Guid("dde268dc-355e-476b-bae5-11f15d1383cd"), null, "Is scripting enabled.", NodaTime.Instant.FromUnixTimeTicks(17313017283652584L), null, false, "scripting.enabled", null, null, 0, null, "false" },
                    { 42, new Guid("2f324070-1a4b-40e6-941e-783eb5ecf312"), null, "Script to run before processing the inbound directory, leave blank to disable.", NodaTime.Instant.FromUnixTimeTicks(17313017283652584L), null, false, "scripting.preDiscoveryScript", null, null, 0, null, "" },
                    { 43, new Guid("e1c58d56-d5cc-416b-8262-7444f670dae5"), null, "Script to run after processing the inbound directory, leave blank to disable.", NodaTime.Instant.FromUnixTimeTicks(17313017283652584L), null, false, "scripting.postDiscoveryScript", null, null, 0, null, "" },
                    { 44, new Guid("b366bd0f-ab90-49a7-819e-e638508931ab"), null, "The maximum value a media number can have for an album. The length of this is used for formatting song names.", NodaTime.Instant.FromUnixTimeTicks(17313017283652584L), null, false, "validation.maximumMediaNumber", null, null, 0, null, "999" },
                    { 45, new Guid("faa45e72-e02d-4c8a-8cc4-16c02dba5fef"), null, "The maximum value a song number can have for an album. The length of this is used for formatting song names.", NodaTime.Instant.FromUnixTimeTicks(17313017283652584L), null, false, "validation.maximumSongNumber", null, null, 0, null, "9999" },
                    { 46, new Guid("4f9213ae-c197-4082-938e-1ccc5eebe4c7"), null, "Minimum allowed year for an album.", NodaTime.Instant.FromUnixTimeTicks(17313017283652584L), null, false, "validation.minimumAlbumYear", null, null, 0, null, "1860" },
                    { 47, new Guid("402c7421-4909-4e4f-91ca-7f3ad9203b38"), null, "Maximum allowed year for an album.", NodaTime.Instant.FromUnixTimeTicks(17313017283652584L), null, false, "validation.maximumAlbumYear", null, null, 0, null, "2150" },
                    { 48, new Guid("c6da2f3e-6f63-494a-b719-8774aca0f39c"), null, "Private key used to encrypt/decrypt passwords for Subsonic authentication. Use https://generate-random.org/encryption-key-generator?count=1&bytes=32&cipher=aes-256-cbc&string=&password= to generate a new key.", NodaTime.Instant.FromUnixTimeTicks(17313017283652584L), null, false, "encryption.privateKey", null, null, 0, null, "generate_a_new_key_and_update_this_value" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_AlbumDiscs_AlbumId_DiscNumber",
                table: "AlbumDiscs",
                columns: new[] { "AlbumId", "DiscNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Albums_ApiKey",
                table: "Albums",
                column: "ApiKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Albums_ArtistId",
                table: "Albums",
                column: "ArtistId");

            migrationBuilder.CreateIndex(
                name: "IX_Albums_Name",
                table: "Albums",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Albums_NameNormalized",
                table: "Albums",
                column: "NameNormalized");

            migrationBuilder.CreateIndex(
                name: "IX_Albums_SortName",
                table: "Albums",
                column: "SortName");

            migrationBuilder.CreateIndex(
                name: "IX_Artists_ApiKey",
                table: "Artists",
                column: "ApiKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Artists_Name",
                table: "Artists",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Artists_NameNormalized",
                table: "Artists",
                column: "NameNormalized");

            migrationBuilder.CreateIndex(
                name: "IX_Artists_SortName",
                table: "Artists",
                column: "SortName");

            migrationBuilder.CreateIndex(
                name: "IX_Bookmarks_ApiKey",
                table: "Bookmarks",
                column: "ApiKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Bookmarks_SongId",
                table: "Bookmarks",
                column: "SongId");

            migrationBuilder.CreateIndex(
                name: "IX_Bookmarks_UserId_SongId",
                table: "Bookmarks",
                columns: new[] { "UserId", "SongId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ClientApplications_ApiKey",
                table: "ClientApplications",
                column: "ApiKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Contributors_AlbumId",
                table: "Contributors",
                column: "AlbumId");

            migrationBuilder.CreateIndex(
                name: "IX_Contributors_ApiKey",
                table: "Contributors",
                column: "ApiKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Contributors_ArtistId",
                table: "Contributors",
                column: "ArtistId");

            migrationBuilder.CreateIndex(
                name: "IX_Contributors_SongId",
                table: "Contributors",
                column: "SongId");

            migrationBuilder.CreateIndex(
                name: "IX_Libraries_ApiKey",
                table: "Libraries",
                column: "ApiKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Libraries_Type",
                table: "Libraries",
                column: "Type",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LibraryScanHistories_LibraryId",
                table: "LibraryScanHistories",
                column: "LibraryId");

            migrationBuilder.CreateIndex(
                name: "IX_Players_ApiKey",
                table: "Players",
                column: "ApiKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Players_UserId_Client_UserAgent",
                table: "Players",
                columns: new[] { "UserId", "Client", "UserAgent" });

            migrationBuilder.CreateIndex(
                name: "IX_Playlists_ApiKey",
                table: "Playlists",
                column: "ApiKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Playlists_SongId",
                table: "Playlists",
                column: "SongId");

            migrationBuilder.CreateIndex(
                name: "IX_Playlists_UserId_Name",
                table: "Playlists",
                columns: new[] { "UserId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PlaylistSong_PlaylistId",
                table: "PlaylistSong",
                column: "PlaylistId");

            migrationBuilder.CreateIndex(
                name: "IX_PlaylistSong_SongId_PlaylistId",
                table: "PlaylistSong",
                columns: new[] { "SongId", "PlaylistId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PlayQues_ApiKey",
                table: "PlayQues",
                column: "ApiKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PlayQues_SongId",
                table: "PlayQues",
                column: "SongId");

            migrationBuilder.CreateIndex(
                name: "IX_PlayQues_UserId",
                table: "PlayQues",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_RadioStations_ApiKey",
                table: "RadioStations",
                column: "ApiKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Scrobbles_ApiKey",
                table: "Scrobbles",
                column: "ApiKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Scrobbles_SongId",
                table: "Scrobbles",
                column: "SongId");

            migrationBuilder.CreateIndex(
                name: "IX_Scrobbles_UserId_ServiceUrl_SongId_PlayTimeInMs",
                table: "Scrobbles",
                columns: new[] { "UserId", "ServiceUrl", "SongId", "PlayTimeInMs" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Settings_ApiKey",
                table: "Settings",
                column: "ApiKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Settings_Category",
                table: "Settings",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_Settings_Key",
                table: "Settings",
                column: "Key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Shares_ApiKey",
                table: "Shares",
                column: "ApiKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Shares_UserId",
                table: "Shares",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Songs_AlbumDiscId_SongNumber",
                table: "Songs",
                columns: new[] { "AlbumDiscId", "SongNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Songs_ApiKey",
                table: "Songs",
                column: "ApiKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Songs_LibraryId",
                table: "Songs",
                column: "LibraryId");

            migrationBuilder.CreateIndex(
                name: "IX_Songs_Title",
                table: "Songs",
                column: "Title");

            migrationBuilder.CreateIndex(
                name: "IX_UserAlbums_AlbumId",
                table: "UserAlbums",
                column: "AlbumId");

            migrationBuilder.CreateIndex(
                name: "IX_UserAlbums_ApiKey",
                table: "UserAlbums",
                column: "ApiKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserAlbums_UserId_AlbumId",
                table: "UserAlbums",
                columns: new[] { "UserId", "AlbumId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserArtists_ApiKey",
                table: "UserArtists",
                column: "ApiKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserArtists_ArtistId",
                table: "UserArtists",
                column: "ArtistId");

            migrationBuilder.CreateIndex(
                name: "IX_UserArtists_UserId_ArtistId",
                table: "UserArtists",
                columns: new[] { "UserId", "ArtistId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_ApiKey",
                table: "Users",
                column: "ApiKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_UserName",
                table: "Users",
                column: "UserName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserSongs_ApiKey",
                table: "UserSongs",
                column: "ApiKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserSongs_SongId",
                table: "UserSongs",
                column: "SongId");

            migrationBuilder.CreateIndex(
                name: "IX_UserSongs_UserId_SongId",
                table: "UserSongs",
                columns: new[] { "UserId", "SongId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Bookmarks");

            migrationBuilder.DropTable(
                name: "ClientApplications");

            migrationBuilder.DropTable(
                name: "Contributors");

            migrationBuilder.DropTable(
                name: "LibraryScanHistories");

            migrationBuilder.DropTable(
                name: "Players");

            migrationBuilder.DropTable(
                name: "PlaylistSong");

            migrationBuilder.DropTable(
                name: "PlayQues");

            migrationBuilder.DropTable(
                name: "RadioStations");

            migrationBuilder.DropTable(
                name: "Scrobbles");

            migrationBuilder.DropTable(
                name: "Settings");

            migrationBuilder.DropTable(
                name: "Shares");

            migrationBuilder.DropTable(
                name: "UserAlbums");

            migrationBuilder.DropTable(
                name: "UserArtists");

            migrationBuilder.DropTable(
                name: "UserSongs");

            migrationBuilder.DropTable(
                name: "Playlists");

            migrationBuilder.DropTable(
                name: "Songs");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "AlbumDiscs");

            migrationBuilder.DropTable(
                name: "Libraries");

            migrationBuilder.DropTable(
                name: "Albums");

            migrationBuilder.DropTable(
                name: "Artists");
        }
    }
}
