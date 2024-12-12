using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NodaTime;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Melodee.Common.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate2 : Migration
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
                    { 1, null, new Guid("a60ccf09-354a-476a-becc-19af40126788"), null, NodaTime.Instant.FromUnixTimeTicks(17339787808360764L), "Files in this directory are scanned and Album information is gathered via processing.", false, null, null, "Inbound", null, "/storage/inbound/", null, 0, null, 1 },
                    { 2, null, new Guid("98181cdd-c1a8-44f6-9539-f8c7be1560d5"), null, NodaTime.Instant.FromUnixTimeTicks(17339787808360764L), "The staging directory to place processed files into (Inbound -> Staging -> Library).", false, null, null, "Staging", null, "/storage/staging/", null, 0, null, 2 },
                    { 3, null, new Guid("56c4f745-6d59-4de8-9311-8a39cc4f2b33"), null, NodaTime.Instant.FromUnixTimeTicks(17339787808360764L), "The library directory to place processed, reviewed and ready to use music files into.", false, null, null, "Library", null, "/storage/library/", null, 0, null, 3 },
                    { 4, null, new Guid("627d757a-f80b-483c-a5dd-0d19a1a48f5e"), null, NodaTime.Instant.FromUnixTimeTicks(17339787808360764L), "Library where user images are stored.", false, null, null, "User Images", null, "/storage/images/users/", null, 0, null, 4 }
                });

            migrationBuilder.InsertData(
                table: "Settings",
                columns: new[] { "Id", "ApiKey", "Category", "Comment", "CreatedAt", "Description", "IsLocked", "Key", "LastUpdatedAt", "Notes", "SortOrder", "Tags", "Value" },
                values: new object[,]
                {
                    { 1, new Guid("6fbf7ff8-775e-4396-9ff3-7fa351515aa6"), null, "Add a default filter to show only albums with this or less number of songs.", NodaTime.Instant.FromUnixTimeTicks(17339787808360764L), null, false, "filtering.lessThanSongCount", null, null, 0, null, "3" },
                    { 2, new Guid("b14cdd6c-d1a8-47e2-8ea5-3c625bd7d35f"), null, "Add a default filter to show only albums with this or less duration.", NodaTime.Instant.FromUnixTimeTicks(17339787808360764L), null, false, "filtering.lessThanDuration", null, null, 0, null, "720000" },
                    { 3, new Guid("4d6c8d25-3a8a-414e-9158-1415a963286e"), null, "Maximum number of albums to scan when processing inbound directory.", NodaTime.Instant.FromUnixTimeTicks(17339787808360764L), null, false, "processing.stagingDirectoryScanLimit", null, null, 0, null, "250" },
                    { 4, new Guid("7322749e-31aa-40ab-8887-f90bd149715f"), null, "Default page size when view including pagination.", NodaTime.Instant.FromUnixTimeTicks(17339787808360764L), null, false, "defaults.pagesize", null, null, 0, null, "100" },
                    { 6, new Guid("01042b7e-75ef-46ec-95e4-db2801457281"), null, "Amount of time to display a Toast then auto-close (in milliseconds.)", NodaTime.Instant.FromUnixTimeTicks(17339787808360764L), null, false, "userinterface.toastAutoCloseTime", null, null, 0, null, "2000" },
                    { 9, new Guid("8cbaccda-e345-46b1-9d2d-1120ffde1198"), null, "List of ignored articles when scanning media (pipe delimited).", NodaTime.Instant.FromUnixTimeTicks(17339787808360764L), null, false, "processing.ignoredArticles", null, null, 0, null, "THE|EL|LA|LOS|LAS|LE|LES|OS|AS|O|A" },
                    { 26, new Guid("4fa94360-d72b-4314-8f3d-ccd9efb383f7"), null, "Fragments of artist names to replace (JSON Dictionary).", NodaTime.Instant.FromUnixTimeTicks(17339787808360764L), null, false, "processing.artistNameReplacements", null, null, 0, null, "{'AC/DC': ['AC; DC', 'AC;DC', 'AC/ DC', 'AC DC'] , 'Love/Hate': ['Love; Hate', 'Love;Hate', 'Love/ Hate', 'Love Hate'] }" },
                    { 27, new Guid("ceafb84d-7677-4408-a5d6-8a6d213242df"), null, "If OrigAlbumYear [TOR, TORY, TDOR] value is invalid use current year.", NodaTime.Instant.FromUnixTimeTicks(17339787808360764L), null, false, "processing.doUseCurrentYearAsDefaultOrigAlbumYearValue", null, null, 0, null, "true" },
                    { 28, new Guid("53cbb969-dc81-4492-b096-27fd0f776c90"), null, "Delete original files when processing. When false a copy if made, else original is deleted after processed.", NodaTime.Instant.FromUnixTimeTicks(17339787808360764L), null, false, "processing.doDeleteOriginal", null, null, 0, null, "false" },
                    { 29, new Guid("c949891a-ade2-4b58-ad44-b142c53b228b"), null, "Extension to add to file when converted, leave blank to disable.", NodaTime.Instant.FromUnixTimeTicks(17339787808360764L), null, false, "processing.convertedExtension", null, null, 0, null, "_converted" },
                    { 30, new Guid("1d13f203-77fa-4b9e-bc39-331a0ea41a2d"), null, "Extension to add to file when processed, leave blank to disable.", NodaTime.Instant.FromUnixTimeTicks(17339787808360764L), null, false, "processing.processedExtension", null, null, 0, null, "_processed" },
                    { 31, new Guid("3abd3834-f7f8-4802-8879-82f16bb05bcb"), null, "Extension to add to file to indicate other files in the same category where processed and this file was skipped during processing, leave blank to disable.", NodaTime.Instant.FromUnixTimeTicks(17339787808360764L), null, false, "processing.skippedExtension", null, null, 0, null, "_skipped" },
                    { 32, new Guid("0df6fa3d-481f-4cf1-b425-6a7c251ee0de"), null, "When processing over write any existing Melodee data files, otherwise skip and leave in place.", NodaTime.Instant.FromUnixTimeTicks(17339787808360764L), null, false, "processing.doOverrideExistingMelodeeDataFiles", null, null, 0, null, "true" },
                    { 34, new Guid("bb2c0351-2444-4d12-aa60-ee15523fa0e5"), null, "The maximum number of files to process, set to zero for unlimited.", NodaTime.Instant.FromUnixTimeTicks(17339787808360764L), null, false, "processing.maximumProcessingCount", null, null, 0, null, "0" },
                    { 35, new Guid("dadbe7e4-8d64-4f83-acc7-ee7f7aaa3e7e"), null, "Maximum allowed length of album directory name.", NodaTime.Instant.FromUnixTimeTicks(17339787808360764L), null, false, "processing.maximumAlbumDirectoryNameLength", null, null, 0, null, "255" },
                    { 36, new Guid("7a93349f-0811-4057-b4d4-0888208dceef"), null, "Maximum allowed length of artist directory name.", NodaTime.Instant.FromUnixTimeTicks(17339787808360764L), null, false, "processing.maximumArtistDirectoryNameLength", null, null, 0, null, "255" },
                    { 37, new Guid("33a85a1d-9c47-4dc1-865e-65fe663d20f6"), null, "Fragments to remove from album titles (JSON array).", NodaTime.Instant.FromUnixTimeTicks(17339787808360764L), null, false, "processing.albumTitleRemovals", null, null, 0, null, "['^', '~', '#']" },
                    { 38, new Guid("1303e024-7d8f-49b8-8cca-7e16a13fea18"), null, "Fragments to remove from song titles (JSON array).", NodaTime.Instant.FromUnixTimeTicks(17339787808360764L), null, false, "processing.songTitleRemovals", null, null, 0, null, "[';', '(Remaster)', 'Remaster']" },
                    { 39, new Guid("7e3ca529-6e90-41f5-bba3-dad7124bb288"), null, "Continue processing if an error is encountered.", NodaTime.Instant.FromUnixTimeTicks(17339787808360764L), null, false, "processing.doContinueOnDirectoryProcessingErrors", null, null, 0, null, "true" },
                    { 41, new Guid("2d6f2b7d-71ce-47a2-92e7-fe6f5386551c"), null, "Is scripting enabled.", NodaTime.Instant.FromUnixTimeTicks(17339787808360764L), null, false, "scripting.enabled", null, null, 0, null, "false" },
                    { 42, new Guid("a27ab013-b210-4fa2-bff5-da8e9ae50aab"), null, "Script to run before processing the inbound directory, leave blank to disable.", NodaTime.Instant.FromUnixTimeTicks(17339787808360764L), null, false, "scripting.preDiscoveryScript", null, null, 0, null, "" },
                    { 43, new Guid("1939fcf9-3cfb-4926-9f79-72d40a9bcc8c"), null, "Script to run after processing the inbound directory, leave blank to disable.", NodaTime.Instant.FromUnixTimeTicks(17339787808360764L), null, false, "scripting.postDiscoveryScript", null, null, 0, null, "" },
                    { 44, new Guid("95bb0b29-bd5f-4f99-846d-ff0a57526e82"), 13, "The maximum value a media number can have for an album. The length of this is used for formatting song names.", NodaTime.Instant.FromUnixTimeTicks(17339787808360764L), null, false, "validation.maximumMediaNumber", null, null, 0, null, "999" },
                    { 48, new Guid("d2a0021c-6d5c-4314-ba54-10d8614faf67"), null, "Private key used to encrypt/decrypt passwords for Subsonic authentication. Use https://generate-random.org/encryption-key-generator?count=1&bytes=32&cipher=aes-256-cbc&string=&password= to generate a new key.", NodaTime.Instant.FromUnixTimeTicks(17339787808360764L), null, false, "encryption.privateKey", null, null, 0, null, "H+Kiik6VMKfTD2MesF1GoMjczTrD5RhuKckJ5+/UQWOdWajGcsEC3yEnlJ5eoy8Y" },
                    { 53, new Guid("7bd7668c-8d77-43ee-98bc-a6285362d87a"), null, "Processing batching size. Allowed range is between [250] and [1000]. ", NodaTime.Instant.FromUnixTimeTicks(17339787808360764L), null, false, "defaults.batchSize", null, null, 0, null, "250" },
                    { 100, new Guid("f554e5af-37d7-48eb-b816-05e88fb526f7"), 1, "OpenSubsonic server supported Subsonic API version.", NodaTime.Instant.FromUnixTimeTicks(17339787808360764L), null, false, "openSubsonicServer.openSubsonic.serverSupportedVersion", null, null, 0, null, "1.16.1" },
                    { 101, new Guid("32286b90-4fb3-4e77-b07d-1b3e1253b04f"), 1, "OpenSubsonic server name.", NodaTime.Instant.FromUnixTimeTicks(17339787808360764L), null, false, "openSubsonicServer.openSubsonicServer.type", null, null, 0, null, "Melodee" },
                    { 102, new Guid("6e7d7173-e008-4b92-beb4-f34f853dc9ba"), 1, "OpenSubsonic server actual version. [Ex: 1.2.3 (beta)]", NodaTime.Instant.FromUnixTimeTicks(17339787808360764L), null, false, "openSubsonicServer.openSubsonicServer.version", null, null, 0, null, "1.0.1 (beta)" },
                    { 103, new Guid("2cc2366d-2c36-4fb0-9e41-d02356c98785"), 1, "OpenSubsonic email to use in License responses.", NodaTime.Instant.FromUnixTimeTicks(17339787808360764L), null, false, "openSubsonicServer.openSubsonicServerLicenseEmail", null, null, 0, null, "noreply@localhost.lan" },
                    { 104, new Guid("d63b4188-9dc2-4824-9eb6-5a70df93b064"), 1, "Limit the number of artists to include in an indexes request, set to zero for 32k per index (really not recommended with tens of thousands of artists and mobile clients timeout downloading indexes, a user can find an artist by search)", NodaTime.Instant.FromUnixTimeTicks(17339787808360764L), null, false, "openSubsonicServer.openSubsonicServer.index.artistLimit", null, null, 0, null, "1000" },
                    { 200, new Guid("f48d1a0d-b46f-423a-9645-80951958fd2b"), 2, "Enable Melodee to convert non-mp3 media files during processing.", NodaTime.Instant.FromUnixTimeTicks(17339787808360764L), null, false, "conversion.enabled", null, null, 0, null, "true" },
                    { 201, new Guid("7e8743d2-324e-41ae-9749-96dc69c8ce4d"), 2, "Bitrate to convert non-mp3 media files during processing.", NodaTime.Instant.FromUnixTimeTicks(17339787808360764L), null, false, "conversion.bitrate", null, null, 0, null, "384" },
                    { 202, new Guid("cde5060f-77c0-4936-b036-82ef8459d702"), 2, "Vbr to convert non-mp3 media files during processing.", NodaTime.Instant.FromUnixTimeTicks(17339787808360764L), null, false, "conversion.vbrLevel", null, null, 0, null, "4" },
                    { 203, new Guid("dc1502e0-0d29-4adf-a713-9273d9b77b64"), 2, "Sampling rate to convert non-mp3 media files during processing.", NodaTime.Instant.FromUnixTimeTicks(17339787808360764L), null, false, "conversion.samplingRate", null, null, 0, null, "48000" },
                    { 300, new Guid("bcf196c7-a0d1-433a-b39c-abc02bbaeaf2"), 3, "Short Format to use when displaying full dates.", NodaTime.Instant.FromUnixTimeTicks(17339787808360764L), null, false, "formatting.dateTimeDisplayFormatShort", null, null, 0, null, "yyyyMMdd HH\\:mm" },
                    { 301, new Guid("653849c7-89f0-47f7-8c3e-b86a3bab9348"), 3, "Format to use when displaying activity related dates (e.g., processing messages)", NodaTime.Instant.FromUnixTimeTicks(17339787808360764L), null, false, "formatting.dateTimeDisplayActivityFormat", null, null, 0, null, "hh\\:mm\\:ss\\.ffff" },
                    { 400, new Guid("fd8e447b-b85e-4340-97ac-919a1b962751"), 4, "Include any embedded images from media files into the Melodee data file.", NodaTime.Instant.FromUnixTimeTicks(17339787808360764L), null, false, "imaging.doLoadEmbeddedImages", null, null, 0, null, "true" },
                    { 401, new Guid("4e08abdb-26da-41e9-874e-dafd9a48c00c"), 4, "Small image size (square image, this is both width and height).", NodaTime.Instant.FromUnixTimeTicks(17339787808360764L), null, false, "imaging.smallSize", null, null, 0, null, "300" },
                    { 402, new Guid("ddc0d6fa-ac02-45be-aaf4-af6ad86b9973"), 4, "Medium image size (square image, this is both width and height).", NodaTime.Instant.FromUnixTimeTicks(17339787808360764L), null, false, "imaging.mediumSize", null, null, 0, null, "600" },
                    { 403, new Guid("2d755c00-e5cb-47cc-81cc-f5dc1559afb9"), 4, "Large image size (square image, this is both width and height), if larger than will be resized to this image, leave blank to disable.", NodaTime.Instant.FromUnixTimeTicks(17339787808360764L), null, false, "imaging.largeSize", null, null, 0, null, "1600" },
                    { 404, new Guid("66c96e65-7f95-40c9-86d6-f48816b3703e"), 4, "Maximum allowed number of images for an album, this includes all image types (Front, Rear, etc.), set to zero for unlimited.", NodaTime.Instant.FromUnixTimeTicks(17339787808360764L), null, false, "imaging.maximumNumberOfAlbumImages", null, null, 0, null, "25" },
                    { 405, new Guid("b00df589-ca04-454f-bce1-8f46fe5586d5"), 4, "Maximum allowed number of images for an artist, set to zero for unlimited.", NodaTime.Instant.FromUnixTimeTicks(17339787808360764L), null, false, "imaging.maximumNumberOfArtistImages", null, null, 0, null, "25" },
                    { 406, new Guid("beb7a01b-b238-43c3-8747-1b3f85b7e176"), 4, "Images under this size are considered invalid, set to zero to disable.", NodaTime.Instant.FromUnixTimeTicks(17339787808360764L), null, false, "imaging.minimumImageSize", null, null, 0, null, "300" },
                    { 500, new Guid("b50022bf-fa32-493b-a493-34a1aaebc812"), 5, "Is Magic processing enabled.", NodaTime.Instant.FromUnixTimeTicks(17339787808360764L), null, false, "magic.enabled", null, null, 0, null, "true" },
                    { 501, new Guid("b6f539ab-be93-4d16-8b75-a31c357d8c62"), 5, "Renumber songs when doing magic processing.", NodaTime.Instant.FromUnixTimeTicks(17339787808360764L), null, false, "magic.doRenumberSongs", null, null, 0, null, "true" },
                    { 502, new Guid("3bd10275-0abc-4297-b9c3-4654a35bc469"), 5, "Remove featured artists from song artist when doing magic.", NodaTime.Instant.FromUnixTimeTicks(17339787808360764L), null, false, "magic.doRemoveFeaturingArtistFromSongArtist", null, null, 0, null, "true" },
                    { 503, new Guid("685a3493-d9c6-46a6-bc2b-365169abad70"), 5, "Remove featured artists from song title when doing magic.", NodaTime.Instant.FromUnixTimeTicks(17339787808360764L), null, false, "magic.doRemoveFeaturingArtistFromSongTitle", null, null, 0, null, "true" },
                    { 504, new Guid("c14c01db-4651-4952-9b61-c9aae02e3f80"), 5, "Replace song artist separators with standard ID3 separator ('/') when doing magic.", NodaTime.Instant.FromUnixTimeTicks(17339787808360764L), null, false, "magic.doReplaceSongsArtistSeparators", null, null, 0, null, "true" },
                    { 505, new Guid("e98ff812-c80a-4d8c-85cd-dc4540216a6b"), 5, "Set the song year to current year if invalid or missing when doing magic.", NodaTime.Instant.FromUnixTimeTicks(17339787808360764L), null, false, "magic.doSetYearToCurrentIfInvalid", null, null, 0, null, "true" },
                    { 506, new Guid("4b89744e-73f9-4582-b0eb-642173dba13c"), 5, "Remove unwanted text from album title when doing magic.", NodaTime.Instant.FromUnixTimeTicks(17339787808360764L), null, false, "magic.doRemoveUnwantedTextFromAlbumTitle", null, null, 0, null, "true" },
                    { 507, new Guid("a56c5ab7-9b6b-4dc3-9fe0-df763db5920b"), 5, "Remove unwanted text from song titles when doing magic.", NodaTime.Instant.FromUnixTimeTicks(17339787808360764L), null, false, "magic.doRemoveUnwantedTextFromSongTitles", null, null, 0, null, "true" },
                    { 700, new Guid("42378c54-5f0a-402d-ad2b-178ead17bd70"), 7, "Process of CueSheet files during processing.", NodaTime.Instant.FromUnixTimeTicks(17339787808360764L), null, false, "plugin.cueSheet.enabled", null, null, 0, null, "true" },
                    { 701, new Guid("fb238a89-80ac-4204-9d96-64286fc84cf1"), 7, "Process of M3U files during processing.", NodaTime.Instant.FromUnixTimeTicks(17339787808360764L), null, false, "plugin.m3u.enabled", null, null, 0, null, "true" },
                    { 702, new Guid("b030df4e-1a3e-4a6d-a8a6-9c0b445d2b08"), 7, "Process of NFO files during processing.", NodaTime.Instant.FromUnixTimeTicks(17339787808360764L), null, false, "plugin.nfo.enabled", null, null, 0, null, "true" },
                    { 703, new Guid("06b7c757-84d4-454b-9812-5a9347068426"), 7, "Process of Simple File Verification (SFV) files during processing.", NodaTime.Instant.FromUnixTimeTicks(17339787808360764L), null, false, "plugin.simpleFileVerification.enabled", null, null, 0, null, "true" },
                    { 704, new Guid("f26810bc-ec02-4e3a-b0a9-cc430c86a304"), 7, "If true then all comments will be removed from media files.", NodaTime.Instant.FromUnixTimeTicks(17339787808360764L), null, false, "processing.doDeleteComments", null, null, 0, null, "true" },
                    { 900, new Guid("3cc52b67-1459-4b65-814d-7d4c62361b09"), 9, "Use Bing search engine to find images for albums and artists.", NodaTime.Instant.FromUnixTimeTicks(17339787808360764L), null, false, "searchEngine.bingImage.enabled", null, null, 0, null, "false" },
                    { 901, new Guid("ed75fea2-f832-46ea-a588-e21e52d0a2c0"), 9, "Bing search ApiKey (Ocp-Apim-Subscription-Key), leave blank to disable.", NodaTime.Instant.FromUnixTimeTicks(17339787808360764L), null, false, "searchEngine.bingImage.apiKey", null, null, 0, null, "" },
                    { 902, new Guid("d582d23b-2ff7-4829-87b4-d97d5664b62b"), 9, "User agent to send with Search engine requests.", NodaTime.Instant.FromUnixTimeTicks(17339787808360764L), null, false, "searchEngine.userAgent", null, null, 0, null, "Mozilla/5.0 (X11; Linux x86_64; rv:131.0) Gecko/20100101 Firefox/131.0" },
                    { 903, new Guid("2b23732a-d7a6-4cdb-a0d3-bffb8d48caa8"), 9, "Default page size when performing a search engine search.", NodaTime.Instant.FromUnixTimeTicks(17339787808360764L), null, false, "searchEngine.defaultPageSize", null, null, 0, null, "20" },
                    { 904, new Guid("86093dd0-b035-40df-a8af-7292b6398948"), 9, "Is MusicBrainz search engine enabled.", NodaTime.Instant.FromUnixTimeTicks(17339787808360764L), null, false, "searchEngine.musicbrainz.enabled", null, null, 0, null, "true" },
                    { 905, new Guid("f2e2fbbb-8bec-4e64-9286-10fc181c733a"), 9, "Storage path to hold MusicBrainz downloaded files and SQLite db.", NodaTime.Instant.FromUnixTimeTicks(17339787808360764L), null, false, "searchEngine.musicbrainz.storagePath", null, null, 0, null, "/melodee_test/search-engine-storage/musicbrainz/" },
                    { 906, new Guid("5427a957-d255-4e0b-95e1-ab0dc5380be3"), 9, "Maximum number of batches import from MusicBrainz downloaded db dump (this setting is usually used during debugging), set to zero for unlimited.", NodaTime.Instant.FromUnixTimeTicks(17339787808360764L), null, false, "searchEngine.musicbrainz.importMaximumToProcess", null, null, 0, null, "0" },
                    { 907, new Guid("a38d26ab-7b7c-472b-8085-47d8fdd5d601"), 9, "Number of records to import from MusicBrainz downloaded db dump before commiting to local SQLite database.", NodaTime.Instant.FromUnixTimeTicks(17339787808360764L), null, false, "searchEngine.musicbrainz.importBatchSize", null, null, 0, null, "100000" },
                    { 908, new Guid("df64072e-1690-4382-b7f8-cc136b31b330"), 9, "Timestamp of when last MusicBrainz import was successful.", NodaTime.Instant.FromUnixTimeTicks(17339787808360764L), null, false, "searchEngine.musicbrainz.importLastImportTimestamp", null, null, 0, null, "" },
                    { 1000, new Guid("28ecfc16-1af9-4732-a2bf-37ba768ba568"), 10, "Is scrobbling enabled.", NodaTime.Instant.FromUnixTimeTicks(17339787808360764L), null, false, "scrobbling.enabled", null, null, 0, null, "false" },
                    { 1001, new Guid("bb2e42ab-d55f-4968-a555-2264e2651001"), 10, "Is scrobbling to Last.fm enabled.", NodaTime.Instant.FromUnixTimeTicks(17339787808360764L), null, false, "scrobbling.lastFm.Enabled", null, null, 0, null, "false" },
                    { 1002, new Guid("a254b450-07bc-4972-8335-38a3b02daa24"), 10, "ApiKey used to scrobble to last FM. See https://www.last.fm/api/authentication for more details.", NodaTime.Instant.FromUnixTimeTicks(17339787808360764L), null, false, "scrobbling.lastFm.apikey", null, null, 0, null, "" },
                    { 1100, new Guid("393ef47f-d9fb-4452-8c1d-75def173821a"), 11, "Base URL for Melodee to use when building shareable links and image urls (e.g., 'https://server.domain.com:8080', 'http://server.domain.com').", NodaTime.Instant.FromUnixTimeTicks(17339787808360764L), null, false, "system.baseUrl", null, null, 0, null, "** REQUIRED: THIS MUST BE EDITED **" },
                    { 1101, new Guid("b610c8b8-2230-4015-95e1-95ded3cd0479"), 11, "Is downloading enabled.", NodaTime.Instant.FromUnixTimeTicks(17339787808360764L), null, false, "system.isDownloadingEnabled", null, null, 0, null, "true" },
                    { 1200, new Guid("bc9d0e3f-a30f-4080-817e-6d626045b944"), 12, "Default format for transcoding.", NodaTime.Instant.FromUnixTimeTicks(17339787808360764L), null, false, "transcoding.default", null, null, 0, null, "raw" },
                    { 1201, new Guid("b7acadfc-364b-4308-90b7-e1bd87e60db9"), 12, "Default command to transcode MP3 for streaming.", NodaTime.Instant.FromUnixTimeTicks(17339787808360764L), null, false, "transcoding.command.mp3", null, null, 0, null, "{ 'format': 'Mp3', 'bitrate: 192, 'command': 'ffmpeg -i %s -ss %t -map 0:a:0 -b:a %bk -v 0 -f mp3 -' }" },
                    { 1202, new Guid("3974846f-5d89-487e-bd44-42212bbd296e"), 12, "Default command to transcode using libopus for streaming.", NodaTime.Instant.FromUnixTimeTicks(17339787808360764L), null, false, "transcoding.command.opus", null, null, 0, null, "{ 'format': 'Opus', 'bitrate: 128, 'command': 'ffmpeg -i %s -ss %t -map 0:a:0 -b:a %bk -v 0 -c:a libopus -f opus -' }" },
                    { 1203, new Guid("efeb0402-0326-410e-985c-0aac2d04168e"), 12, "Default command to transcode to aac for streaming.", NodaTime.Instant.FromUnixTimeTicks(17339787808360764L), null, false, "transcoding.command.aac", null, null, 0, null, "{ 'format': 'Aac', 'bitrate: 256, 'command': 'ffmpeg -i %s -ss %t -map 0:a:0 -b:a %bk -v 0 -c:a aac -f adts -' }" },
                    { 1300, new Guid("32550e31-8140-4fce-816c-f3576be57465"), 13, "The maximum value a song number can have for an album. The length of this is used for formatting song names.", NodaTime.Instant.FromUnixTimeTicks(17339787808360764L), null, false, "validation.maximumSongNumber", null, null, 0, null, "9999" },
                    { 1301, new Guid("216b4f4f-4826-4582-b460-360eff06d2dd"), 13, "Minimum allowed year for an album.", NodaTime.Instant.FromUnixTimeTicks(17339787808360764L), null, false, "validation.minimumAlbumYear", null, null, 0, null, "1860" },
                    { 1302, new Guid("1d919e15-d307-4a32-9424-cb181e69e3f4"), 13, "Maximum allowed year for an album.", NodaTime.Instant.FromUnixTimeTicks(17339787808360764L), null, false, "validation.maximumAlbumYear", null, null, 0, null, "2150" }
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
