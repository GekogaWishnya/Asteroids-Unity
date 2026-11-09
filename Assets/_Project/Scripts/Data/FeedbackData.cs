using System;
using Asteroids.Core.IConfigs;
using UnityEngine;

namespace Asteroids.Data
{
    [Serializable]
    public sealed class FeedbackData : IFeedbackConfig
    {
        [Tooltip("Played where it was when it loses health. Optional")]
        [SerializeField] private EffectDefinition hit;
        [Tooltip("Also play the hit effect for the hit that destroys it, on top of the death effect")]
        [SerializeField] private bool playHitOnFatal;
        [Tooltip("Played where it was destroyed. Optional")]
        [SerializeField] private EffectDefinition death;

        public IEffectConfig Hit => hit ? hit : null;
        public bool PlayHitOnFatal => playHitOnFatal;
        public IEffectConfig Death => death ? death : null;
    }
}