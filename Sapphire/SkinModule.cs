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

    public class SkinModule
    {

        private string sourcePath;
        private XmlDocument document;
        private Skin skin;

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
            catch (Exception ex)
            {
                Debug.LogErrorFormat("Exception while parsing SkinModule xml ({0}): {1}", path, ex.Message);
            }

            return skinModule;
        }

        private SkinModule(Skin _skin, string path, XmlDocument _document)
        {
            skin = _skin;
            sourcePath = path;
            document = _document;
        }

        public void Apply()
        {
            Debug.LogWarningFormat("Applying skin \"{0}\"", sourcePath);

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
                    var nameAttrib = XmlUtil.GetAttribute(childNode, "name");

                    var regexAttrib = XmlUtil.TryGetAttribute(childNode, "name_regex");
                    bool regex = regexAttrib != null && regexAttrib.Value == "true";

                    var recursiveAttrib = XmlUtil.TryGetAttribute(childNode, "recursive");
                    bool recursive = recursiveAttrib != null && recursiveAttrib.Value == "true";

                    var optionalAttrib = XmlUtil.TryGetAttribute(childNode, "optional");
                    bool optional = optionalAttrib != null && optionalAttrib.Value == "true";

                    var childComponents = FindComponentsInChildren(node, component, nameAttrib.Value, regex, recursive, optional);

                    foreach (var childComponent in childComponents)
                    {
                        ApplyInternalRecursive(childNode, childComponent);
                    }
                }
                else if(component != null)
                {
                    var optionalAttrib = XmlUtil.TryGetAttribute(childNode, "optional");
                    bool optional = optionalAttrib != null && optionalAttrib.Value == "true";

                    SetPropertyValue(childNode, node, component, optional);
                }
            }
        }

        private void SetPropertyValue(XmlNode setNode, XmlNode node, UIComponent component, bool optional)
        {
            var rProperty = component.GetType().GetProperty(setNode.Name, BindingFlags.Instance | BindingFlags.Public);

            if (rProperty == null)
            {
                if (optional)
                {
                    return;
                }

                throw new MissingComponentPropertyException(setNode.Name, component, node);
            }

            object value = null;

            var rawAttrib = XmlUtil.TryGetAttribute(setNode, "raw");
            bool raw = rawAttrib != null && rawAttrib.Value == "true";

            if (rProperty.PropertyType == typeof (Color32) && !raw)
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
                value = GetValueForType(setNode, rProperty.PropertyType, setNode.InnerText);
            }

            rProperty.SetValue(component, value, null);
        }

        private static List<UIComponent> FindComponentsInChildren(XmlNode node, UIComponent component, string childName, bool regex, bool recursive, bool optional, int depth = 0)
        {
            var results = new List<UIComponent>();

            Transform parentTransform = null;

            if (component == null)
            {
                parentTransform = GameObject.FindObjectOfType<UIView>().gameObject.transform;
            }
            else
            {
                parentTransform = component.transform;
            }

            for (int i = 0; i < parentTransform.childCount; i++)
            {
                var child = parentTransform.GetChild(i);
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
                    var childResults = FindComponentsInChildren(node, childComponent, childName, regex, true, optional, depth+1);
                    results = results.Concat(childResults).ToList();
                }
            }

            if (depth == 0 && !results.Any() && !optional)
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

        private object GetValueForType(XmlNode node, Type t, string value)
        {
            if (t == typeof(int))
            {
                if (string.IsNullOrEmpty(value))
                {
                    throw new ParseException(String.Format("Empty value for type \"{0}\" is not allowed", t), node);
                }

                return int.Parse(value);
            }

            if (t == typeof(uint))
            {
                if (string.IsNullOrEmpty(value))
                {
                    throw new ParseException(String.Format("Empty value for type \"{0}\" is not allowed", t), node);
                }

                return uint.Parse(value);
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
                return bool.Parse(value);
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

                return new RectOffset(int.Parse(values[0]), int.Parse(values[1]), int.Parse(values[2]), int.Parse(values[3]));
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

                return new Color32(byte.Parse(values[0]), byte.Parse(values[1]), byte.Parse(values[2]), byte.Parse(values[3]));
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
                return Enum.Parse(t, value);
            }   

            throw new UnsupportedTypeException(t, node);
        }

    }

}
