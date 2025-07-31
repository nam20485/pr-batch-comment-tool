using GitHubPrTool.Core.Interfaces;
using GitHubPrTool.Core.Models;
using GitHubPrTool.Infrastructure.Utilities;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace GitHubPrTool.Infrastructure.Services;

/// <summary>
/// AI-powered comment suggestion service
/// </summary>
public class CommentSuggestionService : ICommentSuggestionService
{
    private readonly IAIService _aiService;
    private readonly ILogger<CommentSuggestionService> _logger;

    public CommentSuggestionService(
        IAIService aiService,
        ILogger<CommentSuggestionService> logger)
    {
        _aiService = aiService ?? throw new ArgumentNullException(nameof(aiService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task<IEnumerable<CommentSuggestion>> GenerateCodeReviewSuggestionsAsync(
        Dictionary<string, string> codeChanges,
        IEnumerable<Comment>? existingComments = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Generating code review suggestions for {FileCount} files", codeChanges.Count);

            var codeAnalysis = string.Join("\n\n", codeChanges.Select(kvp => 
                $"File: {kvp.Key}\n```\n{kvp.Value}\n```"));

            var existingCommentsText = existingComments != null 
                ? string.Join("\n", existingComments.Select(c => $"- {c.Body}"))
                : "None";

            var prompt = $@"Analyze the code changes and generate helpful code review suggestions:

Code Changes:
{codeAnalysis}

Existing Comments:
{existingCommentsText}

Please respond with a JSON array of suggestions:
[
  {{
    ""suggestedText"": ""The suggested comment text"",
    ""context"": ""Why this suggestion is relevant"",
    ""suggestionType"": ""improvement|question|praise|concern"",
    ""confidence"": 0.0-1.0,
    ""relevance"": 0.0-1.0,
    ""tone"": ""constructive|neutral|appreciative"",
    ""targetFile"": ""file path if applicable"",
    ""targetLine"": line_number_if_applicable,
    ""codeSnippet"": ""relevant code snippet"",
    ""alternatives"": [""alternative suggestion 1"", ""alternative suggestion 2""],
    ""tags"": [""tag1"", ""tag2""]
  }}
]

Focus on:
- Code quality improvements
- Best practices
- Potential bugs or issues
- Performance considerations
- Security concerns
- Maintainability
- Testing suggestions
- Documentation needs

Avoid duplicating existing comments and provide constructive, actionable feedback.";

            var response = await _aiService.GenerateStructuredTextAsync(
                prompt,
                "json",
                null,
                cancellationToken);

            return ParseCommentSuggestions(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating code review suggestions");
            return new List<CommentSuggestion>();
        }
    }

    /// <inheritdoc />
    public async Task<IEnumerable<CommentSuggestion>> SuggestResponsesAsync(
        Comment originalComment,
        string? context = null,
        string? responseType = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Suggesting responses to comment ID: {CommentId}", originalComment.Id);

            var prompt = $@"Generate appropriate response suggestions to this comment:

Original Comment: {originalComment.Body}
Author: {originalComment.Author.Login}
Type: {originalComment.Type}
Context: {context ?? "None"}
Desired Response Type: {responseType ?? "Any appropriate response"}

Please respond with a JSON array of response suggestions:
[
  {{
    ""suggestedText"": ""The suggested response text"",
    ""context"": ""Why this response is appropriate"",
    ""suggestionType"": ""acknowledgment|clarification|solution|agreement|disagreement"",
    ""confidence"": 0.0-1.0,
    ""relevance"": 0.0-1.0,
    ""tone"": ""professional|friendly|helpful|diplomatic"",
    ""alternatives"": [""alternative response 1"", ""alternative response 2""],
    ""tags"": [""tag1"", ""tag2""]
  }}
]

Consider:
- The original comment's tone and intent
- Professional and constructive responses
- Addressing specific points raised
- Providing helpful information or solutions
- Maintaining positive team dynamics";

            var response = await _aiService.GenerateStructuredTextAsync(
                prompt,
                "json",
                null,
                cancellationToken);

            return ParseCommentSuggestions(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error suggesting responses to comment ID: {CommentId}", originalComment.Id);
            return new List<CommentSuggestion>();
        }
    }

    /// <inheritdoc />
    public async Task<IEnumerable<CommentSuggestion>> SuggestImprovementsAsync(
        Comment comment,
        IEnumerable<string>? improvementGoals = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Suggesting improvements for comment ID: {CommentId}", comment.Id);

            var goals = improvementGoals?.Any() == true 
                ? string.Join(", ", improvementGoals) 
                : "clarity, tone, actionability";

            var prompt = $@"Suggest improvements for this comment:

Original Comment: {comment.Body}
Author: {comment.Author.Login}
Improvement Goals: {goals}

Please respond with a JSON array of improvement suggestions:
[
  {{
    ""suggestedText"": ""The improved comment text"",
    ""context"": ""What was improved and why"",
    ""suggestionType"": ""improvement"",
    ""confidence"": 0.0-1.0,
    ""relevance"": 0.0-1.0,
    ""tone"": ""improved tone"",
    ""alternatives"": [""alternative improvement 1"", ""alternative improvement 2""],
    ""tags"": [""clarity"", ""tone"", ""actionability""]
  }}
]

Focus on:
- Making the message clearer and more specific
- Improving the tone to be more constructive
- Adding actionable suggestions
- Better structure and organization
- More professional language
- Removing ambiguity";

            var response = await _aiService.GenerateStructuredTextAsync(
                prompt,
                "json",
                null,
                cancellationToken);

            return ParseCommentSuggestions(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error suggesting improvements for comment ID: {CommentId}", comment.Id);
            return new List<CommentSuggestion>();
        }
    }

    /// <inheritdoc />
    public async Task<IEnumerable<CommentSuggestion>> GenerateContextualSuggestionsAsync(
        PullRequest pullRequest,
        string? suggestionType = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Generating contextual suggestions for PR: {PullRequestId}", pullRequest.Id);

            var prompt = $@"Generate contextual comment suggestions for this pull request:

PR Title: {pullRequest.Title}
PR Description: {pullRequest.Body}
Author: {pullRequest.Author.Login}
State: {pullRequest.State}
Suggestion Type: {suggestionType ?? "General helpful comments"}

Please respond with a JSON array of contextual suggestions:
[
  {{
    ""suggestedText"": ""The suggested comment text"",
    ""context"": ""Why this comment would be helpful in this PR context"",
    ""suggestionType"": ""review|documentation|testing|architecture|performance"",
    ""confidence"": 0.0-1.0,
    ""relevance"": 0.0-1.0,
    ""tone"": ""constructive|helpful|appreciative"",
    ""alternatives"": [""alternative suggestion 1""],
    ""tags"": [""contextual"", ""helpful""]
  }}
]

Consider:
- The PR's purpose and scope
- Potential areas that need attention
- Helpful questions to ask
- Suggestions for testing
- Documentation needs
- Appreciation for good practices";

            var response = await _aiService.GenerateStructuredTextAsync(
                prompt,
                "json",
                null,
                cancellationToken);

            return ParseCommentSuggestions(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating contextual suggestions for PR: {PullRequestId}", pullRequest.Id);
            return new List<CommentSuggestion>();
        }
    }

    /// <inheritdoc />
    public async Task ProvideFeedbackAsync(
        CommentSuggestion suggestion,
        string feedback,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Recording feedback for suggestion: {SuggestionId}", suggestion.Id);

            // Validate and sanitize feedback input
            var sanitizedFeedback = SanitizeInput(feedback);
            
            // Update the suggestion with user feedback
            suggestion.UserFeedback = sanitizedFeedback;
            suggestion.IsUsed = true;

            // In a real implementation, this would be saved to a database
            // For now, we just log the feedback for future model improvement
            _logger.LogInformation("Feedback recorded: {Feedback} for suggestion type: {Type}", 
                feedback, suggestion.SuggestionType);

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error providing feedback for suggestion: {SuggestionId}", suggestion.Id);
            throw;
        }
    }

    private IEnumerable<CommentSuggestion> ParseCommentSuggestions(string jsonResponse)
    {
        try
        {
            using var document = JsonDocument.Parse(jsonResponse);
            var root = document.RootElement;

            if (root.ValueKind == JsonValueKind.Array)
            {
                return root.EnumerateArray()
                    .Select(element => new CommentSuggestion
                    {
                        SuggestedText = GetStringProperty(element, "suggestedText") ?? "",
                        Context = GetStringProperty(element, "context") ?? "",
                        SuggestionType = GetStringProperty(element, "suggestionType") ?? "general",
                        Confidence = GetDoubleProperty(element, "confidence") ?? 0.5,
                        Relevance = GetDoubleProperty(element, "relevance") ?? 0.5,
                        Tone = GetStringProperty(element, "tone") ?? "neutral",
                        TargetFile = GetStringProperty(element, "targetFile"),
                        TargetLine = GetIntProperty(element, "targetLine"),
                        CodeSnippet = GetStringProperty(element, "codeSnippet"),
                        Alternatives = GetStringArrayProperty(element, "alternatives"),
                        Tags = GetStringArrayProperty(element, "tags"),
                        ModelVersion = _aiService.GetModelInfo()
                    })
                    .ToList();
            }

            return new List<CommentSuggestion>();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error parsing comment suggestions JSON: {Response}", jsonResponse);
            
            return new []
            {
                new CommentSuggestion
                {
                    SuggestedText = "Unable to generate suggestions at this time",
                    Context = "Parsing error occurred",
                    SuggestionType = "error",
                    Confidence = 0.1,
                    Relevance = 0.1,
                    Tone = "neutral",
                    ModelVersion = _aiService.GetModelInfo()
                }
            };
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

    /// <summary>
    /// Sanitizes user input to prevent injection attacks and ensure data safety
    /// </summary>
    /// <param name="input">Raw user input</param>
    /// <returns>Sanitized input safe for storage and processing</returns>
    private static string SanitizeInput(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return string.Empty;
        }

        // Remove potentially dangerous characters and limit length
        var sanitized = input
            .Replace("<", "&lt;")
            .Replace(">", "&gt;")
            .Replace("\"", "&quot;")
            .Replace("'", "&#x27;")
            .Replace("&", "&amp;")
            .Replace("\0", "")
            .Trim();

        // Limit length to prevent excessive storage/processing
        const int maxLength = 2000;
        if (sanitized.Length > maxLength)
        {
            sanitized = sanitized.Substring(0, maxLength) + "...";
        }

        return sanitized;
    }
}