# FanControl.WatchTower

A portable Windows program that configures a family of [FanControl](https://github.com/Rem0o/FanControl.Releases) monitoring plugins (CurveReader, RemoteSensors and DataServer) and can also record the data of a remote FanControl into a local log.

WatchTower never sends commands to FanControl and never changes any fan speed. It only reads FanControl's profile files and writes the configuration files of the plugins.

## What it does

- Configures [FanControl.CurveReader](https://github.com/JesusRomero77/FanControl.CurveReader): which sensors and fan controls of each profile are logged, how often and how much their value must change.
- Configures [FanControl.RemoteSensors](https://github.com/JesusRomero77/FanControl.RemoteSensors): the name or IP address, the port and the password of the remote computer.
- Configures [FanControl.DataServer](https://github.com/JesusRomero77/FanControl.DataServer): the port and the password of the server.
- Records the data that DataServer publishes on another computer into a local log file, with the same per-element settings as CurveReader (the RemoteSensorLogger tab). This tab does not need FanControl on the computer where WatchTower runs.
- One program, one **File > Save** that saves all the tabs. Only the values you changed are written; the rest of each file is kept.
- Interface in English and Spanish (Settings > Language). The choice is remembered.

## Requirements

- Windows with .NET Framework 4.8.
- The CurveReader, RemoteSensors and DataServer tabs configure plugins, so FanControl (the .NET Framework 4.8 version) and the matching plugin must be installed on the same computer as WatchTower:
  - FanControl.CurveReader **2.3 or later**
  - FanControl.RemoteSensors **2.0 or later**
  - FanControl.DataServer **2.0 or later**
- The RemoteSensorLogger tab only needs network access to a computer running FanControl with DataServer 2.0 or later.

Run WatchTower with the same Windows user that runs FanControl: the plugins read their configuration from that user's `%LocalAppData%\FanControl` folder.

## Installation

WatchTower is portable: it is a single `WatchTower.exe` with no installer. Download it from the Releases page and run it from any folder.

## Getting started

1. Run `WatchTower.exe`.
2. If you will use the CurveReader tab, open **Settings > Specify FanControl path** and select `FanControl.exe` (or a shortcut to it). WatchTower finds your profiles from there and remembers the path.
3. Choose a tab, change what you need and use **File > Save**. When you close the program with unsaved changes, it asks whether to save them.
4. Restart the FanControl interface so the plugins read the new configuration. The plugins only read their files when they start (CurveReader also when the active profile changes).

## The tabs

### CurveReader

Choose a profile in the dropdown (the active FanControl profile is selected by default). For every control and sensor of the profile you can set:

- **Log**: whether it is recorded or not.
- **Interval (sec)**: seconds (1 to 3600) the element waits after being recorded before it is checked again.
- **Minimum change**: how much the value must differ from the last recorded one (0 to 10) to be recorded again. With 0, the element is recorded every time its interval passes, even if its value has not changed.

The first reading of each element is always recorded. Curves are listed in the profile but not logged. Settings are saved per profile in `CurveReaderLoggerConfig.xml`. The language chosen in WatchTower is saved in the same file and used by the plugin to write its log.

### RemoteSensors

Settings of the plugin that shows another computer's sensors inside this FanControl:

- **Remote PC name** and/or **Remote PC IP**: one of the two is enough. If both are filled in, the IP is used.
- **Remote PC port**: the port DataServer listens on at the remote computer. Leave it empty for the default (8765).
- **Password**: it must be the same as the one set in DataServer.

The tab shows the same warning as the plugin: **remote sensors are for monitoring only.** Never use them in curves or controls of the computer where RemoteSensors is installed. Settings are saved in `RemoteSensorsConfig.xml`.

### DataServer

Settings of the plugin that publishes this computer's sensors and controls over HTTP. Use this tab on the computer where DataServer is installed.

- **Port**: between 1 and 65535. Leave it empty for the default (8765).
- **Password**: the field shows the current password in grey; leave it empty to keep it, or type a new one to change it.

If WatchTower knows where FanControl is and `FanControl.DataServer.dll` is not in its `Plugins` folder, the tab warns you. Settings are saved in `DataServerConfig.xml`.

### RemoteSensorLogger

Records the data of a remote DataServer into a log on this computer.

1. Fill in the remote PC name or IP, the port (empty for 8765) and the password.
2. Press the refresh button to get the active profile of the remote computer. Its controls and sensors appear below with the same Log / Interval / Minimum change settings as in CurveReader (without curves).
3. Press **REC** to start recording and **STOP** to finish. The status line shows the file being written and any problem.

Things to know:

- The log is written to `%LocalAppData%\FanControl\RemoteSensorLogger-<IP or name>.log`, with one file per remote computer. It has the same format as the CurveReader log: a summary of the profile followed by one line per record, with the time of this computer. It is written in the language chosen in WatchTower.
- The file is limited to 100 MB. When the limit is reached, the oldest records are removed until 20% is free.
- You can change the settings of the items while recording; the change is applied immediately. The connection boxes are locked until you stop.
- If the remote computer cannot be reached, WatchTower keeps retrying. If the password is wrong, the recording stops. If the readings stop updating, the status line says so (the FanControl interface must be open on the remote computer).
- When the recording ends, an end-of-log marker is written. If you record the same computer again, the marker is removed and the file continues as a single log.
- It records one remote computer at a time and only while WatchTower is open. Closing the program stops the recording.

Settings are saved in `RemoteSensorLoggerConfig.xml`: the connection and the settings of each remote profile.

## Typical setup with two computers

1. On computer A (the one to monitor), install FanControl and the DataServer plugin. In the DataServer tab, choose a port (or leave it empty) and **change the default password**.
2. On computer A, allow the port in the firewall and give permission to listen on the network (see the DataServer README).
3. On computer B, install the RemoteSensors plugin. In the RemoteSensors tab, enter computer A's name or IP and the same port and password.
4. Restart the FanControl interface on both computers. The sensors of computer A appear on computer B as `REMOTE <name>`.
5. To keep a log of computer A on computer B instead, use the RemoteSensorLogger tab.

## Files

WatchTower reads and writes these files in `%LocalAppData%\FanControl`:

| File | Used for |
|------|----------|
| `CurveReaderLoggerConfig.xml` | CurveReader settings, read by the plugin |
| `RemoteSensorsConfig.xml` | RemoteSensors settings, read by the plugin |
| `DataServerConfig.xml` | DataServer settings, read by the plugin |
| `RemoteSensorLoggerConfig.xml` | Connection and settings of the remote logger (WatchTower only) |
| `WatchTowerSettings.xml` | FanControl folder and language of WatchTower |
| `RemoteSensorLogger-<IP or name>.log` | Logs recorded from a remote DataServer |

FanControl's own files are only read, never modified.

## Security

- **Passwords are stored as plain text** in the configuration files above, and the RemoteSensorLogger password is shown in clear on screen. Over the network the password is never sent: the plugins use a signed challenge (HMAC-SHA256).
- The default password (`Password1234`) is public. Change it in DataServer and RemoteSensors before opening the port to other computers.
- Passwords can only contain visible ASCII characters, with no spaces or accented characters.
- The traffic between computers is plain HTTP, not encrypted. Use WatchTower and the plugins only on networks you trust and never expose the DataServer port to the internet. See the DataServer README for details.

## Known limitations

- The plugins only read their configuration when they start, so changes need a restart of the FanControl interface.
- DataServer and the remote logger only get up-to-date data while the FanControl interface is open on the monitored computer.
- Curves are not recorded in the logs yet.
- Only the custom sensors and the visible, enabled controls of the remote profile are available, because that is what DataServer serves.

## Building from source

Open `Source/WatchTower.slnx` (or `Source/WatchTower.csproj`) in Visual Studio with the .NET desktop development workload and the .NET Framework 4.8 targeting pack. Build in Release for x64. NuGet restores Costura.Fody (which embeds Newtonsoft.Json into the single `.exe`), Fody and Newtonsoft.Json.

To add a language, add a class that inherits from `Language` in `Language.cs`; WatchTower finds it by itself and adds it to the language menu. To make the CurveReader log use it as well, the same class must be added to the plugin's `Language.cs`.

## Related projects

- [FanControl.CurveReader](https://github.com/JesusRomero77/FanControl.CurveReader): a FanControl plugin that logs local temperature and PWM changes to a file.
- [FanControl.DataServer](https://github.com/JesusRomero77/FanControl.DataServer): a FanControl plugin that serves the sensors and controls of the active profile over HTTP.
- [FanControl.RemoteSensors](https://github.com/JesusRomero77/FanControl.RemoteSensors): a FanControl plugin that shows the sensors of another computer inside FanControl.

## Disclaimer

This project is not affiliated with FanControl or its author.

## License

This project is licensed under the [MIT License](LICENSE).

WatchTower embeds [Newtonsoft.Json](https://www.newtonsoft.com/json) (Copyright © James Newton-King), used under the MIT License. FanControl and the libraries it provides to plugins are not part of this repository and remain under their own licenses.

## Version

1.0
