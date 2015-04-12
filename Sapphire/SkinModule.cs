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
                SetPropertyValue(property.childNode, property.node, property.component, true, false);
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

            Debug.LogWarning("SkinModule successfully rolled back!");
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

            var hash = XmlUtil.TryGetStringAttribute(node, "hash", null);
            
            foreach (var childComponent in childComponents)
            {
                if (hash != null)
                {
                    var componentHash =
                        HashUtil.HashToString(
                            HashUtil.HashRect(new Rect(childComponent.relativePosition.x,
                                childComponent.relativePosition.y,
                                childComponent.size.x, childComponent.size.y)));

                    if (componentHash == hash)
                    {
                        ApplyInternalRecursive(node, childComponent);
                        break;
                    }

                    continue;
                }
             
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
                try
                {
                    var property = ReflectionCache.GetPropertyForType(sprites[index].GetType(), stateNode.Name);
                    if (property == null)
                    {
                        throw new ParseException(String.Format
                        ("Invalid property \"{0}\" for SpriteState, allowed are \"normal\", \"hovered\", \"focused\", \"pressed\", \"disabled\"",
                            stateNode.InnerText), node);
                    }

                    SetPropertyValueWithRollback(sprites[index], property, stateNode.InnerText);
                }
                catch (Exception ex)
                {
                    throw new ParseException(String.Format
                        ("Exception while processing SpriteState node - {0}",
                            ex.ToString()), node);
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

            SetPropertyValue(node, node, component, optional, true);
        }

        private void SetPropertyValue(XmlNode setNode, XmlNode node, UIComponent component, bool optional, bool rollback)
        {
            var property = ReflectionCache.GetPropertyForType(component.GetType(), setNode.Name);

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
                value = XmlUtil.GetValueForType(setNode, property.PropertyType, setNode.InnerText, skin.spriteAtlases);
            }

            if (rollback)
            {
                SetPropertyValueWithRollback(component, property, value);
            }
            else
            {
                SetPropertyValue(component, property, value);
            }
        }

        private void SetPropertyValueWithRollback(object component, PropertyInfo property, object value)
        {
            var originalValue = property.GetValue(component, null);

            rollbackStack.Add(() =>
            {
                property.SetValue(component, originalValue, null);
            });

            if (originalValue != value)
            {
                SetPropertyValue(component, property, value);
            }
        }

        private void SetPropertyValue(object component, PropertyInfo property, object value)
        {
            property.SetValue(component, value, null);
        }

    }

}
