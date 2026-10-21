; Inno Setup script for Tmatmaia.
; Built by scripts/publish.ps1, which passes the version and the publish folder:
;   ISCC.exe /DAppVersion=1.0.0 /DSourceDir=..\dist\Tmatmaia installer\Tmatmaia.iss

#ifndef AppVersion
  #define AppVersion "0.0.0"
#endif
#ifndef SourceDir
  #define SourceDir "..\dist\Tmatmaia"
#endif

#define AppName "Tmatmaia"
#define AppExe "Tmatmaia.exe"
#define AppUrl "https://github.com/Salah-Elsayed404/tmatmaia"

[Setup]
; Never change AppId: Windows uses it to recognize upgrades and uninstalls.
AppId={{81A960D5-A4B7-41D2-A412-8B48212297F1}
AppName={#AppName}
AppVersion={#AppVersion}
AppVerName={#AppName} {#AppVersion}
AppPublisher=Salah Elsayed
AppPublisherURL={#AppUrl}
AppSupportURL={#AppUrl}/issues
AppUpdatesURL={#AppUrl}/releases
; Per-user install: no admin prompt.
PrivilegesRequired=lowest
DefaultDirName={localappdata}\Programs\{#AppName}
DisableProgramGroupPage=yes
LicenseFile=..\LICENSE
SetupIconFile=..\src\Tmatmaia\Assets\tmatmaia.ico
UninstallDisplayIcon={app}\{#AppExe}
OutputDir=..\dist
OutputBaseFilename={#AppName}-{#AppVersion}-setup
Compression=lzma2/max
SolidCompression=yes
WizardStyle=modern
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
; Windows 11 is required for the taskbar embedding.
MinVersion=10.0.22000
CloseApplications=force

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
Source: "{#SourceDir}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{autoprograms}\{#AppName}"; Filename: "{app}\{#AppExe}"
Name: "{autodesktop}\{#AppName}"; Filename: "{app}\{#AppExe}"; Tasks: desktopicon

[Run]
; Both show as checkboxes on the final page.
; The startup entry writes the same Run value the app uses for Settings > Start with Windows.
Filename: "{sys}\reg.exe"; Parameters: "add ""HKCU\Software\Microsoft\Windows\CurrentVersion\Run"" /v {#AppName} /t REG_SZ /d ""\""{app}\{#AppExe}\"""" /f"; Description: "Start {#AppName} when Windows starts"; Flags: postinstall runhidden skipifsilent
Filename: "{app}\{#AppExe}"; Description: "{cm:LaunchProgram,{#AppName}}"; Flags: nowait postinstall skipifsilent

[UninstallRun]
Filename: "{sys}\taskkill.exe"; Parameters: "/F /IM {#AppExe}"; Flags: runhidden; RunOnceId: "StopTmatmaia"

[UninstallDelete]
Type: filesandordirs; Name: "{userappdata}\{#AppName}"

[Code]
// Remove the startup entry whether the installer or the app's Settings added it.
procedure CurUninstallStepChanged(CurUninstallStep: TUninstallStep);
begin
  if CurUninstallStep = usPostUninstall then
    RegDeleteValue(HKEY_CURRENT_USER, 'Software\Microsoft\Windows\CurrentVersion\Run', '{#AppName}');
end;
