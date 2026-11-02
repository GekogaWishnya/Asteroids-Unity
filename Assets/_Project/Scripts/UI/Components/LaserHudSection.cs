using Asteroids.Core;
using UnityEngine;
using Zenject;

namespace Asteroids.UI.Components
{
    /// <summary>The laser part of a HUD. Hides itself when the loadout has no laser.</summary>
    [DisallowMultipleComponent]
    [AddComponentMenu("Asteroids/UI/Laser Hud Section")]
    public sealed class LaserHudSection : MonoBehaviour
    {
        [Inject]
        private void Construct([InjectOptional] ILaserStatus laser)
        {
            if (laser == null) gameObject.SetActive(false);
        }
    }
}
