using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WildNatureExplorer.Infrastructure.Migrations
{
    /// <summary>
    /// Fix for a regression introduced in <c>20260428180000_DatabaseHardening</c>:
    /// the <c>CK_AiMessages_Role_Allowed</c> CHECK constraint was added with the
    /// allowed set <c>('user','assistant')</c>, but <c>AiService</c> has always
    /// written rows with <c>Role = 'User'</c> / <c>'AI'</c>. Every AI analyze /
    /// chat call therefore failed with SQLSTATE 23514.
    ///
    /// This migration drops the broken constraint and re-creates it using the
    /// values the application code actually writes. No data fixup is needed —
    /// the original constraint was strict enough to block any non-conforming
    /// rows from ever being inserted, so existing rows already conform.
    /// </summary>
    public partial class FixAiMessagesRoleConstraint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                ALTER TABLE ""AiMessages""
                    DROP CONSTRAINT IF EXISTS ""CK_AiMessages_Role_Allowed"";

                DO $$ BEGIN
                    ALTER TABLE ""AiMessages""
                        ADD CONSTRAINT ""CK_AiMessages_Role_Allowed""
                        CHECK (""Role"" IN ('User', 'AI'));
                EXCEPTION WHEN duplicate_object THEN NULL; END $$;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                ALTER TABLE ""AiMessages""
                    DROP CONSTRAINT IF EXISTS ""CK_AiMessages_Role_Allowed"";

                DO $$ BEGIN
                    ALTER TABLE ""AiMessages""
                        ADD CONSTRAINT ""CK_AiMessages_Role_Allowed""
                        CHECK (""Role"" IN ('user', 'assistant'));
                EXCEPTION WHEN duplicate_object THEN NULL; END $$;
            ");
        }
    }
}
