# BrowserSelector API Documentation

## 📋 Overview

This document describes the public APIs and interfaces provided by BrowserSelector. Target framework: `net10.0-windows` (all projects, as of v0.2.0).

## 🏗️ Architecture

### Project Structure

```
BrowserSelector.WPF/
├── src/
│   ├── BrowserSelector.Core/           # Domain Layer
│   │   ├── Models/                     # Domain Models
│   │   ├── Services/                   # Service Interfaces
│   │   ├── Enums/                      # Enumerations
│   │   └── Extensions/                 # Extension Methods
│   ├── BrowserSelector.Infrastructure/ # Infrastructure Layer
│   │   ├── Services/                   # Service Implementations
│   │   ├── SystemIntegration/          # System Integration
│   │   ├── Updates/                    # Update System
│   │   └── Localization/               # Localization
│   ├── BrowserSelector.Presentation/   # Presentation Layer
│   │   ├── ViewModels/                 # ViewModels
│   │   ├── Views/                      # Views
│   │   ├── Controls/                   # Custom Controls
│   │   └── Converters/                 # Value Converters
│   └── BrowserSelector.App/            # Application Layer
│       ├── DependencyInjection/        # DI Configuration
│       └── Configuration/              # App Configuration
```

## 🔧 Core Services

All interfaces live under `src/BrowserSelector.Core/Services/`. XML documentation comments on these interfaces (previously corrupted/garbled) were repaired in Phase B-4c.

### IBrowserService

Manages browser detection, CRUD, and launch operations.

```csharp
public interface IBrowserService
{
    Task<IEnumerable<Browser>> DetectBrowsersAsync();
    Task<bool> LaunchBrowserAsync(Browser browser, string url);
    Task<bool> LaunchBrowserAsync(Browser browser, Uri url);
    Task<bool> AddBrowserAsync(Browser browser);
    Task<bool> UpdateBrowserAsync(Browser browser);
    Task<bool> RemoveBrowserAsync(Guid browserId);
    Task<IEnumerable<Browser>> GetAllBrowsersAsync();
    Task<bool> SetDefaultBrowserAsync(Guid browserId);
    Task<Browser?> GetDefaultBrowserAsync();
    Task UpdateBrowserUsageAsync(Guid browserId);
    Task UpdateUsageAsync(Browser browser);
}
```

**Methods:**
- `DetectBrowsersAsync()`: Detects installed browsers from the registry
- `LaunchBrowserAsync(Browser, string|Uri)`: Launches a browser with a URL (string or `Uri` overload)
- `AddBrowserAsync(Browser)` / `UpdateBrowserAsync(Browser)` / `RemoveBrowserAsync(Guid)`: Custom browser CRUD
- `GetAllBrowsersAsync()`: Gets all registered browsers
- `SetDefaultBrowserAsync(Guid)` / `GetDefaultBrowserAsync()`: Manages the app's own "default tile" — the browser
  auto-launched by the countdown/`--silent` flow. **This is not the OS-level default browser.** See
  `IDefaultBrowserService` below for that.
- `UpdateBrowserUsageAsync(Guid)` / `UpdateUsageAsync(Browser)`: Updates browser usage statistics

### IDefaultBrowserService

Determines whether BrowserSelector is registered as the Windows default browser, and opens the OS settings
screen to change it. Distinct from `IBrowserService.SetDefaultBrowserAsync`/`GetDefaultBrowserAsync`, which
manage an in-app preference only.

```csharp
public interface IDefaultBrowserService
{
    bool IsDefaultBrowser();
    void OpenDefaultAppsSettings();
}
```

**Methods:**
- `IsDefaultBrowser()`: Reads `HKCU\...\UrlAssociations\https\UserChoice` to check whether BrowserSelector's
  ProgId (`BrowserSelector.https`) is the current default. Read-only — Windows 10/11 protect this key with a
  hash and do not allow programmatic writes.
