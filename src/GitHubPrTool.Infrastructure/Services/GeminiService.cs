using GitHubPrTool.Core.Interfaces;
using GitHubPrTool.Core.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;

namespace GitHubPrTool.Infrastructure.Services;

/// <summary>
/// Implementation of Gemini AI service for PR review assistance
/// </summary>
public class GeminiService : IGeminiService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<GeminiService> _logger;
    private readonly string _apiKey;
    private readonly string _baseUrl;

    public GeminiService(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<GeminiService> logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        
        _apiKey = configuration["Gemini:ApiKey"] ?? throw new InvalidOperationException("Gemini API key not configured");
        _baseUrl = configuration["Gemini:BaseUrl"] ?? "https://generativelanguage.googleapis.com/v1beta";
    }

    public async Task<string> ExplainRepositoryArchitectureAsync(string repositoryContext, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Generating architecture explanation for repository context");

            var prompt = $"""
                As a software architecture expert, analyze the following repository information and provide:
                1. An overview of the architecture pattern being used
                2. Key components and their relationships
                3. Strengths and potential improvements
                4. Technology stack assessment
                5. Best practices recommendations

                Repository Context:
                {repositoryContext}

                Provide a detailed but concise analysis focusing on actionable insights.
                """;

            return await CallGeminiApiAsync(prompt, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating architecture explanation");
            return "Unable to generate architecture explanation at this time.";
        }
    }

    public async Task<string> GenerateProjectKickstartPlanAsync(
        string repositoryName,
        string repositoryDescription,
        IEnumerable<string> technologies,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Generating project kickstart plan for repository: {RepositoryName}", repositoryName);

            var techStack = string.Join(", ", technologies);
            var prompt = $"""
                Create a comprehensive project kickstart plan for the following repository:

                Repository Name: {repositoryName}
                Description: {repositoryDescription}
                Technologies: {techStack}

                Please provide:
                1. Development Environment Setup
                2. Initial Architecture Decisions
                3. Development Workflow Recommendations
                4. Testing Strategy
                5. CI/CD Pipeline Suggestions
                6. Documentation Structure
                7. Team Collaboration Guidelines
                8. Next Steps and Milestones

                Format the response as a structured plan with clear sections and actionable items.
                """;

            return await CallGeminiApiAsync(prompt, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating project kickstart plan for {RepositoryName}", repositoryName);
            return "Unable to generate project kickstart plan at this time.";
        }
    }

    public async Task<CommentAnalysisResult> AnalyzeCommentsAsync(
        IEnumerable<Comment> comments,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Analyzing {Count} comments for insights", comments.Count());

            var commentTexts = comments.Select(c => $"- {c.Body}").ToList();
            var commentsText = string.Join("\n", commentTexts);

            var prompt = "Analyze the following PR review comments and provide:\n" +
                "1. Categorization by type (bug, enhancement, style, documentation, etc.)\n" +
                "2. Overall sentiment analysis\n" +
                "3. Key insights and patterns\n" +
                "4. Recommended actions for the PR author\n" +
                "5. Priority assessment of feedback\n\n" +
                "Comments to analyze:\n" +
                commentsText + "\n\n" +
                "Respond in JSON format with the following structure:\n" +
                "{\n" +
                "  \"summary\": \"overall summary\",\n" +
                "  \"categories\": {\"category\": [\"comment1\", \"comment2\"]},\n" +
                "  \"insights\": [\"insight1\", \"insight2\"],\n" +
                "  \"recommendations\": [\"action1\", \"action2\"]\n" +
                "}";

            var response = await CallGeminiApiAsync(prompt, cancellationToken);
            return ParseCommentAnalysisResponse(response, comments);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing comments");
            return new CommentAnalysisResult
            {
                Summary = "Unable to analyze comments at this time.",
                Insights = new List<string> { "Analysis service temporarily unavailable" }
            };
        }
    }

    public async Task<IEnumerable<string>> GenerateCommentSuggestionsAsync(
        string codeContext,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Generating comment suggestions for code context");

            var prompt = $"""
                Based on the following code context, suggest 3-5 constructive review comments that would be helpful:

                Code Context:
                {codeContext}

                Provide suggestions for:
                1. Code quality improvements
                2. Best practices recommendations
                3. Potential bug prevention
                4. Performance considerations
                5. Maintainability enhancements

                Return only the suggested comments, one per line, without numbering.
                """;

            var response = await CallGeminiApiAsync(prompt, cancellationToken);
            return response.Split('\n', StringSplitOptions.RemoveEmptyEntries)
                          .Where(line => !string.IsNullOrWhiteSpace(line))
                          .Select(line => line.Trim())
                          .Take(5);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating comment suggestions");
            return new[] { "Unable to generate comment suggestions at this time." };
        }
    }

    public async Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Checking Gemini service availability");
            
            // Simple test call to verify API connectivity
            var testPrompt = "Respond with 'OK' if you can process this request.";
            var response = await CallGeminiApiAsync(testPrompt, cancellationToken);
            
            return !string.IsNullOrWhiteSpace(response);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Gemini service is not available");
            return false;
        }
    }

    private async Task<string> CallGeminiApiAsync(string prompt, CancellationToken cancellationToken)
    {
        var requestBody = new
        {
            contents = new[]
            {
                new
                {
                    parts = new[] { new { text = prompt } }
                }
            },
            generationConfig = new
            {
                temperature = 0.7,
                topK = 40,
                topP = 0.95,
                maxOutputTokens = 2048
            }
        };

        var json = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var requestUri = $"{_baseUrl}/models/gemini-pro:generateContent?key={_apiKey}";
        var response = await _httpClient.PostAsync(requestUri, content, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogError("Gemini API error: {StatusCode} - {Content}", response.StatusCode, errorContent);
            throw new HttpRequestException($"Gemini API error: {response.StatusCode}");
        }

        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
        return ExtractTextFromGeminiResponse(responseContent);
    }

    private string ExtractTextFromGeminiResponse(string responseJson)
    {
        try
        {
            var jsonDoc = JsonDocument.Parse(responseJson);
            var candidates = jsonDoc.RootElement.GetProperty("candidates");
            
            if (candidates.GetArrayLength() > 0)
            {
                var firstCandidate = candidates[0];
                var content = firstCandidate.GetProperty("content");
                var parts = content.GetProperty("parts");
                
                if (parts.GetArrayLength() > 0)
                {
                    var firstPart = parts[0];
                    return firstPart.GetProperty("text").GetString() ?? string.Empty;
                }
            }
            
            return string.Empty;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing Gemini response: {Response}", responseJson);
            return "Error parsing AI response.";
        }
    }

    private CommentAnalysisResult ParseCommentAnalysisResponse(string response, IEnumerable<Comment> originalComments)
    {
        try
        {
            var jsonDoc = JsonDocument.Parse(response);
            var root = jsonDoc.RootElement;

            var result = new CommentAnalysisResult
            {
                Summary = root.GetProperty("summary").GetString() ?? "No summary available"
            };

            // Parse categories
            if (root.TryGetProperty("categories", out var categoriesElement))
            {
                foreach (var category in categoriesElement.EnumerateObject())
                {
                    var categoryName = category.Name;
                    var commentsList = new List<Comment>();
                    
                    foreach (var commentText in category.Value.EnumerateArray())
                    {
                        var text = commentText.GetString();
                        var matchingComment = originalComments.FirstOrDefault(c => 
                            c.Body.Contains(text?.Replace("- ", "") ?? "", StringComparison.OrdinalIgnoreCase));
                        if (matchingComment != null)
                        {
                            commentsList.Add(matchingComment);
                        }
                    }
                    
                    result.CategorizedComments[categoryName] = commentsList;
                }
            }

            // Parse insights
            if (root.TryGetProperty("insights", out var insightsElement))
            {
                foreach (var insight in insightsElement.EnumerateArray())
                {
                    var insightText = insight.GetString();
                    if (!string.IsNullOrWhiteSpace(insightText))
                    {
                        result.Insights.Add(insightText);
                    }
                }
            }

            // Parse recommendations
            if (root.TryGetProperty("recommendations", out var recommendationsElement))
            {
                foreach (var recommendation in recommendationsElement.EnumerateArray())
                {
                    var recommendationText = recommendation.GetString();
                    if (!string.IsNullOrWhiteSpace(recommendationText))
                    {
                        result.RecommendedActions.Add(recommendationText);
                    }
                }
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing comment analysis response");
            return new CommentAnalysisResult
            {
                Summary = response, // Fallback to raw response
                Insights = new List<string> { "Unable to parse detailed analysis" }
            };
        }
    }
}