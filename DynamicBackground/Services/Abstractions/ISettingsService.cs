namespace DynamicBackground.Services.Abstractions
{
    /// <summary>
    /// Service for managing application settings.
    /// </summary>
    public interface ISettingsService
    {
        /// <summary>
        /// Gets a setting value by key. Returns null if not found.
        /// </summary>
        string? GetSetting(string key);

        /// <summary>
        /// Sets a setting value.
        /// </summary>
        void SetSetting(string key, string value);

        /// <summary>
        /// Gets a setting as integer with default fallback.
        /// </summary>
        int GetSettingAsInt(string key, int defaultValue = 0);

        /// <summary>
        /// Sets a setting from integer value.
        /// </summary>
        void SetSetting(string key, int value);
    }
}
