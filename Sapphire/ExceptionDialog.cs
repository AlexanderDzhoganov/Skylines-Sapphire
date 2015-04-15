using UnityEngine;

namespace Sapphire
{
    public class ExceptionDialog : MonoBehaviour
    {

        private Rect windowRect = new Rect(Screen.width*0.5f-256.0f, Screen.height*0.5f-128.0f, 512.0f, 256.0f);
        public string message;

        public static ExceptionDialog currentDialog;

        public static void Show(string message)
        {
            if (currentDialog != null)
            {
                return;
            }

            var go = new GameObject("SapphireExceptionDialog");
            currentDialog = go.AddComponent<ExceptionDialog>();
            currentDialog.message = message;
        }

        void OnGUI()
        {
            GUI.Window(512621, windowRect, DrawWindow, "Sapphire skin exception!");
        }

        void DrawWindow(int index)
        {
            GUILayout.Label("Sapphire has encountered an error while processing a skin.");
            GUILayout.Label("This is most likely a bug in the skin itself which you should report to its author.");
            GUILayout.Label("Copy/ paste the text below in your support issue.");

            GUILayout.TextArea(message, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            
            if (GUILayout.Button("Close"))
            {
                Destroy(gameObject);
            }

            GUILayout.EndHorizontal();
        }

    }

}
