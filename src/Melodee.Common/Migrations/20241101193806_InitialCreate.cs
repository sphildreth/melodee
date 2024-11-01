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
                    RealName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    Roles = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    AlbumCount = table.Column<int>(type: "integer", nullable: false),
                    SongCount = table.Column<int>(type: "integer", nullable: false),
                    Biography = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    IsLocked = table.Column<bool>(type: "boolean", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    ApiKey = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<Instant>(type: "timestamp with time zone", nullable: false),
                    LastUpdatedAt = table.Column<Instant>(type: "timestamp with time zone", nullable: true),
                    Tags = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Notes = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    Description = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    SortName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    AlternateNames = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    LastPlayedAt = table.Column<Instant>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("PK_Artists", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Libraries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Path = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
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
                    PasswordHash = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
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
                    AlbumStatus = table.Column<short>(type: "smallint", nullable: false),
                    AlbumType = table.Column<short>(type: "smallint", nullable: false),
                    OriginalReleaseDate = table.Column<LocalDate>(type: "date", nullable: false),
                    ReleaseDate = table.Column<LocalDate>(type: "date", nullable: false),
                    IsCompilation = table.Column<bool>(type: "boolean", nullable: false),
                    SongCount = table.Column<short>(type: "smallint", nullable: true),
                    DiscCount = table.Column<short>(type: "smallint", nullable: true),
                    Duration = table.Column<int>(type: "integer", nullable: false),
                    Genres = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    IsLocked = table.Column<bool>(type: "boolean", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    ApiKey = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<Instant>(type: "timestamp with time zone", nullable: false),
                    LastUpdatedAt = table.Column<Instant>(type: "timestamp with time zone", nullable: true),
                    Tags = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Notes = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    Description = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    SortName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    AlternateNames = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    LastPlayedAt = table.Column<Instant>(type: "timestamp with time zone", nullable: true),
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
                    LibraryId = table.Column<int>(type: "integer", nullable: false),
                    ForArtistId = table.Column<int>(type: "integer", nullable: true),
                    ForAlbumId = table.Column<int>(type: "integer", nullable: true),
                    NewArtistsCount = table.Column<int>(type: "integer", nullable: false),
                    NewAlbumsCount = table.Column<int>(type: "integer", nullable: false),
                    NewSongsCount = table.Column<int>(type: "integer", nullable: false),
                    DurationInMs = table.Column<long>(type: "bigint", nullable: false),
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
                    AlbumId = table.Column<int>(type: "integer", nullable: false),
                    DiscNumber = table.Column<int>(type: "integer", nullable: false),
                    SongCount = table.Column<short>(type: "smallint", nullable: true),
                    Title = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AlbumDiscs", x => new { x.AlbumId, x.DiscNumber });
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
                    AlbumDiscAlbumId = table.Column<int>(type: "integer", nullable: false),
                    AlbumDiscDiscNumber = table.Column<int>(type: "integer", nullable: false),
                    Title = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    TrackNumber = table.Column<int>(type: "integer", nullable: false),
                    FilePath = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    FileName = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    Lyrics = table.Column<string>(type: "character varying(62000)", maxLength: 62000, nullable: true),
                    FileSize = table.Column<int>(type: "integer", nullable: false),
                    FileHash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    PartTitles = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Duration = table.Column<int>(type: "integer", nullable: false),
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
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    SortName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    AlternateNames = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    LastPlayedAt = table.Column<Instant>(type: "timestamp with time zone", nullable: true),
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
                        name: "FK_Songs_AlbumDiscs_AlbumDiscAlbumId_AlbumDiscDiscNumber",
                        columns: x => new { x.AlbumDiscAlbumId, x.AlbumDiscDiscNumber },
                        principalTable: "AlbumDiscs",
                        principalColumns: new[] { "AlbumId", "DiscNumber" },
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
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    SortName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    AlternateNames = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    LastPlayedAt = table.Column<Instant>(type: "timestamp with time zone", nullable: true),
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
                    Duration = table.Column<int>(type: "integer", nullable: false),
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
                table: "Settings",
                columns: new[] { "Id", "ApiKey", "Category", "Comment", "CreatedAt", "Description", "IsLocked", "Key", "LastUpdatedAt", "Notes", "SortOrder", "Tags", "Value" },
                values: new object[,]
                {
                    { 1, new Guid("5dd9e98b-d3a4-40ec-af91-487abaef36fc"), null, "Files in this directory are scanned and Album information is gathered.", NodaTime.Instant.FromUnixTimeTicks(17304898857849431L), null, false, "directory.inbound", null, null, 0, null, "/storage/inbound" },
                    { 2, new Guid("d6532389-c512-4841-9fac-751b20923ca5"), null, "This is the main storage library (holds all previously scanned/edited/approved Albums. This is directory is where the API gets media file data from.", NodaTime.Instant.FromUnixTimeTicks(17304898857849431L), null, false, "directory.library", null, null, 0, null, "/storage/library" },
                    { 3, new Guid("50032f5d-be48-4ffd-9761-e89832f2b568"), null, "When one is happy with scan results one can select Albums (and all associated files) to move to this directory in the proper directory structure.", NodaTime.Instant.FromUnixTimeTicks(17304898857849431L), null, false, "directory.staging", null, null, 0, null, "/storage/staging" },
                    { 4, new Guid("a409fb83-8e33-4b7f-8e1c-f336483d82a4"), null, "Add a default filter to show only albums with this or less number of songs.", NodaTime.Instant.FromUnixTimeTicks(17304898857849431L), null, false, "filtering.lessThanSongCount", null, null, 0, null, "3" },
                    { 5, new Guid("65b919a2-ecdc-4996-81f4-1074357a4fac"), null, "Add a default filter to show only albums with this or less duration.", NodaTime.Instant.FromUnixTimeTicks(17304898857849431L), null, false, "filtering.lessThanDuration", null, null, 0, null, "720000" },
                    { 6, new Guid("cb6e9e5e-4adf-4d92-bd9b-db18f619744a"), null, "Maximum number of albums to scan when processing inbound directory.", NodaTime.Instant.FromUnixTimeTicks(17304898857849431L), null, false, "processing.stagingDirectoryScanLimit", null, null, 0, null, "250" },
                    { 7, new Guid("33cc93b1-cbde-4011-97fe-71a7900f0d28"), null, "Default page size when view including pagination.", NodaTime.Instant.FromUnixTimeTicks(17304898857849431L), null, false, "defaults.pagesize", null, null, 0, null, "100" },
                    { 8, new Guid("1e510e32-3565-4cdf-aa8e-c09f50d0bac5"), null, "When true then move the Melodee.json data file when moving Albums, otherwise delete.", NodaTime.Instant.FromUnixTimeTicks(17304898857849431L), null, false, "processing.moveMelodeeJsonDataFileToLibrary", null, null, 0, null, "false" },
                    { 9, new Guid("db977dff-4d7e-49ff-8487-95a306f96375"), null, "Amount of time to display a Toast then auto-close (in milliseconds.)", NodaTime.Instant.FromUnixTimeTicks(17304898857849431L), null, false, "userinterface.toastAutoCloseTime", null, null, 0, null, "2000" },
                    { 10, new Guid("138f4a66-ad5f-4500-8444-240515b8e3d5"), null, "Short Format to use when displaying full dates.", NodaTime.Instant.FromUnixTimeTicks(17304898857849431L), null, false, "formatting.dateTimeDisplayFormatShort", null, null, 0, null, "yyyyMMdd HH:mm" },
                    { 11, new Guid("f69acfe0-67c6-4622-8996-38d84b84c852"), null, "Format to use when displaying activity related dates (e.g. processing messages)", NodaTime.Instant.FromUnixTimeTicks(17304898857849431L), null, false, "formatting.dateTimeDisplayActivityFormat", null, null, 0, null, "HH:mm:ss.fff" },
                    { 12, new Guid("9dcf2354-45b6-4ec4-8316-1713b682fe29"), null, "List of ignored articles when scanning media (pipe delimited).", NodaTime.Instant.FromUnixTimeTicks(17304898857849431L), null, false, "processing.ignoredArticles", null, null, 0, null, "THE|EL|LA|LOS|LAS|LE|LES|OS|AS|O|A" },
                    { 13, new Guid("e018d65b-9c57-4cf6-b49f-b9f8cefbb3a8"), null, "Is Magic processing enabled.", NodaTime.Instant.FromUnixTimeTicks(17304898857849431L), null, false, "magic.enabled", null, null, 0, null, "true" },
                    { 14, new Guid("ed5e2678-da5e-4cfc-9547-f7ba77ca2456"), null, "Renumber songs when doing magic processing.", NodaTime.Instant.FromUnixTimeTicks(17304898857849431L), null, false, "magic.doRenumberSongs", null, null, 0, null, "true" },
                    { 15, new Guid("11d360d3-f952-4238-8258-a953d628a3f1"), null, "Remove featured artists from song artist when doing magic.", NodaTime.Instant.FromUnixTimeTicks(17304898857849431L), null, false, "magic.doRemoveFeaturingArtistFromSongArtist", null, null, 0, null, "true" },
                    { 16, new Guid("bc324096-a9d8-4716-aef2-6e9346be892b"), null, "Remove featured artists from song title when doing magic.", NodaTime.Instant.FromUnixTimeTicks(17304898857849431L), null, false, "magic.doRemoveFeaturingArtistFromSongTitle", null, null, 0, null, "true" },
                    { 17, new Guid("3d997a48-5359-4fb0-b38e-9464cd68ba1f"), null, "Replace song artist separators with standard ID3 separator ('/') when doing magic.", NodaTime.Instant.FromUnixTimeTicks(17304898857849431L), null, false, "magic.doReplaceSongsArtistSeparators", null, null, 0, null, "true" },
                    { 18, new Guid("42a7a53c-7454-41cb-a725-88d7aea97db5"), null, "Set the song year to current year if invalid or missing when doing magic.", NodaTime.Instant.FromUnixTimeTicks(17304898857849431L), null, false, "magic.doSetYearToCurrentIfInvalid", null, null, 0, null, "true" },
                    { 19, new Guid("045ff8c0-17d2-4643-ae45-42572b492814"), null, "Remove unwanted text from album title when doing magic.", NodaTime.Instant.FromUnixTimeTicks(17304898857849431L), null, false, "magic.doRemoveUnwantedTextFromAlbumTitle", null, null, 0, null, "true" },
                    { 20, new Guid("27888233-a9c7-456d-a407-d7da69212969"), null, "Enable Melodee to convert non-mp3 media files during processing.", NodaTime.Instant.FromUnixTimeTicks(17304898857849431L), null, false, "conversion.enabled", null, null, 0, null, "true" },
                    { 21, new Guid("38469d1a-ffc3-4799-8755-bc19bcab8888"), null, "Bitrate to convert non-mp3 media files during processing.", NodaTime.Instant.FromUnixTimeTicks(17304898857849431L), null, false, "conversion.bitrate", null, null, 0, null, "384" },
                    { 22, new Guid("0cf7ab5d-f2c2-4b56-bace-ea53c15a85dd"), null, "Vbr to convert non-mp3 media files during processing.", NodaTime.Instant.FromUnixTimeTicks(17304898857849431L), null, false, "conversion.vbrLevel", null, null, 0, null, "4" },
                    { 23, new Guid("8672e3fa-22e1-48a8-a59c-758e237173ed"), null, "Sampling rate to convert non-mp3 media files during processing.", NodaTime.Instant.FromUnixTimeTicks(17304898857849431L), null, false, "conversion.samplingRate", null, null, 0, null, "48000" },
                    { 24, new Guid("f310cafe-325c-4204-9d18-75f2496837ab"), null, "Process of CueSheet files during processing.", NodaTime.Instant.FromUnixTimeTicks(17304898857849431L), null, false, "plugin.cueSheet.enabled", null, null, 0, null, "true" },
                    { 25, new Guid("9a1dbfb9-75b3-4ca1-8e2d-b26327949290"), null, "Process of M3U files during processing.", NodaTime.Instant.FromUnixTimeTicks(17304898857849431L), null, false, "plugin.m3u.enabled", null, null, 0, null, "true" },
                    { 26, new Guid("d2828a22-bfea-473f-953f-ed2d6bbda99a"), null, "Process of NFO files during processing.", NodaTime.Instant.FromUnixTimeTicks(17304898857849431L), null, false, "plugin.nfo.enabled", null, null, 0, null, "true" },
                    { 27, new Guid("c1a8c270-00fe-4dee-91bd-794878c57f41"), null, "Process of Simple File Verification (SFV) files during processing.", NodaTime.Instant.FromUnixTimeTicks(17304898857849431L), null, false, "plugin.simpleFileVerification.enabled", null, null, 0, null, "true" },
                    { 28, new Guid("5c3450d0-ca05-42a7-9a04-248d2dafad87"), null, "Fragments of artist names to replace (JSON Dictionary).", NodaTime.Instant.FromUnixTimeTicks(17304898857849431L), null, false, "processing.artistNameReplacements", null, null, 0, null, "{'AC/DC', ['AC; DC', 'AC;DC', 'AC/ DC', 'AC DC'] , 'Love/Hate', ['Love; Hate', 'Love;Hate', 'Love/ Hate', 'Love Hate'] }" },
                    { 29, new Guid("5c955b61-df76-4647-aa89-0cba25707b50"), null, "If OrigAlbumYear [TOR, TORY, TDOR] value is invalid use current year.", NodaTime.Instant.FromUnixTimeTicks(17304898857849431L), null, false, "processing.doUseCurrentYearAsDefaultOrigAlbumYearValue", null, null, 0, null, "true" },
                    { 30, new Guid("91e24acc-7491-48b1-aeff-512f0192f80f"), null, "Delete original files when processing. When false a copy if made, else original is deleted after processed.", NodaTime.Instant.FromUnixTimeTicks(17304898857849431L), null, false, "processing.doDeleteOriginal", null, null, 0, null, "true" },
                    { 31, new Guid("d0f1a835-176a-4ffd-87e2-d669d74b50a8"), null, "Extension to add to file when converted, leave blank to disable.", NodaTime.Instant.FromUnixTimeTicks(17304898857849431L), null, false, "processing.convertedExtension", null, null, 0, null, "_converted" },
                    { 32, new Guid("6d4ace11-b989-4d8a-82b6-7e72331a677f"), null, "Extension to add to file when processed, leave blank to disable.", NodaTime.Instant.FromUnixTimeTicks(17304898857849431L), null, false, "processing.processedExtension", null, null, 0, null, "_processed" },
                    { 33, new Guid("477ca776-0110-44bb-9c00-9049d2725af9"), null, "Extension to add to file to indicate other files in the same category where processed and this file was skipped during processing, leave blank to disable.", NodaTime.Instant.FromUnixTimeTicks(17304898857849431L), null, false, "processing.skippedExtension", null, null, 0, null, "_skipped" },
                    { 34, new Guid("6b4a8675-1eb9-4451-9dc3-7a8deaa232c1"), null, "When processing over write any existing Melodee data files, otherwise skip and leave in place.", NodaTime.Instant.FromUnixTimeTicks(17304898857849431L), null, false, "processing.doOverrideExistingMelodeeDataFiles", null, null, 0, null, "true" },
                    { 35, new Guid("4afea052-a785-4504-ae2d-1687edd93efd"), null, "Include any embedded images from media files into the Melodee data file.", NodaTime.Instant.FromUnixTimeTicks(17304898857849431L), null, false, "processing.doLoadEmbeddedImages", null, null, 0, null, "true" },
                    { 36, new Guid("077bb596-7b2f-4166-b6c7-498ff4330ab7"), null, "The maximum number of files to process, set to zero for infinite.", NodaTime.Instant.FromUnixTimeTicks(17304898857849431L), null, false, "processing.maximumProcessingCount", null, null, 0, null, "0" },
                    { 37, new Guid("e6a13505-ed8c-400d-814d-8a8ab0140425"), null, "Maximum allowed length of album directory name.", NodaTime.Instant.FromUnixTimeTicks(17304898857849431L), null, false, "processing.maximumAlbumDirectoryNameLength", null, null, 0, null, "255" },
                    { 38, new Guid("9f8bae3b-cd6a-4f27-b5c5-8cc4de106e5b"), null, "Maximum allowed length of artist directory name.", NodaTime.Instant.FromUnixTimeTicks(17304898857849431L), null, false, "processing.maximumArtistDirectoryNameLength", null, null, 0, null, "255" },
                    { 39, new Guid("a6787a85-2ad6-4b4a-a68d-8aa00bd55d22"), null, "Fragments to remove from album titles (JSON array).", NodaTime.Instant.FromUnixTimeTicks(17304898857849431L), null, false, "processing.albumTitleRemovals", null, null, 0, null, "['^', '~', '#']" },
                    { 40, new Guid("dc281926-fd12-4066-8c4a-7654d40f017b"), null, "Fragments to remove from song titles (JSON array).", NodaTime.Instant.FromUnixTimeTicks(17304898857849431L), null, false, "processing.songTitleRemovals", null, null, 0, null, "[';', '(Remaster)', 'Remaster']" },
                    { 41, new Guid("db808f2b-263f-404d-b6bc-c2a9e15d5dd8"), null, "Continue processing if an error is encountered.", NodaTime.Instant.FromUnixTimeTicks(17304898857849431L), null, false, "processing.doContinueOnDirectoryProcessingErrors", null, null, 0, null, "true" },
                    { 42, new Guid("1a68660f-0a1e-492f-83c1-173c7344c702"), null, "When true then move Album Melodee json files to the Staging directory.", NodaTime.Instant.FromUnixTimeTicks(17304898857849431L), null, false, "processing.doMoveMelodeeDataFileToStagingDirectory", null, null, 0, null, "true" },
                    { 43, new Guid("46bee281-c05f-431b-85d0-049db930ff28"), null, "Is scripting enabled.", NodaTime.Instant.FromUnixTimeTicks(17304898857849431L), null, false, "scripting.enabled", null, null, 0, null, "false" },
                    { 44, new Guid("b9eaac67-1614-424d-891e-786656d564f0"), null, "Script to run before processing the inbound directory, leave blank to disable.", NodaTime.Instant.FromUnixTimeTicks(17304898857849431L), null, false, "scripting.preDiscoveryScript", null, null, 0, null, "" },
                    { 45, new Guid("533d787f-4965-44d3-8167-b0105ef135f7"), null, "Script to run after processing the inbound directory, leave blank to disable.", NodaTime.Instant.FromUnixTimeTicks(17304898857849431L), null, false, "scripting.postDiscoveryScript", null, null, 0, null, "" },
                    { 46, new Guid("35673fcd-25db-440c-a2d2-1a552bf93a45"), null, "The maximum value a media number can have for an album.", NodaTime.Instant.FromUnixTimeTicks(17304898857849431L), null, false, "validation.maximumMediaNumber", null, null, 0, null, "500" },
                    { 47, new Guid("e5ce3283-800e-482d-9880-834278261e3f"), null, "The maximum value a song number can have for an album.", NodaTime.Instant.FromUnixTimeTicks(17304898857849431L), null, false, "validation.maximumSongNumber", null, null, 0, null, "1000" },
                    { 48, new Guid("d5ce7a4f-a0f5-45ff-a14a-50e1e7ace9b0"), null, "Minimum allowed year for an album.", NodaTime.Instant.FromUnixTimeTicks(17304898857849431L), null, false, "validation.minimumAlbumYear", null, null, 0, null, "1860" },
                    { 49, new Guid("7177c1f2-1038-48a1-a6d5-019353880fbc"), null, "Maximum allowed year for an album.", NodaTime.Instant.FromUnixTimeTicks(17304898857849431L), null, false, "validation.maximumAlbumYear", null, null, 0, null, "2150" }
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
                name: "IX_Artists_SortName",
                table: "Artists",
                column: "SortName");

            migrationBuilder.CreateIndex(
                name: "IX_Bookmarks_ApiKey",
                table: "Bookmarks",
                column: "ApiKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Bookmarks_Name",
                table: "Bookmarks",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Bookmarks_SongId",
                table: "Bookmarks",
                column: "SongId");

            migrationBuilder.CreateIndex(
                name: "IX_Bookmarks_SortName",
                table: "Bookmarks",
                column: "SortName");

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
                name: "IX_LibraryScanHistories_ApiKey",
                table: "LibraryScanHistories",
                column: "ApiKey",
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
                name: "IX_Songs_AlbumDiscAlbumId_AlbumDiscDiscNumber",
                table: "Songs",
                columns: new[] { "AlbumDiscAlbumId", "AlbumDiscDiscNumber" });

            migrationBuilder.CreateIndex(
                name: "IX_Songs_AlbumDiscId_TrackNumber",
                table: "Songs",
                columns: new[] { "AlbumDiscId", "TrackNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Songs_ApiKey",
                table: "Songs",
                column: "ApiKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Songs_Name",
                table: "Songs",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Songs_SortName",
                table: "Songs",
                column: "SortName");

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
                name: "Libraries");

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
        }
    }
}
