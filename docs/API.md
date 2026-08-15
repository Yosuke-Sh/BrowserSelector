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
- `SetDefaultBrowserAsync(Guid)` / `GetDefaultBrowserAsync()`: Default browser management
- `UpdateBrowserUsageAsync(Guid)` / `UpdateUsageAsync(Browser)`: Updates browser usage statistics

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

Skeleton interface for automatic updates. **Not yet implemented — planned for v0.3.0.** Only the interface and a minimal `UpdateService` shell exist today; none of the methods below perform real GitHub Releases integration yet.

```csharp
public interface IUpdateService : IDisposable
{
    event EventHandler<UpdateAvailableEventArgs>? UpdateAvailable;
    Task<UpdateInfo?> CheckForUpdatesAsync();
    Task<bool> DownloadUpdateAsync(UpdateInfo updateInfo, IProgress<int>? progress = null);
    Task<bool> InstallUpdateAsync(UpdateInfo updateInfo);
    Task<bool> RollbackUpdateAsync();
    bool CreateBackup();
}
```

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
    services.AddSingleton<IUpdateService>(provider => /* ... */); // skeleton only, not functional yet
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

### Update Check (not yet functional — v0.3.0)

`IUpdateService` currently only exposes the skeleton shown below; `CheckForUpdatesAsync()` and friends do not perform real GitHub Releases integration yet. The example illustrates the intended future usage once v0.3.0 lands.

```csharp
var updateService = serviceProvider.GetRequiredService<IUpdateService>();
updateService.UpdateAvailable += (sender, args) =>
{
    Console.WriteLine($"Update available: {args.UpdateInfo.Version}");
};

var updateInfo = await updateService.CheckForUpdatesAsync();
if (updateInfo != null)
{
    await updateService.DownloadUpdateAsync(updateInfo);
    await updateService.InstallUpdateAsync(updateInfo);
}
```

## 🔒 Security Considerations

- All file operations are validated for path traversal
- Registry access is restricted to safe keys
- Process execution is validated and sanitized
- User input is sanitized to prevent injection attacks
- Executable/installer are **not code-signed** at this time
- Update download verification (checksums) is planned for v0.3.0 alongside the real `IUpdateService` implementation — not yet in place

## 📚 Additional Resources

- [Architecture Overview](ARCHITECTURE.md)
- [Testing Guide](TESTING.md)
- [Localization Guide](LOCALIZATION.md)
- [Update System](UPDATES.md)
