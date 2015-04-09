using UnityEngine;

namespace Sapphire
{

    public class CameraHook : MonoBehaviour
    {

        private Camera camera;
        private Rect originalRect;

        void Start()
        {
            camera = GetComponent<Camera>();
            originalRect = camera.rect;
            camera.rect = new Rect(0.0f, 0.0f, 1.0f, 1.0f);
        }

        void OnPreCull()
        {
            camera.rect = new Rect(0.0f, 0.0f, 1.0f, 1.0f);
        }

        void OnPostRender()
        {
        }

    }

}
