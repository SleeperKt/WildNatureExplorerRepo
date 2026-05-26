namespace WildNatureExplorer.Application.Common;

/// <summary>
/// Thrown when the client supplies a session id that does not exist, is ended, or belongs to another user.
/// </summary>
public class InvalidAiSessionException : Exception
{
    public InvalidAiSessionException(string message) : base(message)
    {
    }
}
