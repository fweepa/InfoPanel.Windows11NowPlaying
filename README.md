# Windows 11 Now Playing Plugin for InfoPanel

This plugin displays currently playing media information from Windows 11's Global Media Transport Controls (GMTC) in InfoPanel.

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

MIT License

Copyright (c) 2025 fweepa

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
