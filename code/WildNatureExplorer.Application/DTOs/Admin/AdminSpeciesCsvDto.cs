namespace WildNatureExplorer.Application.DTOs.Admin;

public class AdminSpeciesCsvDto
{
    public Stream FileStream { get; init; } = default!;
    public string FileName { get; init; } = string.Empty;
}
