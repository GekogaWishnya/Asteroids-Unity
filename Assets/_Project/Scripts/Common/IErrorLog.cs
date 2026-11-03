using System;

namespace Asteroids.Common
{
    /// <summary>Engine-free debug console, so pure code can report failures.</summary>
    public interface IErrorLog
    {
        /// <param name="failure">What was caught.</param>
        /// <param name="context">What failed.</param>
        void Report(Exception failure, string context);
    }
}
