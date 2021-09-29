[Setup]
; NOTE: The value of AppId uniquely identifies this application.
; Do not use the same AppId value in installers for other applications.
; (To generate a new GUID, click Tools | Generate GUID inside the IDE.)
AppId={{a8515e18-85b1-4271-b823-842272b6c69c}
AppName="WsFileSystemMonitor"
AppVersion="0.20.1207.2211"
AppVerName="Serviço de Monitoramento e Transferência de Arquivos"
AppCopyright="Copyright © Tecfy 2020"
AppPublisher="Tecfy"
AppPublisherURL="http://www.tecfy.com.br"
AppSupportURL="http://www.tecfy.com.br"
DefaultDirName="{commonpf}\Tecfy\WsFileSystemMonitor"
DefaultGroupName="Tecfy"
OutputDir="Setup"
OutputBaseFilename="WsFileSystemMonitor"
Compression=lzma
SolidCompression=yes
VersionInfoVersion="0.20.1207.2211"
ArchitecturesInstallIn64BitMode=x64

[Languages]
Name: "brazilianportuguese"; MessagesFile: "compiler:Languages\BrazilianPortuguese.isl"

[Files]
Source: "WsFileSystemMonitor\bin\Release\*.config"; DestDir: "{app}"; Flags: onlyifdoesntexist
Source: "WsFileSystemMonitor\bin\Release\*.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "WsFileSystemMonitor\bin\Release\*.dll"; DestDir: "{app}"; Flags: ignoreversion

[Run]
Filename: "{app}\WsFileSystemMonitor.exe"; Parameters: "--install"; 