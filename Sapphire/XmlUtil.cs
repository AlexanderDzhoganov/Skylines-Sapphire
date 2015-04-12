using System;
using System.Collections.Generic;
using System.Xml;
using ColossalFramework.UI;
using UnityEngine;

namespace Sapphire
{
    public static class XmlUtil
    {

        public static string XmlNodeInfo(XmlNode node)
        {
            if (node == null)
            {
                return "null";
            }

            using (var sw = new System.IO.StringWriter())
            {
                if (node.ParentNode != null)
                {
                    try
                    {
                        sw.WriteLine("<{0} name=\"{1}\">", node.ParentNode.Name, node.ParentNode.Attributes["name"].Value);
                    }
                    catch (Exception)
                    {
                    }
                }

                try
                {
                    sw.WriteLine("  <{0} name=\"{1}\">..</{0}", node.Name, node.Attributes["name"].Value);
                }
                catch (Exception)
                {
                }

                if (node.ParentNode != null)
                {
                    sw.WriteLine("</{0}>", node.ParentNode.Name);
                }

                return sw.ToString();
            }
        }

        public static object GetValueForType(XmlNode node, Type t, string value, Dictionary<string, UITextureAtlas> spriteAtlases)
        {
            if (t == typeof(int))
            {
                if (string.IsNullOrEmpty(value))
                {
                    throw new ParseException(String.Format("Empty value for type \"{0}\" is not allowed", t), node);
                }

                int result;
                if (!int.TryParse(value, out result))
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

            if (t == typeof(UITextureAtlas))
            {
                if (string.IsNullOrEmpty(value))
                {
                    throw new ParseException(String.Format("Empty value for type \"{0}\" is not allowed", t), node);
                }

                var atlasName = value;
                if (!spriteAtlases.ContainsKey(atlasName))
                {
                    throw new ParseException(String.Format("Invalid or unknown atlas name \"{0}\"", atlasName), node);
                }

                return spriteAtlases[atlasName];
            }

            if (t.IsEnum)
            {
                if (t.GetCustomAttributes(typeof(FlagsAttribute), false).Length > 0 && value.Contains("|"))
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

        public static string GetStringAttribute(XmlNode node, string attributeName)
        {
            return GetAttribute(node, attributeName).Value;
        }

        public static string TryGetStringAttribute(XmlNode node, string attributeName, string defaultValue = "")
        {
            XmlAttribute attribute = TryGetAttribute(node, attributeName);
            if (attribute == null)
            {
                return defaultValue;
            }

            return attribute.Value;
        }

        public static bool GetBoolAttribute(XmlNode node, string attributeName)
        {
            XmlAttribute attribute = GetAttribute(node, attributeName);
            if (attribute.Value == "true")
            {
                return true;
            }
            
            if (attribute.Value == "false")
            {
                return false;
            }

            throw new ParseException(String.Format
                ("\"{0}\" is not a valid value for boolean attribute \"{1}\", expected \"true\" or \"false\"",
                attribute.Value, attributeName), node);
        }

        public static bool TryGetBoolAttribute(XmlNode node, string attributeName, bool defaultValue = false)
        {
            XmlAttribute attribute = TryGetAttribute(node, attributeName);
            if (attribute == null)
            {
                return defaultValue;
            }

            if (attribute.Value == "true")
            {
                return true;
            }

            if (attribute.Value == "false")
            {
                return false;
            }

            throw new ParseException(String.Format
                ("\"{0}\" is not a valid value for boolean attribute \"{1}\", expected \"true\" or \"false\"",
                attribute.Value, attributeName), node);
        }

        public static int GetIntAttribute(XmlNode node, string attributeName)
        {
            XmlAttribute attribute = GetAttribute(node, attributeName);

            int result;
            if (!int.TryParse(attribute.Value, out result))
            {
                throw new ParseException(String.Format
                    ("\"{0}\" is not a valid value for integer attribute \"{1}\", expected an integer",
                        attribute.Value, attributeName), node);
            }

            return result;
        }

        public static int TryGetIntAttribute(XmlNode node, string attributeName, int defaultValue = 0)
        {
            XmlAttribute attribute = TryGetAttribute(node, attributeName);
            if (attribute == null)
            {
                return defaultValue;
            }

            int result;
            if (!int.TryParse(attribute.Value, out result))
            {
                throw new ParseException(String.Format
                    ("\"{0}\" is not a valid value for integer attribute \"{1}\", expected an integer",
                        attribute.Value, attributeName), node);
            }

            return result;
        }

        private static XmlAttribute GetAttribute(XmlNode node, string attributeName)
        {
            XmlAttribute attribute = null;

            if (node.Attributes == null)
            {
                throw new MissingAttributeException(attributeName, node);
            }

            try
            {
                attribute = node.Attributes[attributeName];
            }
            catch (Exception)
            {
                throw new MissingAttributeException(attributeName, node);
            }

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

    }

}
