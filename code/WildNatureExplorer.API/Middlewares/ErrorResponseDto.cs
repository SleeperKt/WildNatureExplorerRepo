namespace WildNatureExplorer.API.Middlewares;

public class ErrorResponse
{
    public string Message { get; set; } = null!;
    public int Status { get; set; }
    public string TraceId { get; set; } = null!;
}