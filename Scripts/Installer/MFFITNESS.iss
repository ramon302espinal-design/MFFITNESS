; Inno Setup - MFFITNESS POS
; Compilar: .\Scripts\Build-Installer.ps1
; Requiere Inno Setup 6: https://jrsoftware.org/isdl.php

#ifndef AppVersion
#define AppVersion "1.0.0"
#endif

#define MyAppName "MFFITNESS POS"
#define MyAppPublisher "MFFITNESS"
#define MyAppURL "https://github.com"
#define MyAppExeName "UI.exe"
#define MyLauncher "Start-MFFITNESS.cmd"
#define PublishDir "..\..\artifacts\pos"

[Setup]
AppId={{A7B3C9D1-4E2F-5A6B-8C9D-0E1F2A3B4C5D}
AppName={#MyAppName}
AppVersion={#AppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
DefaultDirName={localappdata}\Programs\MFFITNESS
DefaultGroupName={#MyAppName}
DisableProgramGroupPage=yes
PrivilegesRequired=lowest
OutputDir=..\..\artifacts\installer
OutputBaseFilename=MFFITNESS-Setup-{#AppVersion}
SetupIconFile={#PublishDir}\Resources\IMG_1722.ico
UninstallDisplayIcon={app}\{#MyAppExeName}
Compression=lzma2/ultra64
SolidCompression=yes
WizardStyle=modern
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible

[Languages]
Name: "spanish"; MessagesFile: "compiler:Languages\Spanish.isl"

[Tasks]
Name: "desktopicon"; Description: "Crear acceso directo en el Escritorio"; GroupDescription: "Accesos directos:"; Flags: checkedonce

[Files]
Source: "{#PublishDir}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyLauncher}"; IconFilename: "{app}\{#MyAppExeName}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyLauncher}"; IconFilename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyLauncher}"; Description: "Abrir {#MyAppName}"; Flags: nowait postinstall skipifsilent

[UninstallRun]
Filename: "taskkill"; Parameters: "/F /IM UI.exe /T"; Flags: runhidden skipifdoesntexist
Filename: "taskkill"; Parameters: "/F /IM WhatsAppHost.exe /T"; Flags: runhidden skipifdoesntexist
Filename: "taskkill"; Parameters: "/F /IM ngrok.exe /T"; Flags: runhidden skipifdoesntexist

[Code]
function InitializeSetup(): Boolean;
begin
  Result := True;
end;
