using System;
using System.Xml;

namespace Sapphire
{
    public static class XmlUtil
    {

        public static XmlAttribute GetAttribute(XmlNode node, string attributeName)
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

        public static XmlAttribute TryGetAttribute(XmlNode node, string attributeName)
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
