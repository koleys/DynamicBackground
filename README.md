# DynamicBackground User Manual

## Overview
DynamicBackground is a Windows application (now targeting .NET 8 LTS) that automatically downloads Bing's daily background image and sets it as your desktop wallpaper. It also allows you to manually set any image as your wallpaper, choose wallpaper style, and configure automatic updates.

## Features
- Download and set Bing's daily image as wallpaper
- Set any local or online image as wallpaper
- Choose wallpaper style: Fill, Fit, Stretch, Tile, Center, Span
- Schedule automatic wallpaper updates
- Save downloaded images to a custom folder
- Error logging to Windows Event Viewer or a local log file
- Self-contained user settings (stored in `DynamicBackground.settings.json`)

## Getting Started
1. **Run the Application**
   - Launch `DynamicBackground.exe` (requires .NET 8 Desktop Runtime).

2. **Set a Local Image as Wallpaper**
   - Click `Browse` to select an image file.
   - Choose a wallpaper style from the dropdown.
   - Click `Set` to apply the wallpaper.

3. **Set Bing Image as Wallpaper**
   - Click `Set Bing Image` to download and set the latest Bing image.
   - Choose a wallpaper style from the dropdown.

4. **Schedule Automatic Updates**
   - Check the `Auto Update` box to enable scheduled updates.
   - Set the interval (in minutes) and click `Set Interval`.

5. **Change Download Location**
   - Click `Download Location` to choose where Bing images are saved.

6. **System Tray**
   - When minimized, the app hides in the system tray. Double-click the tray icon to restore.

## Error Logging
- All errors are logged to the Windows Event Viewer under the source `DynamicBackgroundApp`.
- If Event Viewer logging is not available, errors are logged to `DynamicBackground.log` in the application directory.
- To view logs, open Event Viewer (`eventvwr.msc`), navigate to `Windows Logs > Application`, and filter by source `DynamicBackgroundApp`.

## Settings Storage
- User settings are now stored in `DynamicBackground.settings.json` in the application directory for cross-platform and .NET 8 compatibility.

## Requirements
- .NET 8 Desktop Runtime (https://dotnet.microsoft.com/en-us/download/dotnet/8.0)
- Windows 10/11 recommended

## Troubleshooting
- If you encounter issues, check the Event Viewer or the local log file for error details.
- Ensure you have .NET 8 Desktop Runtime installed.
- The app may require administrator privileges to write to the Event Viewer the first time.

## Uninstall
- Delete the application files. No registry or system changes are made except for wallpaper, settings file, and log file.

## Support
For issues or feature requests, please contact the project maintainer or submit an issue on the project repository.

## Testing & Code Coverage

### Running Tests
- The project uses MSTest for unit testing. To run all tests:dotnet test- Tests cover all major code paths, including error handling and file operations.

### Code Coverage
- The test suite is designed for 100% code coverage of all public and critical private logic.
- To check code coverage, use a tool like [coverlet](https://github.com/coverlet-coverage/coverlet):dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=lcov- Review the generated coverage report to verify all code paths are tested.
