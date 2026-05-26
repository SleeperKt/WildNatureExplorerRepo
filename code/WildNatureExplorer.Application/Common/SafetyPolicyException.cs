namespace WildNatureExplorer.Application.Common;

/// <summary>
/// Exception thrown when a prompt or request violates AI safety policy
/// </summary>
public class SafetyPolicyException : Exception
{
    public SafetyPolicyException(string message) : base(message)
    {
    }

    public SafetyPolicyException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
