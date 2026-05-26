namespace WildNatureExplorer.Application.DTOs.AI
{
    /// <summary>Standard payload shape for AI session endpoints.</summary>
    public class AiSessionResponseDto
    {
        /// <summary>
        /// Unique session identifier
        /// </summary>
        public Guid SessionId { get; set; }

        /// <summary>
        /// Animal information (if available from image analysis)
        /// </summary>
        public AnimalInfoDto? Animal { get; set; }

        /// <summary>Assistant reply text when applicable.</summary>
        public string? Answer { get; set; }

        /// <summary>
        /// Technical information about API usage
        /// </summary>
        public TechnicalInfoDto? Technical { get; set; }

        /// <summary>
        /// Whether the session is still active
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Session creation timestamp
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Session end timestamp (if ended)
        /// </summary>
        public DateTime? EndedAt { get; set; }
    }
}
