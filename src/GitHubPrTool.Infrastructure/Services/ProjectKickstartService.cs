using GitHubPrTool.Core.Interfaces;
using GitHubPrTool.Core.Models;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace GitHubPrTool.Infrastructure.Services;

/// <summary>
/// AI-powered project kickstart plan generation service
/// </summary>
public class ProjectKickstartService : IProjectKickstartService
{
    private readonly IAIService _aiService;
    private readonly ILogger<ProjectKickstartService> _logger;

    /// <summary>
    /// Maximum number of project files to analyze for kickstart planning
    /// </summary>
    private const int MaxProjectFilesToAnalyze = 15;

    public ProjectKickstartService(
        IAIService aiService,
        ILogger<ProjectKickstartService> logger)
    {
        _aiService = aiService ?? throw new ArgumentNullException(nameof(aiService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task<ProjectKickstartPlan> GenerateKickstartPlanAsync(
        string projectName,
        string requirements,
        string? constraints = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Generating kickstart plan for project: {ProjectName}", projectName);

            var prompt = $@"Generate a comprehensive project kickstart plan:

Project Name: {projectName}
Requirements: {requirements}
Constraints: {constraints ?? "None specified"}

Please respond with a detailed JSON plan:
{{
    ""projectName"": ""{projectName}"",
    ""description"": ""Project overview and goals"",
    ""technologyStack"": [""tech1"", ""tech2"", ""tech3""],
    ""milestones"": [
        {{
            ""name"": ""Milestone name"",
            ""description"": ""Milestone description"",
            ""deliverables"": [""deliverable1"", ""deliverable2""],
            ""durationWeeks"": 2,
            ""dependencies"": [""prerequisite milestone""]
        }}
    ],
    ""teamStructure"": [
        {{
            ""title"": ""Role title"",
            ""description"": ""Role responsibilities"",
            ""requiredSkills"": [""skill1"", ""skill2""],
            ""commitment"": ""Full-time|Part-time|Consultant""
        }}
    ],
    ""phases"": [
        {{
            ""name"": ""Phase name"",
            ""description"": ""Phase description"",
            ""activities"": [""activity1"", ""activity2""],
            ""outcomes"": [""outcome1"", ""outcome2""],
            ""durationWeeks"": 4
        }}
    ],
    ""estimatedTimelineWeeks"": 12,
    ""risks"": [
        {{
            ""risk"": ""Risk description"",
            ""probability"": 1-5,
            ""impact"": 1-5,
            ""mitigationStrategies"": [""strategy1"", ""strategy2""]
        }}
    ],
    ""successCriteria"": [""criteria1"", ""criteria2""]
}}

Consider:
- Modern development practices
- Scalability and maintainability
- Team skills and resources
- Market timing and constraints
- Risk mitigation
- Realistic timelines
- Clear success metrics";

            var response = await _aiService.GenerateStructuredTextAsync(
                prompt,
                "json",
                null,
                cancellationToken);

            return ParseProjectKickstartPlan(response, projectName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating kickstart plan for project: {ProjectName}", projectName);
            
            // Return a basic fallback plan
            return CreateFallbackPlan(projectName, requirements);
        }
    }

    /// <inheritdoc />
    public async Task<ProjectKickstartPlan> GenerateEnhancementPlanAsync(
        string repositoryPath,
        string enhancementGoals,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Generating enhancement plan for repository: {RepositoryPath}", repositoryPath);

            // Analyze the existing repository structure
            var projectStructure = await AnalyzeRepositoryStructure(repositoryPath);

            var prompt = $@"Generate an enhancement plan for this existing project:

Repository Structure:
{projectStructure}

Enhancement Goals: {enhancementGoals}

Please respond with a detailed JSON enhancement plan:
{{
    ""projectName"": ""Inferred project name"",
    ""description"": ""Enhancement plan overview"",
    ""technologyStack"": [""current and new technologies""],
    ""milestones"": [
        {{
            ""name"": ""Enhancement milestone"",
            ""description"": ""What will be achieved"",
            ""deliverables"": [""specific deliverables""],
            ""durationWeeks"": 2,
            ""dependencies"": [""prerequisites""]
        }}
    ],
    ""teamStructure"": [
        {{
            ""title"": ""Role needed for enhancement"",
            ""description"": ""Specific responsibilities"",
            ""requiredSkills"": [""required skills""],
            ""commitment"": ""Full-time|Part-time|Consultant""
        }}
    ],
    ""phases"": [
        {{
            ""name"": ""Enhancement phase"",
            ""description"": ""Phase description"",
            ""activities"": [""phase activities""],
            ""outcomes"": [""expected outcomes""],
            ""durationWeeks"": 3
        }}
    ],
    ""estimatedTimelineWeeks"": 8,
    ""risks"": [
        {{
            ""risk"": ""Enhancement-specific risk"",
            ""probability"": 1-5,
            ""impact"": 1-5,
            ""mitigationStrategies"": [""mitigation approaches""]
        }}
    ],
    ""successCriteria"": [""measurable success criteria""]
}}

Focus on:
- Leveraging existing codebase
- Minimizing disruption
- Incremental improvements
- Technical debt reduction
- Performance enhancements
- User experience improvements";

            var response = await _aiService.GenerateStructuredTextAsync(
                prompt,
                "json",
                null,
                cancellationToken);

            return ParseProjectKickstartPlan(response, "Enhancement Plan");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating enhancement plan for repository: {RepositoryPath}", repositoryPath);
            
            return CreateFallbackPlan("Enhancement Plan", enhancementGoals);
        }
    }

    /// <inheritdoc />
    public async Task<ProjectKickstartPlan> EstimateProjectTimelineAsync(
        string requirements,
        int teamSize,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Estimating project timeline for team size: {TeamSize}", teamSize);

            var prompt = $@"Estimate a realistic project timeline and structure:

Requirements: {requirements}
Team Size: {teamSize} people

Please respond with a timeline-focused JSON plan:
{{
    ""projectName"": ""Timeline Estimation"",
    ""description"": ""Project timeline and resource allocation"",
    ""technologyStack"": [""recommended technologies""],
    ""milestones"": [
        {{
            ""name"": ""Timeline milestone"",
            ""description"": ""Milestone objectives"",
            ""deliverables"": [""key deliverables""],
            ""durationWeeks"": estimated_weeks,
            ""dependencies"": [""blocking factors""]
        }}
    ],
    ""teamStructure"": [
        {{
            ""title"": ""Team role"",
            ""description"": ""Role in timeline"",
            ""requiredSkills"": [""essential skills""],
            ""commitment"": ""workload estimate""
        }}
    ],
    ""phases"": [
        {{
            ""name"": ""Development phase"",
            ""description"": ""Phase focus"",
            ""activities"": [""parallel activities""],
            ""outcomes"": [""phase outcomes""],
            ""durationWeeks"": estimated_duration
        }}
    ],
    ""estimatedTimelineWeeks"": total_estimated_weeks,
    ""risks"": [
        {{
            ""risk"": ""Timeline risk"",
            ""probability"": 1-5,
            ""impact"": 1-5,
            ""mitigationStrategies"": [""schedule protection strategies""]
        }}
    ],
    ""successCriteria"": [""timeline success metrics""]
}}

Consider:
- Team velocity and experience
- Complexity of requirements
- Dependencies and blockers
- Testing and quality assurance
- Deployment and DevOps setup
- Buffer time for unknowns
- Parallel vs sequential work";

            var response = await _aiService.GenerateStructuredTextAsync(
                prompt,
                "json",
                null,
                cancellationToken);

            return ParseProjectKickstartPlan(response, "Timeline Estimation");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error estimating project timeline");
            
            return CreateFallbackPlan("Timeline Estimation", requirements);
        }
    }

    private Task<string> AnalyzeRepositoryStructure(string repositoryPath)
    {
        try
        {
            // Validate and normalize the repository path for security
            if (string.IsNullOrWhiteSpace(repositoryPath))
            {
                return Task.FromResult("Repository path cannot be null or empty");
            }

            var fullPath = Path.GetFullPath(repositoryPath);
            
            // Basic security check - ensure we're not accessing system directories
            var prohibitedPaths = new[]
            {
                Environment.GetFolderPath(Environment.SpecialFolder.System),
                Environment.GetFolderPath(Environment.SpecialFolder.Windows),
                Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
                Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86)
            };

            if (prohibitedPaths.Any(prohibited => 
                !string.IsNullOrEmpty(prohibited) && 
                fullPath.StartsWith(prohibited, StringComparison.OrdinalIgnoreCase)))
            {
                _logger.LogWarning("Access denied to protected system directory: {RepositoryPath}", repositoryPath);
                return Task.FromResult($"Access denied to protected directory: {repositoryPath}");
            }

            if (!Directory.Exists(fullPath))
            {
                return Task.FromResult($"Repository path does not exist: {repositoryPath}");
            }

            var structure = new List<string>();
            
            // Get project files
            var projectFiles = Directory.GetFiles(fullPath, "*.csproj", SearchOption.AllDirectories)
                .Concat(Directory.GetFiles(fullPath, "*.sln", SearchOption.AllDirectories))
                .Concat(Directory.GetFiles(fullPath, "package.json", SearchOption.AllDirectories))
                .Take(MaxProjectFilesToAnalyze);

            foreach (var file in projectFiles)
            {
                var relativePath = Path.GetRelativePath(fullPath, file);
                structure.Add($"Project file: {relativePath}");
            }

            // Get key folders and file counts
            var keyFolders = new[] { "src", "test", "tests", "docs", "scripts", "Controllers", "Services", "Models", "Views", "Components" };
            foreach (var folder in keyFolders)
            {
                var folderPath = Path.Combine(fullPath, folder);
                if (Directory.Exists(folderPath))
                {
                    var fileCount = Directory.GetFiles(folderPath, "*", SearchOption.AllDirectories).Length;
                    structure.Add($"Folder {folder}: {fileCount} files");
                }
            }

            // Get README or documentation files
            var docFiles = Directory.GetFiles(fullPath, "README*", SearchOption.TopDirectoryOnly)
                .Concat(Directory.GetFiles(fullPath, "*.md", SearchOption.TopDirectoryOnly))
                .Take(5);

            foreach (var file in docFiles)
            {
                var relativePath = Path.GetRelativePath(fullPath, file);
                structure.Add($"Documentation: {relativePath}");
            }

            return Task.FromResult(string.Join("\n", structure));
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error analyzing repository structure: {RepositoryPath}", repositoryPath);
            return Task.FromResult($"Unable to analyze structure: {ex.Message}");
        }
    }

