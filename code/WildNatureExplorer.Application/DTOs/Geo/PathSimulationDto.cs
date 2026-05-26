namespace WildNatureExplorer.Application.DTOs.Geo;

public class PathSimulationRequest
{
    public Guid CountryId { get; set; }
    public List<WaypointDto> Waypoints { get; set; } = new();
    public int StepsPerSegment { get; set; } = 15;
    public float DangerRadiusKm { get; set; } = 10.0f;
    public string SimulationMode { get; set; } = "database"; // "database" or "client"
}

public class WaypointDto
{
    public double Lat { get; set; }
    public double Lng { get; set; }
}

public class PathSimulationResponse
{
    public string SimulationMode { get; set; } = "client";
    public List<SimulationStepDto> PathSteps { get; set; } = new();
    public List<DangerAlertDto> Alerts { get; set; } = new();
    public int TotalSteps { get; set; }
    public int TotalAlerts { get; set; }
    public double TotalDistanceKm { get; set; }
    public DateTime SimulatedAt { get; set; }
}

public class SimulationStepDto
{
    public int StepId { get; set; }
    public int SegmentId { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public DateTime TimestampStep { get; set; }
    public double DistanceFromStartKm { get; set; }
}

public class DangerAlertDto
{
    public Guid AlertId { get; set; }
    public int StepId { get; set; }
    public Guid AnimalId { get; set; }
    public string AnimalName { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public double DistanceToAnimalKm { get; set; }
    public string WarningLevel { get; set; } = "CAUTION"; // CRITICAL, WARNING, CAUTION
}
