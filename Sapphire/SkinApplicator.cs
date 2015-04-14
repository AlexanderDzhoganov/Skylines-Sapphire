using System;
using System.Collections.Generic;
using System.Net;
using System.Reflection;
using System.Xml;
using ColossalFramework.UI;
using UnityEngine;

namespace Sapphire
{


    public class SkinApplicator
    {

        struct StickyProperty
        {
            public XmlNode childNode;
            public XmlNode node;
            public UIComponent component;
        }

        private Skin skin;

        private List<StickyProperty> stickyProperties = new List<StickyProperty>();

        private delegate void RollbackAction();
        private List<RollbackAction> rollbackStack = new List<RollbackAction>();
        public Dictionary<object, List<KeyValuePair<PropertyInfo, object>>> rollbackDataMap = new Dictionary<object, List<KeyValuePair<PropertyInfo, object>>>();

        private AspectRatio currentAspectRatio = AspectRatio.R16_9;

        public SkinApplicator(Skin _skin)
        {
            skin = _skin;
        }

        private void ApplyUIMultiStateButtonSpriteStateProperty(XmlNode node, UIComponent component)
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

        private void ApplyGenericProperty(XmlNode node, UIComponent component)
        {
            bool optional = XmlUtil.TryGetBoolAttribute(node, "optional");
            bool sticky = XmlUtil.TryGetBoolAttribute(node, "sticky");
            string aspect = XmlUtil.TryGetStringAttribute(node, "aspect", "any");
            if (aspect != "any")
            {
                if (Util.AspectRatioFromString(aspect) != currentAspectRatio)
                {
                    return;
                }
            }

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

            if (!property.CanWrite)
            {
                throw new ParseException(String.Format("Property \"{0}\" of component \"{1}\" is read-only", property.Name, component.name), setNode);
            }

            object value = null;

            bool raw = XmlUtil.TryGetBoolAttribute(setNode, "raw");

            if (property.PropertyType == typeof(Color32) && !raw)
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
            if (!rollbackDataMap.ContainsKey(component))
            {
                rollbackDataMap.Add(component, new List<KeyValuePair<PropertyInfo, object>>());
            }

            bool valueFound = false;
            object originalValue = null;
            foreach (var item in rollbackDataMap[component])
            {
                if (item.Key == property)
                {
                    originalValue = item.Value;
                    valueFound = true;
                    break;
                }
            }

            if (!valueFound)
            {
                originalValue = property.GetValue(component, null);
                rollbackDataMap[component].Add(new KeyValuePair<PropertyInfo, object>(property, originalValue));
            }

            if (originalValue != value)
            {
                SetPropertyValue(component, property, value);

                rollbackStack.Add(() =>
                {
                    property.SetValue(component, originalValue, null);
                });
            }
        }

        private void SetPropertyValue(object component, PropertyInfo property, object value)
        {
            property.SetValue(component, value, null);
        }

        public void ApplyStickyProperties()
        {
            try
            {
                ApplyStickyPropertiesInternal();
            }
            catch (ParseException ex)
            {
                Debug.LogErrorFormat("Error while applying sticky properties for skin \"{1}\" at node \"{2}\": {3}",
                    ex.GetType(), skin.Name, XmlUtil.XmlNodeInfo(ex.Node), ex.ToString());
            }
            catch (XmlNodeException ex)
            {
                Debug.LogErrorFormat("{0} while applying sticky properties for skin \"{1}\" at node \"{2}\": {3}",
                    ex.GetType(), skin.Name, XmlUtil.XmlNodeInfo(ex.Node), ex.ToString());
            }
            catch (Exception ex)
            {
                Debug.LogErrorFormat("{0} while applying sticky properties for skin \"{1}\": {2}", ex.GetType(), skin.Name, ex.ToString());
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
            Debug.LogWarningFormat("Rolling back changes");

            stickyProperties = new List<StickyProperty>();

            try
            {
                RollbackInternal();
            }
            catch (Exception ex)
            {
                Debug.LogErrorFormat("{0} during skin \"{1}\" rollback: {2}", ex.GetType(), skin.Name, ex.ToString());
                return;
            }

            Debug.LogFormat("Skin \"{0}\" successfully rolled back!", skin.Name);
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


        public bool Apply(List<SkinModule> skinModules)
        {
            float aspect = (float)Screen.width/Screen.height;

            if (Util.DeltaCompare(aspect, 1.777f))
            {
                currentAspectRatio = AspectRatio.R16_9;
            }
            else if (Util.DeltaCompare(aspect, 1.6f))
            {
                currentAspectRatio = AspectRatio.R16_10;
            }
            else if (Util.DeltaCompare(aspect, 1.333f))
            {
                currentAspectRatio = AspectRatio.R4_3;
            }
            else if (Util.DeltaCompare(aspect, 2.333f))
            {
                currentAspectRatio = AspectRatio.R16_9;
            }
            else
            {
                currentAspectRatio = AspectRatio.R16_9;
            }

            stickyProperties = new List<StickyProperty>();
            rollbackStack = new List<RollbackAction>();

            foreach (var skinModule in skinModules)
            {
                Debug.LogFormat("Applying skin module \"{0}\"", skinModule.SourcePath);

                try
                {
                    ApplyInternal(skinModule);
                    Debug.LogFormat("Skin module \"{0}\" successfully applied!", skinModule.SourcePath);
                }
                catch (ParseException ex)
                {
                    Debug.LogErrorFormat("Error while applying skin module \"{1}\" at node \"{2}\": {3}",
                        ex.GetType(), skinModule.SourcePath, XmlUtil.XmlNodeInfo(ex.Node), ex.ToString());
                    return false;
                }
                catch (XmlNodeException ex)
                {
                    Debug.LogErrorFormat("{0} while applying skin module \"{1}\" at node \"{2}\": {3}",
                        ex.GetType(), skinModule.SourcePath, XmlUtil.XmlNodeInfo(ex.Node), ex.ToString());
                    return false;
                }
                catch (Exception ex)
                {
                    Debug.LogErrorFormat("{0} while applying skin module \"{1}\": {2}", ex.GetType(), skinModule.SourcePath, ex.ToString());
                    return false;
                }
            }

            return true;
        }

        private void ApplyInternal(SkinModule skinModule)
        {
            skinModule.WalkModule(ApplyInternalRecursive);
        }

        private void ApplyInternalRecursive(XmlNode node, UIComponent component)
        {
            if (component != null)
            {
                if (component.GetType() == typeof(UIMultiStateButton) && node.Name == "SpriteState")
                {
                    ApplyUIMultiStateButtonSpriteStateProperty(node, component);
                }
                else
                {
                    ApplyGenericProperty(node, component);
                }
            }
            else
            {
                throw new ParseException("Setting properties on the UIView object is not allowed!", node);
            }
        }

    }

}
