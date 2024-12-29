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
                    LastFmSessionKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
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
                    DiscCount = table.Column<short>(type: "smallint", nullable: true),
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
                    LastPlayedAt = table.Column<Instant>(type: "timestamp with time zone", nullable: true),
                    IsStarred = table.Column<bool>(type: "boolean", nullable: false),
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
                name: "Songs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AlbumDiscId = table.Column<int>(type: "integer", nullable: false),
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
                        name: "FK_Songs_AlbumDiscs_AlbumDiscId",
                        column: x => x.AlbumDiscId,
                        principalTable: "AlbumDiscs",
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
                    { 1, null, new Guid("a5b51652-3c5b-454b-9b01-1ba77dcf5aa8"), null, NodaTime.Instant.FromUnixTimeTicks(17354427900482877L), "Files in this directory are scanned and Album information is gathered via processing.", false, null, null, "Inbound", null, "/storage/inbound/", null, 0, null, 1 },
                    { 2, null, new Guid("3dcc06e7-69fd-47a0-a6ce-175f5fc35133"), null, NodaTime.Instant.FromUnixTimeTicks(17354427900482877L), "The staging directory to place processed files into (Inbound -> Staging -> Library).", false, null, null, "Staging", null, "/storage/staging/", null, 0, null, 2 },
                    { 3, null, new Guid("1cf5a952-470a-468a-874e-bf9ec1bfdcc2"), null, NodaTime.Instant.FromUnixTimeTicks(17354427900482877L), "The library directory to place processed, reviewed and ready to use music files into.", false, null, null, "Library", null, "/storage/library/", null, 0, null, 3 },
                    { 4, null, new Guid("e71bd154-1ed3-471b-81fa-36130a8ad585"), null, NodaTime.Instant.FromUnixTimeTicks(17354427900482877L), "Library where user images are stored.", false, null, null, "User Images", null, "/storage/images/users/", null, 0, null, 4 }
                });

            migrationBuilder.InsertData(
                table: "Settings",
                columns: new[] { "Id", "ApiKey", "Category", "Comment", "CreatedAt", "Description", "IsLocked", "Key", "LastUpdatedAt", "Notes", "SortOrder", "Tags", "Value" },
                values: new object[,]
                {
                    { 1, new Guid("a0616056-f8d5-438f-b362-545d26ab5b11"), null, "Add a default filter to show only albums with this or less number of songs.", NodaTime.Instant.FromUnixTimeTicks(17354427900482877L), null, false, "filtering.lessThanSongCount", null, null, 0, null, "3" },
                    { 2, new Guid("5e76c1bd-ac79-49bb-84db-5224735b7737"), null, "Add a default filter to show only albums with this or less duration.", NodaTime.Instant.FromUnixTimeTicks(17354427900482877L), null, false, "filtering.lessThanDuration", null, null, 0, null, "720000" },
                    { 3, new Guid("bcc3bc19-89d8-4b19-8fc6-86737c14cb2a"), null, "Maximum number of albums to scan when processing inbound directory.", NodaTime.Instant.FromUnixTimeTicks(17354427900482877L), null, false, "processing.stagingDirectoryScanLimit", null, null, 0, null, "250" },
                    { 4, new Guid("35b73c95-c081-41bf-9039-b0efaaf2be78"), null, "Default page size when view including pagination.", NodaTime.Instant.FromUnixTimeTicks(17354427900482877L), null, false, "defaults.pagesize", null, null, 0, null, "100" },
                    { 6, new Guid("66fa0029-80e6-47d3-a1ef-fa46bccb052f"), null, "Amount of time to display a Toast then auto-close (in milliseconds.)", NodaTime.Instant.FromUnixTimeTicks(17354427900482877L), null, false, "userinterface.toastAutoCloseTime", null, null, 0, null, "2000" },
                    { 9, new Guid("e4975c5e-0dc2-472b-b94c-0359c8337924"), null, "List of ignored articles when scanning media (pipe delimited).", NodaTime.Instant.FromUnixTimeTicks(17354427900482877L), null, false, "processing.ignoredArticles", null, null, 0, null, "THE|EL|LA|LOS|LAS|LE|LES|OS|AS|O|A" },
                    { 26, new Guid("ea23613c-0b37-4e3c-b0f9-22d2770a4897"), null, "Fragments of artist names to replace (JSON Dictionary).", NodaTime.Instant.FromUnixTimeTicks(17354427900482877L), null, false, "processing.artistNameReplacements", null, null, 0, null, "{'AC/DC': ['AC; DC', 'AC;DC', 'AC/ DC', 'AC DC'] , 'Love/Hate': ['Love; Hate', 'Love;Hate', 'Love/ Hate', 'Love Hate'] }" },
                    { 27, new Guid("68523c46-4fba-43f1-a7b9-14a43bd6a610"), null, "If OrigAlbumYear [TOR, TORY, TDOR] value is invalid use current year.", NodaTime.Instant.FromUnixTimeTicks(17354427900482877L), null, false, "processing.doUseCurrentYearAsDefaultOrigAlbumYearValue", null, null, 0, null, "true" },
                    { 28, new Guid("2a8208c6-6e3b-44af-85e5-7661237eabfd"), null, "Delete original files when processing. When false a copy if made, else original is deleted after processed.", NodaTime.Instant.FromUnixTimeTicks(17354427900482877L), null, false, "processing.doDeleteOriginal", null, null, 0, null, "false" },
                    { 29, new Guid("1a65d00d-a5a2-483a-8908-4ea70d689f75"), null, "Extension to add to file when converted, leave blank to disable.", NodaTime.Instant.FromUnixTimeTicks(17354427900482877L), null, false, "processing.convertedExtension", null, null, 0, null, "_converted" },
                    { 30, new Guid("275d567e-17a3-4cc6-b82e-4a19a834694f"), null, "Extension to add to file when processed, leave blank to disable.", NodaTime.Instant.FromUnixTimeTicks(17354427900482877L), null, false, "processing.processedExtension", null, null, 0, null, "_processed" },
                    { 31, new Guid("275395d3-025f-4a36-8765-aa5eb691eb6b"), null, "Extension to add to file to indicate other files in the same category where processed and this file was skipped during processing, leave blank to disable.", NodaTime.Instant.FromUnixTimeTicks(17354427900482877L), null, false, "processing.skippedExtension", null, null, 0, null, "_skipped" },
                    { 32, new Guid("0539c670-9542-4748-9dc9-e8941e911626"), null, "When processing over write any existing Melodee data files, otherwise skip and leave in place.", NodaTime.Instant.FromUnixTimeTicks(17354427900482877L), null, false, "processing.doOverrideExistingMelodeeDataFiles", null, null, 0, null, "true" },
                    { 34, new Guid("f4e2914a-6fbe-41bc-b24a-237b60c9c017"), null, "The maximum number of files to process, set to zero for unlimited.", NodaTime.Instant.FromUnixTimeTicks(17354427900482877L), null, false, "processing.maximumProcessingCount", null, null, 0, null, "0" },
                    { 35, new Guid("bd635f27-7405-427c-8077-9ac000eb144a"), null, "Maximum allowed length of album directory name.", NodaTime.Instant.FromUnixTimeTicks(17354427900482877L), null, false, "processing.maximumAlbumDirectoryNameLength", null, null, 0, null, "255" },
                    { 36, new Guid("5812b4ee-d378-45b1-9609-f34c153d4faf"), null, "Maximum allowed length of artist directory name.", NodaTime.Instant.FromUnixTimeTicks(17354427900482877L), null, false, "processing.maximumArtistDirectoryNameLength", null, null, 0, null, "255" },
                    { 37, new Guid("9cd7c096-87cd-48e9-ba14-65413acafa24"), null, "Fragments to remove from album titles (JSON array).", NodaTime.Instant.FromUnixTimeTicks(17354427900482877L), null, false, "processing.albumTitleRemovals", null, null, 0, null, "['^', '~', '#']" },
                    { 38, new Guid("34d65689-5e73-4bd9-8d2a-169170e51ca5"), null, "Fragments to remove from song titles (JSON array).", NodaTime.Instant.FromUnixTimeTicks(17354427900482877L), null, false, "processing.songTitleRemovals", null, null, 0, null, "[';', '(Remaster)', 'Remaster']" },
                    { 39, new Guid("406bc688-3c94-4680-8350-419341143f7c"), null, "Continue processing if an error is encountered.", NodaTime.Instant.FromUnixTimeTicks(17354427900482877L), null, false, "processing.doContinueOnDirectoryProcessingErrors", null, null, 0, null, "true" },
                    { 41, new Guid("1f6bd2b6-ae24-4c69-82e7-cd41606d0717"), null, "Is scripting enabled.", NodaTime.Instant.FromUnixTimeTicks(17354427900482877L), null, false, "scripting.enabled", null, null, 0, null, "false" },
                    { 42, new Guid("6ec2ad03-64f3-4b19-8d11-273cb7fe69af"), null, "Script to run before processing the inbound directory, leave blank to disable.", NodaTime.Instant.FromUnixTimeTicks(17354427900482877L), null, false, "scripting.preDiscoveryScript", null, null, 0, null, "" },
                    { 43, new Guid("c830b994-de88-4b8d-bf81-04ac1f1eabb7"), null, "Script to run after processing the inbound directory, leave blank to disable.", NodaTime.Instant.FromUnixTimeTicks(17354427900482877L), null, false, "scripting.postDiscoveryScript", null, null, 0, null, "" },
                    { 44, new Guid("1677d817-3b4a-4244-9774-5ac6b260b96f"), 13, "The maximum value a media number can have for an album. The length of this is used for formatting song names.", NodaTime.Instant.FromUnixTimeTicks(17354427900482877L), null, false, "validation.maximumMediaNumber", null, null, 0, null, "999" },
                    { 45, new Guid("4419651a-364d-4983-ade1-6314b375bacd"), null, "Don't create performer contributors for these performer names.", NodaTime.Instant.FromUnixTimeTicks(17354427900482877L), null, false, "processing.ignoredPerformers", null, null, 0, null, "" },
                    { 46, new Guid("b93e6d5b-aeb3-48ff-b736-9937e4ec7549"), null, "Don't create production contributors for these production names.", NodaTime.Instant.FromUnixTimeTicks(17354427900482877L), null, false, "processing.ignoredProduction", null, null, 0, null, "['www.t.me;pmedia_music']" },
                    { 47, new Guid("57035470-97ff-4ff8-86fb-d5a15f73851c"), null, "Don't create publisher contributors for these artist names.", NodaTime.Instant.FromUnixTimeTicks(17354427900482877L), null, false, "processing.ignoredPublishers", null, null, 0, null, "['P.M.E.D.I.A','PMEDIA','PMEDIA GROUP']" },
                    { 48, new Guid("b6e14f11-debf-4496-8ef8-e21be3efc716"), null, "Prefix to apply to directories to skip processing. This is also used then a directory throws an error attempting to be processed, to prevent future processing.", NodaTime.Instant.FromUnixTimeTicks(17354427900482877L), null, false, "processing.skippedDirectoryPrefix", null, null, 0, null, "_skip_ " },
                    { 49, new Guid("7d6ce1d0-ecb1-4289-85ba-76458c683676"), null, "Private key used to encrypt/decrypt passwords for Subsonic authentication. Use https://generate-random.org/encryption-key-generator?count=1&bytes=32&cipher=aes-256-cbc&string=&password= to generate a new key.", NodaTime.Instant.FromUnixTimeTicks(17354427900482877L), null, false, "encryption.privateKey", null, null, 0, null, "H+Kiik6VMKfTD2MesF1GoMjczTrD5RhuKckJ5+/UQWOdWajGcsEC3yEnlJ5eoy8Y" },
                    { 50, new Guid("c9e72248-ae6b-4c2c-857f-b172b7cce0bd"), null, "Prefix to apply to indicate an album directory is a duplicate album for an artist. If left blank the default of '__duplicate_' will be used.", NodaTime.Instant.FromUnixTimeTicks(17354427900482877L), null, false, "processing.duplicateAlbumPrefix", null, null, 0, null, "__duplicate_ " },
                    { 53, new Guid("5f9f1646-1abc-4152-bede-214a9aaf27a4"), null, "Processing batching size. Allowed range is between [250] and [1000]. ", NodaTime.Instant.FromUnixTimeTicks(17354427900482877L), null, false, "defaults.batchSize", null, null, 0, null, "250" },
                    { 100, new Guid("e60e5734-548e-43c4-ae72-5b0b6c98ecf7"), 1, "OpenSubsonic server supported Subsonic API version.", NodaTime.Instant.FromUnixTimeTicks(17354427900482877L), null, false, "openSubsonicServer.openSubsonic.serverSupportedVersion", null, null, 0, null, "1.16.1" },
                    { 101, new Guid("5362c9db-adaf-412b-8171-9417f6f83d5e"), 1, "OpenSubsonic server name.", NodaTime.Instant.FromUnixTimeTicks(17354427900482877L), null, false, "openSubsonicServer.openSubsonicServer.type", null, null, 0, null, "Melodee" },
                    { 102, new Guid("6f9baf7d-4726-436e-9c1d-3f077b246478"), 1, "OpenSubsonic server actual version. [Ex: 1.2.3 (beta)]", NodaTime.Instant.FromUnixTimeTicks(17354427900482877L), null, false, "openSubsonicServer.openSubsonicServer.version", null, null, 0, null, "1.0.1 (beta)" },
                    { 103, new Guid("85dac714-92bf-4dd9-a4a9-f12a1c07b4bf"), 1, "OpenSubsonic email to use in License responses.", NodaTime.Instant.FromUnixTimeTicks(17354427900482877L), null, false, "openSubsonicServer.openSubsonicServerLicenseEmail", null, null, 0, null, "noreply@localhost.lan" },
                    { 104, new Guid("5a9716b8-328d-4b62-9e12-175cc3fc042e"), 1, "Limit the number of artists to include in an indexes request, set to zero for 32k per index (really not recommended with tens of thousands of artists and mobile clients timeout downloading indexes, a user can find an artist by search)", NodaTime.Instant.FromUnixTimeTicks(17354427900482877L), null, false, "openSubsonicServer.openSubsonicServer.index.artistLimit", null, null, 0, null, "1000" },
                    { 200, new Guid("2cab51a5-4694-4fd1-b5bd-dda96fa6a10d"), 2, "Enable Melodee to convert non-mp3 media files during processing.", NodaTime.Instant.FromUnixTimeTicks(17354427900482877L), null, false, "conversion.enabled", null, null, 0, null, "true" },
                    { 201, new Guid("e13de816-4a61-4272-8b41-19acd68eb082"), 2, "Bitrate to convert non-mp3 media files during processing.", NodaTime.Instant.FromUnixTimeTicks(17354427900482877L), null, false, "conversion.bitrate", null, null, 0, null, "384" },
                    { 202, new Guid("0f55f71b-ef31-41c9-9a2b-0e3565078bec"), 2, "Vbr to convert non-mp3 media files during processing.", NodaTime.Instant.FromUnixTimeTicks(17354427900482877L), null, false, "conversion.vbrLevel", null, null, 0, null, "4" },
                    { 203, new Guid("b2328b25-fe42-4af3-8bc7-58d6cd795c19"), 2, "Sampling rate to convert non-mp3 media files during processing.", NodaTime.Instant.FromUnixTimeTicks(17354427900482877L), null, false, "conversion.samplingRate", null, null, 0, null, "48000" },
                    { 300, new Guid("88bc33f7-0768-4abc-9dd5-253029552895"), 3, "Short Format to use when displaying full dates.", NodaTime.Instant.FromUnixTimeTicks(17354427900482877L), null, false, "formatting.dateTimeDisplayFormatShort", null, null, 0, null, "yyyyMMdd HH\\:mm" },
                    { 301, new Guid("9baf43dc-bdc0-4954-9d1c-a448dd570902"), 3, "Format to use when displaying activity related dates (e.g., processing messages)", NodaTime.Instant.FromUnixTimeTicks(17354427900482877L), null, false, "formatting.dateTimeDisplayActivityFormat", null, null, 0, null, "hh\\:mm\\:ss\\.ffff" },
                    { 400, new Guid("12e95699-7491-41fb-a8e9-8ab8ec3a80bc"), 4, "Include any embedded images from media files into the Melodee data file.", NodaTime.Instant.FromUnixTimeTicks(17354427900482877L), null, false, "imaging.doLoadEmbeddedImages", null, null, 0, null, "true" },
                    { 401, new Guid("55695d5e-f1e9-475e-a383-e28549deb1a2"), 4, "Small image size (square image, this is both width and height).", NodaTime.Instant.FromUnixTimeTicks(17354427900482877L), null, false, "imaging.smallSize", null, null, 0, null, "300" },
                    { 402, new Guid("4e12c794-38fb-465f-b0d3-dc05e80f1305"), 4, "Medium image size (square image, this is both width and height).", NodaTime.Instant.FromUnixTimeTicks(17354427900482877L), null, false, "imaging.mediumSize", null, null, 0, null, "600" },
                    { 403, new Guid("caf28a4f-cb28-411b-986f-1444a22f8630"), 4, "Large image size (square image, this is both width and height), if larger than will be resized to this image, leave blank to disable.", NodaTime.Instant.FromUnixTimeTicks(17354427900482877L), null, false, "imaging.largeSize", null, null, 0, null, "1600" },
                    { 404, new Guid("7938b7f5-9482-4b88-a358-cbc55d3b7e50"), 4, "Maximum allowed number of images for an album, this includes all image types (Front, Rear, etc.), set to zero for unlimited.", NodaTime.Instant.FromUnixTimeTicks(17354427900482877L), null, false, "imaging.maximumNumberOfAlbumImages", null, null, 0, null, "25" },
                    { 405, new Guid("37b3225b-cfe2-4c40-9196-9847e9d6e161"), 4, "Maximum allowed number of images for an artist, set to zero for unlimited.", NodaTime.Instant.FromUnixTimeTicks(17354427900482877L), null, false, "imaging.maximumNumberOfArtistImages", null, null, 0, null, "25" },
                    { 406, new Guid("e39cbb70-97ac-4ff5-a328-bb08768cde80"), 4, "Images under this size are considered invalid, set to zero to disable.", NodaTime.Instant.FromUnixTimeTicks(17354427900482877L), null, false, "imaging.minimumImageSize", null, null, 0, null, "300" },
                    { 500, new Guid("d75ab2e8-1795-4a66-9259-8f964dd1078a"), 5, "Is Magic processing enabled.", NodaTime.Instant.FromUnixTimeTicks(17354427900482877L), null, false, "magic.enabled", null, null, 0, null, "true" },
                    { 501, new Guid("a83f1d88-1cf8-4cd1-8227-72a764989bd7"), 5, "Renumber songs when doing magic processing.", NodaTime.Instant.FromUnixTimeTicks(17354427900482877L), null, false, "magic.doRenumberSongs", null, null, 0, null, "true" },
                    { 502, new Guid("8c114059-9908-40c3-bf05-34030c310546"), 5, "Remove featured artists from song artist when doing magic.", NodaTime.Instant.FromUnixTimeTicks(17354427900482877L), null, false, "magic.doRemoveFeaturingArtistFromSongArtist", null, null, 0, null, "true" },
                    { 503, new Guid("0a39e43e-bbfb-484f-a2f5-ff07212d73fc"), 5, "Remove featured artists from song title when doing magic.", NodaTime.Instant.FromUnixTimeTicks(17354427900482877L), null, false, "magic.doRemoveFeaturingArtistFromSongTitle", null, null, 0, null, "true" },
                    { 504, new Guid("cc75e2a7-cf9c-42ff-a535-6b7e8427cfda"), 5, "Replace song artist separators with standard ID3 separator ('/') when doing magic.", NodaTime.Instant.FromUnixTimeTicks(17354427900482877L), null, false, "magic.doReplaceSongsArtistSeparators", null, null, 0, null, "true" },
                    { 505, new Guid("a8245e32-1582-4ac9-aca7-aea28b964b5b"), 5, "Set the song year to current year if invalid or missing when doing magic.", NodaTime.Instant.FromUnixTimeTicks(17354427900482877L), null, false, "magic.doSetYearToCurrentIfInvalid", null, null, 0, null, "true" },
                    { 506, new Guid("3e2e5a07-3bb4-42fd-a522-06b71145684e"), 5, "Remove unwanted text from album title when doing magic.", NodaTime.Instant.FromUnixTimeTicks(17354427900482877L), null, false, "magic.doRemoveUnwantedTextFromAlbumTitle", null, null, 0, null, "true" },
                    { 507, new Guid("dd14aa54-dc4a-47b7-8deb-2cb4fd5cf4b6"), 5, "Remove unwanted text from song titles when doing magic.", NodaTime.Instant.FromUnixTimeTicks(17354427900482877L), null, false, "magic.doRemoveUnwantedTextFromSongTitles", null, null, 0, null, "true" },
                    { 700, new Guid("3187bc18-7f84-4bb1-823c-d7b2e0eb8131"), 7, "Process of CueSheet files during processing.", NodaTime.Instant.FromUnixTimeTicks(17354427900482877L), null, false, "plugin.cueSheet.enabled", null, null, 0, null, "true" },
                    { 701, new Guid("33098613-4a65-4ea8-9e24-65f85ba7239c"), 7, "Process of M3U files during processing.", NodaTime.Instant.FromUnixTimeTicks(17354427900482877L), null, false, "plugin.m3u.enabled", null, null, 0, null, "true" },
                    { 702, new Guid("ffe68452-346b-4fd6-8338-463f88636408"), 7, "Process of NFO files during processing.", NodaTime.Instant.FromUnixTimeTicks(17354427900482877L), null, false, "plugin.nfo.enabled", null, null, 0, null, "true" },
                    { 703, new Guid("96490670-44a0-46e1-9d24-8a3a63924ddb"), 7, "Process of Simple File Verification (SFV) files during processing.", NodaTime.Instant.FromUnixTimeTicks(17354427900482877L), null, false, "plugin.simpleFileVerification.enabled", null, null, 0, null, "true" },
                    { 704, new Guid("baebc37a-ce5c-41bb-b3e2-66909b2862e3"), 7, "If true then all comments will be removed from media files.", NodaTime.Instant.FromUnixTimeTicks(17354427900482877L), null, false, "processing.doDeleteComments", null, null, 0, null, "true" },
                    { 902, new Guid("1eb7e8c8-6389-43c6-8ec4-b8786a2c28ea"), 9, "User agent to send with Search engine requests.", NodaTime.Instant.FromUnixTimeTicks(17354427900482877L), null, false, "searchEngine.userAgent", null, null, 0, null, "Mozilla/5.0 (X11; Linux x86_64; rv:131.0) Gecko/20100101 Firefox/131.0" },
                    { 903, new Guid("c29a9ae8-a4af-45db-a39a-617b6e1a94db"), 9, "Default page size when performing a search engine search.", NodaTime.Instant.FromUnixTimeTicks(17354427900482877L), null, false, "searchEngine.defaultPageSize", null, null, 0, null, "20" },
                    { 904, new Guid("a06968bc-4082-4201-ad68-ffba6f5d2fbf"), 9, "Is MusicBrainz search engine enabled.", NodaTime.Instant.FromUnixTimeTicks(17354427900482877L), null, false, "searchEngine.musicbrainz.enabled", null, null, 0, null, "true" },
                    { 905, new Guid("730ef646-856d-4a64-8b61-1a0f59162cad"), 9, "Storage path to hold MusicBrainz downloaded files and SQLite db.", NodaTime.Instant.FromUnixTimeTicks(17354427900482877L), null, false, "searchEngine.musicbrainz.storagePath", null, null, 0, null, "/melodee_test/search-engine-storage/musicbrainz/" },
                    { 906, new Guid("19726a53-fdf7-4b54-b929-7b6e117aed11"), 9, "Maximum number of batches import from MusicBrainz downloaded db dump (this setting is usually used during debugging), set to zero for unlimited.", NodaTime.Instant.FromUnixTimeTicks(17354427900482877L), null, false, "searchEngine.musicbrainz.importMaximumToProcess", null, null, 0, null, "0" },
                    { 907, new Guid("cb61d6ed-870f-4f9b-9344-289e8178a5dd"), 9, "Number of records to import from MusicBrainz downloaded db dump before commiting to local SQLite database.", NodaTime.Instant.FromUnixTimeTicks(17354427900482877L), null, false, "searchEngine.musicbrainz.importBatchSize", null, null, 0, null, "50000" },
                    { 908, new Guid("8cb6a244-096a-4dfd-acff-d1036dacd9a2"), 9, "Timestamp of when last MusicBrainz import was successful.", NodaTime.Instant.FromUnixTimeTicks(17354427900482877L), null, false, "searchEngine.musicbrainz.importLastImportTimestamp", null, null, 0, null, "" },
                    { 910, new Guid("37fd89f5-53b3-4e1a-a76a-200e32dfe406"), 9, "Is Spotify search engine enabled.", NodaTime.Instant.FromUnixTimeTicks(17354427900482877L), null, false, "searchEngine.spotify.enabled", null, null, 0, null, "false" },
                    { 911, new Guid("5df86b98-624f-42bf-b954-d8b56b2c6769"), 9, "ApiKey used used with Spotify. See https://developer.spotify.com/ for more details.", NodaTime.Instant.FromUnixTimeTicks(17354427900482877L), null, false, "searchEngine.spotify.apiKey", null, null, 0, null, "" },
                    { 912, new Guid("b640199f-a1ff-494c-bbad-d482859e9543"), 9, "Shared secret used with Spotify. See https://developer.spotify.com/ for more details.", NodaTime.Instant.FromUnixTimeTicks(17354427900482877L), null, false, "searchEngine.spotify.sharedSecret", null, null, 0, null, "" },
                    { 913, new Guid("5c0eb3dc-69d4-4e0c-ae16-4aecb6bf9181"), 9, "Token obtained from Spotify using the ApiKey and the Secret, this json contains expiry information.", NodaTime.Instant.FromUnixTimeTicks(17354427900482877L), null, false, "searchEngine.spotify.accessToken", null, null, 0, null, "" },
                    { 1000, new Guid("d0f82ecf-0a85-468a-ad39-49de1895f51a"), 10, "Is scrobbling enabled.", NodaTime.Instant.FromUnixTimeTicks(17354427900482877L), null, false, "scrobbling.enabled", null, null, 0, null, "false" },
                    { 1001, new Guid("82a9367b-07d0-4bbc-848c-533d64e63b16"), 10, "Is scrobbling to Last.fm enabled.", NodaTime.Instant.FromUnixTimeTicks(17354427900482877L), null, false, "scrobbling.lastFm.Enabled", null, null, 0, null, "false" },
                    { 1002, new Guid("52627912-8627-4795-8b94-50f338ba345d"), 10, "ApiKey used used with last FM. See https://www.last.fm/api/authentication for more details.", NodaTime.Instant.FromUnixTimeTicks(17354427900482877L), null, false, "scrobbling.lastFm.apiKey", null, null, 0, null, "" },
                    { 1003, new Guid("701b5f9c-7274-430e-ba15-0436177ed3cb"), 10, "Shared secret used with last FM. See https://www.last.fm/api/authentication for more details.", NodaTime.Instant.FromUnixTimeTicks(17354427900482877L), null, false, "scrobbling.lastFm.sharedSecret", null, null, 0, null, "" },
                    { 1100, new Guid("00d18557-a183-4e22-b438-dcdaabe8863a"), 11, "Base URL for Melodee to use when building shareable links and image urls (e.g., 'https://server.domain.com:8080', 'http://server.domain.com').", NodaTime.Instant.FromUnixTimeTicks(17354427900482877L), null, false, "system.baseUrl", null, null, 0, null, "** REQUIRED: THIS MUST BE EDITED **" },
                    { 1101, new Guid("99aae6c8-f739-41e5-8c10-9a485fd1ea9f"), 11, "Is downloading enabled.", NodaTime.Instant.FromUnixTimeTicks(17354427900482877L), null, false, "system.isDownloadingEnabled", null, null, 0, null, "true" },
                    { 1200, new Guid("bdb131d4-17cb-4fbd-993b-029cf60f068f"), 12, "Default format for transcoding.", NodaTime.Instant.FromUnixTimeTicks(17354427900482877L), null, false, "transcoding.default", null, null, 0, null, "raw" },
                    { 1201, new Guid("6a2ada84-c749-4d42-8137-a435466f3a43"), 12, "Default command to transcode MP3 for streaming.", NodaTime.Instant.FromUnixTimeTicks(17354427900482877L), null, false, "transcoding.command.mp3", null, null, 0, null, "{ 'format': 'Mp3', 'bitrate: 192, 'command': 'ffmpeg -i %s -ss %t -map 0:a:0 -b:a %bk -v 0 -f mp3 -' }" },
                    { 1202, new Guid("10f5e155-ed2d-4352-87d2-cdc0fc5a433a"), 12, "Default command to transcode using libopus for streaming.", NodaTime.Instant.FromUnixTimeTicks(17354427900482877L), null, false, "transcoding.command.opus", null, null, 0, null, "{ 'format': 'Opus', 'bitrate: 128, 'command': 'ffmpeg -i %s -ss %t -map 0:a:0 -b:a %bk -v 0 -c:a libopus -f opus -' }" },
                    { 1203, new Guid("d842636a-3e2b-419b-b4f5-dbd4031759cf"), 12, "Default command to transcode to aac for streaming.", NodaTime.Instant.FromUnixTimeTicks(17354427900482877L), null, false, "transcoding.command.aac", null, null, 0, null, "{ 'format': 'Aac', 'bitrate: 256, 'command': 'ffmpeg -i %s -ss %t -map 0:a:0 -b:a %bk -v 0 -c:a aac -f adts -' }" },
                    { 1300, new Guid("3ece6ffb-0b9f-4c8b-9037-bd0daf30c3c0"), 13, "The maximum value a song number can have for an album. The length of this is used for formatting song names.", NodaTime.Instant.FromUnixTimeTicks(17354427900482877L), null, false, "validation.maximumSongNumber", null, null, 0, null, "9999" },
                    { 1301, new Guid("9ab96a41-dbd5-4e0b-b220-e9e59b7af5aa"), 13, "Minimum allowed year for an album.", NodaTime.Instant.FromUnixTimeTicks(17354427900482877L), null, false, "validation.minimumAlbumYear", null, null, 0, null, "1860" },
                    { 1302, new Guid("c599d922-ba77-4cdf-8b2c-a53bb506b953"), 13, "Maximum allowed year for an album.", NodaTime.Instant.FromUnixTimeTicks(17354427900482877L), null, false, "validation.maximumAlbumYear", null, null, 0, null, "2150" }
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
                name: "Albums");

            migrationBuilder.DropTable(
                name: "Artists");

            migrationBuilder.DropTable(
                name: "Libraries");
        }
    }
}
