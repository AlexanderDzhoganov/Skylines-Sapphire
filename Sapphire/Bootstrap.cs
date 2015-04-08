using System;
using System.Collections.Generic;
using System.Reflection;
using ColossalFramework;
using ColossalFramework.UI;
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

        private static readonly string configPath = "SapphireConfig.xml";
        private Configuration config = new Configuration();

        private Skin currentSkin = null;

        private void LoadConfig()
        {
            config = Configuration.Deserialize(configPath);
            if (config == null)
            {
                config = new Configuration();
                SaveConfig();
            }
        }

        private void SaveConfig()
        {
            Configuration.Serialize(configPath, config);
        }

        void OnDestroy()
        {
            bootstrapped = false;
        }

        void Awake()
        {
            LoadConfig();

            ReloadSkins();

            if (!string.IsNullOrEmpty(config.selectedSkinPath) && config.applySkinOnStartup)
            {
                foreach (var skin in loadedSkins)
                {
                    if (skin.SapphirePath == config.selectedSkinPath)
                    {
                        currentSkin = skin;
                        skin.Apply(currentModuleClass);
                        break;
                    }
                }
            }

            var sapphirePanel = CreateSapphirePanel();
            var sapphireButton = CreateSapphireButton();
            sapphireButton.eventClick += (component, param) => { sapphirePanel.isVisible = !sapphirePanel.isVisible; };
        }

        private List<UICheckBox> skinCheckBoxes = new List<UICheckBox>(); 

        private UIPanel CreateSapphirePanel()
        {
            var uiView = GameObject.Find("UIView").GetComponent<UIView>();
            if (uiView == null)
            {
                Debug.LogError("UIView is null!");
                return null;
            }

            var panel = uiView.AddUIComponent(typeof(UIPanel)) as UIPanel;

            panel.size = new Vector2(300, 400);
            panel.isVisible = false;
            panel.backgroundSprite = "UnlockingPanel2";

            if (currentModuleClass == Skin.ModuleClass.MainMenu)
            {
                panel.relativePosition = new Vector3(2.0f, 34.0f);
            }
            else
            {
                panel.relativePosition = new Vector3(2.0f, 34.0f + 64.0f);
            }

            panel.name = "SapphireSkinManager";

            var title = panel.AddUIComponent<UILabel>();
            title.relativePosition = new Vector3(2.0f, 2.0f);
            title.text = "Sapphire Skin Manager";

            MakeCheckbox(panel, "AutoApplySkin", "Apply skin on start-up", 48.0f, config.applySkinOnStartup, value =>
            {
                config.applySkinOnStartup = value;
                SaveConfig();
            });

            var skinsList = panel.AddUIComponent<UIScrollablePanel>();
            skinsList.name = "SkinsList";
            skinsList.relativePosition = new Vector3(2.0f, 80.0f);
            skinsList.size = new Vector2(panel.size.x - 4.0f, panel.size.y - 64.0f - 2.0f);
            skinsList.backgroundSprite = "SubcategoriesPanel";
            skinsList.autoLayout = true;
            skinsList.scrollPosition = new Vector2(0.0f, 0.0f);

            int i = 0;
            foreach (var skin in loadedSkins)
            {
                var skinPanel = skinsList.AddUIComponent<UIPanel>();
                skinPanel.size = new Vector2(skinsList.size.x, 24.0f);
                skinPanel.relativePosition = new Vector3(0.0f, 0.0f);

                var isActive = currentSkin == skin;
                
                var checkbox = MakeCheckbox(skinPanel, "Skin" + i, String.Format("{0} (by {1})", skin.Name, skin.Author),
                    64.0f, isActive, null);

                var skinCopy = skin;
                checkbox.eventCheckChanged += (component, value) =>
                {
                    if (value == false)
                    {
                        return;
                    }

                    foreach (var cb in skinCheckBoxes)
                    {
                        if (cb != checkbox)
                        {
                            cb.isChecked = false;
                        }
                    }

                    currentSkin = skinCopy;
                    currentSkin.Apply(currentModuleClass);
                    config.selectedSkinPath = currentSkin.SapphirePath;
                    SaveConfig();
                };
                
                skinCheckBoxes.Add(checkbox);

                i++;
            }

            MakeButton(panel, "ReloadSkin", "Reload active skin", panel.size.y - 32.0f, () =>
            {
                ReloadActiveSkin();
            });

            return panel;
        }

        private UIButton CreateSapphireButton()
        {
            var uiView = GameObject.Find("UIView").GetComponent<UIView>();
            if (uiView == null)
            {
                Debug.LogError("UIView is null!");
                return null;
            }

            var button = uiView.AddUIComponent(typeof(UIButton)) as UIButton;

            button.name = "SapphireButton";
            button.gameObject.name = "SapphireButton";
            button.width = 32;
            button.height = 32;

            button.pressedBgSprite = "";
            button.normalBgSprite = "";
            button.hoveredBgSprite = "";
            button.disabledBgSprite = "";

            button.atlas = GetSapphireAtlas();
            button.normalFgSprite = "SapphireIcon";
            button.hoveredFgSprite = "SapphireIconHover";
            button.pressedFgSprite = "SapphireIconPressed";
            button.foregroundSpriteMode = UIForegroundSpriteMode.Scale;
            button.scaleFactor = 1.0f;

            button.tooltip = "Sapphire Skin Manager";
            button.tooltipBox = uiView.defaultTooltipBox;

            if (currentModuleClass == Skin.ModuleClass.MainMenu)
            {
                button.relativePosition = new Vector3(2.0f, 2.0f, 0.0f);
            }
            else
            {
                button.relativePosition = new Vector3(2.0f, 64.0f, 0.0f);
            }
            return button;
        }

        private delegate void ButtonClicked();

        private static UIButton MakeButton(UIPanel panel, string name, string text, float y, ButtonClicked clicked)
        {
            var button = panel.AddUIComponent<UIButton>();
            button.name = name;
            button.text = text;
            button.relativePosition = new Vector3(2.0f, y - 6.0f);
            button.size = new Vector2(200.0f, 24.0f);
            button.normalBgSprite = "ButtonMenu";
            button.disabledBgSprite = "ButtonMenuDisabled";
            button.hoveredBgSprite = "ButtonMenuHovered";
            button.focusedBgSprite = "ButtonMenu";
            button.pressedBgSprite = "ButtonMenuPressed";

            button.eventClick += (component, param) =>
            {
                clicked();
            };

            return button;
        }

        private delegate void CheckboxSetValue(bool value);

        private static UICheckBox MakeCheckbox(UIPanel panel, string name, string text, float y, bool value,
            CheckboxSetValue setValue)
        {
            var label = panel.AddUIComponent<UILabel>();
            label.name = name;
            label.text = text;
            label.relativePosition = new Vector3(4.0f, y);
            label.textScale = 0.8f;

            var checkbox = panel.AddUIComponent<UICheckBox>();
            checkbox.AlignTo(label, UIAlignAnchor.TopLeft);
            checkbox.relativePosition = new Vector3(checkbox.relativePosition.x + 200.0f, checkbox.relativePosition.y - 6.0f);
            checkbox.size = new Vector2(16.0f, 16.0f);
            checkbox.isVisible = true;
            checkbox.canFocus = true;
            checkbox.isInteractive = true;

            if (setValue != null)
            {
                checkbox.eventCheckChanged += (component, newValue) =>
                {
                    setValue(newValue);
                };
            }

            var uncheckSprite = checkbox.AddUIComponent<UISprite>();
            uncheckSprite.size = new Vector2(16.0f, 16.0f);
            uncheckSprite.relativePosition = new Vector3(0, 0);
            uncheckSprite.spriteName = "check-unchecked";
            uncheckSprite.isVisible = true;

            var checkSprite = checkbox.AddUIComponent<UISprite>();
            checkSprite.size = new Vector2(16.0f, 16.0f);
            checkSprite.relativePosition = new Vector3(0, 0);
            checkSprite.spriteName = "check-checked";

            checkbox.isChecked = value;
            checkbox.checkedBoxObject = checkSprite;
            return checkbox;
        }

        private void ReloadActiveSkin()
        {
            try
            {
                if (currentSkin == null)
                {
                    return;
                }

                var currentPath = currentSkin.SapphirePath;
                currentSkin = null;

                ReloadSkins();

                foreach (var skin in loadedSkins)
                {
                    if (skin.SapphirePath == currentPath)
                    {
                        currentSkin = skin;
                        break;
                    }
                }

                if (currentSkin != null)
                {
                    currentSkin.Apply(currentModuleClass);
                    config.selectedSkinPath = currentSkin.SapphirePath;
                    SaveConfig();
                }
            }
            catch (Exception ex)
            {
                Debug.LogErrorFormat("Failed to load skin: {0}", ex.Message);
            }
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
