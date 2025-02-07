using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NodaTime;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Melodee.Common.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Libraries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    ArtistCount = table.Column<int>(type: "integer", nullable: true),
                    AlbumCount = table.Column<int>(type: "integer", nullable: true),
                    SongCount = table.Column<int>(type: "integer", nullable: true),
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
                name: "SearchHistories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CreatedAt = table.Column<Instant>(type: "timestamp with time zone", nullable: false),
                    ByUserId = table.Column<int>(type: "integer", nullable: false),
                    ByUserAgent = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    SearchQuery = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    FoundArtistsCount = table.Column<int>(type: "integer", nullable: false),
                    FoundAlbumsCount = table.Column<int>(type: "integer", nullable: false),
                    FoundSongsCount = table.Column<int>(type: "integer", nullable: false),
                    FoundOtherItems = table.Column<int>(type: "integer", nullable: false),
                    SearchDurationInMs = table.Column<double>(type: "double precision", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SearchHistories", x => x.Id);
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
                    IsEditor = table.Column<bool>(type: "boolean", nullable: false),
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
                    LastFmSessionKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    HatedGenres = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
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
                name: "Artists",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    NameNormalized = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    SortName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    RealName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    Directory = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Roles = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    AlbumCount = table.Column<int>(type: "integer", nullable: false),
                    SongCount = table.Column<int>(type: "integer", nullable: false),
                    LibraryId = table.Column<int>(type: "integer", nullable: false),
                    Biography = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    ImageCount = table.Column<int>(type: "integer", nullable: true),
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
                    WikiDataId = table.Column<string>(type: "text", nullable: true),
                    MusicBrainzId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastFmId = table.Column<string>(type: "text", nullable: true),
                    SpotifyId = table.Column<string>(type: "text", nullable: true),
                    CalculatedRating = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Artists", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Artists_Libraries_LibraryId",
                        column: x => x.LibraryId,
                        principalTable: "Libraries",
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
                    TranscodingId = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    Hostname = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
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
                    ShareIds = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
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
                name: "Albums",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ArtistId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    NameNormalized = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    SortName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    AlbumStatus = table.Column<short>(type: "smallint", nullable: false),
                    MetaDataStatus = table.Column<int>(type: "integer", nullable: false),
                    ImageCount = table.Column<int>(type: "integer", nullable: true),
                    AlbumType = table.Column<short>(type: "smallint", nullable: false),
                    OriginalReleaseDate = table.Column<LocalDate>(type: "date", nullable: true),
                    ReleaseDate = table.Column<LocalDate>(type: "date", nullable: false),
                    IsCompilation = table.Column<bool>(type: "boolean", nullable: false),
                    SongCount = table.Column<short>(type: "smallint", nullable: true),
                    Duration = table.Column<double>(type: "double precision", nullable: false),
                    Genres = table.Column<string[]>(type: "text[]", maxLength: 2000, nullable: true),
                    Moods = table.Column<string[]>(type: "text[]", maxLength: 2000, nullable: true),
                    Comment = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    ReplayGain = table.Column<double>(type: "double precision", nullable: true),
                    ReplayPeak = table.Column<double>(type: "double precision", nullable: true),
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
                    WikiDataId = table.Column<string>(type: "text", nullable: true),
                    MusicBrainzId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastFmId = table.Column<string>(type: "text", nullable: true),
                    SpotifyId = table.Column<string>(type: "text", nullable: true),
                    CalculatedRating = table.Column<decimal>(type: "numeric", nullable: false)
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
                name: "ArtistRelation",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ArtistId = table.Column<int>(type: "integer", nullable: false),
                    RelatedArtistId = table.Column<int>(type: "integer", nullable: false),
                    ArtistRelationType = table.Column<int>(type: "integer", nullable: false),
                    RelationStart = table.Column<Instant>(type: "timestamp with time zone", nullable: true),
                    RelationEnd = table.Column<Instant>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("PK_ArtistRelation", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ArtistRelation_Artists_ArtistId",
                        column: x => x.ArtistId,
                        principalTable: "Artists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ArtistRelation_Artists_RelatedArtistId",
                        column: x => x.RelatedArtistId,
                        principalTable: "Artists",
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
                    IsHated = table.Column<bool>(type: "boolean", nullable: false),
                    StarredAt = table.Column<Instant>(type: "timestamp with time zone", nullable: true),
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
                name: "Songs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AlbumId = table.Column<int>(type: "integer", nullable: false),
                    Title = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    TitleSort = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    TitleNormalized = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Genres = table.Column<string[]>(type: "text[]", maxLength: 2000, nullable: true),
                    Moods = table.Column<string[]>(type: "text[]", maxLength: 2000, nullable: true),
                    Comment = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    ReplayGain = table.Column<double>(type: "double precision", nullable: true),
                    ReplayPeak = table.Column<double>(type: "double precision", nullable: true),
                    ImageCount = table.Column<int>(type: "integer", nullable: true),
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
                    ContentType = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    ChannelCount = table.Column<int>(type: "integer", nullable: true),
                    IsVbr = table.Column<bool>(type: "boolean", nullable: false),
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
                    WikiDataId = table.Column<string>(type: "text", nullable: true),
                    MusicBrainzId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastFmId = table.Column<string>(type: "text", nullable: true),
                    SpotifyId = table.Column<string>(type: "text", nullable: true),
                    CalculatedRating = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Songs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Songs_Albums_AlbumId",
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
                    LastPlayedAt = table.Column<Instant>(type: "timestamp with time zone", nullable: true),
                    IsStarred = table.Column<bool>(type: "boolean", nullable: false),
                    IsHated = table.Column<bool>(type: "boolean", nullable: false),
                    StarredAt = table.Column<Instant>(type: "timestamp with time zone", nullable: true),
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
                    WikiDataId = table.Column<string>(type: "text", nullable: true),
                    MusicBrainzId = table.Column<Guid>(type: "uuid", nullable: true),
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
                    ArtistId = table.Column<int>(type: "integer", nullable: true),
                    ContributorName = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    MetaTagIdentifier = table.Column<int>(type: "integer", nullable: false),
                    SongId = table.Column<int>(type: "integer", nullable: true),
                    AlbumId = table.Column<int>(type: "integer", nullable: false),
                    ContributorType = table.Column<int>(type: "integer", nullable: false),
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
                        principalColumn: "Id");
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
                    HasCustomImage = table.Column<bool>(type: "boolean", nullable: false),
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
                    IsCurrentSong = table.Column<bool>(type: "boolean", nullable: false),
                    ChangedBy = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Position = table.Column<double>(type: "double precision", nullable: false),
                    PlayQueId = table.Column<int>(type: "integer", nullable: false),
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
                name: "UserSongs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    SongId = table.Column<int>(type: "integer", nullable: false),
                    PlayedCount = table.Column<int>(type: "integer", nullable: false),
                    LastPlayedAt = table.Column<Instant>(type: "timestamp with time zone", nullable: true),
                    IsStarred = table.Column<bool>(type: "boolean", nullable: false),
                    IsHated = table.Column<bool>(type: "boolean", nullable: false),
                    StarredAt = table.Column<Instant>(type: "timestamp with time zone", nullable: true),
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
                columns: new[] { "Id", "AlbumCount", "ApiKey", "ArtistCount", "CreatedAt", "Description", "IsLocked", "LastScanAt", "LastUpdatedAt", "Name", "Notes", "Path", "SongCount", "SortOrder", "Tags", "Type" },
                values: new object[,]
                {
                    { 1, null, new Guid("7e78f282-eeeb-4ab0-835a-85393a04e574"), null, NodaTime.Instant.FromUnixTimeTicks(17389509828221072L), "Files in this directory are scanned and Album information is gathered via processing.", false, null, null, "Inbound", null, "/storage/inbound/", null, 0, null, 1 },
                    { 2, null, new Guid("50c385f0-e4c5-4be5-90fd-8f9f42d91c99"), null, NodaTime.Instant.FromUnixTimeTicks(17389509828221072L), "The staging directory to place processed files into (Inbound -> Staging -> Library).", false, null, null, "Staging", null, "/storage/staging/", null, 0, null, 2 },
                    { 3, null, new Guid("da615c00-2f35-46dd-93a3-87c3a92b91d6"), null, NodaTime.Instant.FromUnixTimeTicks(17389509828221072L), "The library directory to place processed, reviewed and ready to use music files into.", false, null, null, "Storage", null, "/storage/library/", null, 0, null, 3 },
                    { 4, null, new Guid("b158b8a2-8cde-42a9-a922-b5fd996f1b1b"), null, NodaTime.Instant.FromUnixTimeTicks(17389509828221072L), "Library where user images are stored.", false, null, null, "User Images", null, "/storage/images/users/", null, 0, null, 4 }
                });

            migrationBuilder.InsertData(
                table: "Settings",
                columns: new[] { "Id", "ApiKey", "Category", "Comment", "CreatedAt", "Description", "IsLocked", "Key", "LastUpdatedAt", "Notes", "SortOrder", "Tags", "Value" },
                values: new object[,]
                {
                    { 1, new Guid("183e17db-d55a-4d45-9e11-b3c16f4d0071"), null, "Add a default filter to show only albums with this or less number of songs.", NodaTime.Instant.FromUnixTimeTicks(17389509828221072L), null, false, "filtering.lessThanSongCount", null, null, 0, null, "3" },
                    { 2, new Guid("df55f94e-9704-405b-b34a-ef0bf62788cc"), null, "Add a default filter to show only albums with this or less duration.", NodaTime.Instant.FromUnixTimeTicks(17389509828221072L), null, false, "filtering.lessThanDuration", null, null, 0, null, "720000" },
                    { 3, new Guid("7836d7a3-1c3e-4cea-adc9-032445a1c57a"), null, "Maximum number of albums to scan when processing inbound directory.", NodaTime.Instant.FromUnixTimeTicks(17389509828221072L), null, false, "processing.stagingDirectoryScanLimit", null, null, 0, null, "250" },
                    { 4, new Guid("14494761-1203-4d6f-ab5d-5f35360a5e56"), null, "Default page size when view including pagination.", NodaTime.Instant.FromUnixTimeTicks(17389509828221072L), null, false, "defaults.pagesize", null, null, 0, null, "100" },
                    { 6, new Guid("5952a2c2-f75f-41a2-9e73-6c4bde40753b"), null, "Amount of time to display a Toast then auto-close (in milliseconds.)", NodaTime.Instant.FromUnixTimeTicks(17389509828221072L), null, false, "userinterface.toastAutoCloseTime", null, null, 0, null, "2000" },
                    { 9, new Guid("998422d2-9b63-48a2-92d3-daf8a8ed79cb"), null, "List of ignored articles when scanning media (pipe delimited).", NodaTime.Instant.FromUnixTimeTicks(17389509828221072L), null, false, "processing.ignoredArticles", null, null, 0, null, "THE|EL|LA|LOS|LAS|LE|LES|OS|AS|O|A" },
                    { 26, new Guid("d94c8ca4-0e95-4b6f-994a-01c64f54c9c2"), null, "Fragments of artist names to replace (JSON Dictionary).", NodaTime.Instant.FromUnixTimeTicks(17389509828221072L), null, false, "processing.artistNameReplacements", null, null, 0, null, "{'AC/DC': ['AC; DC', 'AC;DC', 'AC/ DC', 'AC DC'] , 'Love/Hate': ['Love; Hate', 'Love;Hate', 'Love/ Hate', 'Love Hate'] }" },
                    { 27, new Guid("07c75d0a-3dab-4493-8717-1a2b0d1f757a"), null, "If OrigAlbumYear [TOR, TORY, TDOR] value is invalid use current year.", NodaTime.Instant.FromUnixTimeTicks(17389509828221072L), null, false, "processing.doUseCurrentYearAsDefaultOrigAlbumYearValue", null, null, 0, null, "false" },
                    { 28, new Guid("309f4150-0aeb-4964-af20-c141aeb883ea"), null, "Delete original files when processing. When false a copy if made, else original is deleted after processed.", NodaTime.Instant.FromUnixTimeTicks(17389509828221072L), null, false, "processing.doDeleteOriginal", null, null, 0, null, "false" },
                    { 29, new Guid("6fd611e1-1c72-4ddc-a36f-c9d7f4f0578f"), null, "Extension to add to file when converted, leave blank to disable.", NodaTime.Instant.FromUnixTimeTicks(17389509828221072L), null, false, "processing.convertedExtension", null, null, 0, null, "_converted" },
                    { 30, new Guid("cadc9f0c-9361-413c-ab73-80e09edebbcf"), null, "Extension to add to file when processed, leave blank to disable.", NodaTime.Instant.FromUnixTimeTicks(17389509828221072L), null, false, "processing.processedExtension", null, null, 0, null, "_processed" },
                    { 31, new Guid("fd1870b6-464c-4b76-a04b-ff5fdbaae6f3"), null, "Extension to add to file to indicate other files in the same category where processed and this file was skipped during processing, leave blank to disable.", NodaTime.Instant.FromUnixTimeTicks(17389509828221072L), null, false, "processing.skippedExtension", null, null, 0, null, "_skipped" },
                    { 32, new Guid("e078b0bd-9a88-4a0a-bfa1-e4a9beb3e255"), null, "When processing over write any existing Melodee data files, otherwise skip and leave in place.", NodaTime.Instant.FromUnixTimeTicks(17389509828221072L), null, false, "processing.doOverrideExistingMelodeeDataFiles", null, null, 0, null, "true" },
                    { 34, new Guid("da99e1a1-e074-43ca-b2a3-567ea0d1c48e"), null, "The maximum number of files to process, set to zero for unlimited.", NodaTime.Instant.FromUnixTimeTicks(17389509828221072L), null, false, "processing.maximumProcessingCount", null, null, 0, null, "0" },
                    { 35, new Guid("3655d8cd-f30b-4e78-8ef2-6a5fe5017339"), null, "Maximum allowed length of album directory name.", NodaTime.Instant.FromUnixTimeTicks(17389509828221072L), null, false, "processing.maximumAlbumDirectoryNameLength", null, null, 0, null, "255" },
                    { 36, new Guid("1b498504-b5e7-4b48-b270-7b64cc6cb305"), null, "Maximum allowed length of artist directory name.", NodaTime.Instant.FromUnixTimeTicks(17389509828221072L), null, false, "processing.maximumArtistDirectoryNameLength", null, null, 0, null, "255" },
                    { 37, new Guid("3e13190f-6a71-46a3-8aba-9f198b701aec"), null, "Fragments to remove from album titles (JSON array).", NodaTime.Instant.FromUnixTimeTicks(17389509828221072L), null, false, "processing.albumTitleRemovals", null, null, 0, null, "['^', '~', '#']" },
                    { 38, new Guid("c0f9d18b-b0ff-4554-b6e9-fe87d20b6362"), null, "Fragments to remove from song titles (JSON array).", NodaTime.Instant.FromUnixTimeTicks(17389509828221072L), null, false, "processing.songTitleRemovals", null, null, 0, null, "[';', '(Remaster)', 'Remaster']" },
                    { 39, new Guid("67978df6-39b7-4568-b623-7fad4893fa2e"), null, "Continue processing if an error is encountered.", NodaTime.Instant.FromUnixTimeTicks(17389509828221072L), null, false, "processing.doContinueOnDirectoryProcessingErrors", null, null, 0, null, "true" },
                    { 41, new Guid("0f62dcc1-0a04-400b-abed-509ca1a1f0fa"), null, "Is scripting enabled.", NodaTime.Instant.FromUnixTimeTicks(17389509828221072L), null, false, "scripting.enabled", null, null, 0, null, "false" },
                    { 42, new Guid("90915ff3-5362-4736-a44f-ef45aa731c21"), null, "Script to run before processing the inbound directory, leave blank to disable.", NodaTime.Instant.FromUnixTimeTicks(17389509828221072L), null, false, "scripting.preDiscoveryScript", null, null, 0, null, "" },
                    { 43, new Guid("a3c88c3d-c4b3-4659-9cd7-ae1279699020"), null, "Script to run after processing the inbound directory, leave blank to disable.", NodaTime.Instant.FromUnixTimeTicks(17389509828221072L), null, false, "scripting.postDiscoveryScript", null, null, 0, null, "" },
                    { 44, new Guid("40628b5e-626e-429c-9586-072c4fe4585e"), 13, "The maximum value a media number can have for an album.", NodaTime.Instant.FromUnixTimeTicks(17389509828221072L), null, false, "validation.maximumMediaNumber", null, null, 0, null, "999" },
                    { 45, new Guid("d1fc60bd-2e94-4ccb-9523-69e9798fc825"), null, "Don't create performer contributors for these performer names.", NodaTime.Instant.FromUnixTimeTicks(17389509828221072L), null, false, "processing.ignoredPerformers", null, null, 0, null, "" },
                    { 46, new Guid("26c19506-4d37-4855-b353-e2a612cb6e4b"), null, "Don't create production contributors for these production names.", NodaTime.Instant.FromUnixTimeTicks(17389509828221072L), null, false, "processing.ignoredProduction", null, null, 0, null, "['www.t.me;pmedia_music']" },
                    { 47, new Guid("c39c60df-a2e2-43a0-b002-30e0b300544e"), null, "Don't create publisher contributors for these artist names.", NodaTime.Instant.FromUnixTimeTicks(17389509828221072L), null, false, "processing.ignoredPublishers", null, null, 0, null, "['P.M.E.D.I.A','PMEDIA','PMEDIA GROUP']" },
                    { 49, new Guid("783cdb69-29ec-47f8-bf10-bfa63e591d1a"), null, "Private key used to encrypt/decrypt passwords for Subsonic authentication. Use https://generate-random.org/encryption-key-generator?count=1&bytes=32&cipher=aes-256-cbc&string=&password= to generate a new key.", NodaTime.Instant.FromUnixTimeTicks(17389509828221072L), null, false, "encryption.privateKey", null, null, 0, null, "H+Kiik6VMKfTD2MesF1GoMjczTrD5RhuKckJ5+/UQWOdWajGcsEC3yEnlJ5eoy8Y" },
                    { 50, new Guid("95926283-e4c3-40ac-94bf-6865d3866968"), null, "Prefix to apply to indicate an album directory is a duplicate album for an artist. If left blank the default of '__duplicate_' will be used.", NodaTime.Instant.FromUnixTimeTicks(17389509828221072L), null, false, "processing.duplicateAlbumPrefix", null, null, 0, null, "_duplicate_ " },
                    { 53, new Guid("c936a6a9-29a0-4008-a58c-2508cc589956"), null, "Processing batching size. Allowed range is between [250] and [1000]. ", NodaTime.Instant.FromUnixTimeTicks(17389509828221072L), null, false, "defaults.batchSize", null, null, 0, null, "250" },
                    { 100, new Guid("8381d394-d36e-4b94-9e4e-8745492c5205"), 1, "OpenSubsonic server supported Subsonic API version.", NodaTime.Instant.FromUnixTimeTicks(17389509828221072L), null, false, "openSubsonicServer.openSubsonic.serverSupportedVersion", null, null, 0, null, "1.16.1" },
                    { 101, new Guid("1079801e-8f7d-4b0b-9354-cd54d28ce7d5"), 1, "OpenSubsonic server name.", NodaTime.Instant.FromUnixTimeTicks(17389509828221072L), null, false, "openSubsonicServer.openSubsonicServer.type", null, null, 0, null, "Melodee" },
                    { 102, new Guid("74961329-7b0c-4b22-94ec-28fb996b78e4"), 1, "OpenSubsonic server actual version. [Ex: 1.2.3 (beta)]", NodaTime.Instant.FromUnixTimeTicks(17389509828221072L), null, false, "openSubsonicServer.openSubsonicServer.version", null, null, 0, null, "1.0.1 (beta)" },
                    { 103, new Guid("e796c2a5-0a3f-4664-80af-966bc1aaf625"), 1, "OpenSubsonic email to use in License responses.", NodaTime.Instant.FromUnixTimeTicks(17389509828221072L), null, false, "openSubsonicServer.openSubsonicServerLicenseEmail", null, null, 0, null, "noreply@localhost.lan" },
                    { 104, new Guid("393e5fc9-0fc2-40c4-bbf4-e7855c485266"), 1, "Limit the number of artists to include in an indexes request, set to zero for 32k per index (really not recommended with tens of thousands of artists and mobile clients timeout downloading indexes, a user can find an artist by search)", NodaTime.Instant.FromUnixTimeTicks(17389509828221072L), null, false, "openSubsonicServer.openSubsonicServer.index.artistLimit", null, null, 0, null, "1000" },
                    { 200, new Guid("f91fa7f7-a7f3-4ec4-a10e-b56e37daf311"), 2, "Enable Melodee to convert non-mp3 media files during processing.", NodaTime.Instant.FromUnixTimeTicks(17389509828221072L), null, false, "conversion.enabled", null, null, 0, null, "true" },
                    { 201, new Guid("788c3bce-a407-4546-8774-21f40d720f52"), 2, "Bitrate to convert non-mp3 media files during processing.", NodaTime.Instant.FromUnixTimeTicks(17389509828221072L), null, false, "conversion.bitrate", null, null, 0, null, "384" },
                    { 202, new Guid("f2d7844a-3674-496e-86ba-ec70d464f9e9"), 2, "Vbr to convert non-mp3 media files during processing.", NodaTime.Instant.FromUnixTimeTicks(17389509828221072L), null, false, "conversion.vbrLevel", null, null, 0, null, "4" },
                    { 203, new Guid("376dec8c-eca9-4c76-9b79-43ff668a7ea0"), 2, "Sampling rate to convert non-mp3 media files during processing.", NodaTime.Instant.FromUnixTimeTicks(17389509828221072L), null, false, "conversion.samplingRate", null, null, 0, null, "48000" },
                    { 300, new Guid("3c314b0e-5b20-463a-acdb-b424453704aa"), 3, "Short Format to use when displaying full dates.", NodaTime.Instant.FromUnixTimeTicks(17389509828221072L), null, false, "formatting.dateTimeDisplayFormatShort", null, null, 0, null, "yyyyMMdd HH\\:mm" },
                    { 301, new Guid("20a9b888-2547-4b23-8c06-f784a4dbd7cc"), 3, "Format to use when displaying activity related dates (e.g., processing messages)", NodaTime.Instant.FromUnixTimeTicks(17389509828221072L), null, false, "formatting.dateTimeDisplayActivityFormat", null, null, 0, null, "hh\\:mm\\:ss\\.ffff" },
                    { 400, new Guid("c1c5458b-cbe8-4296-b9ad-52c3a2ba171d"), 4, "Include any embedded images from media files into the Melodee data file.", NodaTime.Instant.FromUnixTimeTicks(17389509828221072L), null, false, "imaging.doLoadEmbeddedImages", null, null, 0, null, "true" },
                    { 401, new Guid("0926086f-cb41-4b5c-91d0-5adadf668fe4"), 4, "Small image size (square image, this is both width and height).", NodaTime.Instant.FromUnixTimeTicks(17389509828221072L), null, false, "imaging.smallSize", null, null, 0, null, "300" },
                    { 402, new Guid("1bcb76b7-1040-42b2-a304-2739a95ede49"), 4, "Medium image size (square image, this is both width and height).", NodaTime.Instant.FromUnixTimeTicks(17389509828221072L), null, false, "imaging.mediumSize", null, null, 0, null, "600" },
                    { 403, new Guid("04b6f8db-7942-4a73-9613-cca6feab83a6"), 4, "Large image size (square image, this is both width and height), if larger than will be resized to this image, leave blank to disable.", NodaTime.Instant.FromUnixTimeTicks(17389509828221072L), null, false, "imaging.largeSize", null, null, 0, null, "1600" },
                    { 404, new Guid("a96d1032-71be-4579-9525-8e29950774c1"), 4, "Maximum allowed number of images for an album, this includes all image types (Front, Rear, etc.), set to zero for unlimited.", NodaTime.Instant.FromUnixTimeTicks(17389509828221072L), null, false, "imaging.maximumNumberOfAlbumImages", null, null, 0, null, "25" },
                    { 405, new Guid("c1726225-e14f-4a23-bcb8-96d4da9473db"), 4, "Maximum allowed number of images for an artist, set to zero for unlimited.", NodaTime.Instant.FromUnixTimeTicks(17389509828221072L), null, false, "imaging.maximumNumberOfArtistImages", null, null, 0, null, "25" },
                    { 406, new Guid("799aacfc-60b9-469a-b051-3120312cfc5d"), 4, "Images under this size are considered invalid, set to zero to disable.", NodaTime.Instant.FromUnixTimeTicks(17389509828221072L), null, false, "imaging.minimumImageSize", null, null, 0, null, "300" },
                    { 500, new Guid("7710cfff-178e-40e5-a391-0ff34bc66b8d"), 5, "Is Magic processing enabled.", NodaTime.Instant.FromUnixTimeTicks(17389509828221072L), null, false, "magic.enabled", null, null, 0, null, "true" },
                    { 501, new Guid("c452ef59-4f3d-4f7c-92cf-4febcb705e01"), 5, "Renumber songs when doing magic processing.", NodaTime.Instant.FromUnixTimeTicks(17389509828221072L), null, false, "magic.doRenumberSongs", null, null, 0, null, "true" },
                    { 502, new Guid("03b6aaeb-c2cd-4b2c-8b6f-1894e496f94b"), 5, "Remove featured artists from song artist when doing magic.", NodaTime.Instant.FromUnixTimeTicks(17389509828221072L), null, false, "magic.doRemoveFeaturingArtistFromSongArtist", null, null, 0, null, "true" },
                    { 503, new Guid("e21c5982-72ff-4859-95ea-89f6733438c8"), 5, "Remove featured artists from song title when doing magic.", NodaTime.Instant.FromUnixTimeTicks(17389509828221072L), null, false, "magic.doRemoveFeaturingArtistFromSongTitle", null, null, 0, null, "true" },
                    { 504, new Guid("6b620ceb-d4ad-4070-8b76-ab73e7e7bb62"), 5, "Replace song artist separators with standard ID3 separator ('/') when doing magic.", NodaTime.Instant.FromUnixTimeTicks(17389509828221072L), null, false, "magic.doReplaceSongsArtistSeparators", null, null, 0, null, "true" },
                    { 505, new Guid("65c0dbcd-4d10-4533-94df-fdb6ae36f7a5"), 5, "Set the song year to current year if invalid or missing when doing magic.", NodaTime.Instant.FromUnixTimeTicks(17389509828221072L), null, false, "magic.doSetYearToCurrentIfInvalid", null, null, 0, null, "false" },
                    { 506, new Guid("897a25cd-632a-41fc-a3eb-2ceaeca4cec1"), 5, "Remove unwanted text from album title when doing magic.", NodaTime.Instant.FromUnixTimeTicks(17389509828221072L), null, false, "magic.doRemoveUnwantedTextFromAlbumTitle", null, null, 0, null, "true" },
                    { 507, new Guid("6deb25eb-8d36-4fc0-a90a-8a99029e693c"), 5, "Remove unwanted text from song titles when doing magic.", NodaTime.Instant.FromUnixTimeTicks(17389509828221072L), null, false, "magic.doRemoveUnwantedTextFromSongTitles", null, null, 0, null, "true" },
                    { 700, new Guid("349201e8-e463-4ba5-b9a7-19a92f9c3868"), 7, "Process of CueSheet files during processing.", NodaTime.Instant.FromUnixTimeTicks(17389509828221072L), null, false, "plugin.cueSheet.enabled", null, null, 0, null, "true" },
                    { 701, new Guid("878416b5-3a9e-4542-a7fe-d8753331169d"), 7, "Process of M3U files during processing.", NodaTime.Instant.FromUnixTimeTicks(17389509828221072L), null, false, "plugin.m3u.enabled", null, null, 0, null, "true" },
                    { 702, new Guid("e2f72825-6b45-4611-9ad9-518ab7fb470d"), 7, "Process of NFO files during processing.", NodaTime.Instant.FromUnixTimeTicks(17389509828221072L), null, false, "plugin.nfo.enabled", null, null, 0, null, "true" },
                    { 703, new Guid("af1e0baa-31bd-4c84-a300-2e57737edef5"), 7, "Process of Simple File Verification (SFV) files during processing.", NodaTime.Instant.FromUnixTimeTicks(17389509828221072L), null, false, "plugin.simpleFileVerification.enabled", null, null, 0, null, "true" },
                    { 704, new Guid("b173527c-c4b8-4c00-9b15-dfafde935226"), 7, "If true then all comments will be removed from media files.", NodaTime.Instant.FromUnixTimeTicks(17389509828221072L), null, false, "processing.doDeleteComments", null, null, 0, null, "true" },
                    { 902, new Guid("6634b45d-87e5-48ae-b691-ed542eddb109"), 9, "User agent to send with Search engine requests.", NodaTime.Instant.FromUnixTimeTicks(17389509828221072L), null, false, "searchEngine.userAgent", null, null, 0, null, "Mozilla/5.0 (X11; Linux x86_64; rv:131.0) Gecko/20100101 Firefox/131.0" },
                    { 903, new Guid("60428c65-f8d6-455e-801d-c9141d1df9ef"), 9, "Default page size when performing a search engine search.", NodaTime.Instant.FromUnixTimeTicks(17389509828221072L), null, false, "searchEngine.defaultPageSize", null, null, 0, null, "20" },
                    { 904, new Guid("dd369d22-134c-4767-bb11-b4985986222e"), 9, "Is MusicBrainz search engine enabled.", NodaTime.Instant.FromUnixTimeTicks(17389509828221072L), null, false, "searchEngine.musicbrainz.enabled", null, null, 0, null, "true" },
                    { 905, new Guid("b7c56026-d0f7-4e06-9ae0-78387101aeeb"), 9, "Storage path to hold MusicBrainz downloaded files and SQLite db.", NodaTime.Instant.FromUnixTimeTicks(17389509828221072L), null, false, "searchEngine.musicbrainz.storagePath", null, null, 0, null, "/melodee_test/search-engine-storage/musicbrainz/" },
                    { 906, new Guid("0a3ed2fc-436c-45fb-8211-2875fa143384"), 9, "Maximum number of batches import from MusicBrainz downloaded db dump (this setting is usually used during debugging), set to zero for unlimited.", NodaTime.Instant.FromUnixTimeTicks(17389509828221072L), null, false, "searchEngine.musicbrainz.importMaximumToProcess", null, null, 0, null, "0" },
                    { 907, new Guid("857b7c4b-bbe3-4d07-9b4d-119703f54cde"), 9, "Number of records to import from MusicBrainz downloaded db dump before commiting to local SQLite database.", NodaTime.Instant.FromUnixTimeTicks(17389509828221072L), null, false, "searchEngine.musicbrainz.importBatchSize", null, null, 0, null, "50000" },
                    { 908, new Guid("8c455571-6784-42c3-821d-057fbee9d39a"), 9, "Timestamp of when last MusicBrainz import was successful.", NodaTime.Instant.FromUnixTimeTicks(17389509828221072L), null, false, "searchEngine.musicbrainz.importLastImportTimestamp", null, null, 0, null, "" },
                    { 910, new Guid("97b08687-4113-42da-9628-8da969326c01"), 9, "Is Spotify search engine enabled.", NodaTime.Instant.FromUnixTimeTicks(17389509828221072L), null, false, "searchEngine.spotify.enabled", null, null, 0, null, "false" },
                    { 911, new Guid("c89affc1-5472-4468-8873-ef934053af7e"), 9, "ApiKey used used with Spotify. See https://developer.spotify.com/ for more details.", NodaTime.Instant.FromUnixTimeTicks(17389509828221072L), null, false, "searchEngine.spotify.apiKey", null, null, 0, null, "" },
                    { 912, new Guid("a7c9f2d1-89b2-48e6-aa81-693b3245c815"), 9, "Shared secret used with Spotify. See https://developer.spotify.com/ for more details.", NodaTime.Instant.FromUnixTimeTicks(17389509828221072L), null, false, "searchEngine.spotify.sharedSecret", null, null, 0, null, "" },
                    { 913, new Guid("9ec2c048-4f15-40f4-a7b3-8ec6ba920b34"), 9, "Token obtained from Spotify using the ApiKey and the Secret, this json contains expiry information.", NodaTime.Instant.FromUnixTimeTicks(17389509828221072L), null, false, "searchEngine.spotify.accessToken", null, null, 0, null, "" },
                    { 914, new Guid("0a29f3bd-8357-4a21-8be4-552594e0c1a1"), 9, "Is ITunes search engine enabled.", NodaTime.Instant.FromUnixTimeTicks(17389509828221072L), null, false, "searchEngine.itunes.enabled", null, null, 0, null, "true" },
                    { 915, new Guid("0af0055e-894d-4dd4-967f-883c3a03b52e"), 9, "Is LastFM search engine enabled.", NodaTime.Instant.FromUnixTimeTicks(17389509828221072L), null, false, "searchEngine.lastFm.Enabled", null, null, 0, null, "true" },
                    { 916, new Guid("d8397fb2-64c3-411e-9b6c-c1ce0ced6aff"), 9, "When performing a search engine search, the maximum allowed page size.", NodaTime.Instant.FromUnixTimeTicks(17389509828221072L), null, false, "searchEngine.maximumAllowedPageSize", null, null, 0, null, "1000" },
                    { 917, new Guid("785a5f8e-e2b5-448c-b2b8-356dc15e9fec"), 9, "Refresh albums for artists from search engine database every x days, set to zero to not refresh.", NodaTime.Instant.FromUnixTimeTicks(17389509828221072L), null, false, "searchEngine.artistSearchDatabaseRefreshInDays", null, null, 0, null, "14" },
                    { 1000, new Guid("1c48f25a-8984-40b4-81b5-016ca46c9765"), 10, "Is scrobbling enabled.", NodaTime.Instant.FromUnixTimeTicks(17389509828221072L), null, false, "scrobbling.enabled", null, null, 0, null, "false" },
                    { 1001, new Guid("c5559860-4a74-40d4-9bdb-911967a859b9"), 10, "Is scrobbling to Last.fm enabled.", NodaTime.Instant.FromUnixTimeTicks(17389509828221072L), null, false, "scrobbling.lastFm.Enabled", null, null, 0, null, "false" },
                    { 1002, new Guid("b7939c94-a308-4b55-8e85-0debeb25e873"), 10, "ApiKey used used with last FM. See https://www.last.fm/api/authentication for more details.", NodaTime.Instant.FromUnixTimeTicks(17389509828221072L), null, false, "scrobbling.lastFm.apiKey", null, null, 0, null, "" },
                    { 1003, new Guid("15c072e0-be1c-4250-a1b9-d71787b9a245"), 10, "Shared secret used with last FM. See https://www.last.fm/api/authentication for more details.", NodaTime.Instant.FromUnixTimeTicks(17389509828221072L), null, false, "scrobbling.lastFm.sharedSecret", null, null, 0, null, "" },
                    { 1100, new Guid("05a355c9-5f58-4010-a0e4-d56caf246ef3"), 11, "Base URL for Melodee to use when building shareable links and image urls (e.g., 'https://server.domain.com:8080', 'http://server.domain.com').", NodaTime.Instant.FromUnixTimeTicks(17389509828221072L), null, false, "system.baseUrl", null, null, 0, null, "** REQUIRED: THIS MUST BE EDITED **" },
                    { 1101, new Guid("a1995a05-52e5-4666-9af5-461eb0ed5770"), 11, "Is downloading enabled.", NodaTime.Instant.FromUnixTimeTicks(17389509828221072L), null, false, "system.isDownloadingEnabled", null, null, 0, null, "true" },
                    { 1200, new Guid("28a01915-8c1d-494b-ad53-a839e49c2402"), 12, "Default format for transcoding.", NodaTime.Instant.FromUnixTimeTicks(17389509828221072L), null, false, "transcoding.default", null, null, 0, null, "raw" },
                    { 1201, new Guid("567f1986-73be-4e50-89d8-436eafb5ffe2"), 12, "Default command to transcode MP3 for streaming.", NodaTime.Instant.FromUnixTimeTicks(17389509828221072L), null, false, "transcoding.command.mp3", null, null, 0, null, "{ 'format': 'Mp3', 'bitrate: 192, 'command': 'ffmpeg -i %s -ss %t -map 0:a:0 -b:a %bk -v 0 -f mp3 -' }" },
                    { 1202, new Guid("25bede6f-f75f-4c32-9063-0ab92a57f925"), 12, "Default command to transcode using libopus for streaming.", NodaTime.Instant.FromUnixTimeTicks(17389509828221072L), null, false, "transcoding.command.opus", null, null, 0, null, "{ 'format': 'Opus', 'bitrate: 128, 'command': 'ffmpeg -i %s -ss %t -map 0:a:0 -b:a %bk -v 0 -c:a libopus -f opus -' }" },
                    { 1203, new Guid("426a925d-03d6-4222-99dc-da4438f25af5"), 12, "Default command to transcode to aac for streaming.", NodaTime.Instant.FromUnixTimeTicks(17389509828221072L), null, false, "transcoding.command.aac", null, null, 0, null, "{ 'format': 'Aac', 'bitrate: 256, 'command': 'ffmpeg -i %s -ss %t -map 0:a:0 -b:a %bk -v 0 -c:a aac -f adts -' }" },
                    { 1300, new Guid("7dc84cc0-bbc8-48b2-9f89-a526bde71526"), 13, "The maximum value a song number can have for an album.", NodaTime.Instant.FromUnixTimeTicks(17389509828221072L), null, false, "validation.maximumSongNumber", null, null, 0, null, "9999" },
                    { 1301, new Guid("88335f2c-b255-4847-a916-5701246e0262"), 13, "Minimum allowed year for an album.", NodaTime.Instant.FromUnixTimeTicks(17389509828221072L), null, false, "validation.minimumAlbumYear", null, null, 0, null, "1860" },
                    { 1302, new Guid("1889f347-d475-4b2f-bfbc-723380fb86ef"), 13, "Maximum allowed year for an album.", NodaTime.Instant.FromUnixTimeTicks(17389509828221072L), null, false, "validation.maximumAlbumYear", null, null, 0, null, "2150" },
                    { 1400, new Guid("f4110a7c-d6a0-4964-b856-7bb8aa486a63"), 14, "Cron expression to run the artist housekeeping job, set empty to disable. Default of '0 0 0/1 1/1 * ? *' will run every hour. See https://www.freeformatter.com/cron-expression-generator-quartz.html", NodaTime.Instant.FromUnixTimeTicks(17389509828221072L), null, false, "jobs.artistHousekeeping.cronExpression", null, null, 0, null, "0 0 0/1 1/1 * ? *" },
                    { 1401, new Guid("cb3333cb-646e-4dbc-9000-c423d0f9951f"), 14, "Cron expression to run the library process job, set empty to disable. Default of '0 */10 * ? * *' Every 10 minutes. See https://www.freeformatter.com/cron-expression-generator-quartz.html", NodaTime.Instant.FromUnixTimeTicks(17389509828221072L), null, false, "jobs.libraryProcess.cronExpression", null, null, 0, null, "0 */10 * ? * *" },
                    { 1402, new Guid("6568c5ea-9180-45b0-a042-b08dec20fc9d"), 14, "Cron expression to run the library scan job, set empty to disable. Default of '0 0 0 * * ?' will run every day at 00:00. See https://www.freeformatter.com/cron-expression-generator-quartz.html", NodaTime.Instant.FromUnixTimeTicks(17389509828221072L), null, false, "jobs.libraryInsert.cronExpression", null, null, 0, null, "0 0 0 * * ?" },
                    { 1403, new Guid("f610a0e9-0b0f-41a2-a498-9db992f53eb7"), 14, "Cron expression to run the musicbrainz database house keeping job, set empty to disable. Default of '0 0 12 1 * ?' will run first day of the month. See https://www.freeformatter.com/cron-expression-generator-quartz.html", NodaTime.Instant.FromUnixTimeTicks(17389509828221072L), null, false, "jobs.musicbrainzUpdateDatabase.cronExpression", null, null, 0, null, "0 0 12 1 * ?" },
                    { 1404, new Guid("996b7593-c42a-4424-81bc-6b4af9ff671a"), 14, "Cron expression to run the artist search engine house keeping job, set empty to disable. Default of '0 0 0 * * ?' will run every day at 00:00. See https://www.freeformatter.com/cron-expression-generator-quartz.html", NodaTime.Instant.FromUnixTimeTicks(17389509828221072L), null, false, "jobs.artistSearchEngineHousekeeping.cronExpression", null, null, 0, null, "0 0 0 * * ?" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Albums_ApiKey",
                table: "Albums",
                column: "ApiKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Albums_ArtistId_Name",
                table: "Albums",
                columns: new[] { "ArtistId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Albums_ArtistId_NameNormalized",
                table: "Albums",
                columns: new[] { "ArtistId", "NameNormalized" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Albums_ArtistId_SortName",
                table: "Albums",
                columns: new[] { "ArtistId", "SortName" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Albums_MusicBrainzId",
                table: "Albums",
                column: "MusicBrainzId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ArtistRelation_ApiKey",
                table: "ArtistRelation",
                column: "ApiKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ArtistRelation_ArtistId_RelatedArtistId",
                table: "ArtistRelation",
                columns: new[] { "ArtistId", "RelatedArtistId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ArtistRelation_RelatedArtistId",
                table: "ArtistRelation",
                column: "RelatedArtistId");

            migrationBuilder.CreateIndex(
                name: "IX_Artists_ApiKey",
                table: "Artists",
                column: "ApiKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Artists_LibraryId",
                table: "Artists",
                column: "LibraryId");

            migrationBuilder.CreateIndex(
                name: "IX_Artists_MusicBrainzId",
                table: "Artists",
                column: "MusicBrainzId",
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
                name: "IX_Bookmarks_MusicBrainzId",
                table: "Bookmarks",
                column: "MusicBrainzId",
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
                name: "IX_Contributors_AlbumId",
                table: "Contributors",
                column: "AlbumId");

            migrationBuilder.CreateIndex(
                name: "IX_Contributors_ApiKey",
                table: "Contributors",
                column: "ApiKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Contributors_ArtistId_MetaTagIdentifier_AlbumId",
                table: "Contributors",
                columns: new[] { "ArtistId", "MetaTagIdentifier", "AlbumId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Contributors_ContributorName_MetaTagIdentifier_AlbumId",
                table: "Contributors",
                columns: new[] { "ContributorName", "MetaTagIdentifier", "AlbumId" },
                unique: true);

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
                name: "IX_Songs_AlbumId_SongNumber",
                table: "Songs",
                columns: new[] { "AlbumId", "SongNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Songs_ApiKey",
                table: "Songs",
                column: "ApiKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Songs_MusicBrainzId",
                table: "Songs",
                column: "MusicBrainzId",
                unique: true);

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
                name: "ArtistRelation");

            migrationBuilder.DropTable(
                name: "Bookmarks");

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
                name: "SearchHistories");

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
                name: "Albums");

            migrationBuilder.DropTable(
                name: "Artists");

            migrationBuilder.DropTable(
                name: "Libraries");
        }
    }
}
