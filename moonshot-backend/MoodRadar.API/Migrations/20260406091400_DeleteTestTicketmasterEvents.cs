using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MoodRadar.API.Migrations
{
    /// <inheritdoc />
    public partial class DeleteTestTicketmasterEvents : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Remove old Ticketmaster records only when the Events table exists.
            migrationBuilder.Sql(@"DO $$
BEGIN
    IF EXISTS (
        SELECT 1
        FROM information_schema.tables
        WHERE table_schema = 'public'
          AND table_name = 'Events'
    ) THEN
        DELETE FROM public.""Events"" WHERE ""Source"" = 'Ticketmaster';
    END IF;
END $$;");

            // Drop Category only when present to keep this migration idempotent.
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

            // Reset sequence when it exists. Restart from 1 for PostgreSQL identity defaults.
            migrationBuilder.Sql(@"DO $$
BEGIN
    IF EXISTS (
        SELECT 1
        FROM pg_class
        WHERE relkind = 'S'
          AND relname = 'Events_id_seq'
    ) THEN
        ALTER SEQUENCE ""Events_id_seq"" RESTART WITH 1;
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
