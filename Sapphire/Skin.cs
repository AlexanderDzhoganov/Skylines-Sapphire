using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml;
using ColossalFramework.Packaging;
using ColossalFramework.UI;
using UnityEngine;

namespace Sapphire
{

    public class Skin
    {

        public class XmlNodeException : Exception 
        {
            private XmlNode nodeInternal;
            public XmlNode Node
            {
                get { return nodeInternal; }
            }

            public XmlNodeException(XmlNode node)
            {
                nodeInternal = node;
            }
        }

        public class ParseException : XmlNodeException
        {
            private string msgInternal;

            public ParseException(string msg, XmlNode node) : base(node)
            {
                msgInternal = msg;
            }

            public override string ToString()
            {
                return msgInternal;
            }
        }

        public class MissingComponentPropertyException : XmlNodeException
        {
            private string propertyNameInternal;
            private UIComponent componentInternal;

            public string PropertyName
            {
                get { return propertyNameInternal; }
            }

            public UIComponent Component
            {
                get { return componentInternal; }
            }

            public MissingComponentPropertyException(string propertyName, UIComponent component, XmlNode node)
                : base(node)
            {
                propertyNameInternal = propertyName;
                componentInternal = component;
            }

            public override string ToString()
            {
                return String.Format("Missing component property \"{0}\" for component \"{1}\"",
                    propertyNameInternal, componentInternal == null ? "null" : Component.name);
            }
        }

        public class MissingAttributeException : XmlNodeException
        {
            private string attributeInternal;

            public string Attribute
            {
                get { return attributeInternal; }
            }

            public MissingAttributeException(string attribute, XmlNode node) : base(node)
            {
                attributeInternal = attribute;
            }

            public override string ToString()
            {
                return String.Format("Missing or malformed attribute - \"{0}\"", attributeInternal);
            }
        }

        public class MissingAttributeValueException : XmlNodeException
        {
            private string attributeInternal;

            public string Attribute
            {
                get { return attributeInternal; }
            }

            public MissingAttributeValueException(string attribute, XmlNode node) : base(node)
            {
                attributeInternal = attribute;
            }

            public override string ToString()
            {
                return String.Format("Missing or malformed attribute value - \"{0}\"", attributeInternal);
            }
        }

        public class MissingUIComponentException : XmlNodeException
        {
            private string componentNameInternal;
            private UIComponent componentParentInternal;

            public string ComponentName
            {
                get { return componentNameInternal; }
            }

            public UIComponent ComponentParent
            {
                get { return componentParentInternal; }
            }

            public MissingUIComponentException(string componentName, UIComponent parent, XmlNode node) : base(node)
            {
                componentNameInternal = componentName;
                componentParentInternal = parent;
            }

            public override string ToString()
            {
                return String.Format("Missing UI component - \"{0}\" with parent \"{1}\"", 
                    componentNameInternal, componentParentInternal == null ? "None" : componentParentInternal.name);
            }
        }

        public class UnsupportedTypeException : XmlNodeException
        {
            private Type typeInternal;

            public Type Type
            {
                get { return typeInternal; }
            }

            public UnsupportedTypeException(Type type, XmlNode node) : base(node)
            {
                typeInternal = type;
            }

            public override string ToString()
            {
                return String.Format("Unsupported type \"{0}\"", typeInternal);
            }
        }

        private UITextureAtlas atlas;

        private string sourcePath;

        private XmlDocument document;
        private XmlNode rootNode;

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
            rootNode = document.SelectSingleNode("/UIView");
            if (rootNode == null)
            {
                throw new ParseException("Root UIView node missing", null);
            }
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
                var name = GetAttribute(childNode, "name").Value;
                Debug.LogWarningFormat("Replacing sprite \"{0}\" in atlast \"{1}\"", name, uiAtlas.name);

                if (spriteTextureCache.ContainsKey(path))
                {
                    TextureAtlasUtils.ReplaceSprite(uiAtlas, name, spriteTextureCache[path]);
                    continue;
                }

                var widthAttribute = GetAttribute(childNode, "width");
                var heightAttribute = GetAttribute(childNode, "height");

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
            foreach (XmlNode childNode in rootNode.ChildNodes)
            {
                if (childNode.Attributes == null)
                {
                    Debug.LogWarningFormat("Root child with null attributes: \"{0}\"", childNode.Name);
                    continue;
                }

                if (childNode.Name == "Component")
                {
                    var name = GetAttribute(childNode, "name");
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
                    var name = TryGetAttribute(childNode, "name");
                    var recursive = TryGetAttribute(childNode, "recursive");
                    var regex = TryGetAttribute(childNode, "name_regex");

                    if (name != null)
                    {
                        var childComponents = FindComponentsInChildren(node, component, name.Value, regex.Value == "true", recursive.Value == "true");
                        foreach (var childComponent in childComponents)
                        {
                            ApplyInternalRecursive(childNode, childComponent);
                        }
                    }

                    var nameRegex = TryGetAttribute(childNode, "name_regex");
                    if (nameRegex != null)
                    {
                        
                    }
                }
                else if (childNode.Name == "SetProperty")
                {
                    SetPropertyValue(childNode, node, component);
                }
            }
        }

