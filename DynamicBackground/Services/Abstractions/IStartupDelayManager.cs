using System.Threading.Tasks;

namespace DynamicBackground.Services.Abstractions
{
    /// <summary>
    /// Manages startup delay behavior for Bing wallpaper operations.
    /// Ensures delays are only applied during application startup, not manual actions.
    /// </summary>
    public interface IStartupDelayManager
    {
        /// <summary>
        /// Checks if startup phase is complete.
        /// </summary>
        bool IsStartupComplete { get; }

        /// <summary>
        /// Marks startup as complete, allowing immediate operations.
        /// </summary>
        void MarkStartupComplete();

        /// <summary>
        /// Applies startup delay if startup phase is not complete.
        /// Returns immediately if startup is already complete.
        /// </summary>
        Task ApplyStartupDelayAsync();
    }
}