using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tijdperk.Scenematic.Data.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class Update : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ErrorMessage",
                table: "GenerateCoverJobs");

            migrationBuilder.DropColumn(
                name: "FailureReason",
                table: "GenerateCoverJobs");

            migrationBuilder.CreateTable(
                name: "GenerateCoverJobFailureResults",
                columns: table => new
                {
                    JobId = table.Column<Guid>(type: "uuid", nullable: false),
                    FailureReason = table.Column<int>(type: "integer", nullable: false),
                    ErrorMessage = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GenerateCoverJobFailureResults", x => x.JobId);
                    table.ForeignKey(
                        name: "FK_GenerateCoverJobFailureResults_GenerateCoverJobs_JobId",
                        column: x => x.JobId,
                        principalTable: "GenerateCoverJobs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GenerateCoverJobSuccessResults",
                columns: table => new
                {
                    JobId = table.Column<Guid>(type: "uuid", nullable: false),
                    MainCoverPath = table.Column<string>(type: "text", nullable: false),
                    DebounceCoverPath = table.Column<string>(type: "text", nullable: false),
                    ProcessingCoverPath = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GenerateCoverJobSuccessResults", x => x.JobId);
                    table.ForeignKey(
                        name: "FK_GenerateCoverJobSuccessResults_GenerateCoverJobs_JobId",
                        column: x => x.JobId,
                        principalTable: "GenerateCoverJobs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GenerateCoverJobFailureResults");

            migrationBuilder.DropTable(
                name: "GenerateCoverJobSuccessResults");

            migrationBuilder.AddColumn<string>(
                name: "ErrorMessage",
                table: "GenerateCoverJobs",
                type: "character varying(4000)",
                maxLength: 4000,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FailureReason",
                table: "GenerateCoverJobs",
                type: "integer",
                nullable: true);
        }
    }
}
