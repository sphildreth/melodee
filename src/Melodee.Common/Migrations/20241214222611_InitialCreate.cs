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
                    MediaUniqueId = table.Column<long>(type: "bigint", nullable: false),
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
                    MediaUniqueId = table.Column<long>(type: "bigint", nullable: false),
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
                    MediaUniqueId = table.Column<long>(type: "bigint", nullable: false),
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
                    { 1, null, new Guid("9d9cad81-d4bb-4825-90e9-6fcb16d1679c"), null, NodaTime.Instant.FromUnixTimeTicks(17342151708743578L), "Files in this directory are scanned and Album information is gathered via processing.", false, null, null, "Inbound", null, "/storage/inbound/", null, 0, null, 1 },
                    { 2, null, new Guid("9cf1dac1-fe54-4785-aa2d-8bf5a6fd8412"), null, NodaTime.Instant.FromUnixTimeTicks(17342151708743578L), "The staging directory to place processed files into (Inbound -> Staging -> Library).", false, null, null, "Staging", null, "/storage/staging/", null, 0, null, 2 },
                    { 3, null, new Guid("5a90e3c1-d307-454e-86ee-44442a58fc46"), null, NodaTime.Instant.FromUnixTimeTicks(17342151708743578L), "The library directory to place processed, reviewed and ready to use music files into.", false, null, null, "Library", null, "/storage/library/", null, 0, null, 3 },
                    { 4, null, new Guid("b1ca4442-e9be-444a-a9a9-d06a41250720"), null, NodaTime.Instant.FromUnixTimeTicks(17342151708743578L), "Library where user images are stored.", false, null, null, "User Images", null, "/storage/images/users/", null, 0, null, 4 }
                });

            migrationBuilder.InsertData(
                table: "Settings",
                columns: new[] { "Id", "ApiKey", "Category", "Comment", "CreatedAt", "Description", "IsLocked", "Key", "LastUpdatedAt", "Notes", "SortOrder", "Tags", "Value" },
                values: new object[,]
                {
                    { 1, new Guid("b8155b78-6390-4508-bc17-c95c369ed087"), null, "Add a default filter to show only albums with this or less number of songs.", NodaTime.Instant.FromUnixTimeTicks(17342151708743578L), null, false, "filtering.lessThanSongCount", null, null, 0, null, "3" },
                    { 2, new Guid("e7be7693-383c-4f03-876a-3a68154c8a0b"), null, "Add a default filter to show only albums with this or less duration.", NodaTime.Instant.FromUnixTimeTicks(17342151708743578L), null, false, "filtering.lessThanDuration", null, null, 0, null, "720000" },
                    { 3, new Guid("4c05d0e3-6adc-46b3-854a-fc3da6353816"), null, "Maximum number of albums to scan when processing inbound directory.", NodaTime.Instant.FromUnixTimeTicks(17342151708743578L), null, false, "processing.stagingDirectoryScanLimit", null, null, 0, null, "250" },
                    { 4, new Guid("90552ba1-710d-4d4d-a966-088028f92e6b"), null, "Default page size when view including pagination.", NodaTime.Instant.FromUnixTimeTicks(17342151708743578L), null, false, "defaults.pagesize", null, null, 0, null, "100" },
                    { 6, new Guid("28e6de9f-c99b-4080-b495-a649d7a39224"), null, "Amount of time to display a Toast then auto-close (in milliseconds.)", NodaTime.Instant.FromUnixTimeTicks(17342151708743578L), null, false, "userinterface.toastAutoCloseTime", null, null, 0, null, "2000" },
                    { 9, new Guid("d49de4e8-7838-4322-a0f4-f4f940c23470"), null, "List of ignored articles when scanning media (pipe delimited).", NodaTime.Instant.FromUnixTimeTicks(17342151708743578L), null, false, "processing.ignoredArticles", null, null, 0, null, "THE|EL|LA|LOS|LAS|LE|LES|OS|AS|O|A" },
                    { 26, new Guid("5f43bce6-6c87-4d26-9902-4b80172efd3e"), null, "Fragments of artist names to replace (JSON Dictionary).", NodaTime.Instant.FromUnixTimeTicks(17342151708743578L), null, false, "processing.artistNameReplacements", null, null, 0, null, "{'AC/DC': ['AC; DC', 'AC;DC', 'AC/ DC', 'AC DC'] , 'Love/Hate': ['Love; Hate', 'Love;Hate', 'Love/ Hate', 'Love Hate'] }" },
                    { 27, new Guid("37188441-8ad2-4e78-a0a9-16a8371151ed"), null, "If OrigAlbumYear [TOR, TORY, TDOR] value is invalid use current year.", NodaTime.Instant.FromUnixTimeTicks(17342151708743578L), null, false, "processing.doUseCurrentYearAsDefaultOrigAlbumYearValue", null, null, 0, null, "true" },
                    { 28, new Guid("ee1bcbc6-b96f-4cfb-9f1d-cb1fbca750b2"), null, "Delete original files when processing. When false a copy if made, else original is deleted after processed.", NodaTime.Instant.FromUnixTimeTicks(17342151708743578L), null, false, "processing.doDeleteOriginal", null, null, 0, null, "false" },
                    { 29, new Guid("52f47443-c168-4ffb-8c04-e4378c8822a1"), null, "Extension to add to file when converted, leave blank to disable.", NodaTime.Instant.FromUnixTimeTicks(17342151708743578L), null, false, "processing.convertedExtension", null, null, 0, null, "_converted" },
                    { 30, new Guid("9350f192-2c83-4ef5-9e36-fffbf56a3fc7"), null, "Extension to add to file when processed, leave blank to disable.", NodaTime.Instant.FromUnixTimeTicks(17342151708743578L), null, false, "processing.processedExtension", null, null, 0, null, "_processed" },
                    { 31, new Guid("8ee1417b-13c3-440a-8d21-55afedfe3305"), null, "Extension to add to file to indicate other files in the same category where processed and this file was skipped during processing, leave blank to disable.", NodaTime.Instant.FromUnixTimeTicks(17342151708743578L), null, false, "processing.skippedExtension", null, null, 0, null, "_skipped" },
                    { 32, new Guid("41f1a9b5-28fa-4cec-8499-6990060a3296"), null, "When processing over write any existing Melodee data files, otherwise skip and leave in place.", NodaTime.Instant.FromUnixTimeTicks(17342151708743578L), null, false, "processing.doOverrideExistingMelodeeDataFiles", null, null, 0, null, "true" },
                    { 34, new Guid("5b26f93b-6734-4535-9b68-4092cbc9911e"), null, "The maximum number of files to process, set to zero for unlimited.", NodaTime.Instant.FromUnixTimeTicks(17342151708743578L), null, false, "processing.maximumProcessingCount", null, null, 0, null, "0" },
                    { 35, new Guid("68e84639-8c60-4f80-9bd2-a7f27418ee53"), null, "Maximum allowed length of album directory name.", NodaTime.Instant.FromUnixTimeTicks(17342151708743578L), null, false, "processing.maximumAlbumDirectoryNameLength", null, null, 0, null, "255" },
                    { 36, new Guid("43614b2d-42cf-4e63-a8d8-725a4d48ce01"), null, "Maximum allowed length of artist directory name.", NodaTime.Instant.FromUnixTimeTicks(17342151708743578L), null, false, "processing.maximumArtistDirectoryNameLength", null, null, 0, null, "255" },
                    { 37, new Guid("c827c144-6ef2-49f6-a9fa-94830da3f8e6"), null, "Fragments to remove from album titles (JSON array).", NodaTime.Instant.FromUnixTimeTicks(17342151708743578L), null, false, "processing.albumTitleRemovals", null, null, 0, null, "['^', '~', '#']" },
                    { 38, new Guid("aa1fddc5-ff5a-4f1e-afeb-c0c9c2bd27cd"), null, "Fragments to remove from song titles (JSON array).", NodaTime.Instant.FromUnixTimeTicks(17342151708743578L), null, false, "processing.songTitleRemovals", null, null, 0, null, "[';', '(Remaster)', 'Remaster']" },
                    { 39, new Guid("ff4f07ee-a2e2-4d3a-b5fc-71d3bc9f1f93"), null, "Continue processing if an error is encountered.", NodaTime.Instant.FromUnixTimeTicks(17342151708743578L), null, false, "processing.doContinueOnDirectoryProcessingErrors", null, null, 0, null, "true" },
                    { 41, new Guid("a57bb048-5460-46bb-b6a7-a102f477b4d2"), null, "Is scripting enabled.", NodaTime.Instant.FromUnixTimeTicks(17342151708743578L), null, false, "scripting.enabled", null, null, 0, null, "false" },
                    { 42, new Guid("c85ae045-36eb-4207-be1e-e72f0e426aa6"), null, "Script to run before processing the inbound directory, leave blank to disable.", NodaTime.Instant.FromUnixTimeTicks(17342151708743578L), null, false, "scripting.preDiscoveryScript", null, null, 0, null, "" },
                    { 43, new Guid("db5f2bd6-d754-4e54-9a49-f4109efba5c2"), null, "Script to run after processing the inbound directory, leave blank to disable.", NodaTime.Instant.FromUnixTimeTicks(17342151708743578L), null, false, "scripting.postDiscoveryScript", null, null, 0, null, "" },
                    { 44, new Guid("164abf04-aeef-4550-8e65-e6cda588f9e0"), 13, "The maximum value a media number can have for an album. The length of this is used for formatting song names.", NodaTime.Instant.FromUnixTimeTicks(17342151708743578L), null, false, "validation.maximumMediaNumber", null, null, 0, null, "999" },
                    { 45, new Guid("48640ddf-53e5-415d-a171-d4148d972518"), null, "Don't create performer contributors for these performer names.", NodaTime.Instant.FromUnixTimeTicks(17342151708743578L), null, false, "processing.ignoredPerformers", null, null, 0, null, "" },
                    { 46, new Guid("eb10f6f3-6380-473b-9058-528281381175"), null, "Don't create production contributors for these production names.", NodaTime.Instant.FromUnixTimeTicks(17342151708743578L), null, false, "processing.ignoredProduction", null, null, 0, null, "['www.t.me;pmedia_music']" },
                    { 47, new Guid("4aea0e5e-4c9e-4cd7-93b8-4aa62f5f9daa"), null, "Don't create publisher contributors for these artist names.", NodaTime.Instant.FromUnixTimeTicks(17342151708743578L), null, false, "processing.ignoredPublishers", null, null, 0, null, "['P.M.E.D.I.A','PMEDIA','PMEDIA GROUP']" },
                    { 48, new Guid("22d4e54e-6f4a-4741-b3f1-7e4518b56c10"), null, "Private key used to encrypt/decrypt passwords for Subsonic authentication. Use https://generate-random.org/encryption-key-generator?count=1&bytes=32&cipher=aes-256-cbc&string=&password= to generate a new key.", NodaTime.Instant.FromUnixTimeTicks(17342151708743578L), null, false, "encryption.privateKey", null, null, 0, null, "H+Kiik6VMKfTD2MesF1GoMjczTrD5RhuKckJ5+/UQWOdWajGcsEC3yEnlJ5eoy8Y" },
                    { 53, new Guid("4f36e5fc-2f3a-4d07-ad27-dcfd940d07b9"), null, "Processing batching size. Allowed range is between [250] and [1000]. ", NodaTime.Instant.FromUnixTimeTicks(17342151708743578L), null, false, "defaults.batchSize", null, null, 0, null, "250" },
                    { 100, new Guid("0ef51360-ba86-4374-8305-d6da1f2453d0"), 1, "OpenSubsonic server supported Subsonic API version.", NodaTime.Instant.FromUnixTimeTicks(17342151708743578L), null, false, "openSubsonicServer.openSubsonic.serverSupportedVersion", null, null, 0, null, "1.16.1" },
                    { 101, new Guid("15552a9b-51a1-444b-adc1-3d2014d84740"), 1, "OpenSubsonic server name.", NodaTime.Instant.FromUnixTimeTicks(17342151708743578L), null, false, "openSubsonicServer.openSubsonicServer.type", null, null, 0, null, "Melodee" },
                    { 102, new Guid("5065fdaf-eb4f-450c-886f-11bb0b3a7ac8"), 1, "OpenSubsonic server actual version. [Ex: 1.2.3 (beta)]", NodaTime.Instant.FromUnixTimeTicks(17342151708743578L), null, false, "openSubsonicServer.openSubsonicServer.version", null, null, 0, null, "1.0.1 (beta)" },
                    { 103, new Guid("b7d35477-316f-4aab-b853-9f93f2158e67"), 1, "OpenSubsonic email to use in License responses.", NodaTime.Instant.FromUnixTimeTicks(17342151708743578L), null, false, "openSubsonicServer.openSubsonicServerLicenseEmail", null, null, 0, null, "noreply@localhost.lan" },
                    { 104, new Guid("758b3f1c-47a9-4a17-be9d-139365283e67"), 1, "Limit the number of artists to include in an indexes request, set to zero for 32k per index (really not recommended with tens of thousands of artists and mobile clients timeout downloading indexes, a user can find an artist by search)", NodaTime.Instant.FromUnixTimeTicks(17342151708743578L), null, false, "openSubsonicServer.openSubsonicServer.index.artistLimit", null, null, 0, null, "1000" },
                    { 200, new Guid("8d6a5e12-bccf-4886-9de7-2eab4baabf47"), 2, "Enable Melodee to convert non-mp3 media files during processing.", NodaTime.Instant.FromUnixTimeTicks(17342151708743578L), null, false, "conversion.enabled", null, null, 0, null, "true" },
                    { 201, new Guid("a2665afb-6e96-4a73-a059-c5320630043c"), 2, "Bitrate to convert non-mp3 media files during processing.", NodaTime.Instant.FromUnixTimeTicks(17342151708743578L), null, false, "conversion.bitrate", null, null, 0, null, "384" },
                    { 202, new Guid("f3a5b4a5-f1d4-4630-a0be-28ac9abe7a62"), 2, "Vbr to convert non-mp3 media files during processing.", NodaTime.Instant.FromUnixTimeTicks(17342151708743578L), null, false, "conversion.vbrLevel", null, null, 0, null, "4" },
                    { 203, new Guid("a04f38c4-ab87-449e-b437-09242b32f034"), 2, "Sampling rate to convert non-mp3 media files during processing.", NodaTime.Instant.FromUnixTimeTicks(17342151708743578L), null, false, "conversion.samplingRate", null, null, 0, null, "48000" },
                    { 300, new Guid("7c6d0787-d848-448d-b6a4-824b003aa1c2"), 3, "Short Format to use when displaying full dates.", NodaTime.Instant.FromUnixTimeTicks(17342151708743578L), null, false, "formatting.dateTimeDisplayFormatShort", null, null, 0, null, "yyyyMMdd HH\\:mm" },
                    { 301, new Guid("d2613c66-4840-4779-9750-14f6c5c4b205"), 3, "Format to use when displaying activity related dates (e.g., processing messages)", NodaTime.Instant.FromUnixTimeTicks(17342151708743578L), null, false, "formatting.dateTimeDisplayActivityFormat", null, null, 0, null, "hh\\:mm\\:ss\\.ffff" },
                    { 400, new Guid("e746bef6-325a-4a43-b9b7-55acdca71ec3"), 4, "Include any embedded images from media files into the Melodee data file.", NodaTime.Instant.FromUnixTimeTicks(17342151708743578L), null, false, "imaging.doLoadEmbeddedImages", null, null, 0, null, "true" },
                    { 401, new Guid("7e3aa042-3206-4fc9-b721-870c2f1254f1"), 4, "Small image size (square image, this is both width and height).", NodaTime.Instant.FromUnixTimeTicks(17342151708743578L), null, false, "imaging.smallSize", null, null, 0, null, "300" },
                    { 402, new Guid("56611007-365c-4492-bfbd-78b7b96f12db"), 4, "Medium image size (square image, this is both width and height).", NodaTime.Instant.FromUnixTimeTicks(17342151708743578L), null, false, "imaging.mediumSize", null, null, 0, null, "600" },
                    { 403, new Guid("59771226-74c5-44c9-8bf5-9343d394f2d7"), 4, "Large image size (square image, this is both width and height), if larger than will be resized to this image, leave blank to disable.", NodaTime.Instant.FromUnixTimeTicks(17342151708743578L), null, false, "imaging.largeSize", null, null, 0, null, "1600" },
                    { 404, new Guid("f108268c-974c-4ecc-b916-dd08dee745ba"), 4, "Maximum allowed number of images for an album, this includes all image types (Front, Rear, etc.), set to zero for unlimited.", NodaTime.Instant.FromUnixTimeTicks(17342151708743578L), null, false, "imaging.maximumNumberOfAlbumImages", null, null, 0, null, "25" },
                    { 405, new Guid("19cb36eb-a861-4957-8ed0-6ef8fa687a8b"), 4, "Maximum allowed number of images for an artist, set to zero for unlimited.", NodaTime.Instant.FromUnixTimeTicks(17342151708743578L), null, false, "imaging.maximumNumberOfArtistImages", null, null, 0, null, "25" },
                    { 406, new Guid("2dd8a916-9a23-489b-a16d-5b21f35ec9c3"), 4, "Images under this size are considered invalid, set to zero to disable.", NodaTime.Instant.FromUnixTimeTicks(17342151708743578L), null, false, "imaging.minimumImageSize", null, null, 0, null, "300" },
                    { 500, new Guid("61bdf1f0-fe2c-4adc-b4e1-453e20307895"), 5, "Is Magic processing enabled.", NodaTime.Instant.FromUnixTimeTicks(17342151708743578L), null, false, "magic.enabled", null, null, 0, null, "true" },
                    { 501, new Guid("0e7f3bbe-fb65-4c93-807a-534a92590c21"), 5, "Renumber songs when doing magic processing.", NodaTime.Instant.FromUnixTimeTicks(17342151708743578L), null, false, "magic.doRenumberSongs", null, null, 0, null, "true" },
                    { 502, new Guid("b690d57f-33da-433d-95eb-823191a60c52"), 5, "Remove featured artists from song artist when doing magic.", NodaTime.Instant.FromUnixTimeTicks(17342151708743578L), null, false, "magic.doRemoveFeaturingArtistFromSongArtist", null, null, 0, null, "true" },
                    { 503, new Guid("4a919307-2a3e-4a0b-a7ef-72af01ebe05c"), 5, "Remove featured artists from song title when doing magic.", NodaTime.Instant.FromUnixTimeTicks(17342151708743578L), null, false, "magic.doRemoveFeaturingArtistFromSongTitle", null, null, 0, null, "true" },
                    { 504, new Guid("1629ce99-87f5-477e-93be-d3383810bfd0"), 5, "Replace song artist separators with standard ID3 separator ('/') when doing magic.", NodaTime.Instant.FromUnixTimeTicks(17342151708743578L), null, false, "magic.doReplaceSongsArtistSeparators", null, null, 0, null, "true" },
                    { 505, new Guid("ce9505d3-9869-4a7d-9b75-1cdebf016db0"), 5, "Set the song year to current year if invalid or missing when doing magic.", NodaTime.Instant.FromUnixTimeTicks(17342151708743578L), null, false, "magic.doSetYearToCurrentIfInvalid", null, null, 0, null, "true" },
                    { 506, new Guid("4f56812e-b4d7-4fdc-bb28-cd17cbc02d17"), 5, "Remove unwanted text from album title when doing magic.", NodaTime.Instant.FromUnixTimeTicks(17342151708743578L), null, false, "magic.doRemoveUnwantedTextFromAlbumTitle", null, null, 0, null, "true" },
                    { 507, new Guid("2c81e0e4-e4f1-4fdd-b6d4-dcf9198ce8f4"), 5, "Remove unwanted text from song titles when doing magic.", NodaTime.Instant.FromUnixTimeTicks(17342151708743578L), null, false, "magic.doRemoveUnwantedTextFromSongTitles", null, null, 0, null, "true" },
                    { 700, new Guid("2b902a39-6067-4895-b776-e0a48e58b0c7"), 7, "Process of CueSheet files during processing.", NodaTime.Instant.FromUnixTimeTicks(17342151708743578L), null, false, "plugin.cueSheet.enabled", null, null, 0, null, "true" },
                    { 701, new Guid("386f586a-88d7-40e4-b44f-db701389b78a"), 7, "Process of M3U files during processing.", NodaTime.Instant.FromUnixTimeTicks(17342151708743578L), null, false, "plugin.m3u.enabled", null, null, 0, null, "true" },
                    { 702, new Guid("5c1bd7d5-2191-4265-8d2b-1886f8c0e5de"), 7, "Process of NFO files during processing.", NodaTime.Instant.FromUnixTimeTicks(17342151708743578L), null, false, "plugin.nfo.enabled", null, null, 0, null, "true" },
                    { 703, new Guid("49c39032-f4dd-4185-94d2-f8312c8d00db"), 7, "Process of Simple File Verification (SFV) files during processing.", NodaTime.Instant.FromUnixTimeTicks(17342151708743578L), null, false, "plugin.simpleFileVerification.enabled", null, null, 0, null, "true" },
                    { 704, new Guid("7d197696-b053-4b59-8907-16cc96fa3fbd"), 7, "If true then all comments will be removed from media files.", NodaTime.Instant.FromUnixTimeTicks(17342151708743578L), null, false, "processing.doDeleteComments", null, null, 0, null, "true" },
                    { 900, new Guid("4cc5de23-7624-4247-bb02-5166b25e7d14"), 9, "Use Bing search engine to find images for albums and artists.", NodaTime.Instant.FromUnixTimeTicks(17342151708743578L), null, false, "searchEngine.bingImage.enabled", null, null, 0, null, "false" },
                    { 901, new Guid("d034cf38-9d7b-4303-b1c1-3aad292f3f9e"), 9, "Bing search ApiKey (Ocp-Apim-Subscription-Key), leave blank to disable.", NodaTime.Instant.FromUnixTimeTicks(17342151708743578L), null, false, "searchEngine.bingImage.apiKey", null, null, 0, null, "" },
                    { 902, new Guid("81fd27de-2bb2-4076-8d4c-c240d01d72ac"), 9, "User agent to send with Search engine requests.", NodaTime.Instant.FromUnixTimeTicks(17342151708743578L), null, false, "searchEngine.userAgent", null, null, 0, null, "Mozilla/5.0 (X11; Linux x86_64; rv:131.0) Gecko/20100101 Firefox/131.0" },
                    { 903, new Guid("7d8438e6-40ba-4330-8749-783cf6224b1d"), 9, "Default page size when performing a search engine search.", NodaTime.Instant.FromUnixTimeTicks(17342151708743578L), null, false, "searchEngine.defaultPageSize", null, null, 0, null, "20" },
                    { 904, new Guid("e14cba0a-7751-4204-8220-52e44e067df3"), 9, "Is MusicBrainz search engine enabled.", NodaTime.Instant.FromUnixTimeTicks(17342151708743578L), null, false, "searchEngine.musicbrainz.enabled", null, null, 0, null, "true" },
                    { 905, new Guid("7a06ea0c-2a4c-487b-a9e8-b0931dc4a8f5"), 9, "Storage path to hold MusicBrainz downloaded files and SQLite db.", NodaTime.Instant.FromUnixTimeTicks(17342151708743578L), null, false, "searchEngine.musicbrainz.storagePath", null, null, 0, null, "/melodee_test/search-engine-storage/musicbrainz/" },
                    { 906, new Guid("1c4eeb48-f9a0-4b4c-bd57-0741be1c84d0"), 9, "Maximum number of batches import from MusicBrainz downloaded db dump (this setting is usually used during debugging), set to zero for unlimited.", NodaTime.Instant.FromUnixTimeTicks(17342151708743578L), null, false, "searchEngine.musicbrainz.importMaximumToProcess", null, null, 0, null, "0" },
                    { 907, new Guid("620bbd7e-d1a2-4246-b83b-7353a4d456b5"), 9, "Number of records to import from MusicBrainz downloaded db dump before commiting to local SQLite database.", NodaTime.Instant.FromUnixTimeTicks(17342151708743578L), null, false, "searchEngine.musicbrainz.importBatchSize", null, null, 0, null, "100000" },
                    { 908, new Guid("aa86e9c0-1cb9-4212-800e-cbbbb3e139c3"), 9, "Timestamp of when last MusicBrainz import was successful.", NodaTime.Instant.FromUnixTimeTicks(17342151708743578L), null, false, "searchEngine.musicbrainz.importLastImportTimestamp", null, null, 0, null, "" },
                    { 910, new Guid("f14c73fc-a235-4c3d-9f5b-47233006a778"), 9, "Is Spotify search engine enabled.", NodaTime.Instant.FromUnixTimeTicks(17342151708743578L), null, false, "searchEngine.spotify.enabled", null, null, 0, null, "false" },
                    { 911, new Guid("c23e8955-80f1-46f4-ac78-3b6461ba901a"), 9, "ApiKey used used with Spotify. See https://developer.spotify.com/ for more details.", NodaTime.Instant.FromUnixTimeTicks(17342151708743578L), null, false, "searchEngine.spotify.apiKey", null, null, 0, null, "" },
                    { 912, new Guid("ae2f1579-479f-466e-8f0c-3965a3dea257"), 9, "Shared secret used with Spotify. See https://developer.spotify.com/ for more details.", NodaTime.Instant.FromUnixTimeTicks(17342151708743578L), null, false, "searchEngine.spotify.sharedSecret", null, null, 0, null, "" },
                    { 913, new Guid("6760b485-2715-4775-b0f8-983764174613"), 9, "Token obtained from Spotify using the ApiKey and the Secret, this json contains expiry information.", NodaTime.Instant.FromUnixTimeTicks(17342151708743578L), null, false, "searchEngine.spotify.accessToken", null, null, 0, null, "" },
                    { 1000, new Guid("e79a287b-15e0-4667-b07c-c0ed0df30a9a"), 10, "Is scrobbling enabled.", NodaTime.Instant.FromUnixTimeTicks(17342151708743578L), null, false, "scrobbling.enabled", null, null, 0, null, "false" },
                    { 1001, new Guid("aeb2998f-1d93-4b79-9f23-6a92fa833f9c"), 10, "Is scrobbling to Last.fm enabled.", NodaTime.Instant.FromUnixTimeTicks(17342151708743578L), null, false, "scrobbling.lastFm.Enabled", null, null, 0, null, "false" },
                    { 1002, new Guid("0cbcf603-c440-457a-91a4-f4bcc2a12e62"), 10, "ApiKey used used with last FM. See https://www.last.fm/api/authentication for more details.", NodaTime.Instant.FromUnixTimeTicks(17342151708743578L), null, false, "scrobbling.lastFm.apiKey", null, null, 0, null, "" },
                    { 1003, new Guid("4b4f0af5-2ad3-46a8-80fb-67ecd0821b03"), 10, "Shared secret used with last FM. See https://www.last.fm/api/authentication for more details.", NodaTime.Instant.FromUnixTimeTicks(17342151708743578L), null, false, "scrobbling.lastFm.sharedSecret", null, null, 0, null, "" },
                    { 1100, new Guid("c845bd46-4f2a-490b-b193-faaaac38acf1"), 11, "Base URL for Melodee to use when building shareable links and image urls (e.g., 'https://server.domain.com:8080', 'http://server.domain.com').", NodaTime.Instant.FromUnixTimeTicks(17342151708743578L), null, false, "system.baseUrl", null, null, 0, null, "** REQUIRED: THIS MUST BE EDITED **" },
                    { 1101, new Guid("ca488f40-7264-4634-9dba-f1197a003387"), 11, "Is downloading enabled.", NodaTime.Instant.FromUnixTimeTicks(17342151708743578L), null, false, "system.isDownloadingEnabled", null, null, 0, null, "true" },
                    { 1200, new Guid("1d7c2aeb-d92b-42fd-8d5e-21cadca3c035"), 12, "Default format for transcoding.", NodaTime.Instant.FromUnixTimeTicks(17342151708743578L), null, false, "transcoding.default", null, null, 0, null, "raw" },
                    { 1201, new Guid("2e7fac59-15ff-4b8c-b0cc-ee56902613f6"), 12, "Default command to transcode MP3 for streaming.", NodaTime.Instant.FromUnixTimeTicks(17342151708743578L), null, false, "transcoding.command.mp3", null, null, 0, null, "{ 'format': 'Mp3', 'bitrate: 192, 'command': 'ffmpeg -i %s -ss %t -map 0:a:0 -b:a %bk -v 0 -f mp3 -' }" },
                    { 1202, new Guid("8cfa2628-a5be-4a7e-811c-f88a2241222d"), 12, "Default command to transcode using libopus for streaming.", NodaTime.Instant.FromUnixTimeTicks(17342151708743578L), null, false, "transcoding.command.opus", null, null, 0, null, "{ 'format': 'Opus', 'bitrate: 128, 'command': 'ffmpeg -i %s -ss %t -map 0:a:0 -b:a %bk -v 0 -c:a libopus -f opus -' }" },
                    { 1203, new Guid("a5dda789-c265-4b6c-9b22-97581d034b7b"), 12, "Default command to transcode to aac for streaming.", NodaTime.Instant.FromUnixTimeTicks(17342151708743578L), null, false, "transcoding.command.aac", null, null, 0, null, "{ 'format': 'Aac', 'bitrate: 256, 'command': 'ffmpeg -i %s -ss %t -map 0:a:0 -b:a %bk -v 0 -c:a aac -f adts -' }" },
                    { 1300, new Guid("22cc25c9-22b1-4799-bb43-2a5e39776a36"), 13, "The maximum value a song number can have for an album. The length of this is used for formatting song names.", NodaTime.Instant.FromUnixTimeTicks(17342151708743578L), null, false, "validation.maximumSongNumber", null, null, 0, null, "9999" },
                    { 1301, new Guid("bd1d2867-57ed-42a4-b794-1ca9c427798f"), 13, "Minimum allowed year for an album.", NodaTime.Instant.FromUnixTimeTicks(17342151708743578L), null, false, "validation.minimumAlbumYear", null, null, 0, null, "1860" },
                    { 1302, new Guid("8f05eeee-8d39-4306-986a-a99e20584b82"), 13, "Maximum allowed year for an album.", NodaTime.Instant.FromUnixTimeTicks(17342151708743578L), null, false, "validation.maximumAlbumYear", null, null, 0, null, "2150" }
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
