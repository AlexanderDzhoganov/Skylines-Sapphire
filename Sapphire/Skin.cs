using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Xml;
using ColossalFramework.UI;
using UnityEngine;

namespace Sapphire
{

    public class SkinMetadata
    {
        public string name;
        public string author;
        public string sapphirePath;
    }


    public class Skin
    {

        public enum ModuleClass
        {
            MainMenu = 0,
            InGame = 1,
            MapEditor = 2,
            AssetEditor = 3
        }

        public static Skin FromXmlFile(string skinXmlPath)
        {
            Skin skin = null;

            try
            {
                var document = new XmlDocument();
                document.LoadXml(File.ReadAllText(skinXmlPath));
                skin = new Skin(Path.GetDirectoryName(skinXmlPath), document);
            }
            catch (XmlNodeException ex)
            {
                Debug.LogErrorFormat("{0} while parsing XML at {1} at node \"{2}\": {3}",
                    ex.GetType(), skinXmlPath, ex.Node == null ? "null" : ex.Node.Name, ex.ToString());
            }
            catch (XmlException ex)
            {
                Debug.LogErrorFormat("XmlException while parsing XML \"{0}\" at line {1}, col {2}: {3}",
                    skinXmlPath, ex.LineNumber, ex.LinePosition, ex.Message);
            }
            catch (Exception ex)
            {
                Debug.LogErrorFormat("Exception while parsing XML \"{0}\": {1}",
                    skinXmlPath, ex.ToString());
            }

            return skin;
        }

        public static SkinMetadata MetadataFromXmlFile(string skinXmlPath)
        {
            SkinMetadata metadata = null;

            try
            {
                var document = new XmlDocument();
                document.LoadXml(File.ReadAllText(Path.Combine(skinXmlPath, "skin.xml")));

                var root = document.SelectSingleNode("/SapphireSkin");
                if (root == null)
                {
                    throw new ParseException("Skin missing root SapphireSkin node at " + skinXmlPath, null);
                }

                var name = XmlUtil.GetStringAttribute(root, "name");
                var author = XmlUtil.GetStringAttribute(root, "author");
                metadata = new SkinMetadata {name = name, author = author, sapphirePath = skinXmlPath};
            }
            catch (XmlNodeException ex)
            {
                Debug.LogErrorFormat("{0} while parsing Skin xml ({1}) at node \"{2}\": {3}",
                    ex.GetType(), skinXmlPath, ex.Node == null ? "null" : ex.Node.Name, ex.ToString());
            }
            catch (XmlException ex)
            {
                Debug.LogErrorFormat("XmlException while parsing XML \"{0}\" at line {1}, col {2}: {3}",
                    skinXmlPath, ex.LineNumber, ex.LinePosition, ex.Message);
            }
            catch (Exception ex)
            {
                Debug.LogErrorFormat("Exception while parsing XML \"{0}\": {1}",
                    skinXmlPath, ex.ToString());
            }

            return metadata;
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
            get { return author; }
        }

        public Dictionary<string, Texture2D> spriteTextureCache = new Dictionary<string, Texture2D>();
        public Dictionary<string, UITextureAtlas> spriteAtlases = new Dictionary<string, UITextureAtlas>();

        public Dictionary<string, Color32> colorDefinitions = new Dictionary<string, Color32>(); 

        private string sapphirePath;

        public string SapphirePath
        {
            get { return sapphirePath; }
        }

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

            name = XmlUtil.GetStringAttribute(root, "name");
            author = XmlUtil.GetStringAttribute(root, "author");

            foreach (XmlNode childNode in root.ChildNodes)
            {
                if (childNode.Name == "Module")
                {
                    var modulePath = Path.Combine(sapphirePath, childNode.InnerText);
                    var moduleClass = XmlUtil.GetStringAttribute(childNode, "class");

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

        public void Dispose(ModuleClass moduleClass)
        {
            Rollback(moduleClass);

            foreach (var atlas in spriteAtlases)
            {
                GameObject.Destroy(atlas.Value.material.mainTexture);
                GameObject.Destroy(atlas.Value);
            }

            foreach (var texture in spriteTextureCache)
            {
                GameObject.Destroy(texture.Value);
            }

            spriteAtlases.Clear();
            spriteTextureCache.Clear();
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

                var colorName = XmlUtil.GetStringAttribute(childNode, "name");
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
                    byte r = (byte)((colorHex >> 16) & 0xFF);
                    byte g = (byte)((colorHex >> 8) & 0xFF);
                    byte b = (byte)((colorHex) & 0xFF);
                    color = new Color32(r, g, b, 255);
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

                var atlasName = XmlUtil.GetStringAttribute(childNode, "name");
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
                    var path = spriteNode.InnerText;
                    var name = XmlUtil.GetStringAttribute(spriteNode, "name");
                    Debug.LogWarningFormat("Packing sprite \"{0}\" in atlas", name);

                    if (spriteTextureCache.ContainsKey(path))
                    {
                        continue;
                    }

                    var fullPath = Path.Combine(sapphirePath, path);

                    if (!File.Exists(fullPath))
                    {
                        throw new FileNotFoundException(String.Format("Sprite \"{0}\" not found!", fullPath), fullPath);
                    }

                    var texture = new Texture2D(0, 0);
                    texture.LoadImage(File.ReadAllBytes(fullPath));
                    texture.filterMode = FilterMode.Bilinear;
                    spriteTextureCache.Add(path, texture);

                    atlasPacker.AddSprite(name, texture);
                    count++;
                }

                Debug.LogWarningFormat("Added {0} sprites..", count);
                
                try
                {
                    spriteAtlases[atlasName] = atlasPacker.GenerateAtlas(atlasName);
                }
                catch (AtlasPacker.TooManySprites)
                {
                    Debug.LogError("Too many sprites in atlas \"" + atlasName + "\", move some sprites to a new atlas!");
                    break;
                }

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

        public void ApplyStickyProperties(ModuleClass moduleClass)
        {
            foreach (var module in modules[moduleClass])
            {
                module.ApplyStickyProperties();
            }
        }

        public void Apply(ModuleClass moduleClass)
        {
            foreach (var module in modules[moduleClass])
            {
                module.Apply();
            }
        }

        public void Rollback(ModuleClass moduleClass)
        {
            foreach (var module in modules[moduleClass])
            {
                module.Rollback();
            }
        }

    }
}
