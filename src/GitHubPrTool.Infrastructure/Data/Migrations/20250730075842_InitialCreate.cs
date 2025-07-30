using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GitHubPrTool.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Login = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    Email = table.Column<string>(type: "TEXT", maxLength: 320, nullable: true),
                    AvatarUrl = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    HtmlUrl = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    Bio = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetime", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Repositories",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    FullName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    HtmlUrl = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    CloneUrl = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    OwnerId = table.Column<long>(type: "INTEGER", nullable: false),
                    Private = table.Column<bool>(type: "INTEGER", nullable: false),
                    DefaultBranch = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Language = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    StargazersCount = table.Column<int>(type: "INTEGER", nullable: false),
                    ForksCount = table.Column<int>(type: "INTEGER", nullable: false),
                    OpenIssuesCount = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetime", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetime", nullable: false),
                    PushedAt = table.Column<DateTimeOffset>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Repositories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Repositories_Users_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PullRequests",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Number = table.Column<int>(type: "INTEGER", nullable: false),
                    Title = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    Body = table.Column<string>(type: "TEXT", maxLength: 65000, nullable: true),
                    State = table.Column<string>(type: "TEXT", nullable: false),
                    HtmlUrl = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    AuthorId = table.Column<long>(type: "INTEGER", nullable: false),
                    RepositoryId = table.Column<long>(type: "INTEGER", nullable: false),
                    BaseBranch = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    HeadBranch = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    IsDraft = table.Column<bool>(type: "INTEGER", nullable: false),
                    Mergeable = table.Column<bool>(type: "INTEGER", nullable: true),
                    Commits = table.Column<int>(type: "INTEGER", nullable: false),
                    Additions = table.Column<int>(type: "INTEGER", nullable: false),
                    Deletions = table.Column<int>(type: "INTEGER", nullable: false),
                    ChangedFiles = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetime", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetime", nullable: false),
                    ClosedAt = table.Column<DateTimeOffset>(type: "datetime", nullable: true),
                    MergedAt = table.Column<DateTimeOffset>(type: "datetime", nullable: true),
                    MergedById = table.Column<long>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PullRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PullRequests_Repositories_RepositoryId",
                        column: x => x.RepositoryId,
                        principalTable: "Repositories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PullRequests_Users_AuthorId",
                        column: x => x.AuthorId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PullRequests_Users_MergedById",
                        column: x => x.MergedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Reviews",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Body = table.Column<string>(type: "TEXT", maxLength: 65000, nullable: true),
                    State = table.Column<string>(type: "TEXT", nullable: false),
                    AuthorId = table.Column<long>(type: "INTEGER", nullable: false),
                    PullRequestId = table.Column<long>(type: "INTEGER", nullable: false),
                    HtmlUrl = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    PullRequestUrl = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    CommitId = table.Column<string>(type: "TEXT", maxLength: 40, nullable: true),
                    SubmittedAt = table.Column<DateTimeOffset>(type: "datetime", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetime", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reviews", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Reviews_PullRequests_PullRequestId",
                        column: x => x.PullRequestId,
                        principalTable: "PullRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Reviews_Users_AuthorId",
                        column: x => x.AuthorId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Comments",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Body = table.Column<string>(type: "TEXT", maxLength: 65000, nullable: false),
                    Type = table.Column<string>(type: "TEXT", nullable: false),
                    AuthorId = table.Column<long>(type: "INTEGER", nullable: false),
                    PullRequestId = table.Column<long>(type: "INTEGER", nullable: false),
                    HtmlUrl = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    Path = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    Line = table.Column<int>(type: "INTEGER", nullable: true),
                    OriginalLine = table.Column<int>(type: "INTEGER", nullable: true),
                    CommitId = table.Column<string>(type: "TEXT", maxLength: 40, nullable: true),
                    OriginalCommitId = table.Column<string>(type: "TEXT", maxLength: 40, nullable: true),
                    DiffHunk = table.Column<string>(type: "TEXT", maxLength: 10000, nullable: true),
                    Position = table.Column<int>(type: "INTEGER", nullable: true),
                    OriginalPosition = table.Column<int>(type: "INTEGER", nullable: true),
                    IsMultiLine = table.Column<bool>(type: "INTEGER", nullable: false),
                    StartLine = table.Column<int>(type: "INTEGER", nullable: true),
                    StartSide = table.Column<string>(type: "TEXT", maxLength: 10, nullable: true),
                    Side = table.Column<string>(type: "TEXT", maxLength: 10, nullable: true),
                    ReviewId = table.Column<long>(type: "INTEGER", nullable: true),
                    InReplyToId = table.Column<long>(type: "INTEGER", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetime", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Comments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Comments_Comments_InReplyToId",
                        column: x => x.InReplyToId,
                        principalTable: "Comments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Comments_PullRequests_PullRequestId",
                        column: x => x.PullRequestId,
                        principalTable: "PullRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Comments_Reviews_ReviewId",
                        column: x => x.ReviewId,
                        principalTable: "Reviews",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Comments_Users_AuthorId",
                        column: x => x.AuthorId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Comments_AuthorId",
                table: "Comments",
                column: "AuthorId");

            migrationBuilder.CreateIndex(
                name: "IX_Comments_CreatedAt",
                table: "Comments",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Comments_InReplyToId",
                table: "Comments",
                column: "InReplyToId");

            migrationBuilder.CreateIndex(
                name: "IX_Comments_Path_Line",
                table: "Comments",
                columns: new[] { "Path", "Line" },
                filter: "Path IS NOT NULL AND Line IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Comments_PullRequestId",
                table: "Comments",
                column: "PullRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_Comments_ReviewId",
                table: "Comments",
                column: "ReviewId");

            migrationBuilder.CreateIndex(
                name: "IX_Comments_Type",
                table: "Comments",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_PullRequests_AuthorId",
                table: "PullRequests",
                column: "AuthorId");

            migrationBuilder.CreateIndex(
                name: "IX_PullRequests_CreatedAt",
                table: "PullRequests",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_PullRequests_MergedById",
                table: "PullRequests",
                column: "MergedById");

            migrationBuilder.CreateIndex(
                name: "IX_PullRequests_RepositoryId_Number",
                table: "PullRequests",
                columns: new[] { "RepositoryId", "Number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PullRequests_State",
                table: "PullRequests",
                column: "State");

            migrationBuilder.CreateIndex(
                name: "IX_Repositories_FullName",
                table: "Repositories",
                column: "FullName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Repositories_OwnerId",
                table: "Repositories",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_AuthorId",
                table: "Reviews",
                column: "AuthorId");

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_PullRequestId",
                table: "Reviews",
                column: "PullRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_State",
                table: "Reviews",
                column: "State");

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_SubmittedAt",
                table: "Reviews",
                column: "SubmittedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Login",
                table: "Users",
                column: "Login",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Comments");

            migrationBuilder.DropTable(
                name: "Reviews");

            migrationBuilder.DropTable(
                name: "PullRequests");

            migrationBuilder.DropTable(
                name: "Repositories");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
