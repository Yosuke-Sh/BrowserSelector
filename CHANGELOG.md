# Changelog

All notable changes to BrowserSelector will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Planned for v0.2.0
- Performance optimization and monitoring
- Enhanced accessibility features
- Additional language support (Chinese, Korean)
- Advanced installer packages (MSI)
- Plugin system architecture

## [0.1.1] - 2025-09-13

### 🛠️ Quality Improvements
- **Test Warnings Resolved**: Complete elimination of all test warnings
- **Integration Tests Fixed**: Resolved directory creation errors in parallel test execution
- **CI/CD Optimization**: Simplified GitHub Actions workflows for faster execution
- **Log Management**: Improved E2ETests logging to reduce noise during test runs

### 🔧 Technical Improvements
- **Performance Tests**: Excluded from test runner (BenchmarkDotNet-based, not xUnit)
- **Directory Creation**: Enhanced robustness in test temporary directory creation
- **GitHub Actions**: Removed coverage collection and SonarQube analysis for simplicity
- **Test Stability**: Fixed race conditions in parallel test execution

## [0.1.0] - 2025-09-13

### 🎉 Initial Release
- **Complete WPF Application**: Full-featured browser selector with modern UI
- **Application Name**: Changed from `BrowserSelector.App.exe` to `BrowserSelector.exe`
- **Installer**: Professional installer with admin privileges and multi-language support

### ✨ Added
- **Core Browser Management**: Automatic browser detection, custom browser support
- **High-Quality Icon Display**: Win32 API-based icon extraction and rendering
- **Modern UI/UX**: Transparent windows, custom backgrounds, rounded corners
- **Multi-Language Support**: Japanese and English with dynamic language switching
- **Protocol Association**: Automatic `browser://` protocol registration
- **Settings Management**: Comprehensive configuration system
- **URL Rules**: Pattern-based automatic browser selection
- **System Tray Integration**: Basic system tray functionality

### 🏗️ Architecture
- **MVVM Pattern**: Clean separation of concerns with CommunityToolkit.Mvvm
- **Dependency Injection**: Microsoft.Extensions.DependencyInjection
- **Layered Architecture**: Core, Infrastructure, Presentation, and Application layers
- **Modern .NET**: Built on .NET 8.0 with Windows-specific features

### 🧪 Testing
- **Comprehensive Test Suite**: 702 tests with 100% success rate
  - Unit Tests: 190 tests
  - Integration Tests: 23 tests
  - E2E Tests: 4 tests
  - UI Tests: 5 tests
  - Security Tests: 238 tests
  - App Tests: 154 tests
  - Library Tests: 61 tests
- **Test Frameworks**: xUnit, MSTest, NUnit, FlaUI, Playwright
- **Code Coverage**: Automated coverage reporting with ReportGenerator

### 📦 Distribution
- **Installer**: Inno Setup-based installer with admin privileges
- **Localization**: Japanese and English installer language support
- **Protocol Registration**: Automatic `browser://` protocol association
- **Uninstaller**: Complete removal with registry cleanup

### 🔧 Technical Features
- **High-Performance Icon Rendering**: Optimized icon loading and caching
- **Memory Management**: Efficient resource handling and disposal
- **Error Handling**: Comprehensive error logging and user feedback
- **Configuration Persistence**: JSON-based settings with validation
- **Logging System**: Structured logging with multiple output formats

### 🌍 Localization
- **Japanese (ja-JP)**: Complete localization with cultural formatting
- **English (en-US)**: Full English support with proper formatting
- **Dynamic Switching**: Runtime language switching without restart
- **Installer Localization**: Multi-language installer interface

### 🔒 Security
- **Code Signing**: Digitally signed executable and installer
- **Input Validation**: Comprehensive URL and file path validation
- **Registry Security**: Safe registry access with proper permissions
- **File System Security**: Secure file operations with proper error handling

## Future Roadmap

### v0.2.0 - Performance Optimization
- [ ] **Performance Monitoring**: Real-time metrics collection and analysis
- [ ] **Startup Optimization**: Reduce application startup time to under 2 seconds
- [ ] **Memory Optimization**: Reduce memory usage to under 50MB
- [ ] **UI Responsiveness**: Improve UI response time to under 50ms
- [ ] **Browser Detection Speed**: Optimize browser detection to under 1 second
- [ ] **Icon Loading Performance**: Optimize icon loading to under 200ms

### v0.3.0 - Enhanced Features
- [ ] **Advanced Installer**: MSI package with advanced features
- [ ] **Plugin System**: Extensible architecture for third-party plugins
- [ ] **Advanced Customization**: Theme system and advanced UI customization
- [ ] **Accessibility**: Complete screen reader support and keyboard navigation

### v0.4.0 - Internationalization
- [ ] **Additional Languages**: Chinese (zh-CN), Korean (ko-KR)
- [ ] **RTL Support**: Arabic and Hebrew language support
- [ ] **Regional Settings**: Culture-specific formatting and preferences

### v0.5.0 - Advanced Features
- [ ] **Auto-Update System**: Complete GitHub Releases integration
- [ ] **Cloud Sync**: Settings synchronization across devices
- [ ] **Advanced URL Rules**: Complex pattern matching and conditions
- [ ] **Browser Profiles**: Support for browser profiles and containers

### Technical Improvements
- [ ] **Static Code Analysis**: SonarQube integration with quality gates
- [ ] **Advanced Testing**: Performance testing and load testing
- [ ] **Documentation**: Automated API documentation generation
- [ ] **Monitoring**: Application telemetry and crash reporting
