using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MoodRadar.API.Migrations
{
    /// <inheritdoc />
    public partial class DropCategoryColumnFromEvents : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Keep migration safe for both old and new schemas.
            migrationBuilder.Sql(@"DO $$
BEGIN
    IF EXISTS (
        SELECT 1
        FROM information_schema.columns
        WHERE table_schema = 'public'
          AND table_name = 'Events'
          AND column_name = 'Category'
    ) THEN
        ALTER TABLE ""Events"" DROP COLUMN ""Category"";
    END IF;
END $$;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DO $$
BEGIN
    IF EXISTS (
        SELECT 1
        FROM information_schema.tables
        WHERE table_schema = 'public'
          AND table_name = 'Events'
    )
    AND NOT EXISTS (
        SELECT 1
        FROM information_schema.columns
        WHERE table_schema = 'public'
          AND table_name = 'Events'
          AND column_name = 'Category'
    ) THEN
        ALTER TABLE ""Events"" ADD COLUMN ""Category"" character varying(100) NOT NULL DEFAULT '';
    END IF;
END $$;");
        }
    }
}
