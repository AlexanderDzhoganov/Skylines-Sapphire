using System;
using System.Collections.Generic;
using UnityEngine;

namespace Sapphire
{
    public class SapphireBootstrap : MonoBehaviour
    {

        private static bool bootstrapped = false;
        private static Skin.ModuleClass currentModuleClass;

        public static void Bootstrap(Skin.ModuleClass moduleClass)
        {
            if (bootstrapped)
            {
                return;
            }

            currentModuleClass = moduleClass;

            var go = new GameObject();
            go.name = "Sapphire";
            go.AddComponent<SapphireBootstrap>();

            if (moduleClass != Skin.ModuleClass.MainMenu)
            {
                Camera.main.gameObject.AddComponent<CameraHook>();
            }

            bootstrapped = true;
        }

        private List<Skin> loadedSkins = new List<Skin>(); 

        void OnDestroy()
        {
            bootstrapped = false;
        }

        void Start()
        {
            ReloadSkins();
        }

        private void ReloadSkins()
        {
            try
            {
                loadedSkins = new List<Skin>();
                foreach (var sapphirePath in SkinLoader.FindAllSkins())
                {
                    var skin = SkinLoader.LoadSkin(sapphirePath);
                    loadedSkins.Add(skin);
                    Debug.LogWarningFormat("Loaded skin \"{0}\" from {1}", skin.Name, sapphirePath);
                }
            }
            catch (Exception ex)
            {
                Debug.LogErrorFormat("Failed to load skins: {0}", ex.Message);
            }
        }

        void OnGUI()
        {
            GUI.Window(15125, new Rect(128, 128, 200, 300), DoWindow, "Sapphire");
        }

        void DoWindow(int i)
        {
            if (GUILayout.Button("Reload skins"))
            {
                ReloadSkins();

                foreach (var skin in loadedSkins)
                {
                    if (GUILayout.Button(skin.Name))
                    {
                        skin.Apply(currentModuleClass);
                    }
                }
            }

            GUILayout.Space(8);

            foreach (var skin in loadedSkins)
            {
                if (GUILayout.Button(skin.Name))
                {
                    skin.Apply(currentModuleClass);
                }
            }
        }

    }

}
