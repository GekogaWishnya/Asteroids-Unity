using Asteroids.Data;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;
using Zenject;

namespace Asteroids.World
{
    [RequireComponent(typeof(PixelPerfectCamera))]
    public class WorldCamera : MonoBehaviour
    {
        private WorldConfig _config;
        
        [Inject]
        public void Construct(WorldConfig config)
        {
            _config = config;
            
            var pixelCamera = GetComponent<PixelPerfectCamera>();
            pixelCamera.refResolutionX = _config.Size.x;
            pixelCamera.refResolutionY = _config.Size.y;
            
            var mainCamera = GetComponent<Camera>();
            mainCamera.backgroundColor = _config.BackgroundColor;
        }
    }
}
