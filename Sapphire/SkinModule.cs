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
            catch (XmlNodeException ex)
            {
                Debug.LogErrorFormat("{0} while parsing skin xml ({1}) at node \"{2}\": {3}",
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

                    var childComponents = FindComponentsInChildren(node, component, nameAttrib.Value, regex, recursive);

                    foreach (var childComponent in childComponents)
                    {
                        ApplyInternalRecursive(childNode, childComponent);
                    }
                }
                else if(component != null)
                {
                    SetPropertyValue(childNode, node, component);
                }
            }
        }

        private void SetPropertyValue(XmlNode setNode, XmlNode node, UIComponent component)
        {
            var rProperty = component.GetType().GetProperty(setNode.Name, BindingFlags.Instance | BindingFlags.Public);

            if (rProperty == null)
            {
                throw new MissingComponentPropertyException(setNode.Name, component, node);
            }

            object value = null;

            if (rProperty.PropertyType == typeof (Color32))
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
                value = GetValueForType(node, rProperty.PropertyType, setNode.InnerText);
            }

            rProperty.SetValue(component, value, null);
        }

        private static List<UIComponent> FindComponentsInChildren(XmlNode node, UIComponent component, string childName, bool regex, bool recursive, int depth = 0)
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

        private object GetValueForType(XmlNode node, Type t, string value)
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

                if (t == typeof(float))
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

                if (t == typeof(string))
                {
                    return value;
                }

                if (t == typeof(Vector2))
                {
                    var values = value.Split(',');
                    if (values.Length != 2)
                    {
                        throw new ParseException("Vector2 definition must have two components", node);
                    }

                    return new Vector2(float.Parse(values[0]), float.Parse(values[1]));
                }

                if (t == typeof(Vector3))
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

                if (t == typeof (UITextureAtlas))
                {
                    var atlasName = value;
                    if (!skin.spriteAtlases.ContainsKey(atlasName))
                    {
                        throw new ParseException(String.Format("Invalid or unknown atlas name \"{0}\"", atlasName), node);
                    }

                    return skin.spriteAtlases[atlasName];
                }
            }
            catch (Exception ex)
            {
                throw new ParseException(String.Format(
                    "Failed to parse value \"{0}\" for type \"{1}\" - {2}", value, t, ex.Message), node);
            }

            throw new UnsupportedTypeException(t, node);
        }

    }

}
