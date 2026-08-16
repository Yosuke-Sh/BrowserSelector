@echo off
setlocal enabledelayedexpansion

set "DOTNET_CLI_UI_LANGUAGE=en"
set "ROOT=%~dp0"
set "APP_PROJECT=%ROOT%src\BrowserSelector.App\BrowserSelector.App.csproj"
set "PUBLISH_DIR=%ROOT%src\BrowserSelector.App\bin\Release\net10.0-windows\win-x64\publish"
set "ISS_FILE=%ROOT%deployment\InnoSetup\BrowserSelector.iss"
set "SLN_FILE=%ROOT%BrowserSelector.WPF.sln"

for /f "delims=" %%V in ('powershell -NoProfile -Command "([xml](Get-Content '%ROOT%Directory.Build.props')).Project.PropertyGroup.VersionPrefix | Select-Object -First 1"') do set "VERSION=%%V"

if "%VERSION%"=="" (
    echo [ERROR] Directory.Build.props からバージョンを取得できませんでした。
    exit /b 1
)
echo [INFO] Building installer for version %VERSION%

set "ISCC=%ProgramFiles(x86)%\Inno Setup 6\ISCC.exe"
if not exist "%ISCC%" set "ISCC=%ProgramFiles%\Inno Setup 6\ISCC.exe"
if not exist "%ISCC%" (
    echo [ERROR] ISCC.exe が見つかりません。Inno Setup 6 をインストールしてください。
    exit /b 1
)

echo [INFO] Restoring dependencies...
dotnet restore "%SLN_FILE%"
if errorlevel 1 exit /b 1

echo [INFO] Publishing application (ReadyToRun)...
dotnet publish "%APP_PROJECT%" --configuration Release --runtime win-x64 --self-contained false -p:PublishReadyToRun=true -p:Version=%VERSION% --output "%PUBLISH_DIR%"
if errorlevel 1 exit /b 1

echo [INFO] Building installer (Inno Setup)...
"%ISCC%" "/DMyAppVersion=%VERSION%" "%ISS_FILE%"
if errorlevel 1 exit /b 1

echo [INFO] Installer created at %ROOT%release-artifacts\BrowserSelector-Setup-v%VERSION%.exe
endlocal
