using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using DynamicBackground.Services.Abstractions;
using DynamicBackground.Infrastructure;

namespace DynamicBackground.Services
{
    /// <summary>
    /// Settings service using JSON file storage.
    /// </summary>
    public class SettingsService : ISettingsService
    {
        private readonly string _settingsFilePath;
        private Dictionary<string, string>? _cache;

        public SettingsService(string settingsFilePath)
        {
            _settingsFilePath = settingsFilePath;
            EnsureSettingsFile();
        }

        public string? GetSetting(string key)
        {
            try
            {
                var settings = LoadSettings();
                return settings.TryGetValue(key, out var value) ? value : null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting setting: {ex}");
                return null;
            }
        }

        public void SetSetting(string key, string value)
        {
            try
            {
                var settings = LoadSettings();
                settings[key] = value;
                SaveSettings(settings);
                _cache = null; // Invalidate cache
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error setting: {ex}");
                throw;
            }
        }

        public int GetSettingAsInt(string key, int defaultValue = 0)
        {
            var value = GetSetting(key);
            if (!int.TryParse(value, out var result))
            {
                return defaultValue;
            }

            // Validate against app constants
            if (key == AppConstants.SETTINGS_KEY_STARTUP_DELAY)
            {
                result = Math.Max(AppConstants.MIN_STARTUP_DELAY_SECONDS, result);
                result = Math.Min(AppConstants.MAX_STARTUP_DELAY_SECONDS, result);
            }

            return result;
        }

        public void SetSetting(string key, int value)
        {
            if (key == AppConstants.SETTINGS_KEY_STARTUP_DELAY)
            {
                value = Math.Max(AppConstants.MIN_STARTUP_DELAY_SECONDS, value);
                value = Math.Min(AppConstants.MAX_STARTUP_DELAY_SECONDS, value);
            }
            SetSetting(key, value.ToString());
        }

        private Dictionary<string, string> LoadSettings()
        {
            if (_cache != null)
                return _cache;

            if (!File.Exists(_settingsFilePath))
                return new Dictionary<string, string>();

            try
            {
                var json = File.ReadAllText(_settingsFilePath);
                _cache = JsonConvert.DeserializeObject<Dictionary<string, string>>(json) 
                    ?? new Dictionary<string, string>();
                return _cache;
            }
            catch
            {
                return new Dictionary<string, string>();
            }
        }

        private void SaveSettings(Dictionary<string, string> settings)
        {
            try
            {
                var json = JsonConvert.SerializeObject(settings, Formatting.Indented);
                File.WriteAllText(_settingsFilePath, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving settings: {ex}");
                throw;
            }
        }

        private void EnsureSettingsFile()
        {
            try
            {
                if (!File.Exists(_settingsFilePath))
                {
                    var defaults = new Dictionary<string, string>
                    {
                        { "ImgSaveLoc", Path.Combine(
                            Environment.GetFolderPath(Environment.SpecialFolder.MyPictures),
                            "Bing Backgrounds", DateTime.Now.Year.ToString()) },
                        { "Interval", "720" },
                        { AppConstants.SETTINGS_KEY_STARTUP_DELAY, AppConstants.DEFAULT_STARTUP_DELAY_SECONDS.ToString() }
                    };
                    SaveSettings(defaults);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error ensuring settings file: {ex}");
            }
        }
    }
}
