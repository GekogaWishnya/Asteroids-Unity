using Asteroids.Core;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;
using Zenject;

namespace Asteroids.World
{
    [RequireComponent(typeof(Camera), typeof(PixelPerfectCamera))]
    public sealed class WorldCamera : MonoBehaviour
    {
        [Inject]
        private void Construct(IWorldLayout layout, IWorldAppearance appearance)
        {
            var ppc = GetComponent<PixelPerfectCamera>();
            ppc.assetsPPU = layout.PixelsPerUnit;
            ppc.refResolutionX = layout.ReferenceResolution.x;
            ppc.refResolutionY = layout.ReferenceResolution.y;
            
            var mainCamera = GetComponent<Camera>();
            mainCamera.backgroundColor = appearance.BackgroundColor;
        }
    }
}