    private ProjectKickstartPlan ParseProjectKickstartPlan(string jsonResponse, string fallbackName)
    {
        try
        {
            using var document = JsonDocument.Parse(jsonResponse);
            var root = document.RootElement;

            return new ProjectKickstartPlan
            {
                ProjectName = GetStringProperty(root, "projectName") ?? fallbackName,
                Description = GetStringProperty(root, "description") ?? "",
                TechnologyStack = GetStringArrayProperty(root, "technologyStack"),
                Milestones = ParseMilestones(root),
                TeamStructure = ParseTeamStructure(root),
                Phases = ParsePhases(root),
                EstimatedTimelineWeeks = GetIntProperty(root, "estimatedTimelineWeeks") ?? 12,
                Risks = ParseRisks(root),
                SuccessCriteria = GetStringArrayProperty(root, "successCriteria"),
                ModelVersion = _aiService.GetModelInfo()
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error parsing project kickstart plan JSON: {Response}", jsonResponse);
            
            return CreateFallbackPlan(fallbackName, "Unable to parse detailed plan");
        }
    }

    private List<ProjectMilestone> ParseMilestones(JsonElement root)
    {
        if (root.TryGetProperty("milestones", out var milestonesProperty) && 
            milestonesProperty.ValueKind == JsonValueKind.Array)
        {
            return milestonesProperty.EnumerateArray()
                .Select(element => new ProjectMilestone
                {
                    Name = GetStringProperty(element, "name") ?? "",
                    Description = GetStringProperty(element, "description") ?? "",
                    Deliverables = GetStringArrayProperty(element, "deliverables"),
                    DurationWeeks = GetIntProperty(element, "durationWeeks") ?? 2,
                    Dependencies = GetStringArrayProperty(element, "dependencies")
                })
                .ToList();
        }
        return new List<ProjectMilestone>();
    }

    private List<TeamRole> ParseTeamStructure(JsonElement root)
    {
        if (root.TryGetProperty("teamStructure", out var teamProperty) && 
            teamProperty.ValueKind == JsonValueKind.Array)
        {
            return teamProperty.EnumerateArray()
                .Select(element => new TeamRole
                {
                    Title = GetStringProperty(element, "title") ?? "",
                    Description = GetStringProperty(element, "description") ?? "",
                    RequiredSkills = GetStringArrayProperty(element, "requiredSkills"),
                    Commitment = GetStringProperty(element, "commitment") ?? ""
                })
                .ToList();
        }
        return new List<TeamRole>();
    }

    private List<DevelopmentPhase> ParsePhases(JsonElement root)
    {
        if (root.TryGetProperty("phases", out var phasesProperty) && 
            phasesProperty.ValueKind == JsonValueKind.Array)
        {
            return phasesProperty.EnumerateArray()
                .Select(element => new DevelopmentPhase
                {
                    Name = GetStringProperty(element, "name") ?? "",
                    Description = GetStringProperty(element, "description") ?? "",
                    Activities = GetStringArrayProperty(element, "activities"),
                    Outcomes = GetStringArrayProperty(element, "outcomes"),
                    DurationWeeks = GetIntProperty(element, "durationWeeks") ?? 2
                })
                .ToList();
        }
        return new List<DevelopmentPhase>();
    }

    private List<RiskAssessment> ParseRisks(JsonElement root)
    {
        if (root.TryGetProperty("risks", out var risksProperty) && 
            risksProperty.ValueKind == JsonValueKind.Array)
        {
            return risksProperty.EnumerateArray()
                .Select(element => new RiskAssessment
                {
                    Risk = GetStringProperty(element, "risk") ?? "",
                    Probability = GetIntProperty(element, "probability") ?? 3,
                    Impact = GetIntProperty(element, "impact") ?? 3,
                    MitigationStrategies = GetStringArrayProperty(element, "mitigationStrategies")
                })
                .ToList();
        }
        return new List<RiskAssessment>();
    }

    private ProjectKickstartPlan CreateFallbackPlan(string projectName, string requirements)
    {
        return new ProjectKickstartPlan
        {
            ProjectName = projectName,
            Description = $"Basic plan for: {requirements}",
            TechnologyStack = new List<string> { "ASP.NET Core", "C#", "SQL Server" },
            Milestones = new List<ProjectMilestone>
            {
                new()
                {
                    Name = "Project Setup",
                    Description = "Initial project setup and configuration",
                    Deliverables = new List<string> { "Repository setup", "Basic project structure" },
                    DurationWeeks = 1
                },
                new()
                {
                    Name = "Core Development",
                    Description = "Implement core functionality",
                    Deliverables = new List<string> { "Core features", "Basic testing" },
                    DurationWeeks = 4
                }
            },
            TeamStructure = new List<TeamRole>
            {
                new()
                {
                    Title = "Full-Stack Developer",
                    Description = "Primary development role",
                    RequiredSkills = new List<string> { "C#", "ASP.NET Core", "SQL" },
                    Commitment = "Full-time"
                }
            },
            Phases = new List<DevelopmentPhase>
            {
                new()
                {
                    Name = "Planning & Setup",
                    Description = "Project initialization phase",
                    Activities = new List<string> { "Requirements analysis", "Technical design" },
                    Outcomes = new List<string> { "Project plan", "Technical specification" },
                    DurationWeeks = 2
                }
            },
            EstimatedTimelineWeeks = 8,
            Risks = new List<RiskAssessment>
            {
                new()
                {
                    Risk = "Scope creep",
                    Probability = 3,
                    Impact = 4,
                    MitigationStrategies = new List<string> { "Clear requirements", "Regular reviews" }
                }
            },
            SuccessCriteria = new List<string> { "Working application", "User acceptance", "Code quality standards met" },
            ModelVersion = _aiService.GetModelInfo()
        };
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