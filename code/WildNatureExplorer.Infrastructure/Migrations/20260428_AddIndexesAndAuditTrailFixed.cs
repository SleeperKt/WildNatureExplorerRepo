using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WildNatureExplorer.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddIndexesAndAuditTrailFixed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Use raw SQL to safely add columns only if they don't exist
            migrationBuilder.Sql(@"
                -- Sizes table
                DO $$ BEGIN
                    ALTER TABLE ""Sizes"" ADD COLUMN ""CreatedAt"" timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP;
                EXCEPTION WHEN duplicate_column THEN NULL;
                END $$;
                
                DO $$ BEGIN
                    ALTER TABLE ""Sizes"" ADD COLUMN ""UpdatedAt"" timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP;
                EXCEPTION WHEN duplicate_column THEN NULL;
                END $$;
                
                -- Species table
                DO $$ BEGIN
                    ALTER TABLE ""Species"" ADD COLUMN ""CreatedAt"" timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP;
                EXCEPTION WHEN duplicate_column THEN NULL;
                END $$;
                
                DO $$ BEGIN
                    ALTER TABLE ""Species"" ADD COLUMN ""UpdatedAt"" timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP;
                EXCEPTION WHEN duplicate_column THEN NULL;
                END $$;
                
                -- SpeciesLocations table
                DO $$ BEGIN
                    ALTER TABLE ""SpeciesLocations"" ADD COLUMN ""CreatedAt"" timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP;
                EXCEPTION WHEN duplicate_column THEN NULL;
                END $$;
                
                DO $$ BEGIN
                    ALTER TABLE ""SpeciesLocations"" ADD COLUMN ""UpdatedAt"" timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP;
                EXCEPTION WHEN duplicate_column THEN NULL;
                END $$;
                
                -- Roles table
                DO $$ BEGIN
                    ALTER TABLE ""Roles"" ADD COLUMN ""CreatedAt"" timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP;
                EXCEPTION WHEN duplicate_column THEN NULL;
                END $$;
                
                DO $$ BEGIN
                    ALTER TABLE ""Roles"" ADD COLUMN ""UpdatedAt"" timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP;
                EXCEPTION WHEN duplicate_column THEN NULL;
                END $$;
                
                -- Habitats table
                DO $$ BEGIN
                    ALTER TABLE ""Habitats"" ADD COLUMN ""CreatedAt"" timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP;
                EXCEPTION WHEN duplicate_column THEN NULL;
                END $$;
                
                DO $$ BEGIN
                    ALTER TABLE ""Habitats"" ADD COLUMN ""UpdatedAt"" timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP;
                EXCEPTION WHEN duplicate_column THEN NULL;
                END $$;
                
                -- Countries table
                DO $$ BEGIN
                    ALTER TABLE ""Countries"" ADD COLUMN ""CreatedAt"" timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP;
                EXCEPTION WHEN duplicate_column THEN NULL;
                END $$;
                
                DO $$ BEGIN
                    ALTER TABLE ""Countries"" ADD COLUMN ""UpdatedAt"" timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP;
                EXCEPTION WHEN duplicate_column THEN NULL;
                END $$;
                
                -- Colors table
                DO $$ BEGIN
                    ALTER TABLE ""Colors"" ADD COLUMN ""CreatedAt"" timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP;
                EXCEPTION WHEN duplicate_column THEN NULL;
                END $$;
                
                DO $$ BEGIN
                    ALTER TABLE ""Colors"" ADD COLUMN ""UpdatedAt"" timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP;
                EXCEPTION WHEN duplicate_column THEN NULL;
                END $$;
                
                -- AiSessions table - only UpdatedAt (CreatedAt already exists)
                DO $$ BEGIN
                    ALTER TABLE ""AiSessions"" ADD COLUMN ""UpdatedAt"" timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP;
                EXCEPTION WHEN duplicate_column THEN NULL;
                END $$;
                
                -- AiMessages table - only UpdatedAt (CreatedAt already exists)
                DO $$ BEGIN
                    ALTER TABLE ""AiMessages"" ADD COLUMN ""UpdatedAt"" timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP;
                EXCEPTION WHEN duplicate_column THEN NULL;
                END $$;
                
                -- AiFeedbacks table - only UpdatedAt (CreatedAt already exists)
                DO $$ BEGIN
                    ALTER TABLE ""AiFeedbacks"" ADD COLUMN ""UpdatedAt"" timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP;
                EXCEPTION WHEN duplicate_column THEN NULL;
                END $$;
            ");

            // Add missing foreign key constraints (ignore if they already exist)
            migrationBuilder.Sql(@"
                DO $$ BEGIN
                    ALTER TABLE ""SpeciesLocations"" ADD CONSTRAINT ""FK_SpeciesLocations_Species_SpeciesId"" 
                    FOREIGN KEY (""SpeciesId"") REFERENCES ""Species""(""Id"") ON DELETE CASCADE;
                EXCEPTION WHEN duplicate_object THEN NULL;
                END $$;
                
                DO $$ BEGIN
                    ALTER TABLE ""AiFeedbacks"" ADD CONSTRAINT ""FK_AiFeedbacks_AiSessions_SessionId"" 
                    FOREIGN KEY (""SessionId"") REFERENCES ""AiSessions""(""Id"") ON DELETE CASCADE;
                EXCEPTION WHEN duplicate_object THEN NULL;
                END $$;
            ");

            // Create indexes (ignore if they already exist)
            migrationBuilder.Sql(@"
                CREATE INDEX IF NOT EXISTS ""IX_Species_SizeId"" ON ""Species""(""SizeId"");
                CREATE INDEX IF NOT EXISTS ""IX_Species_CreatedAt"" ON ""Species""(""CreatedAt"");
                CREATE INDEX IF NOT EXISTS ""IX_SpeciesLocations_SpeciesId"" ON ""SpeciesLocations""(""SpeciesId"");
                CREATE INDEX IF NOT EXISTS ""IX_SpeciesLocations_CreatedAt"" ON ""SpeciesLocations""(""CreatedAt"");
                CREATE INDEX IF NOT EXISTS ""IX_SpeciesCountries_SpeciesId"" ON ""SpeciesCountries""(""SpeciesId"");
                CREATE INDEX IF NOT EXISTS ""IX_SpeciesCountries_CountryId"" ON ""SpeciesCountries""(""CountryId"");
                CREATE INDEX IF NOT EXISTS ""IX_SpeciesColors_SpeciesId"" ON ""SpeciesColors""(""SpeciesId"");
                CREATE INDEX IF NOT EXISTS ""IX_SpeciesColors_ColorId"" ON ""SpeciesColors""(""ColorId"");
                CREATE INDEX IF NOT EXISTS ""IX_SpeciesHabitats_SpeciesId"" ON ""SpeciesHabitats""(""SpeciesId"");
                CREATE INDEX IF NOT EXISTS ""IX_SpeciesHabitats_HabitatId"" ON ""SpeciesHabitats""(""HabitatId"");
                CREATE INDEX IF NOT EXISTS ""IX_Roles_CreatedAt"" ON ""Roles""(""CreatedAt"");
                CREATE INDEX IF NOT EXISTS ""IX_AiSessions_UserId"" ON ""AiSessions""(""UserId"");
                CREATE INDEX IF NOT EXISTS ""IX_AiSessions_CreatedAt"" ON ""AiSessions""(""CreatedAt"");
                CREATE INDEX IF NOT EXISTS ""IX_AiMessages_SessionId"" ON ""AiMessages""(""SessionId"");
                CREATE INDEX IF NOT EXISTS ""IX_AiMessages_CreatedAt"" ON ""AiMessages""(""CreatedAt"");
                CREATE INDEX IF NOT EXISTS ""IX_AiFeedbacks_SessionId"" ON ""AiFeedbacks""(""SessionId"");
                CREATE INDEX IF NOT EXISTS ""IX_AiFeedbacks_CreatedAt"" ON ""AiFeedbacks""(""CreatedAt"");
                CREATE INDEX IF NOT EXISTS ""IX_Users_CreatedAt"" ON ""Users""(""CreatedAt"");
                CREATE INDEX IF NOT EXISTS ""IX_Users_IsActive"" ON ""Users""(""IsActive"");
                CREATE INDEX IF NOT EXISTS ""IX_UserRoles_UserId"" ON ""UserRoles""(""UserId"");
                CREATE INDEX IF NOT EXISTS ""IX_UserRoles_RoleId"" ON ""UserRoles""(""RoleId"");
                CREATE INDEX IF NOT EXISTS ""IX_UserRoles_AssignedAt"" ON ""UserRoles""(""AssignedAt"");
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop indexes
            migrationBuilder.Sql(@"
                DROP INDEX IF EXISTS ""IX_UserRoles_AssignedAt"";
                DROP INDEX IF EXISTS ""IX_UserRoles_RoleId"";
                DROP INDEX IF EXISTS ""IX_UserRoles_UserId"";
                DROP INDEX IF EXISTS ""IX_Users_IsActive"";
                DROP INDEX IF EXISTS ""IX_Users_CreatedAt"";
                DROP INDEX IF EXISTS ""IX_AiFeedbacks_CreatedAt"";
                DROP INDEX IF EXISTS ""IX_AiFeedbacks_SessionId"";
                DROP INDEX IF EXISTS ""IX_AiMessages_CreatedAt"";
                DROP INDEX IF EXISTS ""IX_AiMessages_SessionId"";
                DROP INDEX IF EXISTS ""IX_AiSessions_CreatedAt"";
                DROP INDEX IF EXISTS ""IX_AiSessions_UserId"";
                DROP INDEX IF EXISTS ""IX_Roles_CreatedAt"";
                DROP INDEX IF EXISTS ""IX_SpeciesHabitats_HabitatId"";
                DROP INDEX IF EXISTS ""IX_SpeciesHabitats_SpeciesId"";
                DROP INDEX IF EXISTS ""IX_SpeciesColors_ColorId"";
                DROP INDEX IF EXISTS ""IX_SpeciesColors_SpeciesId"";
                DROP INDEX IF EXISTS ""IX_SpeciesCountries_CountryId"";
                DROP INDEX IF EXISTS ""IX_SpeciesCountries_SpeciesId"";
                DROP INDEX IF EXISTS ""IX_SpeciesLocations_CreatedAt"";
                DROP INDEX IF EXISTS ""IX_SpeciesLocations_SpeciesId"";
                DROP INDEX IF EXISTS ""IX_Species_CreatedAt"";
                DROP INDEX IF EXISTS ""IX_Species_SizeId"";
            ");

            // Drop foreign keys
            migrationBuilder.Sql(@"
                DO $$ BEGIN
                    ALTER TABLE ""AiFeedbacks"" DROP CONSTRAINT IF EXISTS ""FK_AiFeedbacks_AiSessions_SessionId"";
                EXCEPTION WHEN undefined_object THEN NULL;
                END $$;
                
                DO $$ BEGIN
                    ALTER TABLE ""SpeciesLocations"" DROP CONSTRAINT IF EXISTS ""FK_SpeciesLocations_Species_SpeciesId"";
                EXCEPTION WHEN undefined_object THEN NULL;
                END $$;
            ");

            // Drop columns
            migrationBuilder.Sql(@"
                DO $$ BEGIN
                    ALTER TABLE ""AiFeedbacks"" DROP COLUMN IF EXISTS ""UpdatedAt"";
                EXCEPTION WHEN undefined_column THEN NULL;
                END $$;
                
                ALTER TABLE ""AiMessages"" DROP COLUMN IF EXISTS ""UpdatedAt"";
                ALTER TABLE ""AiSessions"" DROP COLUMN IF EXISTS ""UpdatedAt"";
                ALTER TABLE ""Colors"" DROP COLUMN IF EXISTS ""UpdatedAt"";
                ALTER TABLE ""Colors"" DROP COLUMN IF EXISTS ""CreatedAt"";
                ALTER TABLE ""Countries"" DROP COLUMN IF EXISTS ""UpdatedAt"";
                ALTER TABLE ""Countries"" DROP COLUMN IF EXISTS ""CreatedAt"";
                ALTER TABLE ""Habitats"" DROP COLUMN IF EXISTS ""UpdatedAt"";
                ALTER TABLE ""Habitats"" DROP COLUMN IF EXISTS ""CreatedAt"";
                ALTER TABLE ""Roles"" DROP COLUMN IF EXISTS ""UpdatedAt"";
                ALTER TABLE ""Roles"" DROP COLUMN IF EXISTS ""CreatedAt"";
                ALTER TABLE ""SpeciesLocations"" DROP COLUMN IF EXISTS ""UpdatedAt"";
                ALTER TABLE ""SpeciesLocations"" DROP COLUMN IF EXISTS ""CreatedAt"";
                ALTER TABLE ""Species"" DROP COLUMN IF EXISTS ""UpdatedAt"";
                ALTER TABLE ""Species"" DROP COLUMN IF EXISTS ""CreatedAt"";
                ALTER TABLE ""Sizes"" DROP COLUMN IF EXISTS ""UpdatedAt"";
                ALTER TABLE ""Sizes"" DROP COLUMN IF EXISTS ""CreatedAt"";
            ");
        }
    }
}
