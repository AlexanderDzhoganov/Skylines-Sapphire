using UnityEngine;

namespace Sapphire
{

    public class CameraHook : MonoBehaviour
    {

        private Camera camera;

        void Start()
        {
            camera = GetComponent<Camera>();
        }

        void OnPreCull()
        {
            camera.rect = new Rect(0.0f, 0.0f, 1.0f, 1.0f);
        }

    }

}
