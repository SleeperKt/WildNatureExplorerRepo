using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using WildNatureExplorer.Application.DTOs.Geo;
using WildNatureExplorer.Application.Interfaces.Repositories;
using WildNatureExplorer.Application.Interfaces.Services;
using WildNatureExplorer.Infrastructure.Data;

namespace WildNatureExplorer.Infrastructure.Services;

public class PathSimulationService : IPathSimulationService
{
    private readonly AppDbContext _context;
    private readonly ISpeciesRepository _speciesRepository;

    public PathSimulationService(
        AppDbContext context,
        ISpeciesRepository speciesRepository)
    {
        _context = context;
        _speciesRepository = speciesRepository;
    }

    public async Task<PathSimulationResponse> SimulatePathDatabaseAsync(PathSimulationRequest request)
    {
        try
        {
            var response = new PathSimulationResponse
            {
                SimulationMode = "database",
                SimulatedAt = DateTime.UtcNow
            };

            var waypointsJson = JsonSerializer.Serialize(request.Waypoints);

            var sql = @"
                SELECT 
                    step_id, 
                    segment_id, 
                    latitude, 
                    longitude, 
                    timestamp_step, 
                    distance_from_start_km,
                    alert_id,
                    alert_type,
                    animal_id,
                    animal_name,
                    distance_to_animal_km,
                    warning_level
                FROM simulate_path_with_dangers(
                    @countryId, 
                    @waypoints::jsonb, 
                    @stepsPerSegment,
                    @dangerRadiusKm
                )";

            using (var command = _context.Database.GetDbConnection().CreateCommand())
            {
                await _context.Database.OpenConnectionAsync();
                try
                {
                    command.CommandText = sql;

                    var paramCountry = command.CreateParameter();
                    paramCountry.ParameterName = "@countryId";
                    paramCountry.Value = request.CountryId;
                    command.Parameters.Add(paramCountry);

                    var paramWaypoints = command.CreateParameter();
                    paramWaypoints.ParameterName = "@waypoints";
                    paramWaypoints.Value = waypointsJson;
                    command.Parameters.Add(paramWaypoints);

                    var paramSteps = command.CreateParameter();
                    paramSteps.ParameterName = "@stepsPerSegment";
                    paramSteps.Value = request.StepsPerSegment;
                    command.Parameters.Add(paramSteps);

                    var paramRadius = command.CreateParameter();
                    paramRadius.ParameterName = "@dangerRadiusKm";
                    paramRadius.Value = request.DangerRadiusKm;
                    command.Parameters.Add(paramRadius);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        var pathSteps = new List<SimulationStepDto>();
                        var alerts = new List<DangerAlertDto>();
                        var alertsSet = new HashSet<string>();

                        while (await reader.ReadAsync())
                        {
                            var stepId = reader.GetInt32(0);
                            var segmentId = reader.GetInt32(1);
                            var latitude = reader.GetFloat(2);
                            var longitude = reader.GetFloat(3);
                            var timestampStep = reader.GetDateTime(4);
                            var distanceFromStart = reader.GetFloat(5);
                            var alertId = reader.IsDBNull(6) ? Guid.Empty : reader.GetGuid(6);
                            var alertType = reader.GetString(7);

                            if (alertType == "PATH")
                            {
                                pathSteps.Add(new SimulationStepDto
                                {
                                    StepId = stepId,
                                    SegmentId = segmentId,
                                    Latitude = latitude,
                                    Longitude = longitude,
                                    TimestampStep = timestampStep,
                                    DistanceFromStartKm = distanceFromStart
                                });
                            }
                            else if (alertType == "ALERT")
                            {
                                var animalId = reader.GetGuid(8);
                                var animalName = reader.GetString(9) ?? string.Empty;
                                var distanceToAnimal = reader.GetFloat(10);
                                var warningLevel = reader.GetString(11) ?? "CAUTION";

                                var alertKey = $"{stepId}_{animalId}";
                                if (!alertsSet.Contains(alertKey))
                                {
                                    alerts.Add(new DangerAlertDto
                                    {
                                        AlertId = alertId,
                                        StepId = stepId,
                                        AnimalId = animalId,
                                        AnimalName = animalName,
                                        Latitude = latitude,
                                        Longitude = longitude,
                                        DistanceToAnimalKm = distanceToAnimal,
                                        WarningLevel = warningLevel
                                    });
                                    alertsSet.Add(alertKey);
                                }
                            }
                        }

                        response.PathSteps = pathSteps;
                        response.Alerts = alerts;
                        response.TotalSteps = pathSteps.Count;
                        response.TotalAlerts = alerts.Count;
                        response.TotalDistanceKm = pathSteps.LastOrDefault()?.DistanceFromStartKm ?? 0;
                    }
                }
                finally
                {
                    await _context.Database.CloseConnectionAsync();
                }
            }

            return response;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Database path simulation failed: {ex.Message}", ex);
        }
    }

    public async Task<PathSimulationResponse> SimulatePathClientAsync(PathSimulationRequest request)
    {
        var response = new PathSimulationResponse
        {
            SimulationMode = "client",
            SimulatedAt = DateTime.UtcNow
        };

        try
        {
            var species = await _speciesRepository.GetByCountryAsync(request.CountryId);
            var dangerousSpecies = species.Where(s => s.IsDangerous).ToList();

            var pathSteps = new List<SimulationStepDto>();
            var alerts = new List<DangerAlertDto>();
            var alertsSet = new HashSet<string>();
            var stepId = 0;
            double totalDistance = 0;

            for (int segment = 0; segment < request.Waypoints.Count - 1; segment++)
            {
                var startWaypoint = request.Waypoints[segment];
                var endWaypoint = request.Waypoints[segment + 1];

                for (int step = 0; step <= request.StepsPerSegment; step++)
                {
                    var fraction = (float)step / request.StepsPerSegment;
                    var currentLat = startWaypoint.Lat + (endWaypoint.Lat - startWaypoint.Lat) * fraction;
                    var currentLng = startWaypoint.Lng + (endWaypoint.Lng - startWaypoint.Lng) * fraction;

                    if (step == 0 && segment == 0)
                    {
                        totalDistance = 0;
                    }
                    else
                    {
                        var segmentDistance = CalculateDistance(
                            startWaypoint.Lat, startWaypoint.Lng,
                            currentLat, currentLng
                        );
                        totalDistance += segmentDistance;
                    }

                    pathSteps.Add(new SimulationStepDto
                    {
                        StepId = stepId,
                        SegmentId = segment,
                        Latitude = currentLat,
                        Longitude = currentLng,
                        TimestampStep = DateTime.UtcNow.AddSeconds(stepId * 2),
                        DistanceFromStartKm = totalDistance
                    });

                    foreach (var animal in dangerousSpecies)
                    {
                        foreach (var location in animal.Locations)
                        {
                            var distance = CalculateDistance(
                                currentLat, currentLng,
                                location.Latitude, location.Longitude
                            );

                            if (distance <= request.DangerRadiusKm)
                            {
                                var alertKey = $"{stepId}_{animal.Id}";
                                if (!alertsSet.Contains(alertKey))
                                {
                                    var warningLevel = distance <= 5.0 ? "CRITICAL"
                                        : distance <= 8.0 ? "WARNING"
                                        : "CAUTION";

                                    alerts.Add(new DangerAlertDto
                                    {
                                        AlertId = Guid.NewGuid(),
                                        StepId = stepId,
                                        AnimalId = animal.Id,
                                        AnimalName = animal.CommonName,
                                        Latitude = (double)location.Latitude,
                                        Longitude = (double)location.Longitude,
                                        DistanceToAnimalKm = distance,
                                        WarningLevel = warningLevel
                                    });
                                    alertsSet.Add(alertKey);
                                }
                            }
                        }
                    }

                    stepId++;
                }
            }

            response.PathSteps = pathSteps;
            response.Alerts = alerts;
            response.TotalSteps = pathSteps.Count;
            response.TotalAlerts = alerts.Count;
            response.TotalDistanceKm = totalDistance;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Client path simulation failed: {ex.Message}", ex);
        }

        return response;
    }

    private static double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
    {
        const double earthRadiusKm = 6371.0;

        var dLat = ToRadians(lat2 - lat1);
        var dLon = ToRadians(lon2 - lon1);

        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        return earthRadiusKm * c;
    }

    private static double ToRadians(double degrees) => degrees * Math.PI / 180.0;
}
