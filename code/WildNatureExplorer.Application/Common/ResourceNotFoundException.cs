namespace WildNatureExplorer.Application.Common;

/// <summary>
/// Exception thrown when a requested resource is not found (HTTP 404 Not Found)
/// </summary>
public class ResourceNotFoundException : Exception
{
    public string ResourceType { get; set; }
    public string ResourceId { get; set; }

    public ResourceNotFoundException(string resourceType, string resourceId) 
        : base($"{resourceType} with ID '{resourceId}' not found.")
    {
        ResourceType = resourceType;
        ResourceId = resourceId;
    }

    public ResourceNotFoundException(string message) : base(message)
    {
        ResourceType = "Resource";
        ResourceId = "Unknown";
    }
}
