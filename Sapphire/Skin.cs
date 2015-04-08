using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Xml;
using ColossalFramework.UI;
using UnityEngine;

namespace Sapphire
{
    public class Skin
    {

        public enum ModuleClass
        {
            MainMenu = 0,
            InGame = 1,
            MapEditor = 2,
            AssetEditor = 3
        }

        public static Skin FromXmlFile(string sapphirePath)
        {
            Skin skin = null;

            try
            {
                var document = new XmlDocument();
                document.LoadXml(File.ReadAllText(sapphirePath));
                skin = new Skin(Path.GetDirectoryName(sapphirePath), document);
            }
            catch (XmlNodeException ex)
            {
                Debug.LogErrorFormat("{0} while parsing Skin xml ({1}) at node \"{2}\": {3}",
                    ex.GetType(), sapphirePath, ex.Node == null ? "null" : ex.Node.Name, ex.ToString());
            }
            catch (Exception ex)
            {
                Debug.LogErrorFormat("Exception while parsing Skin xml ({0}): {1}", sapphirePath, ex.Message);
            }

            return skin;
        }

        private Dictionary<ModuleClass, List<SkinModule>> modules = new Dictionary<ModuleClass, List<SkinModule>>();

        private string name;
        
        public string Name
        {
            get { return name; }
        }

        private string author;

        public string Author
        {
            get { return Author; }
        }

        public Dictionary<string, Texture2D> spriteTextureCache = new Dictionary<string, Texture2D>();
        public Dictionary<string, UITextureAtlas> spriteAtlases = new Dictionary<string, UITextureAtlas>();

        public Dictionary<string, Color32> colorDefinitions = new Dictionary<string, Color32>(); 

        private string sapphirePath;

        private XmlDocument document;

        public Skin(string _sapphirePath, XmlDocument _document)
        {
            modules[ModuleClass.MainMenu] = new List<SkinModule>();
            modules[ModuleClass.InGame] = new List<SkinModule>();
            modules[ModuleClass.MapEditor] = new List<SkinModule>();
            modules[ModuleClass.AssetEditor] = new List<SkinModule>();

            sapphirePath = _sapphirePath;
            document = _document;

            var root = document.SelectSingleNode("/SapphireSkin");
            if (root == null)
            {
                throw new ParseException("Skin missing root SapphireSkin node at " + sapphirePath, null);
            }

            LoadSprites();
            LoadColors();

            name = XmlUtil.GetAttribute(root, "name").Value;
            author = XmlUtil.GetAttribute(root, "author").Value;

            foreach (XmlNode childNode in root.ChildNodes)
            {
                if (childNode.Name == "Module")
                {
                    var modulePath = Path.Combine(sapphirePath, childNode.InnerText);
                    var moduleClass = XmlUtil.GetAttribute(childNode, "class").Value;

                    if (moduleClass == "MainMenu")
                    {
                        AddModuleAtPath(ModuleClass.MainMenu, modulePath);
                    }
                    else if (moduleClass == "InGame")
                    {
                        AddModuleAtPath(ModuleClass.InGame, modulePath);
                    }
                    else if (moduleClass == "MapEditor")
                    {
                        AddModuleAtPath(ModuleClass.MapEditor, modulePath);
                    }
                    else if (moduleClass == "AssetEditor")
                    {
                        AddModuleAtPath(ModuleClass.AssetEditor, modulePath);
                    }
                    else
                    {
                        throw new ParseException(String.Format
                            ("Invalid module class \"{0}\"", moduleClass), childNode);
                    }
                }
            }
        }

        private void LoadColors()
        {
            try
            {
                LoadColorsInternal();
            }
            catch (XmlNodeException ex)
            {
                Debug.LogErrorFormat("{0} while loading colors for skin ({1}) at node \"{2}\": {3}",
                    ex.GetType(), sapphirePath, ex.Node == null ? "null" : ex.Node.Name, ex.ToString());
            }
            catch (Exception ex)
            {
                Debug.LogErrorFormat("Exception while loading colors for skin ({0}): {1}", sapphirePath, ex.Message);
            }
        }

