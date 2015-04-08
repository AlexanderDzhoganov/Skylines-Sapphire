using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml;
using ColossalFramework.UI;
using UnityEngine;

namespace Sapphire
{

    public class Skin
    {

        private string sourcePath;

        private XmlDocument document;

        private Dictionary<string, Texture2D> spriteTextureCache = new Dictionary<string, Texture2D>(); 

        public static Skin FromXmlFile(string path)
        {
            Skin skin = null;

            try
            {
                var document = new XmlDocument();
                document.LoadXml(File.ReadAllText(path));
                skin = new Skin(path, document);
            }
            catch (XmlNodeException ex)
            {
                Debug.LogErrorFormat("{0} while parsing skin xml ({1}) at node \"{2}\": {3}", 
                    ex.GetType(), path, ex.Node == null ? "null" : ex.Node.ToString(), ex.ToString());
            }
            catch (Exception ex)
            {
                Debug.LogErrorFormat("Exception while parsing skin xml ({0}): {1}", path, ex.Message);
            }

            return skin;
        }

        private Skin(string path, XmlDocument _document)
        {
            sourcePath = path;
            document = _document;
        }

        public void LoadSprites()
        {
            try
            {
                LoadSpritesInternal();
            }
            catch (XmlNodeException ex)
            {
                Debug.LogErrorFormat("{0} while loading sprites for skin ({1}) at node \"{2}\": {3}",
                    ex.GetType(), sourcePath, ex.Node == null ? "null" : ex.Node.ToString(), ex.ToString());
            }
            catch (Exception ex)
            {
                Debug.LogErrorFormat("Exception while loading sprites for skin ({0}): {1}", sourcePath, ex.Message);
            }
        }

        private void LoadSpritesInternal()
        {
            Debug.LogWarning("Loading sprites");

            var spritesNode = document.SelectSingleNode("/UIView/Sprites");
            if (spritesNode == null)
            {
                Debug.LogWarning("No sprites defined for skin");
                return;
            }

            var uiAtlas = GetUIAtlas();
            if (uiAtlas == null)
            {
                Debug.LogError("Failed to find UI atlas, cannot replace sprites..");
                return;
            }

            int count = 0;

            foreach (XmlNode childNode in spritesNode)
            {
                var path = childNode.InnerText;
                var name = XmlUtil.GetAttribute(childNode, "name").Value;
                Debug.LogWarningFormat("Replacing sprite \"{0}\" in atlast \"{1}\"", name, uiAtlas.name);

                if (spriteTextureCache.ContainsKey(path))
                {
                    TextureAtlasUtils.ReplaceSprite(uiAtlas, name, spriteTextureCache[path]);
                    continue;
                }

                var widthAttribute = XmlUtil.GetAttribute(childNode, "width");
                var heightAttribute = XmlUtil.GetAttribute(childNode, "height");

                int width = -1;
                int height = -1;

                if (!int.TryParse(widthAttribute.Value, out width))
                {
                    throw new MissingAttributeValueException("width", childNode);
                }

                if (!int.TryParse(heightAttribute.Value, out height))
                {
                    throw new MissingAttributeValueException("height", childNode);
                }

                var fullPath = Path.Combine(Path.GetDirectoryName(sourcePath), path);

                var texture = new Texture2D(width, height);
                texture.LoadImage(File.ReadAllBytes(fullPath));
                spriteTextureCache.Add(path, texture);
                TextureAtlasUtils.ReplaceSprite(uiAtlas, name, texture);
                count++;
            }

            Debug.LogWarningFormat("Replaced {0} sprites..", count);
        }

