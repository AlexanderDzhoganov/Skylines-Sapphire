using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml;
using ColossalFramework.UI;
using UnityEngine;

namespace Sapphire
{

    public class SkinModule
    {

        private string sourcePath;
        private XmlDocument document;
        private Skin skin;

        struct StickyProperty
        {
            public XmlNode childNode;
            public XmlNode node;
            public UIComponent component;
        }

        private List<StickyProperty> stickyProperties = new List<StickyProperty>();

        private delegate void RollbackAction();

        private List<RollbackAction> rollbackStack = new List<RollbackAction>(); 

        public static SkinModule FromXmlFile(Skin skin, string path)
        {
            SkinModule skinModule = null;

            try
            {
                var document = new XmlDocument();
                document.LoadXml(File.ReadAllText(path));
                skinModule = new SkinModule(skin, path, document);
            }
            catch (XmlNodeException ex)
            {
                Debug.LogErrorFormat("{0} while parsing SkinModule xml ({1}) at node \"{2}\": {3}", 
                    ex.GetType(), path, ex.Node == null ? "null" : ex.Node.Name, ex.ToString());
            }
            catch (XmlException ex)
            {
                Debug.LogErrorFormat("XmlException while parsing XML \"{0}\" at line {1}, col {2}: {3}",
                    path, ex.LineNumber, ex.LinePosition, ex.Message);
            }
            catch (Exception ex)
            {
                Debug.LogErrorFormat("Exception while parsing XML \"{0}\": {1}",
                    path, ex.ToString());
            }
           
            return skinModule;
        }

        private SkinModule(Skin _skin, string path, XmlDocument _document)
        {
            skin = _skin;
            sourcePath = path;
            document = _document;
        }

        public void ApplyStickyProperties()
        {
            try
            {
                ApplyStickyPropertiesInternal();
            }
            catch (ParseException ex)
            {
                Debug.LogErrorFormat("Error while applying sticky properties for skin xml ({1}) at node \"{2}\": {3}",
                    ex.GetType(), sourcePath, ex.Node == null ? "null" : ex.Node.Name, ex.ToString());
            }
            catch (XmlNodeException ex)
            {
                Debug.LogErrorFormat("{0} while applying sticky properties for skin xml ({1}) at node \"{2}\": {3}",
                    ex.GetType(), sourcePath, ex.Node == null ? "null" : ex.Node.Name, ex.ToString());
            }
            catch (Exception ex)
            {
                Debug.LogErrorFormat("Exception while applying sticky properties for skin xml ({0}): {1}", sourcePath, ex.Message);
            }
        }

        private void ApplyStickyPropertiesInternal()
        {
            foreach (var property in stickyProperties)
            {
                SetPropertyValue(property.childNode, property.node, property.component, true);
            }
        }

        public void Rollback()
        {
            Debug.LogWarningFormat("Rolling back changes from skin \"{0}\"", sourcePath);

            stickyProperties = new List<StickyProperty>();

            try
            {
                RollbackInternal();
            }
            catch (Exception ex)
            {
                Debug.LogErrorFormat("Exception during skin rollback ({0}): {1}", sourcePath, ex.Message);
            }

            Debug.LogWarning("SkinModule successfully applied!");
        }

        private void RollbackInternal()
        {
            rollbackStack.Reverse();

            foreach (var action in rollbackStack)
            {
                action();
            }

            rollbackStack.Clear();
        }

        public void Apply()
        {
            Debug.LogWarningFormat("Applying skin \"{0}\"", sourcePath);

            stickyProperties = new List<StickyProperty>();

            try
            {
                ApplyInternal();
            }
            catch (ParseException ex)
            {
                Debug.LogErrorFormat("Error while parsing skin xml ({1}) at node \"{2}\": {3}",
                    ex.GetType(), sourcePath, ex.Node == null ? "null" : ex.Node.Name, ex.ToString());
            }
            catch (XmlNodeException ex)
            {
                Debug.LogErrorFormat("{0} while applying skin xml ({1}) at node \"{2}\": {3}",
                    ex.GetType(), sourcePath, ex.Node == null ? "null" : ex.Node.Name, ex.ToString());
            }
            catch (Exception ex)
            {
                Debug.LogErrorFormat("Exception while applying skin xml ({0}): {1}", sourcePath, ex.Message);
            }

            Debug.LogWarning("SkinModule successfully applied!");
        }

