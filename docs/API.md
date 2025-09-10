# BrowserSelector API Documentation

## 📋 Overview

This document describes the public APIs and interfaces provided by BrowserSelector.

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

### IBrowserService

Manages browser detection and operations.

```csharp
public interface IBrowserService
{
    Task<IEnumerable<Browser>> DetectBrowsersAsync();
    Task<Browser?> GetDefaultBrowserAsync();
    Task<bool> SetDefaultBrowserAsync(Browser browser);
    Task<bool> LaunchBrowserAsync(Browser browser, string url);
}
```

**Methods:**
- `DetectBrowsersAsync()`: Detects installed browsers from registry
- `GetDefaultBrowserAsync()`: Gets the current default browser
- `SetDefaultBrowserAsync(Browser)`: Sets a browser as default
- `LaunchBrowserAsync(Browser, string)`: Launches a browser with URL

### ISettingsService

Manages application settings.

```csharp
public interface ISettingsService
{
    AppSettings AppSettings { get; }
    VisualSettings VisualSettings { get; }
    LogSettings LogSettings { get; }
    Task SaveAppSettingsAsync(AppSettings settings);
    Task<AppSettings> LoadAppSettingsAsync();
    Task SaveVisualSettingsAsync(VisualSettings settings);
    Task<VisualSettings> LoadVisualSettingsAsync();
    Task SaveLogSettingsAsync(LogSettings settings);
    Task<LogSettings> LoadLogSettingsAsync();
    Task<bool> ResetSettingsAsync();
    Task<bool> ExportSettingsAsync(string filePath);
    Task<bool> ImportSettingsAsync(string filePath);
}
```

**Properties:**
- `AppSettings`: Application configuration (language, update settings, etc.)
- `VisualSettings`: UI appearance settings (background, buttons, window size, etc.)
- `LogSettings`: Logging configuration (log level, file settings, etc.)

**Methods:**
- `SaveAppSettingsAsync(AppSettings)`: Persists application settings to file
- `LoadAppSettingsAsync()`: Loads application settings from file
- `SaveVisualSettingsAsync(VisualSettings)`: Persists visual settings to file
- `LoadVisualSettingsAsync()`: Loads visual settings from file
- `SaveLogSettingsAsync(LogSettings)`: Persists log settings to file
- `LoadLogSettingsAsync()`: Loads log settings from file
- `ResetSettingsAsync()`: Resets all settings to defaults
- `ExportSettingsAsync(string)`: Exports settings to file
- `ImportSettingsAsync(string)`: Imports settings from file

### IUpdateService

Handles automatic updates with GitHub Releases integration.

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

**Events:**
- `UpdateAvailable`: Fired when an update is available

**Methods:**
- `CheckForUpdatesAsync()`: Checks GitHub Releases for new versions
- `DownloadUpdateAsync(UpdateInfo, IProgress<int>)`: Downloads update files
- `InstallUpdateAsync(UpdateInfo)`: Installs the update (settings files only)
- `RollbackUpdateAsync()`: Rolls back to previous version
- `CreateBackup()`: Creates backup of current settings

### ILocalizationService

Manages multi-language support.

```csharp
public interface ILocalizationService
{
    string GetString(string key);
    string GetString(string key, params object[] args);
    void SetLanguage(CultureInfo culture);
    event EventHandler<LanguageChangedEventArgs>? LanguageChanged;
    IEnumerable<CultureInfo> SupportedLanguages { get; }
}
```

**Methods:**
- `GetString(string)`: Gets localized string
- `GetString(string, object[])`: Gets localized string with formatting
- `SetLanguage(CultureInfo)`: Changes application language

**Properties:**
- `SupportedLanguages`: List of supported cultures

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

UI appearance settings.

```csharp
public class VisualSettings
{
    public double WindowOpacity { get; set; }
    public string BackgroundColor { get; set; }
    public bool EnableGradient { get; set; }
    public string GradientColor { get; set; }
    public double CornerRadius { get; set; }
    public bool ShowTitleBar { get; set; }
    public int BrowserButtonWidth { get; set; }
    public int BrowserButtonHeight { get; set; }
}
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

```csharp
public static IServiceCollection AddBrowserSelectorServices(this IServiceCollection services)
{
    // Core Services
    services.AddScoped<IBrowserService, BrowserService>();
    services.AddScoped<ISettingsService, SettingsService>();
    services.AddScoped<ILocalizationService, LocalizationService>();
    services.AddScoped<IUpdateService, UpdateService>();
    
    // Infrastructure Services
    services.AddSingleton<ILogService, LogService>();
    services.AddScoped<IRegistryService, WindowsRegistryService>();
    services.AddScoped<ISystemTrayService, SystemTrayService>();
    
    // Presentation Services
    services.AddTransient<MainViewModel>();
    services.AddTransient<SettingsViewModel>();
    
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
await settingsService.LoadSettingsAsync();

// Modify settings
settingsService.VisualSettings.WindowOpacity = 0.8;
settingsService.VisualSettings.BackgroundColor = "#FF0000";

// Save changes
await settingsService.SaveSettingsAsync();
```

### Update Check

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
- Update downloads are verified with checksums
- User input is sanitized to prevent injection attacks

## 📚 Additional Resources

- [Architecture Overview](ARCHITECTURE.md)
- [Testing Guide](TESTING.md)
- [Localization Guide](LOCALIZATION.md)
- [Update System](UPDATES.md)
