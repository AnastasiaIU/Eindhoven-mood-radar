using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MoodRadar.API.Migrations
{
    /// <inheritdoc />
    public partial class AddRawDataToEvent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RawData",
                table: "Events",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RawData",
                table: "Events");
        }
    }
}
