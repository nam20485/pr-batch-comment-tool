# Security Policy

## Supported Versions

We actively support the following versions of GitHub PR Review Tool with security updates:

| Version | Supported          |
| ------- | ------------------ |
| 1.x.x   | :white_check_mark: |
| < 1.0   | :x:                |

## Reporting a Vulnerability

We take security vulnerabilities seriously. Please follow these steps to report security issues:

### Where to Report

- **For security vulnerabilities**: Create a [private security advisory](https://github.com/nam20485/pr-batch-comment-tool/security/advisories/new)
- **For general security questions**: Email the maintainer or create a private discussion

### What to Include

When reporting a security vulnerability, please include:

1. **Description**: Clear description of the vulnerability
2. **Impact**: Potential impact and attack scenarios
3. **Reproduction**: Step-by-step instructions to reproduce the issue
4. **Environment**: Version numbers, operating system, and configuration details
5. **Fix Suggestions**: Any suggestions for fixing the vulnerability (if available)

### Response Timeline

- **Initial Response**: Within 48 hours of report
- **Status Update**: Weekly updates until resolution
- **Fix Timeline**: Critical vulnerabilities will be addressed within 7 days, others within 30 days

### Security Update Process

1. **Assessment**: We'll assess the vulnerability and determine severity
2. **Fix Development**: Develop and test a fix
3. **Disclosure**: Coordinate disclosure with the reporter
4. **Release**: Release security update and publish advisory

## Security Best Practices

### For Users

- **Keep Updated**: Always use the latest version
- **Secure Storage**: Store GitHub tokens securely
- **Access Control**: Use minimal required permissions for GitHub tokens
- **Network Security**: Use the application in trusted network environments

### For Contributors

- **Dependency Management**: Keep dependencies updated
- **Secret Management**: Never commit secrets or tokens
- **Code Review**: All code changes require review
- **Testing**: Include security considerations in testing

## Automated Security Measures

Our repository includes several automated security measures:

### Dependency Management
- **Dependabot**: Automated dependency updates
- **Vulnerability Scanning**: Regular dependency vulnerability scans
- **License Compliance**: Automated license compatibility checks

### Code Security
- **CodeQL Analysis**: Static code analysis for security vulnerabilities
- **Secret Scanning**: Automated detection of committed secrets
- **Third-party Security Scans**: Trivy and other security scanners

### CI/CD Security
- **Secure Workflows**: GitHub Actions follow security best practices
- **Artifact Scanning**: Build artifacts are scanned for vulnerabilities
- **Access Controls**: Proper permissions and access controls in workflows

## Security Contact

For security-related questions or concerns:

- **Primary**: Create a GitHub Security Advisory
- **Alternative**: Open a private discussion
- **Emergency**: Contact repository maintainers directly

## Acknowledgments

We appreciate security researchers and community members who help improve our security posture. Contributors who report valid security vulnerabilities will be acknowledged in our security advisories (unless they prefer to remain anonymous).

## Security Resources

- [GitHub Security Best Practices](https://docs.github.com/en/code-security)
- [OWASP Application Security](https://owasp.org/www-project-application-security-verification-standard/)
- [.NET Security Guidelines](https://docs.microsoft.com/en-us/dotnet/standard/security/)