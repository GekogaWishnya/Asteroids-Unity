using System;
using System.Runtime.ExceptionServices;

namespace Asteroids.Common
{
    /// <summary>Keeps the first of several failures so a loop can finish every item and report afterward.</summary>
    public struct FailureCollector
    {
        private Exception _first;

        public bool HasFailed => _first != null;
        
        public void Record(Exception failure)
        {
            if (failure != null) _first ??= failure;
        }
        
        public void ThrowIfFailed()
        {
            if (_first != null) ExceptionDispatchInfo.Capture(_first).Throw();
        }
    }
}