- `OpenDefaultAppsSettings()`: Opens `ms-settings:defaultapps?registeredAppName=BrowserSelector`, deep-linking
  to BrowserSelector's entry in the Windows 11 default-apps screen. The actual HKLM registration
  (`Clients\StartMenuInternet`, `RegisteredApplications`, `Capabilities`) is performed by the installer, which
  runs elevated; the app itself runs `asInvoker` and cannot write HKLM.

### ISettingsService

Manages application settings persistence.

```csharp
public interface ISettingsService
{
    Task<AppSettings> LoadAppSettingsAsync();
    Task<bool> SaveAppSettingsAsync(AppSettings settings);
    Task<VisualSettings> LoadVisualSettingsAsync();
    Task<bool> SaveVisualSettingsAsync(VisualSettings settings);
    Task<LogSettings> LoadLogSettingsAsync();
    Task<bool> SaveLogSettingsAsync(LogSettings settings);
    string GetSettingsFilePath();
    Task<bool> ResetSettingsAsync();
    Task<bool> ImportSettingsAsync(string filePath);
    Task<bool> ExportSettingsAsync(string filePath);
}
```

**Methods:**
- `LoadAppSettingsAsync()` / `SaveAppSettingsAsync(AppSettings)`: Application settings (language, protocol, update check interval, etc.)
- `LoadVisualSettingsAsync()` / `SaveVisualSettingsAsync(VisualSettings)`: UI appearance settings
- `LoadLogSettingsAsync()` / `SaveLogSettingsAsync(LogSettings)`: Logging configuration
- `GetSettingsFilePath()`: Returns the settings file location
- `ResetSettingsAsync()`: Resets all settings to defaults
- `ImportSettingsAsync(string)` / `ExportSettingsAsync(string)`: Settings file import/export

### IUpdateService

Implemented in v0.3.0. Integrates with GitHub Releases, verifies downloads via SHA256, and applies updates through one of two channel-specific paths. Rollback and backup are intentionally **not** exposed here — they only make sense while the app isn't running, so they live in the standalone `BrowserSelector.Updater.exe` process instead.

```csharp
public interface IUpdateService : IDisposable
{
    event EventHandler<UpdateAvailableEventArgs>? UpdateAvailable;

    // Never throws on network failure — returns null instead so callers don't need to catch.
    Task<UpdateInfo?> CheckForUpdatesAsync(CancellationToken cancellationToken = default);

    Task<UpdateDownloadResult> DownloadUpdateAsync(
        UpdateInfo updateInfo,
        UpdateChannel channel,
        IProgress<int>? progress = null,
        CancellationToken cancellationToken = default);

    // Returns success/failure only; shutting down the app is the caller's (ViewModel's) responsibility.
    Task<bool> ApplyUpdateAsync(UpdateInfo updateInfo, CancellationToken cancellationToken = default);

    // Determines whether the running instance is a Program Files install (Installer) or a writable
    // portable deployment (Portable); falls back to Installer when the location isn't writable.
    UpdateChannel ResolveChannel();
}
```

#### `UpdateInfo`

```csharp
public class UpdateInfo
{
    public Version Version { get; set; }              // parsed from tag_name (leading 'v' stripped)
    public string TagName { get; set; }                // raw tag_name, e.g. "v0.3.0"
    public string ReleaseNotes { get; set; }            // release body (Markdown)
    public string ReleasePageUrl { get; set; }          // html_url
    public DateTimeOffset? PublishedAt { get; set; }
    public bool IsPrerelease { get; set; }
    public UpdateAsset? InstallerAsset { get; set; }     // BrowserSelector-Setup-v*.exe
    public UpdateAsset? PortableAsset { get; set; }      // BrowserSelector-v*-win-x64.zip
    public UpdateAsset? ChecksumsAsset { get; set; }     // SHA256SUMS.txt
    public string? LocalFilePath { get; set; }           // set once DownloadUpdateAsync succeeds
    public bool IsDownloaded { get; set; }
}

public sealed record UpdateAsset(string Name, Uri DownloadUrl, long Size, string? Sha256 = null);

public enum UpdateChannel { Installer, Portable }

public enum UpdateDownloadFailure { None, Network, ChecksumMismatch, ChecksumUnavailable, Canceled, Io }
```

