using GitHubPrTool.Core.Interfaces;
using GitHubPrTool.Core.Models;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace GitHubPrTool.Infrastructure.Services;

/// <summary>
/// AI-powered comment analysis service
/// </summary>
public class CommentAnalyzer : ICommentAnalyzer
{
    private readonly IAIService _aiService;
    private readonly ILogger<CommentAnalyzer> _logger;

    public CommentAnalyzer(
        IAIService aiService,
        ILogger<CommentAnalyzer> logger)
    {
        _aiService = aiService ?? throw new ArgumentNullException(nameof(aiService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task<CommentCategory> CategorizeCommentAsync(
        Comment comment,
        string? context = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Categorizing comment ID: {CommentId}", comment.Id);

            var prompt = $@"Analyze and categorize this comment:

Comment: {comment.Body}
Type: {comment.Type}
Context: {context ?? "None"}

Please respond with a JSON object in this format:
{{
    ""category"": ""Bug|Feature|CodeReview|Documentation|Question|Performance|Security|Testing|General"",
    ""subCategory"": ""More specific classification"",
    ""confidence"": 0.0-1.0,
    ""priority"": 1-5,
    ""sentiment"": -1.0 to 1.0,
    ""complexity"": 1-5,
    ""tags"": [""tag1"", ""tag2""]
}}

Consider:
- The comment's intent and content
- Whether it identifies issues, requests features, asks questions, etc.
- The tone and urgency level
- Technical complexity";

            var response = await _aiService.GenerateStructuredTextAsync(
                prompt,
                "json",
                null,
                cancellationToken);

            return ParseCommentCategory(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error categorizing comment ID: {CommentId}", comment.Id);
            
            // Return a default category
            return new CommentCategory
            {
                Category = CommentCategoryType.General,
                Confidence = 0.5,
                Priority = 3,
                Sentiment = 0.0,
                Complexity = 3,
                ModelVersion = _aiService.GetModelInfo()
            };
        }
    }

    /// <inheritdoc />
    public async Task<Dictionary<long, CommentCategory>> CategorizeCommentsAsync(
        IEnumerable<Comment> comments,
        string? context = null,
        CancellationToken cancellationToken = default)
    {
        var result = new Dictionary<long, CommentCategory>();
        
        // Process comments in batches to avoid overwhelming the AI service
        const int batchSize = 5;
        var commentsList = comments.ToList();
        
        for (int i = 0; i < commentsList.Count; i += batchSize)
        {
            var batch = commentsList.Skip(i).Take(batchSize);
            var tasks = batch.Select(comment => 
                CategorizeCommentWithId(comment, context, cancellationToken));
            
            var batchResults = await Task.WhenAll(tasks);
            
            foreach (var (commentId, category) in batchResults)
            {
                result[commentId] = category;
            }
        }

        return result;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<AIInsight>> GenerateInsightsAsync(
        Comment comment,
        string? context = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Generating insights for comment ID: {CommentId}", comment.Id);

            var prompt = $@"Generate insights about this comment:

Comment: {comment.Body}
Author: {comment.Author.Login}
Type: {comment.Type}
Context: {context ?? "None"}

Please respond with a JSON array of insights:
[
  {{
    ""type"": ""insight type (e.g., sentiment, complexity, suggestion)"",
    ""content"": ""insight description"",
    ""confidence"": 0.0-1.0,
    ""metadata"": ""additional data as JSON string""
  }}
]

Generate insights about:
- Sentiment and tone
- Technical complexity
- Potential action items
- Relationships to other comments
- Code quality observations
- Communication effectiveness";

            var response = await _aiService.GenerateStructuredTextAsync(
                prompt,
                "json",
                null,
                cancellationToken);

            return ParseInsights(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating insights for comment ID: {CommentId}", comment.Id);
            
            // Return a basic insight
            return new[]
            {
                new AIInsight
                {
                    Type = "analysis",
                    Content = "Unable to generate detailed insights at this time",
                    Confidence = 0.1,
                    ModelVersion = _aiService.GetModelInfo()
                }
            };
        }
    }

    /// <inheritdoc />
    public async Task<double> AnalyzeSentimentAsync(
        string commentText,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _aiService.AnalyzeTextAsync(
                commentText,
                "sentiment",
                cancellationToken);

            // Try to parse the sentiment score
            if (double.TryParse(response.Trim(), out var sentiment))
            {
                return Math.Max(-1.0, Math.Min(1.0, sentiment)); // Clamp between -1 and 1
            }

            // If parsing fails, return neutral sentiment
            return 0.0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing sentiment for text");
            return 0.0; // Return neutral sentiment on error
        }
    }

    /// <inheritdoc />
    public async Task<Dictionary<string, double>> ExtractTopicsAsync(
        IEnumerable<Comment> comments,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var commentTexts = comments.Select(c => c.Body).ToList();
            var combinedText = string.Join("\n---\n", commentTexts);

            var prompt = $@"Extract the main topics and themes from these comments and return as JSON:

Comments:
{combinedText}

Please respond with a JSON object where keys are topics and values are relevance scores (0.0-1.0):
{{
    ""topic1"": 0.8,
    ""topic2"": 0.6,
    ""topic3"": 0.4
}}

Focus on:
- Technical topics and technologies mentioned
- Common themes across comments
- Issues or problems discussed
- Feature requests or enhancements";

            var response = await _aiService.GenerateStructuredTextAsync(
                prompt,
                "json",
                null,
                cancellationToken);

            return ParseTopics(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extracting topics from comments");
            return new Dictionary<string, double>();
        }
    }

    private async Task<(long commentId, CommentCategory category)> CategorizeCommentWithId(
        Comment comment,
        string? context,
        CancellationToken cancellationToken)
    {
        var category = await CategorizeCommentAsync(comment, context, cancellationToken);
        return (comment.Id, category);
    }

    private CommentCategory ParseCommentCategory(string jsonResponse)
    {
        try
        {
            using var document = JsonDocument.Parse(jsonResponse);
            var root = document.RootElement;

            var categoryString = GetStringProperty(root, "category") ?? "General";
            var category = Enum.TryParse<CommentCategoryType>(categoryString, true, out var parsedCategory) 
                ? parsedCategory 
                : CommentCategoryType.General;

            return new CommentCategory
            {
                Category = category,
                SubCategory = GetStringProperty(root, "subCategory"),
                Confidence = GetDoubleProperty(root, "confidence") ?? 0.5,
                Priority = GetIntProperty(root, "priority") ?? 3,
                Sentiment = GetDoubleProperty(root, "sentiment") ?? 0.0,
                Complexity = GetIntProperty(root, "complexity") ?? 3,
                Tags = GetStringArrayProperty(root, "tags"),
                ModelVersion = _aiService.GetModelInfo()
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error parsing comment category JSON: {Response}", jsonResponse);
            
            return new CommentCategory
            {
                Category = CommentCategoryType.General,
                Confidence = 0.3,
                Priority = 3,
                Sentiment = 0.0,
                Complexity = 3,
                ModelVersion = _aiService.GetModelInfo()
            };
        }
    }

    private IEnumerable<AIInsight> ParseInsights(string jsonResponse)
    {
        try
        {
            using var document = JsonDocument.Parse(jsonResponse);
            var root = document.RootElement;

            if (root.ValueKind == JsonValueKind.Array)
            {
                return root.EnumerateArray()
                    .Select(element => new AIInsight
                    {
                        Type = GetStringProperty(element, "type") ?? "general",
                        Content = GetStringProperty(element, "content") ?? "",
                        Confidence = GetDoubleProperty(element, "confidence") ?? 0.5,
                        Metadata = GetStringProperty(element, "metadata"),
                        ModelVersion = _aiService.GetModelInfo()
                    })
                    .ToList();
            }

            return new List<AIInsight>();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error parsing insights JSON: {Response}", jsonResponse);
            
            return new[]
            {
                new AIInsight
                {
                    Type = "parsing_error",
                    Content = "Unable to parse AI insights response",
                    Confidence = 0.1,
                    ModelVersion = _aiService.GetModelInfo()
                }
            };
        }
    }

    private Dictionary<string, double> ParseTopics(string jsonResponse)
    {
        try
        {
            using var document = JsonDocument.Parse(jsonResponse);
            var root = document.RootElement;

            var topics = new Dictionary<string, double>();

            if (root.ValueKind == JsonValueKind.Object)
            {
                foreach (var property in root.EnumerateObject())
                {
                    if (property.Value.ValueKind == JsonValueKind.Number)
                    {
                        topics[property.Name] = property.Value.GetDouble();
                    }
                }
            }

            return topics;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error parsing topics JSON: {Response}", jsonResponse);
            return new Dictionary<string, double>();
        }
    }

    private static string? GetStringProperty(JsonElement element, string propertyName)
    {
        return element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String 
            ? property.GetString() 
            : null;
    }

    private static int? GetIntProperty(JsonElement element, string propertyName)
    {
        return element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Number 
            ? property.GetInt32() 
            : null;
    }

    private static double? GetDoubleProperty(JsonElement element, string propertyName)
    {
        return element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Number 
            ? property.GetDouble() 
            : null;
    }

    private static List<string> GetStringArrayProperty(JsonElement element, string propertyName)
    {
        if (element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Array)
        {
            return property.EnumerateArray()
                .Where(item => item.ValueKind == JsonValueKind.String)
                .Select(item => item.GetString() ?? "")
                .Where(s => !string.IsNullOrEmpty(s))
                .ToList();
        }
        return new List<string>();
    }
}