; BrowserSelector Inno Setup Script
; Version: 0.3.5
; Author: Yosuke-Sh
; Description: BrowserSelector WPF Application Installer

#define MyAppName "BrowserSelector"
; MyAppVersion（Phase E-2b）: Directory.Build.props の <Version> が単一の情報源。
; ビルド時は `ISCC /DMyAppVersion=0.3.5 BrowserSelector.iss` のように /D で上書きして注入する
; （release.yml からの自動注入はPhase G-4で実装。ここでの既定値はDirectory.Build.propsと手動で同期させておく）。
#ifndef MyAppVersion
  #define MyAppVersion "0.3.5"
#endif
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
SetupIconFile=..\..\src\BrowserSelector.App\BrowserSelector_Icon_256.ico
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
VersionInfoCopyright=Copyright (C) 2025-2026 {#MyAppPublisher}
; Phase H-12: /SILENT /CLOSEAPPLICATIONSを実効にする。RestartApplications=noは
; [Run]のpostinstall起動と二重にならないようにするため。
CloseApplications=yes
CloseApplicationsFilter=BrowserSelector.exe,BrowserSelector.Updater.exe
RestartApplications=no

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"; LicenseFile: "..\..\docs\LICENSE"
Name: "japanese"; MessagesFile: "compiler:Languages\Japanese.isl"; LicenseFile: "..\..\docs\LICENSE_ja.txt"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked
Name: "quicklaunchicon"; Description: "{cm:CreateQuickLaunchIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked; Check: not IsAdminInstallMode
Name: "set_default_browser"; Description: "BrowserSelectorを既定のブラウザとして設定する"; GroupDescription: "Default Browser Settings"; Flags: checkedonce
Name: "open_default_apps"; Description: "インストール後に既定のアプリ設定を開く"; GroupDescription: "Default Browser Settings"; Flags: unchecked

[Files]
Source: "..\..\src\BrowserSelector.App\bin\Release\net10.0-windows\win-x64\publish\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs; Excludes: "*.pdb,*.map,*.ilk"
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
; skipifsilentを外し、/SILENT実行（サイレント更新時）でもアプリを起動する。
; 従来はskipifsilentによりサイレント更新後にアプリが起動しないままとなり、
; 更新されたかどうかユーザーが分からない不具合の原因になっていた。
; runascurrentuserは必須: このインストーラはPrivilegesRequired=adminで昇格実行されるため、
; 指定しないとBrowserSelectorが管理者権限のまま起動してしまう
; （ブラウザ起動が管理者権限になる、UIPIでドラッグ&ドロップが効かなくなる等の実害がある）。
; RestartApplications=noは維持しているため、Inno標準の再起動機能と二重起動することはない。
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall runascurrentuser
; registeredAppNameクエリはWindows環境によって名前解決に失敗し「既定のブラウザに設定」ボタンを
; 押しても何も起きない不具合の原因だったため、クエリなしの単純なURIに変更した。
Filename: "{cmd}"; Parameters: "/c start ms-settings:defaultapps"; Tasks: open_default_apps; Flags: postinstall skipifsilent nowait

[UninstallDelete]
Type: filesandordirs; Name: "{app}\logs"
Type: filesandordirs; Name: "{app}\backup"
Type: files; Name: "{app}\settings.json"
Type: filesandordirs; Name: "{userappdata}\BrowserSelector"
; Phase H-12: 自動アップデートの作業ディレクトリ（%LOCALAPPDATA%\BrowserSelector\updates, \backup）
Type: filesandordirs; Name: "{localappdata}\BrowserSelector\updates"
Type: filesandordirs; Name: "{localappdata}\BrowserSelector\backup"

[Registry]
; アプリケーションの登録
Root: HKLM; Subkey: "SOFTWARE\BrowserSelector"; ValueType: string; ValueName: "InstallPath"; ValueData: "{app}"; Flags: uninsdeletekey
Root: HKLM; Subkey: "SOFTWARE\BrowserSelector"; ValueType: string; ValueName: "Version"; ValueData: "{#MyAppVersion}"; Flags: uninsdeletekey

; --- 既定ブラウザ登録（Windows 10/11対応） ---
; Windows 11で「既定のアプリ」画面の“ブラウザ”カテゴリに表示されるための必須要件は、
; Capabilities を SOFTWARE\Clients\StartMenuInternet\<AppName> 配下に登録すること。
; 従来 SOFTWARE\BrowserSelector\Capabilities 直下に置いていたが、この位置では
; Windows 11のブラウザ選択導線から認識されない。
; また、HKLM\SOFTWARE\Classes\http\shell\open\command 等の直接上書きは
; Win10/11ではHKCUのUserChoice（ハッシュ保護）が優先されるため実効性が無く、
; かつアンインストール時にマシン共有のキーを破壊するため行わない。DDE(ddeexec)キーも
; IE時代の遺物のため設定しない。

; Clients\StartMenuInternet配下のCapabilities登録（Win11の「既定のブラウザ」一覧に表示される要件）
Root: HKLM; Subkey: "SOFTWARE\Clients\StartMenuInternet\BrowserSelector"; ValueType: string; ValueName: ""; ValueData: "BrowserSelector"; Flags: uninsdeletekey; Tasks: set_default_browser
Root: HKLM; Subkey: "SOFTWARE\Clients\StartMenuInternet\BrowserSelector\Capabilities"; ValueType: string; ValueName: "ApplicationName"; ValueData: "BrowserSelector"; Flags: uninsdeletekey; Tasks: set_default_browser
Root: HKLM; Subkey: "SOFTWARE\Clients\StartMenuInternet\BrowserSelector\Capabilities"; ValueType: string; ValueName: "ApplicationDescription"; ValueData: "BrowserSelector - 複数のブラウザから選択できるアプリケーション"; Flags: uninsdeletekey; Tasks: set_default_browser
Root: HKLM; Subkey: "SOFTWARE\Clients\StartMenuInternet\BrowserSelector\Capabilities"; ValueType: string; ValueName: "ApplicationIcon"; ValueData: "{app}\{#MyAppExeName},0"; Flags: uninsdeletekey; Tasks: set_default_browser
Root: HKLM; Subkey: "SOFTWARE\Clients\StartMenuInternet\BrowserSelector\Capabilities\URLAssociations"; ValueType: string; ValueName: "http"; ValueData: "BrowserSelector.http"; Flags: uninsdeletekey; Tasks: set_default_browser
Root: HKLM; Subkey: "SOFTWARE\Clients\StartMenuInternet\BrowserSelector\Capabilities\URLAssociations"; ValueType: string; ValueName: "https"; ValueData: "BrowserSelector.https"; Flags: uninsdeletekey; Tasks: set_default_browser
Root: HKLM; Subkey: "SOFTWARE\Clients\StartMenuInternet\BrowserSelector\Capabilities\FileAssociations"; ValueType: string; ValueName: ".htm"; ValueData: "BrowserSelector.htm"; Flags: uninsdeletekey; Tasks: set_default_browser
Root: HKLM; Subkey: "SOFTWARE\Clients\StartMenuInternet\BrowserSelector\Capabilities\FileAssociations"; ValueType: string; ValueName: ".html"; ValueData: "BrowserSelector.html"; Flags: uninsdeletekey; Tasks: set_default_browser
Root: HKLM; Subkey: "SOFTWARE\Clients\StartMenuInternet\BrowserSelector\DefaultIcon"; ValueType: string; ValueName: ""; ValueData: "{app}\{#MyAppExeName},0"; Flags: uninsdeletekey; Tasks: set_default_browser
Root: HKLM; Subkey: "SOFTWARE\Clients\StartMenuInternet\BrowserSelector\shell\open\command"; ValueType: string; ValueName: ""; ValueData: """{app}\{#MyAppExeName}"""; Flags: uninsdeletekey; Tasks: set_default_browser

; Windowsの既定アプリ一覧（RegisteredApplications）へ登録
Root: HKLM; Subkey: "SOFTWARE\RegisteredApplications"; ValueType: string; ValueName: "BrowserSelector"; ValueData: "SOFTWARE\Clients\StartMenuInternet\BrowserSelector\Capabilities"; Flags: uninsdeletekey; Tasks: set_default_browser

; プロトコル・ファイル関連付け用ProgIdクラスの登録（Capabilities\URLAssociations/FileAssociationsから参照される）
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
const
  DotNetRuntimeDownloadUrl = 'https://aka.ms/dotnet/10.0/windowsdesktop-runtime-win-x64.exe';
  DotNetRuntimeManualDownloadPage = 'https://dotnet.microsoft.com/download/dotnet/10.0';

// {commonpf64}\dotnet\shared\Microsoft.WindowsDesktop.App 配下に "10." で始まるディレクトリがあるか確認する
function FindFirstDotNet10SharedFxDir(): Boolean;
var
  FindRec: TFindRec;
  BaseDir: string;
begin
  Result := False;
  BaseDir := ExpandConstant('{commonpf64}\dotnet\shared\Microsoft.WindowsDesktop.App');

  if FindFirst(BaseDir + '\10.*', FindRec) then
  begin
    try
      Result := True;
    finally
      FindClose(FindRec);
    end;
  end;
end;

// Microsoft.WindowsDesktop.App 10.x が導入済みかどうかを判定する。
// レジストリでの確認を主とし、フォルダー存在確認をフォールバックとして併用する。
function IsWindowsDesktopRuntime10Installed(): Boolean;
var
  Names: TArrayOfString;
  I: Integer;
begin
  Result := False;

  if RegGetSubkeyNames(HKLM, 'SOFTWARE\dotnet\Setup\InstalledVersions\x64\sharedfx\Microsoft.WindowsDesktop.App', Names) then
  begin
    for I := 0 to GetArrayLength(Names) - 1 do
    begin
      if (Length(Names[I]) > 0) and (Names[I][1] = '1') then
      begin
        Result := True;
        Exit;
      end;
    end;
  end;

  if DirExists(ExpandConstant('{commonpf64}\dotnet\shared\Microsoft.WindowsDesktop.App')) then
  begin
    if FindFirstDotNet10SharedFxDir() then
    begin
      Result := True;
      Exit;
    end;
  end;
end;

function OnDownloadProgress(const Url, FileName: string; const Progress, ProgressMax: Int64): Boolean;
begin
  Result := True;
end;

// .NET Desktop Runtime 10 をダウンロードし、サイレントインストールする。
// 失敗した場合は手動導入用のURLを提示してセットアップを中断する。
function EnsureDotNetDesktopRuntimeInstalled(): Boolean;
var
  ResultCode: Integer;
  DownloadOk: Boolean;
begin
  Result := True;

  if IsWindowsDesktopRuntime10Installed() then
    Exit;

  if not WizardSilent() then
  begin
    if MsgBox('BrowserSelectorの実行には .NET 10 Desktop Runtime が必要です。' + #13#10 +
              '未導入のため、ダウンロードしてインストールします。よろしいですか？',
              mbConfirmation, MB_YESNO) = IDNO then
    begin
      Result := False;
      Exit;
    end;
  end;

  DownloadOk := False;
  try
    DownloadTemporaryFile(DotNetRuntimeDownloadUrl, 'windowsdesktop-runtime-win-x64.exe', '', @OnDownloadProgress);
    DownloadOk := True;
  except
    DownloadOk := False;
  end;

  if not DownloadOk then
  begin
    MsgBox('.NET 10 Desktop Runtime のダウンロードに失敗しました。' + #13#10 +
           '以下のURLから手動でダウンロード・インストールしてから、再度セットアップを実行してください:' + #13#10 +
           DotNetRuntimeManualDownloadPage,
           mbError, MB_OK);
    Result := False;
    Exit;
  end;

  if not Exec(ExpandConstant('{tmp}\windowsdesktop-runtime-win-x64.exe'), '/install /quiet /norestart', '',
              SW_SHOW, ewWaitUntilTerminated, ResultCode) then
  begin
    MsgBox('.NET 10 Desktop Runtime のインストールに失敗しました。' + #13#10 +
           '以下のURLから手動でダウンロード・インストールしてから、再度セットアップを実行してください:' + #13#10 +
           DotNetRuntimeManualDownloadPage,
           mbError, MB_OK);
    Result := False;
    Exit;
  end;

  if not IsWindowsDesktopRuntime10Installed() then
  begin
    MsgBox('.NET 10 Desktop Runtime のインストールを完了できませんでした。' + #13#10 +
           '以下のURLから手動でダウンロード・インストールしてから、再度セットアップを実行してください:' + #13#10 +
           DotNetRuntimeManualDownloadPage,
           mbError, MB_OK);
    Result := False;
    Exit;
  end;
end;

function PrepareToInstall(var NeedsRestart: Boolean): string;
begin
  Result := '';
  if not EnsureDotNetDesktopRuntimeInstalled() then
    Result := '.NET 10 Desktop Runtime が導入されなかったため、セットアップを中断しました。';
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