`UpdateDownloadResult` carries `Success`, `FilePath`, and `Failure` — a bool return value alone can't distinguish "checksum mismatch" (dangerous, reject) from "network unreachable" (harmless, retry later), so the UI gets a typed reason instead.

#### Security model

The app is not code-signed, so host allow-listing and checksum verification are the only guarantees that a downloaded asset is genuine:
- Only `https://api.github.com`, `https://github.com`, and hosts ending in `.githubusercontent.com` are accepted for both the API call and asset downloads — checked again on the final URL after redirects (`evil-githubusercontent.com` does not match the suffix check)
- Every download is verified against `SHA256SUMS.txt`; a mismatch or unavailable checksum file both fail closed (asset deleted, not applied)
- Portable ZIP extraction validates each entry against Zip Slip (path traversal, absolute paths, alternate data streams) and enforces entry-count/size caps

### ILocalizationService

Manages multi-language support.

```csharp
public interface ILocalizationService
{
    event EventHandler<LanguageChangedEventArgs>? LanguageChanged;
    CultureInfo CurrentCulture { get; }
    IEnumerable<CultureInfo> SupportedLanguages { get; }
    string GetString(string key);
    string GetString(string key, params object[] args);
    Task SetLanguage(CultureInfo culture);
    Task<IEnumerable<CultureInfo>> GetSupportedLanguagesAsync();
}
```

**Methods:**
- `GetString(string)` / `GetString(string, object[])`: Gets a localized string (with optional formatting)
- `SetLanguage(CultureInfo)`: Changes the application language asynchronously
- `GetSupportedLanguagesAsync()`: Gets supported cultures asynchronously

**Properties:**
- `CurrentCulture`: Currently active culture
- `SupportedLanguages`: List of supported cultures

### ICustomLanguageService

Manages user-supplied custom language files, in addition to the built-in Japanese/English resources.

```csharp
public interface ICustomLanguageService
{
    Task<IEnumerable<LanguageInfo>> GetAvailableLanguagesAsync();
    Task<bool> AddCustomLanguageAsync(string languageFilePath);
    Task<bool> RemoveCustomLanguageAsync(string cultureCode);
    Task<bool> ValidateLanguageFileAsync(string languageFilePath);
    string GetCustomLanguageFolder();
    Task<Dictionary<string, string>?> LoadCustomLanguageAsync(string cultureCode);
    Task<bool> SaveCustomLanguageAsync(string cultureCode, string displayName, Dictionary<string, string> resources);
    Task<bool> GenerateLanguageTemplateAsync(string cultureCode, string displayName);
    Task<IEnumerable<string>> GetAvailableResourceKeysAsync();
}
```

### IUrlRuleService

Manages URL-pattern-based automatic browser selection rules.

```csharp
public interface IUrlRuleService
{
    Task<IEnumerable<UrlRule>> GetAllRulesAsync();
    Task<IEnumerable<UrlRule>> GetEnabledRulesAsync();
    Task<bool> AddRuleAsync(UrlRule rule);
    Task<bool> UpdateRuleAsync(UrlRule rule);
    Task<bool> DeleteRuleAsync(Guid ruleId);
    Task<bool> ToggleRuleAsync(Guid ruleId, bool isEnabled);
    Task<Browser?> FindMatchingBrowserAsync(string url, IEnumerable<Browser> browsers);
    Task<Browser?> FindMatchingBrowserAsync(Uri url, IEnumerable<Browser> browsers);
    Task<bool> ChangePriorityAsync(Guid ruleId, int newPriority);
    Task<bool> ReorderRulesAsync(IEnumerable<Guid> ruleIds);
}
```

### IProtocolHandler

Registers and resolves the `browser://` custom protocol.

