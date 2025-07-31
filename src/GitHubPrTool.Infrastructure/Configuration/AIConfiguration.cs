namespace GitHubPrTool.Infrastructure.Configuration;

/// <summary>
/// Configuration options for AI services
/// </summary>
public class AIConfiguration
{
    /// <summary>
    /// Configuration section name
    /// </summary>
    public const string SectionName = "AI";

    /// <summary>
    /// Google Cloud project ID for Gemini API
    /// </summary>
    public string ProjectId { get; set; } = string.Empty;

    /// <summary>
    /// Location/region for the AI service (e.g., "us-central1")
    /// </summary>
    public string Location { get; set; } = "us-central1";

    /// <summary>
    /// Gemini model name to use
    /// </summary>
    public string ModelName { get; set; } = "gemini-1.5-flash";

    /// <summary>
    /// API key for authentication (if using API key instead of service account)
    /// </summary>
    public string? ApiKey { get; set; }

    /// <summary>
    /// Path to service account key file for authentication
    /// </summary>
    public string? ServiceAccountKeyPath { get; set; }

    /// <summary>
    /// Default temperature for AI text generation (0.0 to 1.0)
    /// </summary>
    public double DefaultTemperature { get; set; } = 0.7;

    /// <summary>
    /// Default maximum tokens for AI responses
    /// </summary>
    public int DefaultMaxTokens { get; set; } = 1000;

    /// <summary>
    /// Timeout for AI API calls in seconds
    /// </summary>
    public int TimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Whether to enable AI features (can be used to disable AI temporarily)
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Cache duration for AI responses in minutes
    /// </summary>
    public int CacheDurationMinutes { get; set; } = 60;
}