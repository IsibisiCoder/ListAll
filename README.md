# ListAll

ListAll is a program that reads file or directory names with their properties and saves them in a file as a list.  

__Prerequisite__:  
Microsoft Framework [.Net Runtime 8.0](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)  

The basic functions are:  
* Reading in file or directory names of one or more configurable directories
* Reading can be recursive if required, the directory depth is configurable
* Instead of the files within a directory, only the directory names can be taken into account
* An additional `Setting` file can be used to configure a header and also the properties to be output
* Both a csv and a markdown file can be created. Several header lines can also be entered in a markdown file
* If required, an md5 hash can be calculated per file

In addition to the basic functions, there are also so-called `MediaPlugins` to which the file name is passed and which can then read out further special properties.  
The following `MediaPlugins` are currently available:  
* MediaInfo - Reading video properties with the help of the `Mediainfo` tool. An additional installation is required for this plugin, see below.  

The program can be started quickly and easily using command line parameters.  
In addition, all commandline parameters can also be configured in the `Settings` file. More extensive setting options are also available here.  

## CommandLine parameters when calling

Example:  

```
dotnet listall.dll --outputfile "c:\\temp" --setting "Settings\\my-csv.json\" -r -d "c:\\temp\files"
```

Parameter|meaning|example
--|--
-o --outputfile|The files or directories found are written to this file|myfiles.csv
-s --setting|The path incl. file name to the setting file "Settings\\my-csv.json"
-r --recursive||Should the directory structure be searched recursively|true or false
-e --extensions|Restriction to file changes|Under Windows with specification of *., i.e. *.txt. Several extensions are written one after the other: *.txt *.pdf
-d --rootdir|Start directory for the file search|"c:\\temp"

## Setting file

Further parameters for the file search are specified in the setting file.  
Example:  

```
{
  "outputfile": "c:\\temp\\myfiles.csv",
  "source-directories": [
    {
      "directory": "c:\\temp"
    }
  ],
  "onlydir": false,
  "filename-with-extension": false,
  "extensions": ".mp4 .mkv .avi .flv .wmv .mov .mpg .mpeg .asf .webm",
  "folder-reverse": false,
  "md5-hash": true,
  "recursive": true,
  "recursive-depth": -1,
  "output": "%Filename%;%FileExtention%;%FileSize%;%CreationTime%;%folder1%;%folder2%;%folder3%;%Md5Hash%;%Path%",
  "headers": [
    {
      "header": "Filename;Ext;Size;Date;Folder1;Folder2;Folder3;Md5Hash;Path"
    }
  ],
  "mediaplugin": ""
}
```

Parameter|Meaning|Example
--|--
onlydir|Only the directory names should be output, not the names of the individual files|true or false
filename-with-extension|The file extension should also be output with the file names|true or false
extensions|The following file extensions should be taken into account. If file extensions have already been transferred with the command line parameters, these are also added to the list. A check for double endings is __not__|*.mp4 *.mkv
folder-reverse|The names of the directories in which a file is located can be output via `folder1`, `folder2` etc. (see `output`). (see `output`). This setting reverses the output, i.e. in `folder1` the direct directory name is output instead of the drive letter (windows) or the mount piont|true or false
md5-hash|The unique md5-hash is calculated for each file|true or false
recursive|If the directory structure is to be searched recursively, this value overwrites the value with the same name from the CommandLine parameter|true or false
recursive-depth|If `recursive` = true, then the maximum level can be set here.|-1 = no restriction, 0 = only the specified directory, 1 = one level depth, etc.
output|This entry specifies the columns to be output. The names must correspond exactly to the specified pattern, as these placeholders are replaced with the values in the program using Replace.|Filename;FileExtention;FileSize
headers|The header line is specified here, which is output first of all. If the file to be written already exists, the search results are appended to the existing content. This header is then not written. "header": "Filename;Ext;Size
mediaplugin|This setting can be used to include a separate MediaPlugin in the output|Name of the plugin
outputfile|The files or directories found are written to this file; if the value is specified in the settings file, the value with the same name from the CommandLine parameter is overwritten|myfiles.csv
source-directories|Additional directories can be added via this parameter|c:\temp

