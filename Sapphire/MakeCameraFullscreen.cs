
using System.Reflection;
using ColossalFramework;
using ColossalFramework.UI;
using UnityEngine;

namespace Sapphire
{
    public class MakeCameraFullscreen
    {

        private static RedirectCallsState cameraControllerRedirect;
        private static bool cameraControllerRedirected = false;

        public static void Initialize()
        {
            if (cameraControllerRedirected)
            {
                return;
            }

            var cameraController = GameObject.FindObjectOfType<CameraController>();
            if (cameraController != null)
            {
                cameraControllerRedirect = RedirectionHelper.RedirectCalls(
                    typeof(CameraController).GetMethod("UpdateFreeCamera",
                        BindingFlags.Instance | BindingFlags.NonPublic),
                    typeof(MakeCameraFullscreen).GetMethod("UpdateFreeCamera",
                        BindingFlags.Instance | BindingFlags.NonPublic));

                cameraControllerRedirected = true;
            }
        }

        public static void Deinitialize()
        {
            if (cameraControllerRedirected)
            {
                RedirectionHelper.RevertRedirect(typeof(CameraController).GetMethod("UpdateFreeCamera",
                        BindingFlags.Instance | BindingFlags.NonPublic), cameraControllerRedirect);
            }
        }

        private void UpdateFreeCamera()
        {
            var cameraController = GameObject.FindObjectOfType<CameraController>();

            if (cameraController == null)
            {
                return;
            }

            var cachedFreeCameraField = typeof(CameraController).GetField("m_cachedFreeCamera", BindingFlags.Instance | BindingFlags.NonPublic);
            if (cachedFreeCameraField == null)
            {
                return;
            }

            var camera = cameraController.GetComponent<Camera>();

            if (cameraController.m_freeCamera != (bool)cachedFreeCameraField.GetValue(cameraController))
            {
                cachedFreeCameraField.SetValue(cameraController, cameraController.m_freeCamera);
                UIView.Show(!cameraController.m_freeCamera);
                Singleton<NotificationManager>.instance.NotificationsVisible = !cameraController.m_freeCamera;
                Singleton<GameAreaManager>.instance.BordersVisible = !cameraController.m_freeCamera;
                Singleton<DistrictManager>.instance.NamesVisible = !cameraController.m_freeCamera;
                Singleton<PropManager>.instance.MarkersVisible = !cameraController.m_freeCamera;
                Singleton<GuideManager>.instance.TutorialDisabled = cameraController.m_freeCamera;
            }

            camera.rect = new Rect(0, 0, 1, 1);
        }

    }

}
