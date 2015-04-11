using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using ColossalFramework.UI;
using UnityEngine;

namespace Sapphire
{

    public class Core : MonoBehaviour
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

            FindObjectOfType<UIView>().gameObject.AddComponent<Core>();
            bootstrapped = true;
        }

        private List<SkinMetadata> availableSkins;

        private static readonly string configPath = "SapphireConfig.xml";
        private Configuration config = new Configuration();

        private Skin currentSkin = null;

        private DebugRenderer debugRenderer;

        private bool autoReloadSkinOnChange = false;
        private float autoReloadCheckTimer = 1.0f;

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
            currentSkin = null;
            config = null;

            MakeCameraFullscreen.Deinitialize();

            bootstrapped = false;
        }

        void Awake()
        {
            MakeCameraFullscreen.Initialize();

            LoadConfig();

            availableSkins = SkinLoader.FindAllSkins();
            
            if (!string.IsNullOrEmpty(config.selectedSkinPath) && config.applySkinOnStartup)
            {
                foreach (var metadata in availableSkins)
                {
                    if (metadata.sapphirePath == config.selectedSkinPath)
                    {
                        currentSkin = Skin.FromXmlFile(Path.Combine(metadata.sapphirePath, "skin.xml"), false);

                        if (currentSkin.IsValid)
                        {
                            currentSkin.Apply(currentModuleClass);
                        }
                        else
                        {
                            Debug.LogWarning("Skin is invalid, will not apply.. (check messages above for errors)");
                        }
                        break;
                    }
                }
            }
            
            CreateUI();

            debugRenderer = gameObject.AddComponent<DebugRenderer>();
        }

        void Update()
        {
            if (Input.GetKey(KeyCode.LeftControl))
            {
                if (Input.GetKeyDown(KeyCode.D))
                {
                    if (debugRenderer != null)
                    {
                        debugRenderer.drawDebugInfo = !debugRenderer.drawDebugInfo;
                    }
                }
                else if (Input.GetKeyDown(KeyCode.S))
                {
                    ReloadActiveSkin();
                }
            }

            if (currentSkin != null)
            {
                currentSkin.ApplyStickyProperties(currentModuleClass);
            }

            if (currentSkin != null && autoReloadSkinOnChange)
            {
                autoReloadCheckTimer -= Time.deltaTime;
                if (autoReloadCheckTimer <= 0.0f)
                {
                    autoReloadCheckTimer = 1.0f;
                }

                currentSkin.ReloadIfChanged();
            }
        }

        private void CreateUI()
        {
            var atlas = GetSapphireAtlas();;
            var sapphirePanel = CreateSapphirePanel(atlas);
            var sapphireButton = CreateSapphireButton(atlas);

            sapphireButton.eventClick += (component, param) => { sapphirePanel.isVisible = !sapphirePanel.isVisible; };

            if (currentModuleClass != Skin.ModuleClass.MainMenu)
            {
                try
                {
                    Camera.main.pixelRect = new Rect(0, 0, Screen.width, Screen.height);
                    GameObject.Find("Underground View").GetComponent<Camera>().pixelRect = new Rect(0, 0, Screen.width, Screen.height);
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                }
            }
        }

        private UIPanel CreateSapphirePanel(UITextureAtlas atlas)
        {
            var uiView = GameObject.Find("UIView").GetComponent<UIView>();
            if (uiView == null)
            {
                Debug.LogError("UIView is null!");
                return null;
            }

            var panel = uiView.AddUIComponent(typeof(UIPanel)) as UIPanel;

            panel.size = new Vector2(300, 180);
            panel.isVisible = false;
            panel.atlas = atlas;
            panel.backgroundSprite = "DefaultPanelBackground";

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
            title.textColor = Color.black;

            float y = 32.0f;

            MakeCheckbox(panel, "AutoApplySkin", "Apply skin on start-up", y, config.applySkinOnStartup, value =>
            {
                config.applySkinOnStartup = value;
                SaveConfig();
            });

            y += 28.0f;

            MakeCheckbox(panel, "DrawDebugInfo", "Developer mode (Ctrl+D)", y, false, value =>
            {
                if (debugRenderer != null)
                {
                    debugRenderer.drawDebugInfo = value;
                }
            });

            y += 28.0f;
            
            MakeCheckbox(panel, "AutoReload", "Auto-reload active skin on file change", y, false, value =>
            {
                autoReloadSkinOnChange = value;
                ReloadActiveSkin();
            });


            y += 28.0f;

            var skinsDropdown = panel.AddUIComponent<UIDropDown>();

            skinsDropdown.AddItem("Vanilla (by Colossal Order)");
            foreach (var skin in availableSkins)
            {
                skinsDropdown.AddItem(String.Format("{0} (by {1})", skin.name, skin.author));
            }

            skinsDropdown.size = new Vector2(296.0f, 32.0f);
            skinsDropdown.relativePosition = new Vector3(2.0f, y);
            skinsDropdown.listBackground = "GenericPanelLight";
            skinsDropdown.itemHeight = 32;
            skinsDropdown.itemHover = "ListItemHover";
            skinsDropdown.itemHighlight = "ListItemHighlight";
            skinsDropdown.normalBgSprite = "ButtonMenu";
            skinsDropdown.listWidth = 300;
            skinsDropdown.listHeight = 500;
            skinsDropdown.foregroundSpriteMode = UIForegroundSpriteMode.Stretch;
            skinsDropdown.popupColor = new Color32(45, 52, 61, 255);
            skinsDropdown.popupTextColor = new Color32(170, 170, 170, 255);
            skinsDropdown.zOrder = 1;
            skinsDropdown.textScale = 0.8f;
            skinsDropdown.verticalAlignment = UIVerticalAlignment.Middle;
            skinsDropdown.horizontalAlignment = UIHorizontalAlignment.Center;
            skinsDropdown.textFieldPadding = new RectOffset(8, 0, 8, 0);
            skinsDropdown.itemPadding = new RectOffset(8, 0, 2, 0);

            skinsDropdown.selectedIndex = 0;

            if(currentSkin != null)
            {
                int i = 1;
                foreach (var skin in availableSkins)
                {
                    if (skin.sapphirePath == currentSkin.SapphirePath)
                    {
                        skinsDropdown.selectedIndex = i;
                    }

                    i++;
                }
            }

            skinsDropdown.eventSelectedIndexChanged += (component, index) =>
            {
                if (index == 0)
                {
                    if (currentSkin != null)
                    {
                        currentSkin.Dispose();
                    }

                    currentSkin = null;
                    return;
                }

                var skin = availableSkins[index-1];
                if (currentSkin != null && currentSkin.SapphirePath == skin.sapphirePath)
                {
                    return;
                }

                if (currentSkin != null)
                {
                    currentSkin.Dispose();
                }

                currentSkin = Skin.FromXmlFile(Path.Combine(skin.sapphirePath, "skin.xml"), autoReloadSkinOnChange);

                if (currentSkin.IsValid)
                {
                    currentSkin.Apply(currentModuleClass);
                }
                else
                {
                    Debug.LogWarning("Skin is invalid, will not apply.. (check messages above for errors)");
                } 
                
                config.selectedSkinPath = currentSkin.SapphirePath;
                SaveConfig();
            };
                        
            var skinsDropdownButton = skinsDropdown.AddUIComponent<UIButton>();
            skinsDropdown.triggerButton = skinsDropdownButton;

            skinsDropdownButton.text = "";
            skinsDropdownButton.size = skinsDropdown.size;
            skinsDropdownButton.relativePosition = new Vector3(0.0f, 0.0f);
            skinsDropdownButton.textVerticalAlignment = UIVerticalAlignment.Middle;
            skinsDropdownButton.textHorizontalAlignment = UIHorizontalAlignment.Center;
            skinsDropdownButton.normalFgSprite = "IconDownArrow";
            skinsDropdownButton.hoveredFgSprite = "IconDownArrowHovered";
            skinsDropdownButton.pressedFgSprite = "IconDownArrowPressed";
            skinsDropdownButton.focusedFgSprite = "IconDownArrowFocused";
            skinsDropdownButton.disabledFgSprite = "IconDownArrowDisabled";
            skinsDropdownButton.foregroundSpriteMode = UIForegroundSpriteMode.Fill;
            skinsDropdownButton.horizontalAlignment = UIHorizontalAlignment.Right;
            skinsDropdownButton.verticalAlignment = UIVerticalAlignment.Middle;
            skinsDropdownButton.zOrder = 0;
            skinsDropdownButton.textScale = 0.8f;

            y += 36.0f;

            MakeButton(panel, "ReloadSkin", "Reload active skin (Ctrl+S)", y, () =>
            {
                ReloadActiveSkin();
            });

            return panel;
        }

        private UIButton CreateSapphireButton(UITextureAtlas atlas)
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

            button.atlas = atlas;
            button.normalFgSprite = "SapphireIcon";
            button.hoveredFgSprite = "SapphireIconHover";
            button.pressedFgSprite = "SapphireIconPressed";
            button.foregroundSpriteMode = UIForegroundSpriteMode.Scale;
            button.scaleFactor = 1.0f;

            button.tooltip = "Sapphire Skin Manager";
            button.tooltipBox = uiView.defaultTooltipBox;

            if (currentModuleClass == Skin.ModuleClass.MainMenu)
            {
                button.relativePosition = new Vector3(4.0f, 2.0f, 0.0f);
            }
            else
            {
                button.relativePosition = new Vector3(4.0f, 64.0f, 0.0f);
            }

            return button;
        }

        private delegate void ButtonClicked();

        private static UIButton MakeButton(UIPanel panel, string name, string text, float y, ButtonClicked clicked)
        {
            var button = panel.AddUIComponent<UIButton>();
            button.name = name;
            button.text = text;
            button.relativePosition = new Vector3(4.0f, y);
            button.size = new Vector2(200.0f, 24.0f);
            button.normalBgSprite = "ButtonMenu";
            button.disabledBgSprite = "ButtonMenuDisabled";
            button.hoveredBgSprite = "ButtonMenuHovered";
            button.focusedBgSprite = "ButtonMenu";
            button.pressedBgSprite = "ButtonMenuPressed";
            button.textScale = 0.8f;

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
            label.textColor = Color.black;

            var checkbox = panel.AddUIComponent<UICheckBox>();
            checkbox.AlignTo(label, UIAlignAnchor.TopLeft);
            checkbox.relativePosition = new Vector3(checkbox.relativePosition.x + 274.0f, checkbox.relativePosition.y - 2.0f);
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

                var path = currentSkin.SapphirePath;
                currentSkin.Dispose();
                currentSkin = Skin.FromXmlFile(Path.Combine(path, "skin.xml"), autoReloadSkinOnChange);

                if (currentSkin.IsValid)
                { 
                    currentSkin.Apply(currentModuleClass);
                }
                else
                {
                    Debug.LogWarning("Skin is invalid, will not apply.. (check messages above for errors)");
                } 
            }
            catch (Exception ex)
            {
                Debug.LogErrorFormat("Failed to load skin: {0}", ex.Message);
            }
        }

        private UITextureAtlas GetSapphireAtlas()
        {
            var atlasPacker = new AtlasPacker();
            atlasPacker.AddSprite("SapphireIcon", GetSapphireIcon());
            atlasPacker.AddSprite("SapphireIconHover", GetSapphireIconHover());
            atlasPacker.AddSprite("SapphireIconPressed", GetSapphireIconPressed());
            atlasPacker.AddSprite("DefaultPanelBackground", GetSapphireBackground());
            return atlasPacker.GenerateAtlas("SapphireIconsAtlas");
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

        private Texture2D GetSapphireBackground()
        {
            var texture = new Texture2D(128, 128);
            texture.LoadImage(GetResource("Sapphire.Resources.DefaultPanelBackground.png"));
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
