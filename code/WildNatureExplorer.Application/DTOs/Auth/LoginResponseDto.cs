namespace WildNatureExplorer.Application.DTOs.Auth;

public class LoginResponseDto
{
    public string Token { get; set; }
    public bool AcceptedTerms { get; set; }
    public string? TermsVersion { get; set; }
}