        public void Apply()
        {
            Debug.LogWarningFormat("Applying skin \"{0}\"", sourcePath);

            try
            {
                ApplyInternal();
            }
            catch (XmlNodeException ex)
            {
                Debug.LogErrorFormat("{0} while parsing skin xml ({1}) at node \"{2}\": {3}",
                    ex.GetType(), sourcePath, ex.Node == null ? "null" : ex.Node.ToString(), ex.ToString());
            }
            catch (Exception ex)
            {
                Debug.LogErrorFormat("Exception while applying skin xml ({0}): {1}", sourcePath, ex.Message);
            }

            Debug.LogWarning("Skin successfully applied!");
        }

        private void ApplyInternal()
        {
            var rootNode = document.SelectSingleNode("/UIView");
            if (rootNode == null)
            {
                throw new Exception("Skin missing root UIView node");    
            }

            foreach (XmlNode childNode in rootNode.ChildNodes)
            {
                if (childNode.Attributes == null)
                {
                    Debug.LogWarningFormat("Root child with null attributes: \"{0}\"", childNode.Name);
                    continue;
                }

                if (childNode.Name == "Component")
                {
                    var name = XmlUtil.GetAttribute(childNode, "name");
                    var component = GameObject.Find(name.Value).GetComponent<UIComponent>();

                    if (component == null)
                    {
                        throw new MissingUIComponentException(name.Value, null, childNode);
                    }

                    ApplyInternalRecursive(childNode, component);
                }
            }
        }

        private void ApplyInternalRecursive(XmlNode node, UIComponent component)
        {
            foreach (XmlNode childNode in node.ChildNodes)
            {
                if (childNode.Attributes == null)
                {
                    Debug.LogWarningFormat("Child with null attributes: \"{0}\"", childNode.Name);
                    continue;
                }

                if (childNode.Name == "Component")
                {
                    var name = XmlUtil.TryGetAttribute(childNode, "name");
                    var recursive = XmlUtil.TryGetAttribute(childNode, "recursive");
                    var regex = XmlUtil.TryGetAttribute(childNode, "name_regex");

                    if (name != null)
                    {
                        var childComponents = FindComponentsInChildren(node, component, name.Value, regex.Value == "true", recursive.Value == "true");
                        foreach (var childComponent in childComponents)
                        {
                            ApplyInternalRecursive(childNode, childComponent);
                        }
                    }
                }
                else
                {
                    SetPropertyValue(childNode, node, component);
                }
            }
        }

        private static void SetPropertyValue(XmlNode setNode, XmlNode node, UIComponent component)
        {
            var rProperty = component.GetType().GetProperty(setNode.Name, BindingFlags.Instance | BindingFlags.Public);

            if (rProperty == null)
            {
                throw new MissingComponentPropertyException(setNode.Name, component, node);
            }

            rProperty.SetValue(component, ValueParser.GetValueForType(node, rProperty.PropertyType, setNode.InnerText), null);
        }

        private static List<UIComponent> FindComponentsInChildren(XmlNode node, UIComponent component, string childName, bool regex, bool recursive, int depth = 0)
        {
            var results = new List<UIComponent>();

            for (int i = 0; i < component.gameObject.transform.childCount; i++)
            {
                var child = component.gameObject.transform.GetChild(i);
                var childComponent = child.GetComponent<UIComponent>();

                if (!regex)
                {
                    if (childComponent != null && childComponent.name == childName)
                    {
                        results.Add(childComponent);
                    }
                }
                else
                {
                    if (childComponent != null && Regex.IsMatch(childComponent.name, childName))
                    {
                        results.Add(childComponent);
                    }
                }

                if (recursive)
                {
                    var childResults = FindComponentsInChildren(node, childComponent, childName, regex, true, depth+1);
                    results = results.Concat(childResults).ToList();
                }
            }

            if (depth == 0 && !results.Any())
            {
                throw new MissingUIComponentException(childName, component, node);
            }

            return results;
        }

        private static UITextureAtlas GetUIAtlas()
        {
            var go = GameObject.Find("(Library) OptionsPanel");
            if (go != null)
            {
                return go.GetComponent<UIPanel>().atlas;
            }

            return null;
        }

    }

}