```csharp
public interface IProtocolHandler
{
    bool RegisterProtocol(string applicationPath);
    bool UnregisterProtocol();
    bool IsProtocolRegistered();
    string? ExtractUrlFromProtocol(string protocolUrl);
    Uri? ExtractUrlFromProtocol(Uri protocolUrl);
    string CreateProtocolUrl(string url);
    Uri CreateProtocolUrl(Uri url);
    ProtocolRegistrationInfo? GetProtocolRegistrationInfo();
}
```

### IRegistryService

Detects installed browsers from the Windows registry.

```csharp
public interface IRegistryService
{
    Task<IEnumerable<Browser>> DetectBrowsersFromRegistryAsync();
}
```

### ILogService

Structured logging facade used throughout the application (all log calls should go through this service, per project convention).

```csharp
public interface ILogService
{
    void LogTrace(string message, string? category = null, Exception? exception = null);
    void LogDebug(string message, string? category = null, Exception? exception = null);
    void LogInformation(string message, string? category = null, Exception? exception = null);
    void LogWarning(string message, string? category = null, Exception? exception = null);
    void LogError(string message, string? category = null, Exception? exception = null);
    void LogCritical(string message, string? category = null, Exception? exception = null);
    void Log(LogLevel level, string message, string? category = null, Exception? exception = null);
    void LogDetailed(LogLevel level, string message, string? category = null, string? eventId = null,
        string? requestTarget = null, string? userInfo = null, string? processTarget = null,
        string? processAction = null, string? processResult = null, Exception? exception = null);
    void UpdateSettings(LogSettings settings);
    void ClearLogs();
    void CleanupOldLogs();
    string GetLogContent(int maxLines = 1000);
    string GetLogFilePath();
}
```

### IIconCacheService

New in v0.2.0 (Phase C/D startup performance work). Extracts and caches executable/image icons for faster startup and reduced repeated icon extraction cost.

```csharp
public interface IIconCacheService
{
    BitmapSource? GetIcon(string filePath, int iconIndex, int size);
    void ClearMemoryCache();
}
```

### IThemeService

New in v0.2.0 (Phase E-1 appearance tab). Applies and tracks the app's Light/Dark/System theme.

```csharp
public interface IThemeService
{
    event EventHandler? ActiveThemeChanged;
    ThemeMode CurrentMode { get; }
    bool IsDarkThemeActive { get; }
    void ApplyTheme(ThemeMode mode);
}
```

`ThemeMode` is `Light | Dark | System`. `BackdropMode` (also new in v0.2.0, used by `VisualSettings`) is `Mica | Acrylic | MicaAlt | SolidTranslucent | Opaque`.

### IExternalLinkService

New in v0.2.0 (Phase E-2, About section). Opens external URLs (GitHub repo, Issues, license) safely — deliberately avoids `Process.Start(url, UseShellExecute = true)` because BrowserSelector may itself be registered as the default browser / `browser://` handler, which would cause recursive self-launch. Falls back to an explicit detected browser instead.

```csharp
public interface IExternalLinkService
{
    Task<bool> OpenAsync(string url);
}
```

## 📊 Models

### Browser

Represents a browser application.

```csharp
public class Browser
{
    public string Name { get; set; }
    public string ExecutablePath { get; set; }
    public string IconPath { get; set; }
    public string Arguments { get; set; }
    public bool IsDefault { get; set; }
    public bool IsValid { get; set; }
}
```

**Properties:**
- `Name`: Display name of the browser
- `ExecutablePath`: Path to browser executable
- `IconPath`: Path to browser icon
- `Arguments`: Command line arguments
- `IsDefault`: Whether this is the default browser
- `IsValid`: Whether the browser is properly installed

### AppSettings

Application configuration settings.

```csharp
public class AppSettings
{
    public bool EnableLogging { get; set; }
    public LogLevel LogLevel { get; set; }
    public bool CheckForUpdates { get; set; }
    public int UpdateCheckInterval { get; set; }
    public string Language { get; set; }
    public string CustomProtocol { get; set; }
    public bool RegisterProtocol { get; set; }
    public bool CloseAfterUrlRuleMatch { get; set; }
}
```

### VisualSettings