        private void LoadColorsInternal()
        {
            Debug.LogWarning("Loading colors");

            var rootColorsNode = document.SelectSingleNode("/SapphireSkin/Colors");

            if (rootColorsNode == null)
            {
                Debug.LogWarning("Skin defines no colors");
                return;
            }

            foreach (XmlNode childNode in rootColorsNode)
            {
                if (childNode.Name != "Color")
                {
                    continue;
                }

                var colorName = XmlUtil.GetAttribute(childNode, "name").Value;
                if (colorDefinitions.ContainsKey(colorName))
                {
                    Debug.LogWarningFormat("Duplicate color name \"{0}\", ignoring second definition..", colorName);
                    continue;
                }

                var text = childNode.InnerText;

                if (text.Length == 0)
                {
                    throw new ParseException(String.Format("Empty color value for color \"{0}\"", colorName), childNode);
                }

                Color32 color = Color.black;

                if (text[0] == '#')
                {
                    int colorHex = Int32.Parse(text.Replace("#", ""), NumberStyles.HexNumber);
                    byte R = (byte)((colorHex >> 16) & 0xFF);
                    byte G = (byte)((colorHex >> 8) & 0xFF);
                    byte B = (byte)((colorHex) & 0xFF);
                    color = new Color32(R, G, B, 255);
                }
                else
                {
                    var values = text.Split(',');
                    if (values.Length != 4)
                    {
                        throw new ParseException("Color32 definition must have four components", childNode);
                    }

                    color = new Color32(byte.Parse(values[0]), byte.Parse(values[1]), byte.Parse(values[2]), byte.Parse(values[3]));
                }
                
                colorDefinitions.Add(colorName, color);
                Debug.LogWarningFormat("Color \"{0}\" defined as \"{1}\"", colorName, color.ToString());
            }
        }

        private void LoadSprites()
        {
            try
            {
                LoadSpritesInternal();
            }
            catch (XmlNodeException ex)
            {
                Debug.LogErrorFormat("{0} while loading sprites for skin ({1}) at node \"{2}\": {3}",
                    ex.GetType(), sapphirePath, ex.Node == null ? "null" : ex.Node.Name, ex.ToString());
            }
            catch (Exception ex)
            {
                Debug.LogErrorFormat("Exception while loading sprites for skin ({0}): {1}", sapphirePath, ex.Message);
            }
        }

        private void LoadSpritesInternal()
        {
            Debug.LogWarning("Loading sprites");

            var rootNode = document.SelectSingleNode("/SapphireSkin");

            if (rootNode == null)
            {
                throw new Exception("Skin missing root SapphireSkin node");
            }

            foreach (XmlNode childNode in rootNode)
            {
                if (childNode.Name != "SpriteAtlas")
                {
                    continue;
                }

                var atlasName = XmlUtil.GetAttribute(childNode, "name").Value;
                if (spriteAtlases.ContainsKey(atlasName))
                {
                    Debug.LogWarningFormat("Duplicate atlas name \"{0}\", ignoring second definition..", atlasName);
                    continue;
                }

                Debug.LogWarningFormat("Generating atlas \"{0}\"", atlasName);

                var atlasPacker = new AtlasPacker();

                int count = 0;
                foreach (XmlNode spriteNode in childNode.ChildNodes)
                {
                    var path = childNode.InnerText;
                    var name = XmlUtil.GetAttribute(spriteNode, "name").Value;
                    Debug.LogWarningFormat("Packing sprite \"{0}\" in atlas", name);

                    if (spriteTextureCache.ContainsKey(path))
                    {
                        continue;
                    }

                    var widthAttribute = XmlUtil.GetAttribute(spriteNode, "width");
                    var heightAttribute = XmlUtil.GetAttribute(spriteNode, "height");

                    int width = -1;
                    int height = -1;

                    if (!int.TryParse(widthAttribute.Value, out width))
                    {
                        throw new MissingAttributeValueException("width", spriteNode);
                    }

                    if (!int.TryParse(heightAttribute.Value, out height))
                    {
                        throw new MissingAttributeValueException("height", spriteNode);
                    }

                    var fullPath = Path.Combine(sapphirePath, path);

                    var texture = new Texture2D(width, height);
                    texture.LoadImage(File.ReadAllBytes(fullPath));
                    spriteTextureCache.Add(path, texture);

                    atlasPacker.AddSprite(name, texture);
                    count++;
                }

                Debug.LogWarningFormat("Added {0} sprites..", count);
                spriteAtlases[atlasName] = atlasPacker.GenerateAtlas();
                Debug.LogWarningFormat("Atlas \"{0}\" generated", atlasName);
            }
        }

        private void AddModuleAtPath(ModuleClass moduleClass, string modulePath)
        {
            var name = Path.GetFileNameWithoutExtension(modulePath);
            if (name == null)
            {
                throw new Exception(String.Format("Invalid skin module path \"{0}\"", modulePath));
            }

            var module = SkinModule.FromXmlFile(this, modulePath);
            modules[moduleClass].Add(module);
        }

        public void Apply(ModuleClass moduleClass)
        {
            foreach (var module in modules[moduleClass])
            {
                module.Apply();
            }
        }

    }
}
