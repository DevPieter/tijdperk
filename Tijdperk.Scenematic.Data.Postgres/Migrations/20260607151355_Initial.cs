using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tijdperk.Scenematic.Data.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CoverMetas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AlbumTitle = table.Column<string>(type: "text", nullable: false),
                    AlbumArtist = table.Column<string>(type: "text", nullable: false),
                    AlbumCoverImageUrl = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CoverMetas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CoverVoteMetas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CoverMetaId = table.Column<Guid>(type: "uuid", nullable: false),
                    VoteLevel = table.Column<int>(type: "integer", nullable: false),
                    AvatarUrl = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CoverVoteMetas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CoverVoteMetas_CoverMetas_CoverMetaId",
                        column: x => x.CoverMetaId,
                        principalTable: "CoverMetas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GenerateCoverJobs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CoverMetaId = table.Column<Guid>(type: "uuid", nullable: true),
                    CoverMetaHash = table.Column<string>(type: "text", nullable: false),
                    State = table.Column<int>(type: "integer", nullable: false),
                    FailureReason = table.Column<int>(type: "integer", nullable: true),
                    ErrorMessage = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    QueuedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    StartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    FailedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GenerateCoverJobs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GenerateCoverJobs_CoverMetas_CoverMetaId",
                        column: x => x.CoverMetaId,
                        principalTable: "CoverMetas",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_CoverVoteMetas_CoverMetaId",
                table: "CoverVoteMetas",
                column: "CoverMetaId");

            migrationBuilder.CreateIndex(
                name: "IX_GenerateCoverJobs_CoverMetaId",
                table: "GenerateCoverJobs",
                column: "CoverMetaId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CoverVoteMetas");

            migrationBuilder.DropTable(
                name: "GenerateCoverJobs");

            migrationBuilder.DropTable(
                name: "CoverMetas");
        }
    }
}
