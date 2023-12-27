# ListAll

ListAll reads the directory structure and saves some selected properties in an external file.


# German description first, English is comming

Das Programm bietet einfache, schnelle Parameter per Argument und komplexere, umfangreichere Einstellungsm�glichkeiten in der Settingdatei.  

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
-e --extensions|Einschr�nkung auf Datei�nderungen|Unter Windows mit Angabe von *., also *.txt. Mehrere Extensions werden hintereinander geschrieben: *.txt *.pdf
-d --rootdir|Startverzeichnis f�r die Dateisuche|"c:\\temp"

## Settingdatei

In der Settingdatei werden weitere Parameter f�r die Dateisuche angegeben.  
Beispiel:  

```
{
  "outputfile": "c:\\temp\\listall\\xxx_f2.csv",
  "source-directories": [
    {
      "directory": "c:\temp"
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
extensions|Folgende Dateiendungen sollen ber�cksichtigt werden. Wurden bereits Dateiendungen mit den Commandline-Parameter �bergeben, so werden diese zus�tzlich zu der Liste hinzugef�gt. Ein Pr�fung auf doppelte Endungen erfolgt __nicht__|*.mp4 *.mkv
folder-reverse|Die Namen der Verzeichnisse, in dem sich eine Datei befindet, kann �ber `folder1`, `folder2` usw. ausgegeben werden. (siehe `output`). �ber diese Einstellung wird die Ausgabe umgedreht, d.h. in `folder1` wird der direkte Verzeichnisname statt dem Laufwerksbuchstaben (windows) oder dem Mountpiont ausgegeben|true oder false
md5-hash|F�r jede Datei wird der eindeutige md5-hash berechnet|true oder false
recursive|Soll die Verzeichnisstruktur rekursiv durchsucht werden, dieser Wert �berschreibt den gleichnamiger Wert aus dem CommandLine-Parameter|true oder false
recursive-depth|Wenn `recursive` = true, dann kann hier die max. Ebene eingestellt werden.|-1 = keine Einschr�nkung, 0 = nur das angegebene Verzeichnis, 1 = eine Ebene Tiefe usw.
output|Diese Angabe spezifiziert die auszugegebenen Spalten. Die Namen m�ssen exakt den vorgegebem Muster entsprechen, da diese Platzhalter im Programm per Replace mit den Werten ersetzt werden.|Filename;FileExtention;FileSize
headers|Hier wird die Kopfzeile angegeben, die als allererstes ausgegeben wird. Existiert die zu schreibende Datei schon, werden die Suchergebnisse an den vorhandenen Inhalt angeh�ngt. Ein Schreiben dieser Kopfzeile erfolgt dann nicht.|"header": "Filename;Ext;Size
mediaplugin|�ber diese Einstellung kann ein seperates MediaPlugin in die Ausgabe eingebunden werden|Name des Plugins
outputfile|Die gefundenen Dateien oder Verzeichnisse werden in diese Datei geschrieben, wird der Wert in der Settingsdatei angegeben, wird der gleichnamiger Wert aus dem CommandLine-Parameter �berschrieben|myfiles.csv
source-directories|Weitere Verzeichnisses k�nnen �ber diesen Parameter hinzugef�gt werden|c:\temp

## Kopfzeile

Die Kopfzeile wird bei neuen Dateien als allererstes ausgegeben. Existiert die zu schreibende Datei schon, werden die Suchergebnisse an den vorhandenen Inhalt angeh�ngt. Ein Schreiben dieser Kopfzeile erfolgt dann nicht.

## Output

Die Output-Angabe spezifiziert die auszugegebenen Spalten. Die Namen m�ssen exakt den vorgegebem Muster entsprechen, da diese Platzhalter im Programm per Replace mit den Werten ersetzt werden. Au�erdem m�ssen die Parameter immer mit `%` umfasst werden, also z.B. `%Filename`.  
 
Es gibt folgende Platzhalter:  

Platzhalter|Bedeutung
--|--
Filename|Dateinamen, wenn `filename-with-extension` = `false`, dann ohne Dateiendung
FileExtention|Dateiendung ohne `*`, nur mit einem Punkt und der Endung, also z.B. `.txt`.
FileSize|Gr��e
CreationTime|Erzeugungsdatum
folder1,folder2,folder3 etc.|
Md5Hash|berechneter md5-hash-Wert, sofern der Parameter `md5-hash` auf `true` steht
Path|kompletter Pfad der Datei

## mediaplugin

Mittels so genannter MediaPlugins k�nnen werden Dateieigenschaften ermittelt und ausgegeben werden.  
F�r die Ausgabe m�ssen dann in `output` wiederum weitere spezielle Platzhalter eingetragen werden.

### Plugin: MediaInfo

Das Plugin `MediaInfo` gibt den Dateinamen an die Bilbiothek des gleichnamigen Programms weiter. Diese Bibliothek ermittelt dann f�r ein Video weitere Eigenschaften:  

Platzhalter|Bedeutung
--|--
duration|L�nge in Millisekunden 
inhours:inminutes:inseconds|Die Duration wird in Stunden, Minuten, Sekunden zerlegt und kann �ber diese Platzhalter ausgegeben werden
width|Breite
height|H�he
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
oder bei �lteren Systemen (noch 32 Bit-System):  
```
x86\MediaInfo.dll
```

In den Settings muss folgender Wert eingetragen werden:  
```
"mediaplugin": "mediainfo"
```

# Verwendete Bibliotheken und Lizenzen

F�r den Zugriff auf MediaInfo werden folndene Bibliotheken verwendet:  

https://github.com/bpoxy/MP-MediaInfo MP-MediaInfo is .NET wrapper for MediaArea MediaInfo by Yaroslav Tatarenko and use the native packages `MediaInfo.Core.Native` and `MediaInfo.Core.Native`.  
Die Bibliothek `MP-MediaInfo` steht unter der `BSD License`.  

