using GitHubPrTool.Core.Interfaces;
using GitHubPrTool.Core.Models;
using GitHubPrTool.Infrastructure.Utilities;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace GitHubPrTool.Infrastructure.Services;

/// <summary>
/// AI-powered architecture analysis service
/// </summary>
public class ArchitectureAnalyzer : IArchitectureAnalyzer
{
    private const int MaxProjectFilesForAnalysis = 10;
    private const int MaxFilesPerFolderForAnalysis = 20;
    
    private readonly IAIService _aiService;
    private readonly ILogger<ArchitectureAnalyzer> _logger;

    public ArchitectureAnalyzer(
        IAIService aiService,
        ILogger<ArchitectureAnalyzer> logger)
    {
        _aiService = aiService ?? throw new ArgumentNullException(nameof(aiService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task<IEnumerable<ArchitectureRecommendation>> AnalyzeArchitectureAsync(
        string repositoryPath,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Analyzing architecture for repository: {RepositoryPath}", repositoryPath);

            // Analyze the repository structure
            var projectStructure = await AnalyzeProjectStructure(repositoryPath);
            
            var prompt = $@"Analyze this software project structure and provide architecture recommendations:

Project Structure:
{projectStructure}

Please provide specific architecture recommendations in the following JSON format:
[
  {{
    ""title"": ""Recommendation title"",
    ""description"": ""Detailed description"",
    ""category"": ""Category (Performance, Security, Maintainability, etc.)"",
    ""priority"": 1-5,
    ""impact"": ""Description of impact"",
    ""effort"": ""Estimated effort"",
    ""affectedFiles"": [""file1.cs"", ""file2.cs""],
    ""implementationSteps"": [""Step 1"", ""Step 2""],
    ""relatedPatterns"": [""Pattern 1"", ""Pattern 2""],
    ""codeExample"": ""Example code if applicable"",
    ""references"": [""Reference 1"", ""Reference 2""]
  }}
]

Focus on:
- Code organization and separation of concerns
- Design patterns and best practices
- Performance optimization opportunities
- Security considerations
- Maintainability improvements
- Testing strategies";

            var response = await _aiService.GenerateStructuredTextAsync(
                prompt, 
                "json", 
                null, 
                cancellationToken);

            return ParseRecommendations(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing architecture for repository: {RepositoryPath}", repositoryPath);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<ArchitectureRecommendation> ExplainRecommendationAsync(
        string codeContext,
        string pattern,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Explaining recommendation for pattern: {Pattern}", pattern);

            var prompt = $@"Explain why the architecture pattern ""{pattern}"" is recommended for this code context:

Code Context:
{codeContext}

Please provide a detailed explanation in the following JSON format:
{{
    ""title"": ""Why {pattern} is Recommended"",
    ""description"": ""Detailed explanation of why this pattern fits"",
    ""category"": ""Architecture"",
    ""priority"": 1-5,
    ""impact"": ""Impact of implementing this pattern"",
    ""effort"": ""Effort required to implement"",
    ""affectedFiles"": [""files that would be affected""],
    ""implementationSteps"": [""detailed implementation steps""],
    ""relatedPatterns"": [""related or complementary patterns""],
    ""codeExample"": ""concrete code example showing the pattern"",
    ""references"": [""documentation or resource links""]
}}

Focus on:
- Benefits of this specific pattern
- How it solves current problems
- Trade-offs and considerations
- Implementation guidance
- Real-world examples";

            var response = await _aiService.GenerateStructuredTextAsync(
                prompt,
                "json",
                null,
                cancellationToken);

            var recommendations = ParseRecommendations(response);
            return recommendations.FirstOrDefault() ?? new ArchitectureRecommendation
            {
                Title = $"Explanation for {pattern}",
                Description = "Unable to generate explanation",
                Category = "Architecture",
                ModelVersion = _aiService.GetModelInfo()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error explaining recommendation for pattern: {Pattern}", pattern);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<IEnumerable<ArchitectureRecommendation>> AnalyzeCodeQualityAsync(
        Dictionary<string, string> codeFiles,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Analyzing code quality for {FileCount} files", codeFiles.Count);

            var codeAnalysis = string.Join("\n\n", codeFiles.Select(kvp => 
                $"File: {kvp.Key}\n```\n{kvp.Value}\n```"));

            var prompt = $@"Analyze the code quality of these files and provide specific improvement recommendations:

{codeAnalysis}

Please provide code quality recommendations in the following JSON format:
[
  {{
    ""title"": ""Code quality issue or improvement"",
    ""description"": ""Detailed description of the issue and solution"",
    ""category"": ""Code Quality category"",
    ""priority"": 1-5,
    ""impact"": ""Impact of fixing this issue"",
    ""effort"": ""Effort required to fix"",
    ""affectedFiles"": [""specific files affected""],
    ""implementationSteps"": [""steps to implement the fix""],
    ""relatedPatterns"": [""relevant patterns or practices""],
    ""codeExample"": ""example of improved code"",
    ""references"": [""documentation references""]
  }}
]

Focus on:
- Code complexity and readability
- Potential bugs or issues
- Performance bottlenecks
- Security vulnerabilities
- Design pattern violations
- Testing gaps
- Documentation needs";

            var response = await _aiService.GenerateStructuredTextAsync(
                prompt,
                "json",
                null,
                cancellationToken);

            return ParseRecommendations(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing code quality");
            throw;
        }
    }

    private Task<string> AnalyzeProjectStructure(string repositoryPath)
    {
        try
        {
            if (!Directory.Exists(repositoryPath))
            {
                return Task.FromResult($"Repository path does not exist: {repositoryPath}");
            }

            var structure = new List<string>();
            
            // Get project files
            var projectFiles = Directory.GetFiles(repositoryPath, "*.csproj", SearchOption.AllDirectories)
                .Concat(Directory.GetFiles(repositoryPath, "*.sln", SearchOption.AllDirectories))
                .Take(MaxProjectFilesForAnalysis); // Limit for analysis

            foreach (var file in projectFiles)
            {
                var relativePath = Path.GetRelativePath(repositoryPath, file);
                structure.Add($"Project: {relativePath}");
            }

            // Get key folders
            var keyFolders = new[] { "src", "test", "tests", "docs", "scripts", "Controllers", "Services", "Models", "Views" };
            foreach (var folder in keyFolders)
            {
                var folderPath = Path.Combine(repositoryPath, folder);
                if (Directory.Exists(folderPath))
                {
                    var files = Directory.GetFiles(folderPath, "*.cs", SearchOption.AllDirectories)
                        .Take(MaxFilesPerFolderForAnalysis) // Limit files for analysis
                        .Select(f => Path.GetRelativePath(repositoryPath, f));
                    
                    structure.Add($"Folder {folder}: {string.Join(", ", files)}");
                }
            }

            return Task.FromResult(string.Join("\n", structure));
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error analyzing project structure for: {RepositoryPath}", repositoryPath);
            return Task.FromResult($"Unable to analyze structure: {ex.Message}");
        }
    }

    private IEnumerable<ArchitectureRecommendation> ParseRecommendations(string jsonResponse)
    {
        try
        {
            // Try to parse as an array first
            try
            {
                using var document = JsonDocument.Parse(jsonResponse);
                var root = document.RootElement;
                
                if (root.ValueKind == JsonValueKind.Array)
                {
                    return root.EnumerateArray()
                        .Select(element => CreateRecommendationFromJsonElement(element))
                        .ToList();
                }
                else if (root.ValueKind == JsonValueKind.Object)
                {
                    return new[] { CreateRecommendationFromJsonElement(root) };
                }
            }
            catch (JsonException)
            {
                // If JSON parsing fails, fallback
            }

            return new List<ArchitectureRecommendation>();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error parsing recommendations JSON: {Response}", jsonResponse);
            
            // Return a fallback recommendation with the raw response
            return new[]
            {
                new ArchitectureRecommendation
                {
                    Title = "Analysis Result",
                    Description = jsonResponse,
                    Category = "General",
                    Priority = 3,
                    Impact = "See description for details",
                    Effort = "To be determined",
                    ModelVersion = _aiService.GetModelInfo()
                }
            };
        }
    }

    private ArchitectureRecommendation CreateRecommendationFromJsonElement(JsonElement element)
    {
        return new ArchitectureRecommendation
        {
            Title = JsonParsingUtils.GetStringProperty(element, "title") ?? "Unknown",
            Description = JsonParsingUtils.GetStringProperty(element, "description") ?? "",
            Category = JsonParsingUtils.GetStringProperty(element, "category") ?? "General",
            Priority = JsonParsingUtils.GetIntProperty(element, "priority") ?? 3,
            Impact = JsonParsingUtils.GetStringProperty(element, "impact") ?? "",
            Effort = JsonParsingUtils.GetStringProperty(element, "effort") ?? "",
            AffectedFiles = JsonParsingUtils.GetStringArrayProperty(element, "affectedFiles"),
            ImplementationSteps = JsonParsingUtils.GetStringArrayProperty(element, "implementationSteps"),
            RelatedPatterns = JsonParsingUtils.GetStringArrayProperty(element, "relatedPatterns"),
            CodeExample = JsonParsingUtils.GetStringProperty(element, "codeExample"),
            References = JsonParsingUtils.GetStringArrayProperty(element, "references"),
            ModelVersion = _aiService.GetModelInfo()
        };
    }
}