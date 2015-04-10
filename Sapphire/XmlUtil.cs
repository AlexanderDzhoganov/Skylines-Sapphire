using System;
using System.Xml;

namespace Sapphire
{
    public static class XmlUtil
    {

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