        private static void SetPropertyValue(XmlNode setNode, XmlNode node, UIComponent component)
        {
            var propertyName = GetAttribute(setNode, "name");
            var rProperty = component.GetType().GetProperty(propertyName.Value, BindingFlags.Instance | BindingFlags.Public);

            if (rProperty == null)
            {
                throw new MissingComponentPropertyException(propertyName.Value, component, node);
            }

            rProperty.SetValue(component, GetParsedValueForType(node, rProperty.PropertyType, setNode.InnerText), null);
        }

        private static object GetParsedValueForType(XmlNode node, Type t, string value)
        {
            try
            {
                if (t == typeof(int))
                {
                    return int.Parse(value);
                }

                if (t == typeof(uint))
                {
                    return uint.Parse(value);
                }

                if (t == typeof (float))
                {
                    return float.Parse(value);
                }

                if (t == typeof(double))
                {
                    return double.Parse(value);
                }

                if (t == typeof(bool))
                {
                    if (value == "true") return true;
                    if (value == "false") return false;
                    if (value == "0") return false;
                    if (value == "1") return true;
                    return bool.Parse(value);
                }

                if (t == typeof (string))
                {
                    return value;
                }

                if (t == typeof (Vector2))
                {
                    var values = value.Split(',');
                    if (values.Length != 2)
                    {
                        throw new ParseException("Vector2 definition must have two components", node);
                    }

                    return new Vector2(float.Parse(values[0]), float.Parse(values[1]));
                }

                if (t == typeof (Vector3))
                {
                    var values = value.Split(',');
                    if (values.Length != 3)
                    {
                        throw new ParseException("Vector3 definition must have three components", node);
                    }

                    return new Vector3(float.Parse(values[0]), float.Parse(values[1]), float.Parse(values[2]));
                }

                if (t == typeof(Vector4))
                {
                    var values = value.Split(',');
                    if (values.Length != 4)
                    {
                        throw new ParseException("Vector4 definition must have four components", node);
                    }

                    return new Vector4(float.Parse(values[0]), float.Parse(values[1]), float.Parse(values[2]), float.Parse(values[3]));
                }

                if (t == typeof(Rect))
                {
                    var values = value.Split(',');
                    if (values.Length != 4)
                    {
                        throw new ParseException("Rect definition must have four components", node);
                    }

                    return new Rect(float.Parse(values[0]), float.Parse(values[1]), float.Parse(values[2]), float.Parse(values[3]));
                }

                if (t == typeof(Color))
                {
                    var values = value.Split(',');
                    if (values.Length != 4)
                    {
                        throw new ParseException("Color definition must have four components", node);
                    }

                    return new Color(float.Parse(values[0]), float.Parse(values[1]), float.Parse(values[2]), float.Parse(values[3]));
                }

                if (t == typeof(Color32))
                {
                    var values = value.Split(',');
                    if (values.Length != 4)
                    {
                        throw new ParseException("Color32 definition must have four components", node);
                    }

                    return new Color32(byte.Parse(values[0]), byte.Parse(values[1]), byte.Parse(values[2]), byte.Parse(values[3]));
                }
            }
            catch (Exception)
            {
                throw new ParseException(String.Format(
                    "Failed to parse value \"{0}\" for type \"{1}\"", value, t), node);
            }

            throw new UnsupportedTypeException(t, node);
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

        private static XmlAttribute GetAttribute(XmlNode node, string attributeName)
        {
            XmlAttribute attribute = null;

            try
            {
                attribute = node.Attributes[attributeName];
            }
            catch (Exception) { }

            if (attribute == null)
            {
                throw new MissingAttributeException(attributeName, node);
            }

            if (string.IsNullOrEmpty(attribute.Value))
            {
                throw new MissingAttributeValueException(attributeName, node);
            }

            return attribute;
        }

        private static XmlAttribute TryGetAttribute(XmlNode node, string attributeName)
        {
            XmlAttribute attribute = null;

            try
            {
                attribute = node.Attributes[attributeName];
            }
            catch (Exception)
            {
                return null;
            }

            if (attribute == null)
            {
                return null;
            }

            if (string.IsNullOrEmpty(attribute.Value))
            {
                return null;
            }

            return attribute;
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
