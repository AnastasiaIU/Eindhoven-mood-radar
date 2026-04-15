using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace MoodRadar.API.Migrations
{
    /// <inheritdoc />
    public partial class FixPsvMatchKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "Weathers",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "PsvMatches",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MatchDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    HomeAway = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    KickOffTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Opponent = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PsvMatches", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ZoneSnapshots",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EventCount = table.Column<int>(type: "integer", nullable: false),
                    PsvMatchCount = table.Column<int>(type: "integer", nullable: false),
                    WeatherSummary = table.Column<string>(type: "text", nullable: true),
                    PredictionScore = table.Column<double>(type: "double precision", nullable: true),
                    RawJson = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ZoneSnapshots", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PsvMatches_MatchDate_Opponent",
                table: "PsvMatches",
                columns: new[] { "MatchDate", "Opponent" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PsvMatches");

            migrationBuilder.DropTable(
                name: "ZoneSnapshots");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "Weathers");
        }
    }
}
