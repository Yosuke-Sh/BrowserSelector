# Changelog

All notable changes to BrowserSelector will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [0.3.3] - 2026-08-17

### 🐛 Fixed
- Fixed dialogs opened from the Settings window (browser add/edit, URL rule add/edit, log viewer, icon picker) appearing behind the main window instead of in front, making them hard to notice and operate. None of these dialogs had `Owner` set; they now use the active window (falling back to the main window).
- Fixed the "Add Browser" duplicate check rejecting a browser whose executable path already existed even when the launch arguments differed (e.g., adding the same browser again with an incognito/private-browsing flag). The duplicate check now compares executable path *and* launch arguments together, so the same executable can be registered multiple times with different arguments.
- Fixed missing English localization keys for the Accessibility settings tab (`Settings.Accessibility.Title`, `EnableFocusIndicator`, `FocusThickness`, `EnableShortcuts`, `EnableScreenReaderSupport`, `ProvideDetailedDescriptions`) that were present in ja-JP/zh-CN but absent from en-US, causing "resource key not found" warnings when running under English UI.

## [0.3.2] - 2026-08-16

### 🐛 Fixed
- Fixed a bug where an update that was detected but not yet applied would stop being reported after the first check. `CheckForUpdatesAsync` used an ETag-based conditional request; once the ETag was cached, subsequent checks received `304 Not Modified` from GitHub and returned `null` unconditionally, without ever comparing the cached release against the running version. If the user dismissed the update notification (e.g., by closing the Settings window without clicking "Update Now" in the main window's notification bar), later checks would incorrectly report "You're up to date" even though the app was still running the old version. `CheckForUpdatesAsync` now compares the cached tag against the current version before deciding whether to send the `If-None-Match` header, falling back to a full request (bypassing the cache) whenever a previously detected update has not yet been applied.
- Fixed the update notification bar in the main window never becoming visible when the countdown auto-launch timer and the background update check completed around the same time (both default to 5 seconds after startup). The notification bar is only shown while `IsCountdownActive` is `false`, so a detected update could go unnoticed while the countdown silently launched the default browser and closed the window. `MainWindow` now pauses the countdown as soon as an update notification is shown.

### ✨ Added
- Added an "Apply update" button to the Settings window, enabled once "Check Now" finds a new version. This lets the update be downloaded, verified, and applied without leaving Settings, instead of requiring the user to also act on the separate notification bar in the main window.

## [0.3.1] - 2026-08-16

### 🐛 Fixed
- Fixed a `System.InvalidOperationException` ("The calling thread cannot access this object because a different thread owns it") that occurred when clicking "Check Now" in Settings or "Update Now" in the update notification bar. The `RelayCommand` implementations for these actions used `ConfigureAwait(false)`, resuming on a background thread after `await` while still updating UI-bound `ObservableProperty` values; when combined with the nested Dispatcher message loop of a modal `ShowDialog()`, this could bypass the thread-affinity checks WPF relies on. Removed `ConfigureAwait(false)` from all update-related `RelayCommand` methods (`SettingsViewModel.CheckForUpdatesNowAsync`/`ClearSkippedVersionAsync`, `MainViewModel.StartUpdateAsync`/`DeferUpdateAsync`/`SkipUpdateAsync`/`OpenUpdateReleaseNotesAsync`) so continuations resume on the UI thread's `SynchronizationContext` as WPF expects.

## [0.3.0] - 2026-08-16

### 🔄 Automatic Update System

- **Update Checking**: Background check against GitHub Releases 5 seconds after startup (does not block "launch → select browser" flow), with ETag-based conditional requests and rate-limit backoff
- **Integrity Verification**: SHA256 checksum verification against `SHA256SUMS.txt`; mismatched or unverifiable downloads are rejected and deleted rather than silently applied
- **Two Apply Paths**: Program Files installs relaunch the signed-path installer with `/SILENT /CLOSEAPPLICATIONS` (UAC elevation); portable installs are replaced by a standalone `BrowserSelector.Updater.exe` that waits for the running process to exit, backs up, swaps files, and relaunches
- **Non-Modal Notification**: A dismissible bar at the bottom of the main window offers "Update Now", "Next Launch", "Skip This Version", and "Release Notes" — no dialog interrupts the startup flow
- **Settings**: "Include prereleases", last-check timestamp, "Check Now", and "Clear Skipped Version" added to the General tab
- **Prerelease Filtering**: Off by default; opt-in via settings

### 🔧 Changed
- **`IUpdateService`** (breaking): `CheckForUpdatesAsync`/`DownloadUpdateAsync`/`ApplyUpdateAsync`/`ResolveChannel`; `RollbackUpdateAsync`/`CreateBackup` removed (moved to `BrowserSelector.Updater.exe`, which only makes sense while the app isn't running)
- **`UpdateInfo`** (breaking): reduced from 18 to 11 properties; `Version` is now `System.Version` instead of `string`; asset details moved into new `UpdateAsset` records (`InstallerAsset`/`PortableAsset`/`ChecksumsAsset`)
- **HTTP**: `IHttpClientFactory` adopted for the update client instead of a raw `HttpClient`, enabling stub-based testing through the same code path as production

### 🐛 Fixed
- GitHub API responses were deserialized directly into `UpdateInfo`, which has no field matching `tag_name`/`assets[]` — updates could never be detected. Replaced with `GitHubReleaseMapper`.
- Downloaded files were saved via `Path.GetRandomFileName()` with no extension, so installers could never be launched via `Process.Start`. Downloads now keep the asset's original file name.
- `AppContext.BaseDirectory` (a directory) was passed where a file path was expected for backup/rollback, making both silently no-ops.

### 🧪 Testing
- **Total**: 858 tests, zero build warnings (up from 658 in v0.2.0)
  - New: `BrowserSelector.UpdaterTests` (36 tests, standalone updater project)
  - InfrastructureTests grew from 28 to 160 (GitHub API mapping, checksum parsing, download/verify, host-allowlist and Zip Slip security tests, channel resolution)
  - UnitTests grew to include update notification/settings ViewModel coverage

## [0.2.0] - 2026-08-16

### 🚀 .NET 10 Migration & Glass UI Overhaul

- **.NET 10 Migration**: All projects retargeted from `net8.0-windows` to `net10.0-windows`
- **Glass UI**: Mica/Acrylic/MicaAlt DWM backdrops, with solid-translucent and fully opaque fallbacks for non-DWM environments
- **Browser Tile Redesign**: Hover animations, focus rings, hotkey badges, layered depth
- **Keyboard Operation**: Esc to close, Enter/Space to launch, arrow keys/Tab for grid navigation, 1-9/A-Z hotkeys per browser, Ctrl+, to open settings, Ctrl+click/Ctrl+Enter to launch without closing
- **Startup Control**: Countdown auto-launch, tray residency, CLI options (`-d`/`--delay`, `-b`/`--browser`, `--silent`, `--auto-launch`, `-h`/`--help`, `-v`/`--version`)
- **Settings Redesign**: New Appearance tab (backdrop mode, opacity, corner radius, title bar toggle, always-on-top, theme), restored Accessibility tab
- **About Section**: New version-info section with `.iss` installer version auto-injected from `Directory.Build.props`
- **Performance**: Single-instance enforcement, icon caching, ReadyToRun publishing for faster startup
- **New Logo**: Refreshed application logo and icon set
- **New Services**: `IIconCacheService`, `IThemeService`, `IExternalLinkService`, `ISingleInstanceManager`, `ICustomLanguageService`
- **CI/CD**: New `release.yml` workflow (tag-triggered Inno Setup installer build and GitHub Release publishing)

### 🧪 Testing
- **Total**: 658 tests (657 passing, one known flaky test excluded), zero build warnings
  - UnitTests: 241 (240 passing)
  - CoreTests: 31
  - InfrastructureTests: 28
  - IntegrationTests: 23
  - SecurityTests: 238
  - AppTests: 88
  - UITests: 5
  - E2ETests: 4

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
- **Modern UI/UX**: Custom backgrounds, rounded corners (note: true window transparency/glass backdrops were not yet implemented at this point — see v0.2.0 for the Mica/Acrylic glass UI)
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
- **Code Signing**: Not implemented (executable and installer are unsigned)
- **Input Validation**: Comprehensive URL and file path validation
- **Registry Security**: Safe registry access with proper permissions
- **File System Security**: Secure file operations with proper error handling

## Future Roadmap

### v0.2.0 - .NET 10 Migration & Glass UI ✅ Completed
- [x] **.NET 10 Migration**: All projects retargeted to `net10.0-windows`
- [x] **Glass UI**: Mica/Acrylic/MicaAlt backdrops, browser tile redesign
- [x] **Keyboard Operation**: Full keyboard navigation and hotkeys
- [x] **Startup Optimization**: Single-instance enforcement, icon caching, ReadyToRun
- [x] **Startup Control**: Countdown auto-launch, tray residency, CLI options
- [x] **Settings Redesign**: Appearance tab, Accessibility tab

### v0.3.0 - Automatic Update System
- [ ] **Auto-Update System**: Complete GitHub Releases integration
- [ ] **Differential Updates**: Settings file differential update logic
- [ ] **Rollback Functionality**: Backup and rollback on failed updates
- [ ] **Secure Update Process**: Checksum/signature verification for update packages

### v0.4.0 - Enhanced Features
- [ ] **Advanced Installer**: MSI package with advanced features
- [ ] **Plugin System**: Extensible architecture for third-party plugins
- [ ] **Advanced Customization**: Theme system and advanced UI customization
- [ ] **Accessibility**: Complete screen reader support and keyboard navigation

### v0.5.0 - Internationalization
- [ ] **Additional Languages**: Chinese (zh-CN), Korean (ko-KR)
- [ ] **RTL Support**: Arabic and Hebrew language support
- [ ] **Regional Settings**: Culture-specific formatting and preferences

### v0.6.0 - Advanced Features
- [ ] **Cloud Sync**: Settings synchronization across devices
- [ ] **Advanced URL Rules**: Complex pattern matching and conditions
- [ ] **Browser Profiles**: Support for browser profiles and containers

### Technical Improvements
- [ ] **Static Code Analysis**: SonarQube integration with quality gates
- [ ] **Advanced Testing**: Performance testing and load testing
- [ ] **Documentation**: Automated API documentation generation
- [ ] **Monitoring**: Application telemetry and crash reporting