        private void ApplyInternal()
        {
            var rootNode = document.SelectSingleNode("/UIView");
            if (rootNode == null)
            {
                throw new Exception("SkinModule missing root UIView node");    
            }

            ApplyInternalRecursive(rootNode, null);
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
                    ApplyComponentSelector(childNode, component);
                }
                else if(component != null)
                {
                    if (component.GetType() == typeof(UIMultiStateButton) && childNode.Name == "SpriteState")
                    {
                        ApplyUIMultiStateButtonSpriteStateProperty(childNode, component);
                    }
                    else
                    {
                        ApplyGenericProperty(childNode, component);
                    }
                }
                else
                {
                    throw new ParseException("Setting properties on the UIView object is not allowed!", childNode);
                }
            }
        }

        void ApplyComponentSelector(XmlNode node, UIComponent component)
        {
            var name = XmlUtil.GetStringAttribute(node, "name");

            bool regex = XmlUtil.TryGetBoolAttribute(node, "name_regex");
            bool recursive = XmlUtil.TryGetBoolAttribute(node, "recursive");
            bool optional = XmlUtil.TryGetBoolAttribute(node, "optional");

            var childComponents = Util.FindComponentsInChildren(node, component, name, regex, recursive, optional);

            foreach (var childComponent in childComponents)
            {
                ApplyInternalRecursive(node, childComponent);
            }
        }

        void ApplyUIMultiStateButtonSpriteStateProperty(XmlNode node, UIComponent component)
        {
            var index = XmlUtil.GetIntAttribute(node, "index");

            var type = XmlUtil.GetStringAttribute(node, "type");
            if (type != "background" && type != "foreground")
            {
                throw new ParseException(String.Format
                    ("Invalid value for SpriteState attribute \"type\" (only \"foreground\" and \"background\" are allowed - \"{0}\"",
                        index), node);
            }

            var button = component as UIMultiStateButton;
            UIMultiStateButton.SpriteSetState sprites = null;

            if (type == "background")
            {
                sprites = button.backgroundSprites;
            }
            else
            {
                sprites = button.foregroundSprites;
            }

            if (index >= sprites.Count)
            {
                throw new ParseException(String.Format
                ("Invalid value for SpriteState attribute \"index\", object has only \"{1}\" states - \"{0}\"",
                   index, sprites.Count), node);
            }

            foreach (XmlNode stateNode in node.ChildNodes)
            {
                if (stateNode.Name == "normal")
                {
                    sprites[index].normal = stateNode.InnerText;
                }
                else if (stateNode.Name == "hovered")
                {
                    sprites[index].hovered = stateNode.InnerText;
                }
                else if (stateNode.Name == "focused")
                {
                    sprites[index].focused = stateNode.InnerText;
                }
                else if (stateNode.Name == "pressed")
                {
                    sprites[index].pressed = stateNode.InnerText;
                }
                else if (stateNode.Name == "disabled")
                {
                    sprites[index].disabled = stateNode.InnerText;
                }
                else
                {
                    throw new ParseException(String.Format
                        ("Invalid property \"{0}\" for SpriteState, allowed are \"normal\", \"hovered\", \"focused\", \"pressed\", \"disabled\",",
                            stateNode.InnerText), node);
                }
            }
        }

        void ApplyGenericProperty(XmlNode node, UIComponent component)
        {
            bool optional = XmlUtil.TryGetBoolAttribute(node, "optional");
            bool sticky = XmlUtil.TryGetBoolAttribute(node, "sticky");

            if (sticky)
            {
                stickyProperties.Add(new StickyProperty
                {
                    childNode = node,
                    component = component,
                    node = node
                });
            }

            SetPropertyValue(node, node, component, optional);
        }

        private void SetPropertyValue(XmlNode setNode, XmlNode node, UIComponent component, bool optional)
        {
            var property = component.GetType().GetProperty(setNode.Name, BindingFlags.Instance | BindingFlags.Public);

            if (property == null)
            {
                if (optional)
                {
                    return;
                }

                throw new MissingComponentPropertyException(setNode.Name, component, node);
            }

            object value = null;

            bool raw = XmlUtil.TryGetBoolAttribute(setNode, "raw");

            if (property.PropertyType == typeof (Color32) && !raw)
            {
                var colorName = setNode.InnerText;
                if (!skin.colorDefinitions.ContainsKey(colorName))
                {
                    throw new ParseException(String.Format("Invalid or undefined color name \"{0}\"", colorName), setNode);
                }

                value = skin.colorDefinitions[colorName];
            }
            else
            {
                value = GetValueForType(setNode, property.PropertyType, setNode.InnerText);
            }

            SetPropertyValueWithRollback(component, property, value);
        }

        private void SetPropertyValueWithRollback(UIComponent component, PropertyInfo property, object value)
        {
            var originalValue = property.GetValue(component, null);

            rollbackStack.Add(() =>
            {
                property.SetValue(component, originalValue, null);
            });

            property.SetValue(component, value, null);
        }

        private object GetValueForType(XmlNode node, Type t, string value)
        {
            if (t == typeof(int))
            {
                if (string.IsNullOrEmpty(value))
                {
                    throw new ParseException(String.Format("Empty value for type \"{0}\" is not allowed", t), node);
                }

                int result;
                if(!int.TryParse(value, out result))
                {
                    throw new ParseException("Incorrect format for integer value", node);
                }

                return result;
            }

            if (t == typeof(uint))
            {
                if (string.IsNullOrEmpty(value))
                {
                    throw new ParseException(String.Format("Empty value for type \"{0}\" is not allowed", t), node);
                }

                uint result;
                if (!uint.TryParse(value, out result))
                {
                    throw new ParseException("Incorrect format for integer value", node);
                }

                return result;
            }

            if (t == typeof(float))
            {
                if (string.IsNullOrEmpty(value))
                {
                    throw new ParseException(String.Format("Empty value for type \"{0}\" is not allowed", t), node);
                }

                return float.Parse(value);
            }

            if (t == typeof(double))
            {
                if (string.IsNullOrEmpty(value))
                {
                    throw new ParseException(String.Format("Empty value for type \"{0}\" is not allowed", t), node);
                }

                return double.Parse(value);
            }

            if (t == typeof(bool))
            {
                if (string.IsNullOrEmpty(value))
                {
                    throw new ParseException(String.Format("Empty value for type \"{0}\" is not allowed", t), node);
                }

                if (value == "true") return true;
                if (value == "false") return false;
                if (value == "0") return false;
                if (value == "1") return true;

                bool result;
                if (!bool.TryParse(value, out result))
                {
                    throw new ParseException("Incorrect format for boolean value", node);
                }

                return result;
            }

            if (t == typeof(string))
            {
                return value;
            }

            if (t == typeof(Vector2))
            {
                if (string.IsNullOrEmpty(value))
                {
                    throw new ParseException(String.Format("Empty value for type \"{0}\" is not allowed", t), node);
                }

                var values = value.Split(',');
                if (values.Length != 2)
                {
                    throw new ParseException("Vector2 definition must have two components", node);
                }

                return new Vector2(float.Parse(values[0]), float.Parse(values[1]));
            }

            if (t == typeof(Vector3))
            {
                if (string.IsNullOrEmpty(value))
                {
                    throw new ParseException(String.Format("Empty value for type \"{0}\" is not allowed", t), node);
                }

                var values = value.Split(',');
                if (values.Length < 2 || values.Length > 4)
                {
                    throw new ParseException("Vector3 definition must have two or three components", node);
                }

                return new Vector3(float.Parse(values[0]), float.Parse(values[1]), values.Length == 3 ? float.Parse(values[2]) : 0.0f);
            }

            if (t == typeof(Vector4))
            {
                if (string.IsNullOrEmpty(value))
                {
                    throw new ParseException(String.Format("Empty value for type \"{0}\" is not allowed", t), node);
                }

                var values = value.Split(',');
                if (values.Length != 4)
                {
                    throw new ParseException("Vector4 definition must have four components", node);
                }

                return new Vector4(float.Parse(values[0]), float.Parse(values[1]), float.Parse(values[2]), float.Parse(values[3]));
            }

            if (t == typeof(Rect))
            {
                if (string.IsNullOrEmpty(value))
                {
                    throw new ParseException(String.Format("Empty value for type \"{0}\" is not allowed", t), node);
                }

                var values = value.Split(',');
                if (values.Length != 4)
                {
                    throw new ParseException("Rect definition must have four components", node);
                }

                return new Rect(float.Parse(values[0]), float.Parse(values[1]), float.Parse(values[2]), float.Parse(values[3]));
            }

            if (t == typeof(RectOffset))
            {
                if (string.IsNullOrEmpty(value))
                {
                    throw new ParseException(String.Format("Empty value for type \"{0}\" is not allowed", t), node);
                }

                var values = value.Split(',');
                if (values.Length != 4)
                {
                    throw new ParseException("RectOffset definition must have four components", node);
                }

                RectOffset result;

                try
                {
                    result = new RectOffset(int.Parse(values[0]), int.Parse(values[1]), int.Parse(values[2]), int.Parse(values[3]));
                }
                catch (Exception)
                {
                    throw new ParseException("RectOffset can contain only integer values", node);
                }

                return result;
            }

            if (t == typeof(Color))
            {
                if (string.IsNullOrEmpty(value))
                {
                    throw new ParseException(String.Format("Empty value for type \"{0}\" is not allowed", t), node);
                }

                var values = value.Split(',');
                if (values.Length != 4)
                {
                    throw new ParseException("Color definition must have four components", node);
                }

                return new Color(float.Parse(values[0]), float.Parse(values[1]), float.Parse(values[2]), float.Parse(values[3]));
            }

            if (t == typeof(Color32))
            {
                if (string.IsNullOrEmpty(value))
                {
                    throw new ParseException(String.Format("Empty value for type \"{0}\" is not allowed", t), node);
                }

                var values = value.Split(',');
                if (values.Length != 4)
                {
                    throw new ParseException("Color32 definition must have four components", node);
                }

                Color32 result;
                try
                {
                    result = new Color32(byte.Parse(values[0]), byte.Parse(values[1]), byte.Parse(values[2]), byte.Parse(values[3]));
                }
                catch (Exception)
                {
                    throw new ParseException("Color32 can contain only byte values", node);
                }

                return result;
            }

            if (t == typeof (UITextureAtlas))
            {
                if (string.IsNullOrEmpty(value))
                {
                    throw new ParseException(String.Format("Empty value for type \"{0}\" is not allowed", t), node);
                }

                var atlasName = value;
                if (!skin.spriteAtlases.ContainsKey(atlasName))
                {
                    throw new ParseException(String.Format("Invalid or unknown atlas name \"{0}\"", atlasName), node);
                }

                return skin.spriteAtlases[atlasName];
            }

            if (t.IsEnum)
            {
                if (t.GetCustomAttributes(typeof (FlagsAttribute), false).Length > 0 && value.Contains("|"))
                {
                    var values = value.Split('|');
                    int result = 0;

                    foreach (var item in values)
                    {
                        var realValue = Enum.Parse(t, item);
                        result |= (int)realValue;
                    }

                    return result;
                }
                else
                {
                    return Enum.Parse(t, value);
                }
            }   

            throw new UnsupportedTypeException(t, node);
        }

    }

}
