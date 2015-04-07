using ColossalFramework.UI;
using UnityEngine;

namespace Sapphire
{
    public class SapphireBootstrap : MonoBehaviour
    {

        private static bool bootstrapped = false;

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
            if (GUILayout.Button("apply skin"))
            {
                var skin = Skin.FromXmlFile("C:\\Users\\nlight\\Documents\\GitHub\\Skylines-Sapphire\\Skins\\Next\\mainmenu.xml");
                skin.LoadSprites();
                skin.Apply();
            }
        }

    }

}
