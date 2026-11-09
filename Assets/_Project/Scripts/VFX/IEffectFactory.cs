using System;
using Asteroids.Core.IConfigs;

namespace Asteroids.VFX
{
    public interface IEffectFactory
    {
        /// <exception cref="InvalidOperationException">The effect cannot be made, for example its asset is broken.</exception>
        IEffect Create(IEffectConfig config);
    }
}