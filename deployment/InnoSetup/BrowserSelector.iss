; BrowserSelector Inno Setup Script
; Version: 0.1.0
; Author: Yosuke-Sh
; Description: BrowserSelector WPF Application Installer

#define MyAppName "BrowserSelector"
#define MyAppVersion "0.1.0"
#define MyAppPublisher "Yosuke-Sh"
#define MyAppURL "https://github.com/Yosuke-Sh/BrowserSelector"
#define MyAppExeName "BrowserSelector.exe"
#define MyAppDescription "A modern WPF application for selecting and opening URLs with multiple browsers on Windows"

[Setup]
; NOTE: The value of AppId uniquely identifies this application.
; Do not use the same AppId value in installers for other applications.
AppId={{A1B2C3D4-E5F6-7890-ABCD-EF1234567890}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppVerName={#MyAppName} {#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}
DefaultDirName={autopf}\{#MyAppName}
DefaultGroupName={#MyAppName}
AllowNoIcons=yes
OutputDir=..\..\release-artifacts
OutputBaseFilename=BrowserSelector-Setup-v{#MyAppVersion}
; SetupIconFile=..\..\src\BrowserSelector.App\BrowserSelector_Icon_256.ico
Compression=lzma
SolidCompression=yes
WizardStyle=modern
PrivilegesRequired=admin
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
MinVersion=10.0.17763
DisableProgramGroupPage=yes
DisableReadyPage=no
DisableFinishedPage=no
UninstallDisplayIcon={app}\{#MyAppExeName}
UninstallDisplayName={#MyAppName}
VersionInfoVersion={#MyAppVersion}
VersionInfoCompany={#MyAppPublisher}
VersionInfoDescription={#MyAppDescription}
VersionInfoCopyright=Copyright (C) 2025 {#MyAppPublisher}

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"; LicenseFile: "..\..\docs\LICENSE"
Name: "japanese"; MessagesFile: "compiler:Languages\Japanese.isl"; LicenseFile: "..\..\docs\LICENSE_ja.txt"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked
Name: "quicklaunchicon"; Description: "{cm:CreateQuickLaunchIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked; Check: not IsAdminInstallMode
Name: "set_default_browser"; Description: "BrowserSelectorを既定のブラウザとして設定する"; GroupDescription: "Default Browser Settings"; Flags: checkedonce
Name: "open_default_apps"; Description: "インストール後に既定のアプリ設定を開く"; GroupDescription: "Default Browser Settings"; Flags: unchecked

[Files]
Source: "..\..\src\BrowserSelector.App\bin\Release\net8.0-windows\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "..\..\src\BrowserSelector.App\BrowserSelector_Icon_256.ico"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\..\README.md"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\..\CHANGELOG.md"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\..\docs\USER_MANUAL.md"; DestDir: "{app}\docs"; Flags: ignoreversion
Source: "..\..\docs\API.md"; DestDir: "{app}\docs"; Flags: ignoreversion
Source: "..\..\docs\CONTRIBUTING.md"; DestDir: "{app}\docs"; Flags: ignoreversion
Source: "..\..\docs\LICENSE"; DestDir: "{app}\docs"; Flags: ignoreversion

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; IconFilename: "{app}\BrowserSelector_Icon_256.ico"
Name: "{group}\{cm:UninstallProgram,{#MyAppName}}"; Filename: "{uninstallexe}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; IconFilename: "{app}\BrowserSelector_Icon_256.ico"; Tasks: desktopicon
Name: "{userappdata}\Microsoft\Internet Explorer\Quick Launch\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; IconFilename: "{app}\BrowserSelector_Icon_256.ico"; Tasks: quicklaunchicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent
Filename: "{cmd}"; Parameters: "/c start ms-settings:defaultapps"; Tasks: open_default_apps; Flags: postinstall skipifsilent nowait

[UninstallDelete]
Type: filesandordirs; Name: "{app}\logs"
Type: filesandordirs; Name: "{app}\backup"
Type: files; Name: "{app}\settings.json"

[Registry]
; HTTPプロトコルハンドラーの設定
Root: HKLM; Subkey: "SOFTWARE\Classes\http\shell\open\command"; ValueType: string; ValueName: ""; ValueData: """{app}\{#MyAppExeName}"" ""%1"""; Flags: uninsdeletekey; Tasks: set_default_browser
Root: HKLM; Subkey: "SOFTWARE\Classes\http\shell\open\ddeexec"; ValueType: string; ValueName: ""; ValueData: ""; Flags: uninsdeletekey; Tasks: set_default_browser
Root: HKLM; Subkey: "SOFTWARE\Classes\http\shell\open\ddeexec\Application"; ValueType: string; ValueName: ""; ValueData: "BrowserSelector"; Flags: uninsdeletekey; Tasks: set_default_browser
Root: HKLM; Subkey: "SOFTWARE\Classes\http\shell\open\ddeexec\Topic"; ValueType: string; ValueName: ""; ValueData: "WWW_OpenURL"; Flags: uninsdeletekey; Tasks: set_default_browser

; HTTPSプロトコルハンドラーの設定
Root: HKLM; Subkey: "SOFTWARE\Classes\https\shell\open\command"; ValueType: string; ValueName: ""; ValueData: """{app}\{#MyAppExeName}"" ""%1"""; Flags: uninsdeletekey; Tasks: set_default_browser
Root: HKLM; Subkey: "SOFTWARE\Classes\https\shell\open\ddeexec"; ValueType: string; ValueName: ""; ValueData: ""; Flags: uninsdeletekey; Tasks: set_default_browser
Root: HKLM; Subkey: "SOFTWARE\Classes\https\shell\open\ddeexec\Application"; ValueType: string; ValueName: ""; ValueData: "BrowserSelector"; Flags: uninsdeletekey; Tasks: set_default_browser
Root: HKLM; Subkey: "SOFTWARE\Classes\https\shell\open\ddeexec\Topic"; ValueType: string; ValueName: ""; ValueData: "WWW_OpenURL"; Flags: uninsdeletekey; Tasks: set_default_browser

; ファイル拡張子の関連付け
Root: HKLM; Subkey: "SOFTWARE\Classes\.htm\shell\open\command"; ValueType: string; ValueName: ""; ValueData: """{app}\{#MyAppExeName}"" ""%1"""; Flags: uninsdeletekey; Tasks: set_default_browser
Root: HKLM; Subkey: "SOFTWARE\Classes\.html\shell\open\command"; ValueType: string; ValueName: ""; ValueData: """{app}\{#MyAppExeName}"" ""%1"""; Flags: uninsdeletekey; Tasks: set_default_browser

; アプリケーションの登録
Root: HKLM; Subkey: "SOFTWARE\BrowserSelector"; ValueType: string; ValueName: "InstallPath"; ValueData: "{app}"; Flags: uninsdeletekey
Root: HKLM; Subkey: "SOFTWARE\BrowserSelector"; ValueType: string; ValueName: "Version"; ValueData: "{#MyAppVersion}"; Flags: uninsdeletekey

; Windowsの既定アプリ一覧に表示されるための登録
Root: HKLM; Subkey: "SOFTWARE\RegisteredApplications"; ValueType: string; ValueName: "BrowserSelector"; ValueData: "SOFTWARE\BrowserSelector\Capabilities"; Flags: uninsdeletekey; Tasks: set_default_browser
Root: HKLM; Subkey: "SOFTWARE\BrowserSelector\Capabilities"; ValueType: string; ValueName: "ApplicationDescription"; ValueData: "BrowserSelector - 複数のブラウザから選択できるアプリケーション"; Flags: uninsdeletekey; Tasks: set_default_browser
Root: HKLM; Subkey: "SOFTWARE\BrowserSelector\Capabilities"; ValueType: string; ValueName: "ApplicationName"; ValueData: "BrowserSelector"; Flags: uninsdeletekey; Tasks: set_default_browser

; プロトコル関連付けの登録
Root: HKLM; Subkey: "SOFTWARE\BrowserSelector\Capabilities\URLAssociations"; ValueType: string; ValueName: "http"; ValueData: "BrowserSelector.http"; Flags: uninsdeletekey; Tasks: set_default_browser
Root: HKLM; Subkey: "SOFTWARE\BrowserSelector\Capabilities\URLAssociations"; ValueType: string; ValueName: "https"; ValueData: "BrowserSelector.https"; Flags: uninsdeletekey; Tasks: set_default_browser

; ファイル関連付けの登録
Root: HKLM; Subkey: "SOFTWARE\BrowserSelector\Capabilities\FileAssociations"; ValueType: string; ValueName: ".htm"; ValueData: "BrowserSelector.htm"; Flags: uninsdeletekey; Tasks: set_default_browser
Root: HKLM; Subkey: "SOFTWARE\BrowserSelector\Capabilities\FileAssociations"; ValueType: string; ValueName: ".html"; ValueData: "BrowserSelector.html"; Flags: uninsdeletekey; Tasks: set_default_browser

; カスタムプロトコルクラスの登録
Root: HKLM; Subkey: "SOFTWARE\Classes\BrowserSelector.http"; ValueType: string; ValueName: ""; ValueData: "BrowserSelector HTTP Protocol"; Flags: uninsdeletekey; Tasks: set_default_browser
Root: HKLM; Subkey: "SOFTWARE\Classes\BrowserSelector.http\shell\open\command"; ValueType: string; ValueName: ""; ValueData: """{app}\{#MyAppExeName}"" ""%1"""; Flags: uninsdeletekey; Tasks: set_default_browser

Root: HKLM; Subkey: "SOFTWARE\Classes\BrowserSelector.https"; ValueType: string; ValueName: ""; ValueData: "BrowserSelector HTTPS Protocol"; Flags: uninsdeletekey; Tasks: set_default_browser
Root: HKLM; Subkey: "SOFTWARE\Classes\BrowserSelector.https\shell\open\command"; ValueType: string; ValueName: ""; ValueData: """{app}\{#MyAppExeName}"" ""%1"""; Flags: uninsdeletekey; Tasks: set_default_browser

Root: HKLM; Subkey: "SOFTWARE\Classes\BrowserSelector.htm"; ValueType: string; ValueName: ""; ValueData: "BrowserSelector HTML File"; Flags: uninsdeletekey; Tasks: set_default_browser
Root: HKLM; Subkey: "SOFTWARE\Classes\BrowserSelector.htm\shell\open\command"; ValueType: string; ValueName: ""; ValueData: """{app}\{#MyAppExeName}"" ""%1"""; Flags: uninsdeletekey; Tasks: set_default_browser

Root: HKLM; Subkey: "SOFTWARE\Classes\BrowserSelector.html"; ValueType: string; ValueName: ""; ValueData: "BrowserSelector HTML File"; Flags: uninsdeletekey; Tasks: set_default_browser
Root: HKLM; Subkey: "SOFTWARE\Classes\BrowserSelector.html\shell\open\command"; ValueType: string; ValueName: ""; ValueData: """{app}\{#MyAppExeName}"" ""%1"""; Flags: uninsdeletekey; Tasks: set_default_browser

; カスタムプロトコル（browser://）
Root: HKLM; Subkey: "SOFTWARE\Classes\browser"; ValueType: string; ValueName: ""; ValueData: "URL:BrowserSelector Protocol"; Flags: uninsdeletekey; Tasks: set_default_browser
Root: HKLM; Subkey: "SOFTWARE\Classes\browser"; ValueType: string; ValueName: "URL Protocol"; ValueData: ""; Flags: uninsdeletekey; Tasks: set_default_browser
Root: HKLM; Subkey: "SOFTWARE\Classes\browser\DefaultIcon"; ValueType: string; ValueName: ""; ValueData: "{app}\{#MyAppExeName},1"; Flags: uninsdeletekey; Tasks: set_default_browser
Root: HKLM; Subkey: "SOFTWARE\Classes\browser\shell\open\command"; ValueType: string; ValueName: ""; ValueData: """{app}\{#MyAppExeName}"" ""%1"""; Flags: uninsdeletekey; Tasks: set_default_browser


[Code]
function InitializeSetup(): Boolean;
var
  ErrorCode: Integer;
  DotNetVersion: string;
  DotNet8Installed: Boolean;
begin
  Result := True;
  DotNet8Installed := False;
  
  // Check for .NET 8.0 Runtime - try multiple registry locations
  // Method 1: Check shared frameworks
  if RegQueryStringValue(HKLM, 'SOFTWARE\dotnet\Setup\InstalledVersions\x64\Microsoft.NETCore.App', '8.0', DotNetVersion) then
    DotNet8Installed := True
  else if RegQueryStringValue(HKLM, 'SOFTWARE\dotnet\Setup\InstalledVersions\x64\Microsoft.WindowsDesktop.App', '8.0', DotNetVersion) then
    DotNet8Installed := True
  else if RegQueryStringValue(HKLM, 'SOFTWARE\dotnet\Setup\InstalledVersions\x64\Microsoft.AspNetCore.App', '8.0', DotNetVersion) then
    DotNet8Installed := True;
    
  // Method 2: Check if dotnet command is available and has .NET 8.0
  if not DotNet8Installed then
  begin
    if Exec('cmd.exe', '/c dotnet --list-runtimes | findstr "Microsoft.WindowsDesktop.App 8.0"', '', SW_HIDE, ewWaitUntilTerminated, ErrorCode) then
    begin
      if ErrorCode = 0 then
        DotNet8Installed := True;
    end;
  end;
  
  if not DotNet8Installed then
  begin
    if ActiveLanguage = 'japanese' then
    begin
      if MsgBox('BrowserSelectorには.NET 8.0 Runtimeが必要です。' + #13#10 + #13#10 +
                '今すぐダウンロードしますか？', mbConfirmation, MB_YESNO) = IDYES then
      begin
        ShellExec('open', 'https://dotnet.microsoft.com/download/dotnet/8.0', '', '', SW_SHOWNORMAL, ewNoWait, ErrorCode);
      end;
    end
    else
    begin
      if MsgBox('BrowserSelector requires .NET 8.0 Runtime to be installed.' + #13#10 + #13#10 +
                'Would you like to download it now?', mbConfirmation, MB_YESNO) = IDYES then
      begin
        ShellExec('open', 'https://dotnet.microsoft.com/download/dotnet/8.0', '', '', SW_SHOWNORMAL, ewNoWait, ErrorCode);
      end;
    end;
    Result := False;
  end;
end;

procedure CurStepChanged(CurStep: TSetupStep);
begin
  if CurStep = ssPostInstall then
  begin
    // Create application data directory
    ForceDirectories(ExpandConstant('{userappdata}\BrowserSelector'));
    ForceDirectories(ExpandConstant('{userappdata}\BrowserSelector\logs'));
    ForceDirectories(ExpandConstant('{userappdata}\BrowserSelector\backup'));
  end;
end;
