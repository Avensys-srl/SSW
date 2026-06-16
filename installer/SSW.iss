#define AppName "Avensys Selection Software"
#define AppPublisher "Avensys s.r.l."
#define AppExeName "SSW.exe"

#ifndef AppVersion
#define AppVersion "0.0.0.0"
#endif

#ifndef SourceDir
#error SourceDir is required. Pass /DSourceDir=...
#endif

#ifndef OutputDir
#define OutputDir "output"
#endif

[Setup]
AppId={{7C326B6C-D147-4F11-A4A1-4B0135F3F0C9}
AppName={#AppName}
AppVersion={#AppVersion}
AppPublisher={#AppPublisher}
DefaultDirName={localappdata}\Programs\Avensys\SSW
DefaultGroupName={#AppName}
DisableProgramGroupPage=yes
OutputDir={#OutputDir}
OutputBaseFilename=SSW_Setup_{#StringChange(AppVersion, '.', '_')}
Compression=lzma2
SolidCompression=yes
PrivilegesRequired=lowest
UninstallDisplayIcon={app}\{#AppExeName}
WizardStyle=modern
ArchitecturesAllowed=x86 x64

#ifdef IconFile
#ifexist IconFile
SetupIconFile={#IconFile}
#endif
#endif

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"
Name: "italian"; MessagesFile: "compiler:Languages\Italian.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
Source: "{#SourceDir}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs; Excludes: "*.pdb,*.xml,*.log,*.vshost.*,*.application,*.manifest,app.publish\*"

[Icons]
Name: "{group}\{#AppName}"; Filename: "{app}\{#AppExeName}"
Name: "{group}\Uninstall {#AppName}"; Filename: "{uninstallexe}"
Name: "{autodesktop}\{#AppName}"; Filename: "{app}\{#AppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#AppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(AppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent
