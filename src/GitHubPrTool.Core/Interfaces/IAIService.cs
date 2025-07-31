namespace GitHubPrTool.Core.Interfaces;

/// <summary>
/// Core AI service interface for Gemini API integration
/// </summary>
public interface IAIService
{
    /// <summary>
    /// Generates text using the AI model
    /// </summary>
    /// <param name="prompt">Input prompt</param>
    /// <param name="systemInstructions">System instructions for the AI</param>
    /// <param name="temperature">Creativity level (0.0 to 1.0)</param>
    /// <param name="maxTokens">Maximum tokens in response</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Generated text response</returns>
    Task<string> GenerateTextAsync(
        string prompt,
        string? systemInstructions = null,
        double temperature = 0.7,
        int maxTokens = 1000,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Analyzes text and extracts structured information
    /// </summary>
    /// <param name="text">Text to analyze</param>
    /// <param name="analysisType">Type of analysis to perform</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Analysis results as JSON string</returns>
    Task<string> AnalyzeTextAsync(
        string text,
        string analysisType,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates text with specific format or structure
    /// </summary>
    /// <param name="prompt">Input prompt</param>
    /// <param name="outputFormat">Desired output format (e.g., "json", "markdown", "yaml")</param>
    /// <param name="schema">Optional schema for structured output</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Formatted response</returns>
    Task<string> GenerateStructuredTextAsync(
        string prompt,
        string outputFormat = "json",
        string? schema = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if the AI service is available and configured
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if service is available</returns>
    Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets information about the AI model being used
    /// </summary>
    /// <returns>Model information</returns>
    string GetModelInfo();
}