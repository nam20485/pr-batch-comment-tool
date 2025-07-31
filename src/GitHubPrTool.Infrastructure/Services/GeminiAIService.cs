using GitHubPrTool.Core.Interfaces;
using GitHubPrTool.Infrastructure.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Net.Http;
using System.Text;

namespace GitHubPrTool.Infrastructure.Services;

/// <summary>
/// Gemini AI service implementation using REST API
/// </summary>
public class GeminiAIService : IAIService
{
    private readonly AIConfiguration _config;
    private readonly ILogger<GeminiAIService> _logger;
    private readonly HttpClient _httpClient;

    public GeminiAIService(
        IOptions<AIConfiguration> config,
        ILogger<GeminiAIService> logger,
        HttpClient httpClient)
    {
        _config = config.Value ?? throw new ArgumentNullException(nameof(config));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    /// <inheritdoc />
    public async Task<string> GenerateTextAsync(
        string prompt,
        string? systemInstructions = null,
        double temperature = 0.7,
        int maxTokens = 1000,
        CancellationToken cancellationToken = default)
    {
        if (!_config.Enabled)
        {
            _logger.LogWarning("AI service is disabled");
            return string.Empty;
        }

        try
        {
            _logger.LogDebug("Generating text with prompt length: {PromptLength}", prompt.Length);

            // For now, return a mock response since setting up Gemini API requires authentication
            // In a real implementation, this would make an HTTP call to the Gemini API
            var mockResponse = GenerateMockResponse(prompt, systemInstructions);
            
            _logger.LogDebug("Generated mock text with length: {ResponseLength}", mockResponse.Length);
            
            return mockResponse;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating text with Gemini API");
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<string> AnalyzeTextAsync(
        string text,
        string analysisType,
        CancellationToken cancellationToken = default)
    {
        var prompt = analysisType.ToLowerInvariant() switch
        {
            "sentiment" => $"Analyze the sentiment of this text and return a score from -1.0 (very negative) to 1.0 (very positive): {text}",
            "categorize" => $"Categorize this text into one of these categories: Bug, Feature, CodeReview, Documentation, Question, Performance, Security, Testing, General. Text: {text}",
            "extract_topics" => $"Extract the main topics and themes from this text as a JSON array: {text}",
            _ => $"Analyze this text for {analysisType}: {text}"
        };

        return await GenerateTextAsync(prompt, null, 0.3, 500, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<string> GenerateStructuredTextAsync(
        string prompt,
        string outputFormat = "json",
        string? schema = null,
        CancellationToken cancellationToken = default)
    {
        var structuredPrompt = outputFormat.ToLowerInvariant() switch
        {
            "json" => $"{prompt}\n\nPlease respond with valid JSON format." + (schema != null ? $" Follow this schema: {schema}" : ""),
            "markdown" => $"{prompt}\n\nPlease respond in Markdown format.",
            "yaml" => $"{prompt}\n\nPlease respond in YAML format.",
            _ => prompt
        };

        return await GenerateTextAsync(structuredPrompt, null, 0.5, 2000, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default)
    {
        if (!_config.Enabled)
            return false;

        try
        {
            // Simple health check - for mock implementation, always return true if enabled
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <inheritdoc />
    public string GetModelInfo()
    {
        return $"Gemini Model: {_config.ModelName}, Location: {_config.Location}, Project: {_config.ProjectId} (Mock Implementation)";
    }

    /// <summary>
    /// Generates a mock response for development/testing purposes
    /// </summary>
    private string GenerateMockResponse(string prompt, string? systemInstructions)
    {
        // This is a simplified mock response generator
        // In a real implementation, this would be replaced with actual Gemini API calls
        
        if (prompt.Contains("JSON format", StringComparison.OrdinalIgnoreCase))
        {
            if (prompt.Contains("architecture recommendations", StringComparison.OrdinalIgnoreCase))
            {
                return @"[
                  {
                    ""title"": ""Implement Repository Pattern"",
                    ""description"": ""Consider implementing the Repository pattern to separate data access logic from business logic."",
                    ""category"": ""Architecture"",
                    ""priority"": 4,
                    ""impact"": ""Improved testability and maintainability"",
                    ""effort"": ""Medium - 2-3 days"",
                    ""affectedFiles"": [""Services/*"", ""Data/*""],
                    ""implementationSteps"": [""Create repository interfaces"", ""Implement concrete repositories"", ""Update dependency injection""],
                    ""relatedPatterns"": [""Unit of Work"", ""Dependency Injection""],
                    ""codeExample"": ""public interface IRepository<T> { Task<T> GetByIdAsync(int id); }"",
                    ""references"": [""Microsoft Documentation on Repository Pattern""]
                  }
                ]";
            }
            
            if (prompt.Contains("code quality", StringComparison.OrdinalIgnoreCase))
            {
                return @"[
                  {
                    ""title"": ""Add Input Validation"",
                    ""description"": ""Add comprehensive input validation to prevent potential security issues."",
                    ""category"": ""Security"",
                    ""priority"": 5,
                    ""impact"": ""Prevents injection attacks and improves reliability"",
                    ""effort"": ""Low - 1 day"",
                    ""affectedFiles"": [""Controllers/*"", ""Services/*""],
                    ""implementationSteps"": [""Add data annotations"", ""Implement custom validators"", ""Add error handling""],
                    ""relatedPatterns"": [""Data Annotations"", ""FluentValidation""],
                    ""codeExample"": ""[Required][StringLength(100)] public string Name { get; set; }"",
                    ""references"": [""ASP.NET Core Validation Documentation""]
                  }
                ]";
            }

            return @"{""result"": ""Mock JSON response for development purposes""}";
        }

        // For non-JSON prompts, return a simple text response
        if (prompt.Contains("sentiment", StringComparison.OrdinalIgnoreCase))
        {
            return "0.3"; // Slightly positive sentiment
        }

        if (prompt.Contains("categorize", StringComparison.OrdinalIgnoreCase))
        {
            return "CodeReview";
        }

        return "This is a mock response from the Gemini AI service for development purposes. In production, this would be replaced with actual AI-generated content.";
    }
}