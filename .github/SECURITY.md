# Security Policy

## Supported Versions

We actively support the following versions of GitHub PR Review Tool with security updates:

| Version | Supported          |
| ------- | ------------------ |
| 1.x.x   | :white_check_mark: |
| < 1.0   | :x:                |

## Reporting a Vulnerability

We take security vulnerabilities seriously. If you discover a security vulnerability in the GitHub PR Review Tool, please report it to us as described below.

### How to Report

**DO NOT** create a public GitHub issue for security vulnerabilities.

Instead, please report security vulnerabilities by:

1. **Email**: Send details to [nam20485@gmail.com](mailto:nam20485@gmail.com)
2. **GitHub Security Advisory**: Use the [private vulnerability reporting](https://github.com/nam20485/pr-batch-comment-tool/security/advisories/new) feature

### What to Include

When reporting a vulnerability, please include:

- **Description**: A clear description of the vulnerability
- **Impact**: What kind of vulnerability it is and how it could be exploited
- **Reproduction**: Step-by-step instructions to reproduce the issue
- **Environment**: Operating system, .NET version, and application version affected
- **Proof of Concept**: If possible, include a minimal proof of concept

### Response Timeline

We will acknowledge receipt of your vulnerability report within **48 hours**.

We will provide a detailed response within **7 days** indicating:
- Our assessment of the report
- Next steps for remediation
- Expected timeline for a fix

### Security Update Process

1. **Triage**: We assess the severity and impact of the vulnerability
2. **Fix Development**: We develop and test a fix in a private repository
3. **Coordinated Disclosure**: We coordinate the release timing with the reporter
4. **Release**: We release the security update and publish a security advisory
5. **Communication**: We notify users through release notes and GitHub Security Advisories

### Scope

This security policy applies to:

- ✅ The main application (GitHubPrTool.Desktop)
- ✅ Core libraries (GitHubPrTool.Core, GitHubPrTool.Infrastructure)
- ✅ Build and release processes
- ✅ Dependencies and third-party components

This policy does NOT apply to:
- ❌ Third-party integrations not under our control
- ❌ Issues in dependencies that are already publicly known
- ❌ Social engineering or phishing attacks

### Security Best Practices

When using the GitHub PR Review Tool:

1. **Keep Updated**: Always use the latest version
2. **Secure Storage**: Store OAuth tokens securely (the app handles this automatically)
3. **Network Security**: Use HTTPS connections (enforced by GitHub API)
4. **Access Control**: Only grant necessary GitHub permissions
5. **Environment**: Run in a secure environment with updated OS and .NET runtime

### Recognition

We appreciate the security research community and will acknowledge researchers who report vulnerabilities responsibly:

- We will credit you in our security advisory (unless you prefer to remain anonymous)
- We will mention your contribution in our release notes
- For significant vulnerabilities, we may offer a small token of appreciation

## Security Features

The GitHub PR Review Tool includes several security features:

- **OAuth 2.0 Authentication**: Secure GitHub authentication without storing credentials
- **Token Management**: Automatic token refresh and secure local storage
- **HTTPS Enforcement**: All API communications use HTTPS
- **Input Validation**: Comprehensive input validation and sanitization
- **Error Handling**: Secure error handling that doesn't leak sensitive information
- **Dependency Scanning**: Automated dependency vulnerability scanning in CI/CD
- **Code Analysis**: Static code analysis with CodeQL

## Compliance

This project follows security best practices including:

- **OWASP Guidelines**: Following OWASP secure coding practices
- **GitHub Security Features**: Utilizing GitHub's security features (Dependabot, CodeQL, etc.)
- **Supply Chain Security**: Verifying and monitoring dependencies
- **Secure Development**: Implementing security throughout the development lifecycle

---

Thank you for helping keep the GitHub PR Review Tool and our users secure!