using System;
using System.Collections.Generic;
using System.Reflection;
using ColossalFramework.UI;
using UnityEngine;

namespace Sapphire
{
    public class SapphireBootstrap : MonoBehaviour
    {

        private static bool bootstrapped = false;
        private static Skin.ModuleClass currentModuleClass;

        private static UIButton sapphireButton;

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

            var uiView = FindObjectOfType<UIView>();
            sapphireButton = uiView.AddUIComponent(typeof(UIButton)) as UIButton;

            sapphireButton.name = "SapphireButton";
            sapphireButton.gameObject.name = "SapphireButton";
            sapphireButton.width = 32;
            sapphireButton.height = 32;

            sapphireButton.pressedBgSprite = "";
            sapphireButton.normalBgSprite = "";
            sapphireButton.hoveredBgSprite = "";
            sapphireButton.disabledBgSprite = "";

            sapphireButton.atlas = GetSapphireAtlas();
            sapphireButton.normalFgSprite = "SapphireIcon";
            sapphireButton.hoveredFgSprite = "SapphireIconHover";
            sapphireButton.pressedBgSprite = "SapphireIconPressed";
            sapphireButton.foregroundSpriteMode = UIForegroundSpriteMode.Scale;
            sapphireButton.scaleFactor = 1.0f;

            sapphireButton.tooltip = "Sapphire Skin Manager";
            sapphireButton.tooltipBox = uiView.defaultTooltipBox;

            sapphireButton.relativePosition = new Vector3(2.0f, 2.0f, 0.0f);

            sapphireButton.eventClick += (component, param) => { };
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

        private UITextureAtlas GetSapphireAtlas()
        {
            var atlasPacker = new AtlasPacker();
            atlasPacker.AddSprite("SapphireIcon", GetSapphireIcon());
            atlasPacker.AddSprite("SapphireIconHover", GetSapphireIconHover());
            atlasPacker.AddSprite("SapphireIconPressed", GetSapphireIconPressed());
            return atlasPacker.GenerateAtlas();
        }

        private Texture2D GetSapphireIcon()
        {
            var texture = new Texture2D(128, 128);
            texture.LoadImage(GetResource("Sapphire.Resources.SapphireIcon.png"));
            return texture;
        }

        private Texture2D GetSapphireIconHover()
        {
            var texture = new Texture2D(128, 128);
            texture.LoadImage(GetResource("Sapphire.Resources.SapphireIconHover.png"));
            return texture;
        }

        private Texture2D GetSapphireIconPressed()
        {
            var texture = new Texture2D(128, 128);
            texture.LoadImage(GetResource("Sapphire.Resources.SapphireIconPressed.png"));
            return texture;
        }

        private static byte[] GetResource(string name)
        {
            var asm = Assembly.GetExecutingAssembly();
            var stream = asm.GetManifestResourceStream(name);
            byte[] data = new byte[stream.Length];
            stream.Read(data, 0, (int)stream.Length);
            return data;
        }

    }

}
