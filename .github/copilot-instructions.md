# Copilot Instructions for DynamicBackground

## Project Overview
DynamicBackground is a Windows desktop application (.NET 8 WinForms) that automatically downloads and sets Bing's daily wallpaper, with features for manual wallpaper management, style selection, and scheduled updates. The application stores user settings in a JSON file rather than the Windows registry.

## Build & Test Commands

### Build
```powershell
dotnet build
# Or for Release
dotnet build -c Release
```

### Run Tests (Full Suite)
```powershell
dotnet test
```

### Run Single Test
```powershell
dotnet test --filter "FullyQualifiedName~TestClassName.TestMethodName"
# Example: dotnet test --filter "FullyQualifiedName~DynamicBackground.Tests.PictureTests.DownloadImage_ValidUrl_SavesImage"
```

### Run Tests with Code Coverage
```powershell
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=lcov
```

### Run Application
```powershell
dotnet run --project DynamicBackground
```

## Architecture

### Core Components

**BingBackground.cs** - Bing integration and settings management
- Downloads Bing's daily image metadata via `HPImageArchive.aspx` API
- Handles screen resolution detection for optimal image quality
- Manages application settings stored in `DynamicBackground.settings.json` (JSON file format)
- Default settings: image save location (`Pictures/Bing Backgrounds/{Year}/`) and update interval (720 minutes)

**Wallpaper.cs** - Windows wallpaper registry manipulation
- Static class that interfaces with Windows Registry to set wallpaper styles
- Implements backup/restore functionality to preserve user's wallpaper history
- Supports 6 wallpaper styles: `Fill`, `Fit`, `Stretch`, `Tile`, `Center`, `Span`
- Uses P/Invoke (`SystemParametersInfo`) to apply wallpaper changes
- Maintains backup of wallpaper history (last 5 entries) in Registry

**Picture.cs** - Image download helper
- Downloads images from URLs with connection pooling and timeout handling
- Saves images using `BingBackground.GetBackgroundImagePath()` for path resolution

**Form1.cs (DynamicBackgroundUI)** - WinForms UI
- Main window minimizes to system tray on load
- Manages user interactions: file browsing, style selection, auto-update scheduling
- Binds `WallpaperStyle` enum to dropdown for style selection

**Logger.cs** - Error logging utility
- Logs errors to Windows Event Viewer under source `DynamicBackgroundApp`
- Falls back to local `DynamicBackground.log` file if Event Viewer unavailable
- All exceptions throughout codebase pass through this logger

### Key Design Patterns

**Settings Persistence**: Use `BingBackground.GetSetting(key)` / `SetSetting(key, value)` for all configuration storage. Settings are JSON-based to support cross-platform future migration.

**Error Handling**: All public methods catch exceptions and log via `Logger.LogError()`, then either throw or silently continue depending on context. No exceptions should bubble up unlogged.

**Wallpaper State Management**: `Wallpaper.BackupState()` must be called before setting wallpaper (already done in `Wallpaper.Set()`), and `Wallpaper.RestoreHistory()` clears the visual history from Windows after programmatic changes to keep History "clean".

## Testing Conventions

- Tests use MSTest framework with `[TestClass]` and `[TestMethod]` attributes
- Tests run in parallel at method level (see `MSTestSettings.cs`)
- Network-dependent tests check connectivity first and use `Assert.Inconclusive()` if unavailable
- Test assembly initialization (`AssemblyInit`) caches settings before tests run
- File I/O in tests cleans up after itself (e.g., `File.Delete(path)`)

## Settings File Format

Settings are stored in `DynamicBackground.settings.json` as a flat key-value dictionary:
```json
{
  "ImgSaveLoc": "C:\\Users\\Username\\Pictures\\Bing Backgrounds\\2026",
  "Interval": "720"
}
```

When modifying settings, use `BingBackground.SetSetting()` to ensure proper JSON serialization with `Newtonsoft.Json`.

## Important Notes

- Application requires .NET 8 Desktop Runtime (WinExe, UseWindowsForms)
- Windows Forms Designer files (`.Designer.cs`, `.resx`) should not be manually edited—use Visual Studio Designer
- Wallpaper history restoration is intentional behavior to keep Registry clean after programmatic changes
- Event Viewer logging requires admin privileges on first run; graceful fallback to file logging is built-in
- Resolution-specific image URLs from Bing may not exist; fallback to `_1920x1080.jpg` is implemented
