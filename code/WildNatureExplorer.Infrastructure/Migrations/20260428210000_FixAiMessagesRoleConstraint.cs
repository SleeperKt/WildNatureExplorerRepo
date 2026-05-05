using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WildNatureExplorer.Infrastructure.Migrations
{
    /// <summary>Align <c>CK_AiMessages_Role_Allowed</c> with application role values (<c>User</c>/<c>AI</c>) after an incorrect lowercase constraint.</summary>
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
