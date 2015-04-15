using System.Reflection;
using ColossalFramework;
using ColossalFramework.UI;
using UnityEngine;

namespace Sapphire
{
    public class SetCameraRectHelper
    {

        private static RedirectCallsState cameraControllerRedirect;
        private static bool cameraControllerRedirected = false;
        private static FieldInfo cachedFreeCameraField;

        private static Rect cameraRect = new Rect(0f, 0.105f, 1f, 0.895f);

        public static Rect CameraRect
        {
            get { return cameraRect; }
            set { cameraRect = value; }
        }

        public static void ResetCameraRect()
        {
            cameraRect = new Rect(0f, 0.105f, 1f, 0.895f);
        }
        
        public static void Initialize()
        {
            if (cameraControllerRedirected)
            {
                return;
            }

            ResetCameraRect();

            cachedFreeCameraField = typeof(CameraController).GetField("m_cachedFreeCamera", BindingFlags.Instance | BindingFlags.NonPublic);

            var cameraController = GameObject.FindObjectOfType<CameraController>();
            if (cameraController != null)
            {
                cameraControllerRedirect = RedirectionHelper.RedirectCalls(
                    typeof(CameraController).GetMethod("UpdateFreeCamera",
                        BindingFlags.Instance | BindingFlags.NonPublic),
                    typeof(SetCameraRectHelper).GetMethod("UpdateFreeCamera",
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

            cameraControllerRedirected = false;
        }


        private void UpdateFreeCamera()
        {
            var cameraController = GameObject.FindObjectOfType<CameraController>();

            if (cameraController == null)
            {
                return;
            }
            
            if (cachedFreeCameraField == null)
            {
                return;
            }

            var camera = cameraController.GetComponent<Camera>();

            if (cameraController.m_freeCamera != (bool)cachedFreeCameraField.GetValue(cameraController))
            {
                cachedFreeCameraField.SetValue(cameraController, cameraController.m_freeCamera);
                UIView.Show(!cameraController.m_freeCamera);
                NotificationManager.instance.NotificationsVisible = !cameraController.m_freeCamera;
                GameAreaManager.instance.BordersVisible = !cameraController.m_freeCamera;
                DistrictManager.instance.NamesVisible = !cameraController.m_freeCamera;
                PropManager.instance.MarkersVisible = !cameraController.m_freeCamera;
                GuideManager.instance.TutorialDisabled = cameraController.m_freeCamera;
            }
            if ((bool)cachedFreeCameraField.GetValue(cameraController))
            {
                camera.rect = new Rect(0f, 0f, 1f, 1f);
            }
            else
            {
                camera.rect = cameraRect;
            }
        }

    }

}
