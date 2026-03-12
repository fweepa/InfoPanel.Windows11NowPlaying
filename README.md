# Windows 11 Now Playing Plugin for InfoPanel

This plugin displays currently playing media information from Windows 11's Global Media Transport Controls (GMTC) in InfoPanel.

**Releases:** Download the latest `InfoPanel.Windows11NowPlayingPlugin.zip` from the [Releases](https://github.com/fweepa/InfoPanel.Windows11NowPlaying/releases) page, or build from source with `.\pack.ps1`.

## Features

- Displays currently playing track title, artist, and album
- Shows the application that is playing media
- Displays playback position and duration
- Shows track progress as percentage (0-100) for gauge display
- Shows volume level
- Displays playback status (Playing, Paused, Stopped, etc.)

## Requirements

- .NET 8.0
- Windows 11 (for Global Media Transport Controls support)
- InfoPanel with plugin support

## Installation

### Option A: Import via InfoPanel (recommended)

1. Download the `InfoPanel.Windows11NowPlayingPlugin.zip` file and import into InfoPanel

### Option B: Manual copy

1. Build the plugin and run `.\pack.ps1`, or unzip the `InfoPanel.Windows11NowPlayingPlugin.zip` file
2. Copy the **entire** `InfoPanel.Windows11NowPlayingPlugin` folder to one of:
   - **User plugins:** `%APPDATA%\Roaming\InfoPanel\Plugins\`
   - **Development:** `[InfoPanel Install Directory]\Plugins\`
3. The folder must stay intact — InfoPanel loads plugins from subfolders. Ensure these files are inside the folder:
   - `InfoPanel.Windows11NowPlayingPlugin.dll`
   - `PluginInfo.ini`

## Development

To debug this plugin:

1. Reference the plugin project in InfoPanel.Plugins.Simulator
2. Set the simulator as the startup project
3. Run in debug mode

## Plugin Sensor IDs

- **Title:** `/windows11-now-playing/now-playing/title`
- **Artist:** `/windows11-now-playing/now-playing/artist`
- **Album:** `/windows11-now-playing/now-playing/album`
- **Application:** `/windows11-now-playing/now-playing/app-name`
- **Position:** `/windows11-now-playing/now-playing/position` (seconds)
- **Duration:** `/windows11-now-playing/now-playing/duration` (seconds)
- **Percentage:** `/windows11-now-playing/now-playing/percentage` (0-100, for gauge display)
- **Volume:** `/windows11-now-playing/now-playing/volume`
- **Status:** `/windows11-now-playing/now-playing/status`

## License

MIT License — see [LICENSE](LICENSE) for full text.
