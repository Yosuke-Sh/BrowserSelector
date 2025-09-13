# Contributing to BrowserSelector

Thank you for your interest in contributing to BrowserSelector! This document provides guidelines and information for contributors.

## 📋 Table of Contents

- [Code of Conduct](#code-of-conduct)
- [Getting Started](#getting-started)
- [Development Setup](#development-setup)
- [Contributing Guidelines](#contributing-guidelines)
- [Pull Request Process](#pull-request-process)
- [Issue Reporting](#issue-reporting)
- [Development Standards](#development-standards)

## 🤝 Code of Conduct

This project adheres to a code of conduct. By participating, you are expected to uphold this code. Please report unacceptable behavior to the project maintainers.

## 🚀 Getting Started

### Prerequisites

- **.NET 8.0 SDK**: [Download here](https://dotnet.microsoft.com/download/dotnet/8.0)
- **Visual Studio 2022** or **VS Code** with C# extension
- **Windows 10/11** (required for WPF development)
- **Git**: For version control

### Development Setup

1. **Fork the Repository**
   ```bash
   # Fork on GitHub, then clone your fork
   git clone https://github.com/YOUR_USERNAME/BrowserSelector.git
   cd BrowserSelector
   ```

2. **Set Up Remote**
   ```bash
   git remote add upstream https://github.com/Yosuke-Sh/BrowserSelector.git
   ```

3. **Restore Dependencies**
   ```bash
   dotnet restore
   ```

4. **Build the Solution**
   ```bash
   dotnet build
   ```

5. **Run Tests**
   ```bash
   dotnet test
   ```

## 📝 Contributing Guidelines

### Branch Strategy

- **main**: Production-ready code
- **developer**: Integration branch for development
- **feature/***: Feature development branches
- **hotfix/***: Critical bug fixes
- **release/***: Release preparation branches

### Workflow

1. **Create a Feature Branch**
   ```bash
   git checkout -b feature/your-feature-name
   ```

2. **Make Your Changes**
   - Follow the coding standards
   - Add tests for new functionality
   - Update documentation as needed

3. **Test Your Changes**
   ```bash
   dotnet test
   dotnet build --configuration Release
   ```

4. **Commit Your Changes**
   ```bash
   git add .
   git commit -m "feat: add new feature description"
   ```

5. **Push and Create Pull Request**
   ```bash
   git push origin feature/your-feature-name
   ```

## 🔄 Pull Request Process

### Before Submitting

- [ ] Code follows the project's coding standards
- [ ] All tests pass (`dotnet test`)
- [ ] No build warnings (`dotnet build`)
- [ ] Code coverage is maintained (85%+)
- [ ] Documentation is updated
- [ ] CHANGELOG.md is updated (if applicable)

### Pull Request Template

```markdown
## Description
Brief description of changes

## Type of Change
- [ ] Bug fix
- [ ] New feature
- [ ] Breaking change
- [ ] Documentation update

## Testing
- [ ] Unit tests added/updated
- [ ] Integration tests added/updated
- [ ] UI tests added/updated
- [ ] Manual testing completed

## Checklist
- [ ] Code follows style guidelines
- [ ] Self-review completed
- [ ] Documentation updated
- [ ] No build warnings
- [ ] All tests pass
```

## 🐛 Issue Reporting

### Bug Reports

When reporting bugs, please include:

- **Environment**: OS version, .NET version
- **Steps to Reproduce**: Clear, numbered steps
- **Expected Behavior**: What should happen
- **Actual Behavior**: What actually happens
- **Screenshots**: If applicable
- **Logs**: Error messages or log files

### Feature Requests

For feature requests, please include:

- **Use Case**: Why is this feature needed?
- **Proposed Solution**: How should it work?
- **Alternatives**: Other solutions considered
- **Additional Context**: Any other relevant information

## 📏 Development Standards

### Code Quality

- **Coverage**: Maintain 85%+ code coverage
- **Warnings**: Zero build warnings
- **Complexity**: Keep cyclomatic complexity ≤ 10
- **Duplication**: Keep code duplication ≤ 5%

### Coding Standards

- **C# Style**: Follow Microsoft C# coding conventions
- **Naming**: Use descriptive, self-documenting names
- **Comments**: Document public APIs and complex logic
- **Error Handling**: Proper exception handling and logging

### Testing Requirements

- **Unit Tests**: Required for all business logic
- **Integration Tests**: Required for external dependencies
- **UI Tests**: Required for user-facing features
- **Performance Tests**: Required for performance-critical code

### Documentation

- **XML Comments**: Required for public APIs
- **README Updates**: Update for user-facing changes
- **CHANGELOG**: Update for significant changes
- **Wiki**: Update for architectural changes

## 🏗️ Architecture Guidelines

### MVVM Pattern

- **Models**: Data and business logic
- **ViewModels**: Presentation logic and data binding
- **Views**: UI and user interaction
- **Services**: Cross-cutting concerns

### Dependency Injection

- Use constructor injection
- Register services in `ServiceCollectionExtensions`
- Prefer interfaces over concrete classes

### Error Handling

- Use structured logging with Serilog
- Implement proper exception handling
- Provide user-friendly error messages

## 🧪 Testing Guidelines

### Unit Tests

```csharp
[Fact]
public void MethodName_Scenario_ExpectedResult()
{
    // Arrange
    var service = new Service();
    
    // Act
    var result = service.Method();
    
    // Assert
    result.Should().Be(expectedValue);
}
```

### Integration Tests

- Test real dependencies
- Use test databases/files
- Clean up after tests

### UI Tests

- Test user interactions
- Verify UI state changes
- Use FlaUI for automation

## 📚 Resources

- [.NET Documentation](https://docs.microsoft.com/dotnet/)
- [WPF Documentation](https://docs.microsoft.com/wpf/)
- [MVVM Pattern](https://docs.microsoft.com/wpf/advanced/commanding-overview)
- [xUnit Testing](https://xunit.net/)
- [FluentAssertions](https://fluentassertions.com/)

## 🆘 Getting Help

- **GitHub Issues**: For bugs and feature requests
- **GitHub Discussions**: For questions and discussions
- **Wiki**: For detailed documentation
- **Code Review**: Ask for help in pull requests

## 📄 License

By contributing to BrowserSelector, you agree that your contributions will be licensed under the MIT License.

---

Thank you for contributing to BrowserSelector! 🎉
