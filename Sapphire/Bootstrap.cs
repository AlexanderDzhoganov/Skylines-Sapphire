using System;
using UnityEngine;

namespace Sapphire
{
    public class SapphireBootstrap : MonoBehaviour
    {

        private static bool bootstrapped = false;
        private Skin currentSkin = null;

        public static void Bootstrap()
        {
            if (bootstrapped)
            {
                return;
            }

            var go = new GameObject();
            go.name = "Sapphire";
            go.AddComponent<SapphireBootstrap>();

            bootstrapped = true;
        }


        void OnGUI()
        {
            GUI.Window(15125, new Rect(128, 128, 100, 100), DoWindow, "Sapphire");
        }

        void DoWindow(int i)
        {
            var path = "C:\\Users\\nlight\\Documents\\GitHub\\Skylines-Sapphire\\Skins\\Next\\mainmenu.xml";

            if (GUILayout.Button("Load skin"))
            {
                try
                {
                    currentSkin = Skin.FromXmlFile(path);
                }
                catch (Exception ex)
                {
                    Debug.LogErrorFormat("Failed to load skin \"{0}\", reason: {1}", path, ex.Message);
                }
            }

            if (currentSkin != null)
            {
                if (GUILayout.Button("Load sprites"))
                {
                    try
                    {
                        currentSkin.LoadSprites();
                    }
                    catch (Exception ex)
                    {
                        Debug.LogErrorFormat("Failed to load sprites for skin \"{0}\", reason: {1}", path, ex.Message);
                    }
                }

                if (GUILayout.Button("Apply"))
                {
                    try
                    {
                        currentSkin.Apply();
                    }
                    catch (Exception ex)
                    {
                        Debug.LogErrorFormat("Failed to apply skin \"{0}\", reason: {1}", path, ex.Message);
                    }
                }
            }

            
        }

    }

}
