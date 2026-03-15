using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Inventory.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ProcessedByGUID : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                ALTER TABLE "Sales"
                ALTER COLUMN "ProcessedBy" TYPE uuid
                USING CASE
                    WHEN "ProcessedBy" ~* '^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$' THEN "ProcessedBy"::uuid
                    ELSE '00000000-0000-0000-0000-000000000000'::uuid
                END;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                ALTER TABLE "Sales"
                ALTER COLUMN "ProcessedBy" TYPE text
                USING "ProcessedBy"::text;
                """);
        }
    }
}
