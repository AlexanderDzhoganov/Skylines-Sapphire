using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using ColossalFramework.IO;
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

            ErrorLogger.ResetSettings();

            currentModuleClass = moduleClass;

            FindObjectOfType<UIView>().gameObject.AddComponent<Core>();
            bootstrapped = true;
        }

        public static void Deinitialize()
        {
            var core = FindObjectOfType<UIView>().GetComponent<Core>();
            if (core != null)
            {
                Destroy(core);
            }
        }

        private static readonly string configPath = Path.Combine(DataLocation.localApplicationData, "SapphireConfig.xml");
        private Configuration config = new Configuration();

        private List<SkinMetadata> availableSkins;
        private Skin currentSkin = null;

        private DebugRenderer debugRenderer;

        private bool autoReloadSkinOnChange = false;
        private float autoReloadCheckTimer = 1.0f;

        private UIButton sapphireButton;
        private UIPanel sapphirePanel;

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
            try
            {
                if (currentSkin != null)
                {
                    currentSkin.Rollback();
                }

                currentSkin = null;
                config = null;

                if (sapphirePanel != null)
                {
                    Destroy(sapphirePanel);
                }

                if (sapphireButton != null)
                {
                    Destroy(sapphireButton);
                }

                SetCameraRectHelper.Deinitialize();

                bootstrapped = false;
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        private bool needToApplyCurrentSkin = false;

        void Start()
        {
            SetCameraRectHelper.Initialize();

            LoadConfig();

            InitializeInGamePanels();

            availableSkins = SkinLoader.FindAllSkins();
            
            if (!string.IsNullOrEmpty(config.selectedSkinPath) && config.applySkinOnStartup)
            {
                foreach (var metadata in availableSkins)
                {
                    if (metadata.sapphirePath == config.selectedSkinPath)
                    {
                        currentSkin = Skin.FromXmlFile(Path.Combine(metadata.sapphirePath, "skin.xml"), false);
                        needToApplyCurrentSkin = true;
                        break;
                    }
                }
            }
            
            CreateUI();

            debugRenderer = gameObject.AddComponent<DebugRenderer>();
        }

        void InitializeInGamePanels()
        {
            var roadsPanels = FindObjectsOfType<RoadsPanel>();

            foreach (var property in typeof(RoadsPanel).GetProperties(BindingFlags.Instance | BindingFlags.NonPublic))
            {
                if (!property.CanRead)
                {
                    continue;
                }

                foreach (var roadsPanel in roadsPanels)
                {
                    try
                    {
                        property.GetValue(roadsPanel, null);
                    }
                    catch (Exception)
                    {
                    }
                }    
            }
        }

        void Update()
        {
            if (needToApplyCurrentSkin)
            {
                if (currentSkin.IsValid)
                {
                    currentSkin.Apply(currentModuleClass);
                }
                else
                {
                    Debug.LogWarning("Skin is invalid, will not apply.. (check messages above for errors)");
                }

                needToApplyCurrentSkin = false;
            }

            if (Input.GetKey(KeyCode.LeftControl))
            {
                if (Input.GetKeyDown(KeyCode.A))
                {
                    if (sapphirePanel != null)
                    {
                        sapphirePanel.isVisible = !sapphirePanel.isVisible;
                    }
                }
                else if (Input.GetKeyDown(KeyCode.D))
                {
                    if (debugRenderer != null)
                    {
                        debugRenderer.drawDebugInfo = !debugRenderer.drawDebugInfo;
                    }
                }
                else if (Input.GetKeyDown(KeyCode.S))
                {
                    ReloadAndApplyActiveSkin();
                }
                else if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.J))
                {
                    var path = "vanilla_ui_dump.xml";
                    if (currentSkin != null)
                    {
                        path = currentSkin.Name + "_dump.xml";
                    }

                    SceneUtil.DumpSceneToXML(path);
                    Debug.LogWarningFormat("Dumped scene to \"{0}\"", path);   
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
            sapphirePanel = CreateSapphirePanel();
            sapphireButton = UIUtil.CreateSapphireButton(currentModuleClass);

            if (currentModuleClass == Skin.ModuleClass.InGame && !config.showSapphireIconInGame)
            {
                sapphireButton.isVisible = false;
            }

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

        private UIDropDown skinsDropdown;

        private UIPanel CreateSapphirePanel()
        {
            var uiView = GameObject.Find("UIView").GetComponent<UIView>();
            if (uiView == null)
            {
                Debug.LogError("UIView is null!");
                return null;
            }

            var panel = uiView.AddUIComponent(typeof(UIPanel)) as UIPanel;

            panel.size = new Vector2(300, 250);
            panel.isVisible = false;
            panel.atlas = EmbeddedResources.GetSapphireAtlas();
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

            UIUtil.MakeCheckbox(panel, "ShowIconInGame", "Show Sapphire icon in-game", new Vector2(4.0f, y), config.showSapphireIconInGame, value =>
            {
                config.showSapphireIconInGame = value;
                SaveConfig();

                if (sapphireButton != null && !config.showSapphireIconInGame && currentModuleClass == Skin.ModuleClass.InGame)
                {
                    sapphireButton.isVisible = false;
                    if (sapphirePanel != null)
                    {
                        sapphirePanel.isVisible = false;
                    }
                }
                else if(sapphireButton != null)
                {
                    sapphireButton.isVisible = true;
                }
            });

            y += 28.0f;

            UIUtil.MakeCheckbox(panel, "AutoApplySkin", "Apply skin on start-up", new Vector2(4.0f, y), config.applySkinOnStartup, value =>
            {
                config.applySkinOnStartup = value;
                SaveConfig();
            });

            y += 28.0f;

            UIUtil.MakeCheckbox(panel, "DrawDebugInfo", "Developer mode (Ctrl+D)", new Vector2(4.0f, y), false, value =>
            {
                if (debugRenderer != null)
                {
                    debugRenderer.drawDebugInfo = value;
                }
            });

            y += 28.0f;

            UIUtil.MakeCheckbox(panel, "AutoReload", "Auto-reload active skin on file change", new Vector2(4.0f, y), false, value =>
            {
                autoReloadSkinOnChange = value;
                ReloadAndApplyActiveSkin();
            });


            y += 28.0f;

            skinsDropdown = panel.AddUIComponent<UIDropDown>();

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
                panel.isVisible = false;
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

            y += 40.0f;

            UIUtil.MakeButton(panel, "ReloadSkin", "Reload active skin (Ctrl+S)", new Vector2(4.0f, y), ReloadAndApplyActiveSkin);

            y += 36.0f;

            UIUtil.MakeButton(panel, "RefreshSkins", "Refresh available skins", new Vector2(4.0f, y), () =>
            {
                RefreshSkinsList();
            });

            return panel;
        }

        void RefreshSkinsList()
        {
            if (skinsDropdown != null)
            {
                availableSkins = SkinLoader.FindAllSkins();
                skinsDropdown.localizedItems = new string[0];

                skinsDropdown.AddItem("Vanilla (by Colossal Order)");
                foreach (var skin in availableSkins)
                {
                    skinsDropdown.AddItem(String.Format("{0} (by {1})", skin.name, skin.author));
                }

                skinsDropdown.selectedIndex = 0;
                skinsDropdown.Invalidate();
            }
        }

        private void ReloadAndApplyActiveSkin()
        {
            try
            {
                if (currentSkin == null)
                {
                    return;
                }

                currentSkin.SafeReload(autoReloadSkinOnChange);

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

    }

}
