using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WildNatureExplorer.Infrastructure.Migrations;

/// <summary>
/// Originally duplicated columns/indexes already applied idempotently by
/// <see cref="AddIndexesAndAuditTrailFixed"/>. This migration now only adds the
/// AiSessions → Users FK that was missing from earlier snapshots.
/// </summary>
public partial class IndexesFix : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            DO $$
            BEGIN
                ALTER TABLE "AiSessions"
                    ADD CONSTRAINT "FK_AiSessions_Users_UserId"
                    FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE CASCADE;
            EXCEPTION
                WHEN duplicate_object THEN NULL;
            END $$;
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            ALTER TABLE "AiSessions" DROP CONSTRAINT IF EXISTS "FK_AiSessions_Users_UserId";
            """);
    }
}
