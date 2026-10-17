using Asteroids.Core;
using Asteroids.Core.IConfigs;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;
using Zenject;

namespace Asteroids.World
{
    [RequireComponent(typeof(Camera), typeof(PixelPerfectCamera))]
    public sealed class WorldCamera : MonoBehaviour
    {
        [Inject]
        private void Construct(IViewLayout view, IWorldAppearance appearance)
        {
            var ppc = GetComponent<PixelPerfectCamera>();
            ppc.assetsPPU = view.PixelsPerUnit;
            ppc.refResolutionX = view.Resolution.x;
            ppc.refResolutionY = view.Resolution.y;
            
            var offset = view.CameraOffset;
            var current = transform;
            current.position = new Vector3(offset.x, offset.y, current.position.z);
            
            var mainCamera = GetComponent<Camera>();
            mainCamera.backgroundColor = appearance.BackgroundColor;
        }
    }
}