## Header line

The header line is the first thing to be output for new files. If the file to be written already exists, the search results are appended to the existing content. This header is then not written.

## Output

The output specification specifies the columns to be output. The names must correspond exactly to the specified pattern, as these placeholders are replaced with the values in the program using Replace. In addition, the parameters must always be enclosed with `%`, e.g. `%Filename`.  
 
There are the following placeholders:  

Placeholder|Meaning
--|--
Filename|Filename, if `filename-with-extension` = `false`, then without file extension
FileExtention|File extension without `*`, only with a dot and the extension, e.g. `.txt`.
FileSize|Size
CreationTime|Creation date
folder1,folder2,folder3 etc.|
Md5Hash|calculated md5-hash value, if the parameter `md5-hash` is set to `true
Path|complete path of the file

## mediaplugin

File properties can be determined and output using so-called MediaPlugins.  
Further special placeholders must then be entered in `output` for the output.

### Plugin: MediaInfo

The plugin `MediaInfo` passes the file name to the image library of the program of the same name. This library then determines further properties for a video:  

Placeholder|Meaning
--|--
duration|length in milliseconds 
inhours:inminutes:inseconds|The duration is broken down into hours, minutes and seconds and can be output using these placeholders
width|width
height|height
framerate|frame rate
aspectRatio|e.g. fullscreen or widescreen
codec|Codec, if available
format|
audioCodec|
audioSampleRate|
audioChannelsFriendly|
videoCodec|
videoRate|
videoResolution|

To use the plugin, the following must be observed during installation:  
From the installation of the MediaInfo program, the libraries must be copied from the program directory into a subdirectory of ListAll.  
Example Windows:
```
x64\MediaInfo.dll
```
or for older systems (still 32 bit system):  
```
x86\MediaInfo.dll
```

The following value must be entered in the settings:  
```
"mediaplugin": "mediainfo"
```

# Libraries and licenses used

The following libraries are used to access MediaInfo:  

[MP-MediaInfo](https://github.com/yartat/MP-MediaInfo) is .NET wrapper for MediaArea MediaInfo by Yaroslav Tatarenko and use the native packages `MediaInfo.Core.Native` and `MediaInfo.Core.Native`.  
The library `MP-MediaInfo` is licensed under the `BSD License`.  


*** Translated with www.DeepL.com/Translator (free version) ***

---
ListAll ist ein Programm, dass Datei- oder Verzeichnisnamen mit seinen Eigenschaften einliest und in einer Datei als Liste speichert.  

__Voraussetzung__:  
Microsoft Framework [.Net Runtime 8.0](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)  

Die Grundfunktionen sind:  
* Einlesen von Datei- oder Verzeichnisnamen eines oder mehrerer konfigurierbarer Verzeichnisse
* Das Einlesen kann bei Bedarf rekursiv erfolgen, die Verzeichnistiefe ist konfigurierbar
* Statt der Dateien innerhalb eines Verzeichnisses können auch nur die Verzeichnisnamen berücksichtigt werden
* Über eine zusätzliche `Setting`-Datei kann ein Header und auch die auszugebenen Eigenschaften konfigueriert werden
* Es kann sowohl eine csv, als auch eine markdown-Datei erstellt werden. Bei einer Markdown-Datei können auch mehrere Headerzeilen erfasst werden
* Bei Bedarf kann ein md5 hash pro Datei berechnet werden

Neben den Grundfunktionen gibt es auch so genannte `MediaPlugins`, an die der Dateiname weitergegeben wird und die dann weitere spezielle Eigenschaften auslesen können.  
Es gibt zurzeit folgende `MediaPlugins`:  
* MediaInfo - Auslesen von Videoeigenschaften mit Hilfe des Tools `Mediainfo`. Für dieses Plugin ist eine weitere Installation notwendig, siehe unten.  

Das Programm kann per Commandline-Parameter schnell und einfach gestartet werden.  
Darüberhinaus können alle Commandline-Parameter auch in der `Settings`-Datei konfiguriert werden. Hier sind auch weitere umfangreichere Einstellungsmöglichkeiten vorhanden.  

## CommandLine-Parameter beim Aufruf

Beispiel:  

```
dotnet listall.dll --outputfile "c:\\temp" --setting "Settings\\my-csv.json\" -r -d "c:\\temp\files"
```

Parameter|Bedeutung|Beispiel
--|--
-o --outputfile|Die gefundenen Dateien oder Verzeichnisse werden in diese Datei geschrieben|myfiles.csv
-s --setting|Der Pfad inkl. Dateiname zur Settingdatei.|"Settings\\my-csv.json"
-r --recursive||Soll die Verzeichnisstruktur rekursiv durchsucht werden|true oder false
-e --extensions|Einschränkung auf Dateiänderungen|Unter Windows mit Angabe von *., also *.txt. Mehrere Extensions werden hintereinander geschrieben: *.txt *.pdf
-d --rootdir|Startverzeichnis für die Dateisuche|"c:\\temp"

## Settingdatei

In der Settingdatei werden weitere Parameter für die Dateisuche angegeben.  
Beispiel:  

```
{
  "outputfile": "c:\\temp\\myfiles.csv",
  "source-directories": [
    {
      "directory": "c:\\temp"
    }
  ],
  "onlydir": false,
  "filename-with-extension": false,
  "extensions": ".mp4 .mkv .avi .flv .wmv .mov .mpg .mpeg .asf .webm",
  "folder-reverse": false,
  "md5-hash": true,
  "recursive": true,
  "recursive-depth": -1,
  "output": "%Filename%;%FileExtention%;%FileSize%;%CreationTime%;%folder1%;%folder2%;%folder3%;%Md5Hash%;%Path%",
  "headers": [
    {
      "header": "Filename;Ext;Size;Date;Folder1;Folder2;Folder3;Md5Hash;Path"
    }
  ],
  "mediaplugin": ""
}
```

Parameter|Bedeutung|Beispiel
--|--
onlydir|Es sollen nur die Verzeichnisnamen ausgegeben werden, nicht die Namen der einzelnen Dateien|true oder false
filename-with-extension|Bei den Dateinamen soll die Dateiendung mit ausgegeben werden|true oder false
extensions|Folgende Dateiendungen sollen berücksichtigt werden. Wurden bereits Dateiendungen mit den Commandline-Parameter übergeben, so werden diese zusätzlich zu der Liste hinzugefügt. Ein Prüfung auf doppelte Endungen erfolgt __nicht__|*.mp4 *.mkv
folder-reverse|Die Namen der Verzeichnisse, in dem sich eine Datei befindet, kann über `folder1`, `folder2` usw. ausgegeben werden. (siehe `output`). Über diese Einstellung wird die Ausgabe umgedreht, d.h. in `folder1` wird der direkte Verzeichnisname statt dem Laufwerksbuchstaben (windows) oder dem Mountpiont ausgegeben|true oder false
md5-hash|Für jede Datei wird der eindeutige md5-hash berechnet|true oder false
recursive|Soll die Verzeichnisstruktur rekursiv durchsucht werden, dieser Wert überschreibt den gleichnamiger Wert aus dem CommandLine-Parameter|true oder false
recursive-depth|Wenn `recursive` = true, dann kann hier die max. Ebene eingestellt werden.|-1 = keine Einschränkung, 0 = nur das angegebene Verzeichnis, 1 = eine Ebene Tiefe usw.
output|Diese Angabe spezifiziert die auszugegebenen Spalten. Die Namen müssen exakt den vorgegebem Muster entsprechen, da diese Platzhalter im Programm per Replace mit den Werten ersetzt werden.|Filename;FileExtention;FileSize
headers|Hier wird die Kopfzeile angegeben, die als allererstes ausgegeben wird. Existiert die zu schreibende Datei schon, werden die Suchergebnisse an den vorhandenen Inhalt angehängt. Ein Schreiben dieser Kopfzeile erfolgt dann nicht.|"header": "Filename;Ext;Size
mediaplugin|Über diese Einstellung kann ein seperates MediaPlugin in die Ausgabe eingebunden werden|Name des Plugins
outputfile|Die gefundenen Dateien oder Verzeichnisse werden in diese Datei geschrieben, wird der Wert in der Settingsdatei angegeben, wird der gleichnamiger Wert aus dem CommandLine-Parameter überschrieben|myfiles.csv
source-directories|Weitere Verzeichnisses können über diesen Parameter hinzugefügt werden|c:\temp

## Kopfzeile

Die Kopfzeile wird bei neuen Dateien als allererstes ausgegeben. Existiert die zu schreibende Datei schon, werden die Suchergebnisse an den vorhandenen Inhalt angehängt. Ein Schreiben dieser Kopfzeile erfolgt dann nicht.

## Output

Die Output-Angabe spezifiziert die auszugegebenen Spalten. Die Namen müssen exakt den vorgegebem Muster entsprechen, da diese Platzhalter im Programm per Replace mit den Werten ersetzt werden. Außerdem müssen die Parameter immer mit `%` umfasst werden, also z.B. `%Filename`.  
 
Es gibt folgende Platzhalter:  

Platzhalter|Bedeutung
--|--
Filename|Dateinamen, wenn `filename-with-extension` = `false`, dann ohne Dateiendung
FileExtention|Dateiendung ohne `*`, nur mit einem Punkt und der Endung, also z.B. `.txt`.
FileSize|Größe
CreationTime|Erzeugungsdatum
folder1,folder2,folder3 etc.|
Md5Hash|berechneter md5-hash-Wert, sofern der Parameter `md5-hash` auf `true` steht
Path|kompletter Pfad der Datei

## mediaplugin

Mittels so genannter MediaPlugins können werden Dateieigenschaften ermittelt und ausgegeben werden.  
Für die Ausgabe müssen dann in `output` wiederum weitere spezielle Platzhalter eingetragen werden.

### Plugin: MediaInfo

Das Plugin `MediaInfo` gibt den Dateinamen an die Bilbiothek des gleichnamigen Programms weiter. Diese Bibliothek ermittelt dann für ein Video weitere Eigenschaften:  

Platzhalter|Bedeutung
--|--
duration|Länge in Millisekunden 
inhours:inminutes:inseconds|Die Duration wird in Stunden, Minuten, Sekunden zerlegt und kann über diese Platzhalter ausgegeben werden
width|Breite
height|Höhe
framerate|Framerate
aspectRatio|z.B. fullscreen oder widescreen
codec|Codec, wenn vorhanden
format|
audioCodec|
audioSampleRate|
audioChannelsFriendly|
videoCodec|
videoRate|
videoResolution|

Um das Plugin zu nutzen, muss man bei der Installation folgendes beachten:  
Aus der Installation des Programms MediaInfo muss aus dem Programmverzeichnis die Bibliotheken in ein Unterverzeichnis von ListAll kopiert werden.  
Beipsiel Windows:
```
x64\MediaInfo.dll
```
oder bei älteren Systemen (noch 32 Bit-System):  
```
x86\MediaInfo.dll
```

In den Settings muss folgender Wert eingetragen werden:  
```
"mediaplugin": "mediainfo"
```

# Verwendete Bibliotheken und Lizenzen

Für den Zugriff auf MediaInfo werden folndene Bibliotheken verwendet:  

[MP-MediaInfo](https://github.com/yartat/MP-MediaInfo) is .NET wrapper for MediaArea MediaInfo by Yaroslav Tatarenko and use the native packages `MediaInfo.Core.Native` and `MediaInfo.Core.Native`.  
Die Bibliothek `MP-MediaInfo` steht unter der `BSD License`.  

