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

#ifdef BootstrapKey
  #ifndef BootstrapEnvironmentName
    #error BootstrapEnvironmentName is required when BootstrapKey is defined
  #endif
  #ifndef BootstrapRegistryValueName
    #error BootstrapRegistryValueName is required when BootstrapKey is defined
  #endif
#endif

[Setup]
AppId={{7C326B6C-D147-4F11-A4A1-4B0135F3F0C9}
AppName={#AppName}
AppVersion={#AppVersion}
AppPublisher={#AppPublisher}
CloseApplications=yes
RestartApplications=yes
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
ArchitecturesAllowed=x86 x64compatible

#ifdef IconFile
#ifexist IconFile
SetupIconFile={#IconFile}
#endif
#endif

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"
Name: "italian"; MessagesFile: "compiler:Languages\Italian.isl"
Name: "bulgarian"; MessagesFile: "compiler:Languages\Bulgarian.isl"
Name: "czech"; MessagesFile: "compiler:Languages\Czech.isl"
Name: "danish"; MessagesFile: "compiler:Languages\Danish.isl"
Name: "dutch"; MessagesFile: "compiler:Languages\Dutch.isl"
Name: "french"; MessagesFile: "compiler:Languages\French.isl"
Name: "german"; MessagesFile: "compiler:Languages\German.isl"
Name: "hungarian"; MessagesFile: "compiler:Languages\Hungarian.isl"
Name: "norwegian"; MessagesFile: "compiler:Languages\Norwegian.isl"
Name: "polish"; MessagesFile: "compiler:Languages\Polish.isl"
Name: "slovenian"; MessagesFile: "compiler:Languages\Slovenian.isl"
Name: "swedish"; MessagesFile: "compiler:Languages\Swedish.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
Source: "{#SourceDir}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs; Excludes: "*.pdb,*.xml,*.log,*.vshost.*,*.application,SSW.exe.manifest,app.publish\*,data\DataCentral___.sdf,data\*.bak,data\*.previous,data\catalog-update.state.json"

[Icons]
Name: "{group}\{#AppName}"; Filename: "{app}\{#AppExeName}"
Name: "{group}\Uninstall {#AppName}"; Filename: "{uninstallexe}"
Name: "{autodesktop}\{#AppName}"; Filename: "{app}\{#AppExeName}"; Tasks: desktopicon

[Registry]
#ifdef BootstrapKey
Root: HKCU; Subkey: "Environment"; ValueType: string; ValueName: "{#BootstrapEnvironmentName}"; ValueData: "{#BootstrapKey}"; Flags: preservestringtype uninsdeletevalue
Root: HKCU; Subkey: "Software\Avensys\SSW\TechnicalSelection"; ValueType: string; ValueName: "{#BootstrapRegistryValueName}"; ValueData: "{#BootstrapKey}"; Flags: uninsdeletevalue
#endif

[Run]
Filename: "{app}\{#AppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(AppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent
