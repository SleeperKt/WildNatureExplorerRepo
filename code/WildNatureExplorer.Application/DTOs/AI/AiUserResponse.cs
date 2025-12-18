namespace WildNatureExplorer.Application.DTOs.AI;

public class AiUserResponse
{
    public string Model { get; set; }
    public DateTimeOffset ResponseTime { get; set; }
    public int TokensUsed { get; set; }
    public string Message { get; set; }
}