UI appearance settings (`ObservableObject`, CommunityToolkit.Mvvm-generated properties). Note: window backdrop mode/opacity/corner radius/always-on-top and theme are configured via `BackdropMode`, `ThemeMode`, and related appearance-tab settings introduced in Phase E-1/E-2, not shown in this simplified excerpt — see `src/BrowserSelector.Core/Models/VisualSettings.cs` and `AppSettings.cs` for the authoritative list.

```csharp
public partial class VisualSettings : ObservableObject
{
    [ObservableProperty] private Color _backgroundColor;
    [ObservableProperty] private bool _useBackgroundGradient;
    [ObservableProperty] private Color _gradientStartColor;
    [ObservableProperty] private Color _gradientEndColor;
    [ObservableProperty] private GradientDirection _gradientDirection;
    [ObservableProperty] private double _iconScale;
    [ObservableProperty] private bool _showFocusIndicator;
    [ObservableProperty] private Color _focusColor;
    [ObservableProperty] private double _focusThickness;
    [ObservableProperty] private double _focusWidth;
    [ObservableProperty] private double _initialWindowWidth;
    [ObservableProperty] private double _initialWindowHeight;
    [ObservableProperty] private bool _showLogo;
    [ObservableProperty] private bool _showUrlInput;
    [ObservableProperty] private double _browserButtonWidth;
    [ObservableProperty] private double _browserButtonHeight;
    [ObservableProperty] private Color _browserButtonBackgroundColor;
    [ObservableProperty] private Color _browserButtonForegroundColor;
    [ObservableProperty] private double _browserButtonOpacity;
    [ObservableProperty] private double _browserButtonCornerRadius;
    [ObservableProperty] private bool _showBrowserName;
    [ObservableProperty] private double _browserIconSize;
    [ObservableProperty] private Color _messageTextColor;
}
```

### BackdropMode / ThemeMode (Enums)

New in v0.2.0 (Phase E-1). Control window backdrop rendering and app theme.

```csharp
public enum BackdropMode { Mica, Acrylic, MicaAlt, SolidTranslucent, Opaque }
public enum ThemeMode { Light, Dark, System }
```


## 🎨 ViewModels

### MainViewModel

Main window view model.

```csharp
public partial class MainViewModel : ObservableObject
{
    [ObservableProperty]
    private string _url = string.Empty;
    
    [ObservableProperty]
    private ObservableCollection<Browser> _browsers = new();
    
    [ObservableProperty]
    private bool _isLoading = false;
    
    [ObservableProperty]
    private string _statusMessage = string.Empty;
    
    [RelayCommand]
    private async Task OpenSettingsAsync();
    
    [RelayCommand]
    private async Task LaunchBrowserAsync(Browser browser);
}
```

**Properties:**
- `Url`: Current URL input
- `Browsers`: Available browsers collection
- `IsLoading`: Loading state indicator
- `StatusMessage`: Status message display

**Commands:**
- `OpenSettingsCommand`: Opens settings window
- `LaunchBrowserCommand`: Launches selected browser

### SettingsViewModel

Settings window view model.

```csharp
public partial class SettingsViewModel : ObservableObject
{
    [ObservableProperty]
    private AppSettings _appSettings = new();
    
    [ObservableProperty]
    private VisualSettings _visualSettings = new();
    
    
    [RelayCommand]
    private async Task SaveSettingsAsync();
    
    [RelayCommand]
    private async Task ResetSettingsAsync();
}
```

## 🔌 Dependency Injection

### Service Registration

All application services are registered as singletons (`AddSingleton`); ViewModels are transient. See `src/BrowserSelector.App/DependencyInjection/` for the authoritative registration code.

