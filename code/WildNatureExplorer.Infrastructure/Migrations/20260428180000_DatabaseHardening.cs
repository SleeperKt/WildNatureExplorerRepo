using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WildNatureExplorer.Infrastructure.Migrations
{
    /// <summary>CHECK constraints, unique AiFeedbacks per session, dimension audit columns, reference seed, DB roles. Idempotent SQL.</summary>
    public partial class DatabaseHardening : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // CHECK constraints
            migrationBuilder.Sql(@"
                DO $$ BEGIN
                    ALTER TABLE ""AiFeedbacks""
                        ADD CONSTRAINT ""CK_AiFeedbacks_Rating_Range""
                        CHECK (""Rating"" BETWEEN 1 AND 5);
                EXCEPTION WHEN duplicate_object THEN NULL; END $$;

                DO $$ BEGIN
                    ALTER TABLE ""AiMessages""
                        ADD CONSTRAINT ""CK_AiMessages_Role_Allowed""
                        CHECK (""Role"" IN ('user', 'assistant'));
                EXCEPTION WHEN duplicate_object THEN NULL; END $$;

                DO $$ BEGIN
                    ALTER TABLE ""AiMessages""
                        ADD CONSTRAINT ""CK_AiMessages_Content_NotEmpty""
                        CHECK (length(btrim(""Content"")) > 0);
                EXCEPTION WHEN duplicate_object THEN NULL; END $$;

                DO $$ BEGIN
                    ALTER TABLE ""SpeciesLocations""
                        ADD CONSTRAINT ""CK_SpeciesLocations_Latitude_Range""
                        CHECK (""Latitude"" BETWEEN -90 AND 90);
                EXCEPTION WHEN duplicate_object THEN NULL; END $$;

                DO $$ BEGIN
                    ALTER TABLE ""SpeciesLocations""
                        ADD CONSTRAINT ""CK_SpeciesLocations_Longitude_Range""
                        CHECK (""Longitude"" BETWEEN -180 AND 180);
                EXCEPTION WHEN duplicate_object THEN NULL; END $$;

                DO $$ BEGIN
                    ALTER TABLE ""AiSessions""
                        ADD CONSTRAINT ""CK_AiSessions_EndedAt_AfterCreatedAt""
                        CHECK (""EndedAt"" IS NULL OR ""EndedAt"" >= ""CreatedAt"");
                EXCEPTION WHEN duplicate_object THEN NULL; END $$;

                DO $$ BEGIN
                    ALTER TABLE ""AiSessions""
                        ADD CONSTRAINT ""CK_AiSessions_IsEnded_Implies_EndedAt""
                        CHECK (""IsEnded"" = false OR ""EndedAt"" IS NOT NULL);
                EXCEPTION WHEN duplicate_object THEN NULL; END $$;

                DO $$ BEGIN
                    ALTER TABLE ""Users""
                        ADD CONSTRAINT ""CK_Users_CreatedAt_LE_UpdatedAt""
                        CHECK (""CreatedAt"" <= ""UpdatedAt"");
                EXCEPTION WHEN duplicate_object THEN NULL; END $$;

                DO $$ BEGIN
                    ALTER TABLE ""Users""
                        ADD CONSTRAINT ""CK_Users_PasswordHash_NotEmpty""
                        CHECK (length(btrim(""PasswordHash"")) > 0);
                EXCEPTION WHEN duplicate_object THEN NULL; END $$;

                DO $$ BEGIN
                    ALTER TABLE ""Users""
                        ADD CONSTRAINT ""CK_Users_Email_Format""
                        CHECK (""Email"" ~* '^[^@\s]+@[^@\s]+\.[^@\s]+$');
                EXCEPTION WHEN duplicate_object THEN NULL; END $$;
            ");

            // AiFeedbacks: dedupe then unique SessionId index
            migrationBuilder.Sql(@"
                DELETE FROM ""AiFeedbacks"" a
                USING ""AiFeedbacks"" b
                WHERE a.""SessionId"" = b.""SessionId""
                  AND a.""CreatedAt"" < b.""CreatedAt"";

                DROP INDEX IF EXISTS ""IX_AiFeedbacks_SessionId"";

                CREATE UNIQUE INDEX IF NOT EXISTS ""IX_AiFeedbacks_SessionId""
                    ON ""AiFeedbacks"" (""SessionId"");
            ");

            // Sizes / Colors / Habitats / Countries: CreatedAt + UpdatedAt if missing (ordering vs later migrations)
            migrationBuilder.Sql(@"
                DO $$ BEGIN
                    ALTER TABLE ""Sizes"" ADD COLUMN ""CreatedAt"" timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP;
                EXCEPTION WHEN duplicate_column THEN NULL; END $$;

                DO $$ BEGIN
                    ALTER TABLE ""Sizes"" ADD COLUMN ""UpdatedAt"" timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP;
                EXCEPTION WHEN duplicate_column THEN NULL; END $$;

                DO $$ BEGIN
                    ALTER TABLE ""Colors"" ADD COLUMN ""CreatedAt"" timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP;
                EXCEPTION WHEN duplicate_column THEN NULL; END $$;

                DO $$ BEGIN
                    ALTER TABLE ""Colors"" ADD COLUMN ""UpdatedAt"" timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP;
                EXCEPTION WHEN duplicate_column THEN NULL; END $$;

                DO $$ BEGIN
                    ALTER TABLE ""Habitats"" ADD COLUMN ""CreatedAt"" timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP;
                EXCEPTION WHEN duplicate_column THEN NULL; END $$;

                DO $$ BEGIN
                    ALTER TABLE ""Habitats"" ADD COLUMN ""UpdatedAt"" timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP;
                EXCEPTION WHEN duplicate_column THEN NULL; END $$;

                DO $$ BEGIN
                    ALTER TABLE ""Countries"" ADD COLUMN ""CreatedAt"" timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP;
                EXCEPTION WHEN duplicate_column THEN NULL; END $$;

                DO $$ BEGIN
                    ALTER TABLE ""Countries"" ADD COLUMN ""UpdatedAt"" timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP;
                EXCEPTION WHEN duplicate_column THEN NULL; END $$;
            ");

            // Roles app_read, app_write, wne_admin + grants
            migrationBuilder.Sql(@"
                DO $$ BEGIN
                    CREATE ROLE app_read NOINHERIT;
                EXCEPTION WHEN duplicate_object THEN NULL; END $$;

                DO $$ BEGIN
                    CREATE ROLE app_write NOINHERIT;
                EXCEPTION WHEN duplicate_object THEN NULL; END $$;

                DO $$ BEGIN
                    CREATE ROLE wne_admin NOINHERIT;
                EXCEPTION WHEN duplicate_object THEN NULL; END $$;

                -- Schema usage
                GRANT USAGE ON SCHEMA public TO app_read, app_write, wne_admin;

                -- Read-only role: SELECT on existing + future tables
                GRANT SELECT ON ALL TABLES IN SCHEMA public TO app_read;
                ALTER DEFAULT PRIVILEGES IN SCHEMA public
                    GRANT SELECT ON TABLES TO app_read;

                -- Read/Write role used by the API
                GRANT SELECT, INSERT, UPDATE, DELETE ON ALL TABLES IN SCHEMA public TO app_write;
                GRANT USAGE, SELECT ON ALL SEQUENCES IN SCHEMA public TO app_write;
                ALTER DEFAULT PRIVILEGES IN SCHEMA public
                    GRANT SELECT, INSERT, UPDATE, DELETE ON TABLES TO app_write;
                ALTER DEFAULT PRIVILEGES IN SCHEMA public
                    GRANT USAGE, SELECT ON SEQUENCES TO app_write;

                -- Admin role can do anything inside the public schema (still NOT a superuser)
                GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA public TO wne_admin;
                GRANT ALL PRIVILEGES ON ALL SEQUENCES IN SCHEMA public TO wne_admin;
                ALTER DEFAULT PRIVILEGES IN SCHEMA public
                    GRANT ALL PRIVILEGES ON TABLES TO wne_admin;
                ALTER DEFAULT PRIVILEGES IN SCHEMA public
                    GRANT ALL PRIVILEGES ON SEQUENCES TO wne_admin;

                -- Explicitly revoke the dangerous default
                REVOKE CREATE ON SCHEMA public FROM PUBLIC;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                DO $$ BEGIN
                    REVOKE ALL PRIVILEGES ON ALL TABLES IN SCHEMA public FROM app_read, app_write, wne_admin;
                    REVOKE ALL PRIVILEGES ON ALL SEQUENCES IN SCHEMA public FROM app_write, wne_admin;
                    REVOKE USAGE ON SCHEMA public FROM app_read, app_write, wne_admin;
                EXCEPTION WHEN undefined_object THEN NULL; END $$;

                DO $$ BEGIN DROP ROLE IF EXISTS wne_admin; EXCEPTION WHEN OTHERS THEN NULL; END $$;
                DO $$ BEGIN DROP ROLE IF EXISTS app_write; EXCEPTION WHEN OTHERS THEN NULL; END $$;
                DO $$ BEGIN DROP ROLE IF EXISTS app_read;  EXCEPTION WHEN OTHERS THEN NULL; END $$;
            ");

            migrationBuilder.Sql(@"
                DROP INDEX IF EXISTS ""IX_AiFeedbacks_SessionId"";
                CREATE INDEX IF NOT EXISTS ""IX_AiFeedbacks_SessionId""
                    ON ""AiFeedbacks"" (""SessionId"");
            ");

            migrationBuilder.Sql(@"
                ALTER TABLE ""AiFeedbacks""    DROP CONSTRAINT IF EXISTS ""CK_AiFeedbacks_Rating_Range"";
                ALTER TABLE ""AiMessages""     DROP CONSTRAINT IF EXISTS ""CK_AiMessages_Role_Allowed"";
                ALTER TABLE ""AiMessages""     DROP CONSTRAINT IF EXISTS ""CK_AiMessages_Content_NotEmpty"";
                ALTER TABLE ""SpeciesLocations"" DROP CONSTRAINT IF EXISTS ""CK_SpeciesLocations_Latitude_Range"";
                ALTER TABLE ""SpeciesLocations"" DROP CONSTRAINT IF EXISTS ""CK_SpeciesLocations_Longitude_Range"";
                ALTER TABLE ""AiSessions""     DROP CONSTRAINT IF EXISTS ""CK_AiSessions_EndedAt_AfterCreatedAt"";
                ALTER TABLE ""AiSessions""     DROP CONSTRAINT IF EXISTS ""CK_AiSessions_IsEnded_Implies_EndedAt"";
                ALTER TABLE ""Users""          DROP CONSTRAINT IF EXISTS ""CK_Users_CreatedAt_LE_UpdatedAt"";
                ALTER TABLE ""Users""          DROP CONSTRAINT IF EXISTS ""CK_Users_PasswordHash_NotEmpty"";
                ALTER TABLE ""Users""          DROP CONSTRAINT IF EXISTS ""CK_Users_Email_Format"";
            ");
        }
    }
}
