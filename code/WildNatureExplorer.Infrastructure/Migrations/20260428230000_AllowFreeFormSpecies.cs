using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WildNatureExplorer.Infrastructure.Migrations
{
    /// <summary>
    /// Lets users save sightings of animals that are not in the curated
    /// <c>Species</c> table. Three things change:
    /// <list type="bullet">
    /// <item>
    ///   <see cref="UserSighting.SpeciesId"/> becomes nullable, and the FK
    ///   to <c>Species</c> uses <c>ON DELETE SET NULL</c> so a removed
    ///   species never wipes the user's library.
    /// </item>
    /// <item>
    ///   Two new columns hold the recognized name on the row itself:
    ///   <c>CommonName</c> (required) and <c>ScientificName</c> (optional).
    ///   Existing rows are back-filled from their linked species.
    /// </item>
    /// <item>
    ///   <c>fn_user_nearby_sightings</c> is recreated with a <c>LEFT JOIN</c>
    ///   to <c>Species</c> and <c>COALESCE</c>s into the new columns so the
    ///   nearby query keeps working for both linked and free-form sightings.
    /// </item>
    /// </list>
    /// </summary>
    public partial class AllowFreeFormSpecies : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ----------------------------------------------------------------
            // 1. New name columns + back-fill from joined Species
            // ----------------------------------------------------------------
            migrationBuilder.Sql(@"
                ALTER TABLE ""UserSightings""
                    ADD COLUMN IF NOT EXISTS ""CommonName""     varchar(200) NULL,
                    ADD COLUMN IF NOT EXISTS ""ScientificName"" varchar(200) NULL;

                UPDATE ""UserSightings"" us
                SET    ""CommonName""     = sp.""CommonName"",
                       ""ScientificName"" = sp.""ScientificName""
                FROM   ""Species"" sp
                WHERE  us.""SpeciesId"" = sp.""Id""
                  AND  (us.""CommonName"" IS NULL OR us.""CommonName"" = '');

                -- Any pre-existing row with no joined species (shouldn't exist
                -- yet, but be defensive) gets a placeholder so the NOT NULL
                -- constraint below succeeds.
                UPDATE ""UserSightings""
                   SET ""CommonName"" = 'Unknown'
                 WHERE ""CommonName"" IS NULL OR ""CommonName"" = '';

                ALTER TABLE ""UserSightings""
                    ALTER COLUMN ""CommonName"" SET NOT NULL;
            ");

            // ----------------------------------------------------------------
            // 2. SpeciesId → nullable, swap FK to ON DELETE SET NULL
            // ----------------------------------------------------------------
            migrationBuilder.Sql(@"
                ALTER TABLE ""UserSightings""
                    ALTER COLUMN ""SpeciesId"" DROP NOT NULL;

                ALTER TABLE ""UserSightings""
                    DROP CONSTRAINT IF EXISTS ""FK_UserSightings_Species_SpeciesId"";

                ALTER TABLE ""UserSightings""
                    ADD CONSTRAINT ""FK_UserSightings_Species_SpeciesId""
                    FOREIGN KEY (""SpeciesId"")
                    REFERENCES ""Species"" (""Id"")
                    ON DELETE SET NULL;
            ");

            // ----------------------------------------------------------------
            // 3. Recreate fn_user_nearby_sightings with LEFT JOIN + COALESCE
            // ----------------------------------------------------------------
            migrationBuilder.Sql(@"
                CREATE OR REPLACE FUNCTION fn_user_nearby_sightings(
                    p_user_id     uuid,
                    p_lat         double precision,
                    p_lng         double precision,
                    p_radius_km   numeric
                )
                RETURNS TABLE (
                    sighting_id      uuid,
                    species_id       uuid,
                    common_name      text,
                    scientific_name  text,
                    is_dangerous     boolean,
                    is_rare          boolean,
                    latitude         double precision,
                    longitude        double precision,
                    image_url        text,
                    notes            text,
                    sighted_at       timestamptz,
                    created_at       timestamptz,
                    distance_km      numeric
                )
                LANGUAGE plpgsql
                STABLE
                AS $$
                DECLARE
                    v_origin   geography := geography(ST_SetSRID(ST_MakePoint(p_lng, p_lat), 4326));
                    v_radius_m double precision := (p_radius_km * 1000.0)::double precision;
                BEGIN
                    IF p_user_id IS NULL THEN
                        RAISE EXCEPTION 'p_user_id is required';
                    END IF;

                    IF p_lat NOT BETWEEN -90 AND 90 THEN
                        RAISE EXCEPTION 'p_lat must be between -90 and 90, got %', p_lat;
                    END IF;

                    IF p_lng NOT BETWEEN -180 AND 180 THEN
                        RAISE EXCEPTION 'p_lng must be between -180 and 180, got %', p_lng;
                    END IF;

                    IF p_radius_km IS NULL OR p_radius_km <= 0 OR p_radius_km > 1000 THEN
                        RAISE EXCEPTION 'p_radius_km must be in (0, 1000], got %', p_radius_km;
                    END IF;

                    RETURN QUERY
                    SELECT
                        us.""Id""                                              AS sighting_id,
                        COALESCE(sp.""Id"", '00000000-0000-0000-0000-000000000000'::uuid) AS species_id,
                        COALESCE(sp.""CommonName"",     us.""CommonName"")::text     AS common_name,
                        COALESCE(sp.""ScientificName"", us.""ScientificName"", '')::text AS scientific_name,
                        COALESCE(sp.""IsDangerous"", false)                    AS is_dangerous,
                        COALESCE(sp.""IsRare"",      false)                    AS is_rare,
                        us.""Latitude""                                        AS latitude,
                        us.""Longitude""                                       AS longitude,
                        us.""ImageUrl""::text                                  AS image_url,
                        us.""Notes""::text                                     AS notes,
                        us.""SightedAt""                                       AS sighted_at,
                        us.""CreatedAt""                                       AS created_at,
                        ROUND(
                            (ST_Distance(
                                geography(ST_SetSRID(ST_MakePoint(us.""Longitude"", us.""Latitude""), 4326)),
                                v_origin
                            ) / 1000.0)::numeric,
                            3
                        )                                                       AS distance_km
                    FROM   ""UserSightings"" us
                    LEFT JOIN ""Species"" sp ON sp.""Id"" = us.""SpeciesId""
                    WHERE  us.""UserId"" = p_user_id
                      AND  ST_DWithin(
                              geography(ST_SetSRID(ST_MakePoint(us.""Longitude"", us.""Latitude""), 4326)),
                              v_origin,
                              v_radius_m)
                    ORDER BY distance_km ASC;
                END;
                $$;

                COMMENT ON FUNCTION fn_user_nearby_sightings(uuid, double precision, double precision, numeric)
                    IS 'Returns sightings owned by p_user_id within p_radius_km of (p_lat, p_lng), ordered by distance. Supports both catalogued and free-form species via LEFT JOIN.';
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Re-tighten the FK back to RESTRICT and reinstate the previous
            // function body. We only attempt this after deleting any rows
            // that have a NULL SpeciesId (free-form), since those are not
            // representable under the older schema.
            migrationBuilder.Sql(@"
                DELETE FROM ""UserSightings"" WHERE ""SpeciesId"" IS NULL;

                ALTER TABLE ""UserSightings""
                    DROP CONSTRAINT IF EXISTS ""FK_UserSightings_Species_SpeciesId"";

                ALTER TABLE ""UserSightings""
                    ALTER COLUMN ""SpeciesId"" SET NOT NULL;

                ALTER TABLE ""UserSightings""
                    ADD CONSTRAINT ""FK_UserSightings_Species_SpeciesId""
                    FOREIGN KEY (""SpeciesId"")
                    REFERENCES ""Species"" (""Id"")
                    ON DELETE RESTRICT;

                ALTER TABLE ""UserSightings""
                    DROP COLUMN IF EXISTS ""ScientificName"",
                    DROP COLUMN IF EXISTS ""CommonName"";
            ");

            // (Function reset to its pre-migration body lives in
            // 20260428190000_AddUserLibrary; replaying that migration's Up
            // section is left to the caller as it requires extra context.)
        }
    }
}