```csharp
public static IServiceCollection AddBrowserSelectorServices(this IServiceCollection services)
{
    // Core / Infrastructure Services (all singleton)
    services.AddSingleton<IBrowserService, BrowserService>();
    services.AddSingleton<ISettingsService>(provider => /* ... */);
    services.AddSingleton<ICustomLanguageService>(provider => /* ... */);
    services.AddSingleton<ILocalizationService>(provider => /* ... */);
    services.AddSingleton<IUrlService>(provider => /* ... */);
    services.AddSingleton<IUrlRuleService>(provider => /* ... */);
    services.AddSingleton<ILogService, BrowserSelector.Infrastructure.Logging.LogService>();
    services.AddSingleton<IIconCacheService>(provider => /* ... */);
    services.AddSingleton<IRegistryService>(provider => /* ... */);
    services.AddSingleton<IProtocolHandler, ProtocolHandler>();

    // Named HttpClient for updates (IHttpClientFactory, not a singleton HttpClient) so tests can
    // substitute a stub handler through the same code path as production.
    services.AddHttpClient(UpdateService.HttpClientName, client => { /* User-Agent, Accept, etc. */ });
    services.AddSingleton<IUpdateService>(provider => /* ... */);
    services.AddSingleton<IExternalLinkService>(provider => /* ... */);
    services.AddSingleton<IThemeService>(provider => /* ... */);

    // Presentation Services (transient)
    services.AddTransient<MainViewModel>();
    services.AddTransient<SettingsViewModel>();
    services.AddTransient<LanguageManagementViewModel>();

    return services;
}
```

## 🧪 Testing APIs

### Test Helpers

```csharp
public static class TestHelpers
{
    public static string GetApplicationPath();
    public static void CleanupTestData();
    public static Browser CreateTestBrowser();
    public static AppSettings CreateTestSettings();
}
```

### Mock Services

```csharp
public class MockBrowserService : IBrowserService
{
    public Task<IEnumerable<Browser>> DetectBrowsersAsync()
    {
        return Task.FromResult(new List<Browser> { CreateTestBrowser() });
    }
    
    // ... other method implementations
}
```

## 📝 Usage Examples

### Basic Browser Detection

```csharp
var browserService = serviceProvider.GetRequiredService<IBrowserService>();
var browsers = await browserService.DetectBrowsersAsync();

foreach (var browser in browsers)
{
    Console.WriteLine($"Browser: {browser.Name}, Path: {browser.ExecutablePath}");
}
```

### Settings Management

```csharp
var settingsService = serviceProvider.GetRequiredService<ISettingsService>();
var visualSettings = await settingsService.LoadVisualSettingsAsync();

// Modify settings
visualSettings.BackgroundColor = Colors.Red;
visualSettings.BrowserButtonCornerRadius = 12.0;

// Save changes
await settingsService.SaveVisualSettingsAsync(visualSettings);
```

### Update Check

```csharp
var updateService = serviceProvider.GetRequiredService<IUpdateService>();
updateService.UpdateAvailable += (sender, args) =>
{
    Console.WriteLine($"Update available: {args.UpdateInfo.Version}");
};

UpdateInfo? updateInfo = await updateService.CheckForUpdatesAsync();
if (updateInfo != null)
{
    UpdateChannel channel = updateService.ResolveChannel();
    UpdateDownloadResult result = await updateService.DownloadUpdateAsync(updateInfo, channel);
    if (result.Success)
    {
        bool applied = await updateService.ApplyUpdateAsync(updateInfo);
        // On success, shut the application down — ApplyUpdateAsync only starts the
        // installer/Updater.exe process; it does not exit the app itself.
    }
}
```

## 🔒 Security Considerations

- All file operations are validated for path traversal
- Registry access is restricted to safe keys
- Process execution is validated and sanitized
- User input is sanitized to prevent injection attacks
- Executable/installer are **not code-signed** at this time
- Update download verification: SHA256 checksums from `SHA256SUMS.txt` are required for every download; a mismatch or unavailable checksum file causes the download to fail closed (see `IUpdateService` above)
- Update host allow-listing: `api.github.com`, `github.com`, and `*.githubusercontent.com` only, re-checked after redirects

## 📚 Additional Resources

- [Architecture Overview](ARCHITECTURE.md)
- [Testing Guide](TESTING.md)
- [Localization Guide](LOCALIZATION.md)
- [Update System](UPDATES.md)
