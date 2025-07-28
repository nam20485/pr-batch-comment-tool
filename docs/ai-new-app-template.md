# **New Application**

## **App Title**

GitHub PR Review Assistant

## **Development Plan**

Implement a Windows desktop application using C\# and Avalonia UI, following the **Repository Pattern with a Local Cache** architecture. The application will serve as a productivity tool for developers to streamline the GitHub pull request review process.

The core development path is as follows:

1. **Authentication:** Implement secure GitHub authentication using the recommended OAuth2 device flow.  
2. **Data Layer:**  
   * Use Octokit.net for all communication with the GitHub REST API.  
   * Set up a local SQLite database using Entity Framework Core to serve as a cache for all GitHub data (repositories, pull requests, comments, users).  
   * Create a central Repository layer that abstracts data access. This layer will be responsible for fetching data from the local cache first, and only calling the GitHub API if the data is missing or stale.  
3. **UI Layer (Avalonia):**  
   * Build a responsive, multi-pane user interface according to the MVVM pattern.  
   * The UI will allow users to navigate from their list of repositories to a list of pull requests, and finally to a detailed view of a selected PR's comments.  
   * Implement advanced filtering and sorting controls for the comment list.  
4. **Core Feature \- Batch Operations:**  
   * Develop the functionality to allow users to select multiple comments from the filtered list.  
   * Implement the logic to post the selected comments as a new review on behalf of the logged-in user.  
5. **AI Feature Integration:**  
   * Integrate calls to the Gemini API to provide value-added features.  
   * Implement an "Explain Recommendation" feature that provides AI-generated context on the chosen architecture.  
   * Implement a "Project Kickstart Plan" generator that provides an AI-generated technical plan to guide development.

## **Description**

### **Overview**

The GitHub PR Review Assistant is a desktop productivity tool designed for developers who frequently engage in code reviews. The application's primary goal is to accelerate the review process by providing powerful tools for managing, filtering, and duplicating review comments. By leveraging a local cache, the application ensures a highly responsive, performant, and offline-capable user experience, distinguishing it from web-based interfaces.

### **Document Links**

* [Analysis -  GitHub PR Review Tool](<Analysis -  GitHub PR Review Tool.md>)  
* [An Interactive Comparison for the GitHub PR Review Tool.html](<An Interactive Comparison for the GitHub PR Review Tool.html>)

### **Requirements**

* Must be a native Windows Desktop Application built with Avalonia UI.  
* Must provide secure, OAuth2-based authentication with GitHub.  
* Must allow users to browse their repositories and the pull requests within them.  
* Must display all review comments for a selected pull request.  
* Must include robust filtering and sorting options for the comment list.  
* Must allow the batch duplication of selected comments into a new review.  
* Must be fully functional offline for previously viewed data.  
* Must maintain a responsive and fluid UI, even when dealing with large amounts of data.

### **Features**

* \[x\] Secure User Authentication  
* \[x\] Repository and Pull Request Navigation  
* \[x\] Comment Listing with Advanced Filtering & Sorting  
* \[x\] Batch Comment Duplication  
* \[x\] Local Caching for Performance and Offline Use  
* \[x\] AI-Powered Architectural Explanations (Gemini API)  
* \[x\] AI-Powered Project Plan Generation (Gemini API)  
* \[ \] Comprehensive Unit and Integration Test Cases  
* \[ \] Application Logging for Diagnostics  
* \[ \] User Documentation and Getting Started Guide  
* \[ \] Clear Acceptance Criteria for each feature

## **Language**

C\#

## **Language Version**

.NET 8.0 (or newer)

## **Frameworks, Tools, Packages**

* **UI Framework:** Avalonia UI  
* **MVVM Framework:** CommunityToolkit.Mvvm  
* **GitHub API Client:** Octokit.net  
* **Local Database / ORM:** Microsoft.EntityFrameworkCore.Sqlite  
* **HTTP Client (for Gemini API):** System.Net.Http  
* **JSON Serialization:** System.Text.Json

## **Project Structure/Package System**

A standard .NET solution will be used, organized into the following projects to ensure a clean separation of concerns:

* **GitHubPrTool.Core**: A class library containing domain models (e.g., Repository, PullRequest), service interfaces (e.g., IGitHubRepository), and core business logic. This project will have no dependencies on specific data access or UI frameworks.  
* **GitHubPrTool.Infrastructure**: A class library for implementing the interfaces defined in Core. This will contain the Octokit.net client logic, the Entity Framework Core DbContext, and the implementation of the GitHubRepository that manages the local cache and API calls.  
* **GitHubPrTool.Desktop**: The main Avalonia application project. It will contain all UI-related code, including Views (XAML) and ViewModels, and will depend on the Core and Infrastructure projects.

## **GitHub**

### **Repo**

*(To be created by the user)*

### **Branch**

A main (for stable releases) and develop (for active development) branching strategy is recommended.

## **Deliverables**

* A complete, functional Windows desktop application, distributable as a self-contained executable or an MSIX installer package.  
* The full source code, hosted on a public GitHub repository.  
* User-facing documentation explaining how to install and use the application.