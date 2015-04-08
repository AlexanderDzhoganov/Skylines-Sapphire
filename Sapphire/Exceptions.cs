using System;
using System.Xml;
using ColossalFramework.UI;

namespace Sapphire
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

        public ParseException(string msg, XmlNode node)
            : base(node)
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

        public MissingAttributeException(string attribute, XmlNode node)
            : base(node)
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

        public MissingAttributeValueException(string attribute, XmlNode node)
            : base(node)
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

        public MissingUIComponentException(string componentName, UIComponent parent, XmlNode node)
            : base(node)
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

        public UnsupportedTypeException(Type type, XmlNode node)
            : base(node)
        {
            typeInternal = type;
        }

        public override string ToString()
        {
            return String.Format("Unsupported type \"{0}\"", typeInternal);
        }
    }

    public class AtlasMissingTextureException : Exception
    {
    }

    public class SpriteNotFoundException : Exception
    {
        private UITextureAtlas atlasInternal;
        private string spriteNameInternal;

        public UITextureAtlas Atlas
        {
            get { return atlasInternal; }
        }

        public string SpriteName
        {
            get { return spriteNameInternal; }
        }

        public SpriteNotFoundException(string spriteName, UITextureAtlas atlas)
        {
            spriteNameInternal = spriteName;
            atlasInternal = atlas;
        }

        public override string ToString()
        {
            return String.Format("Failed to find sprite \"{0}\" in atlas \"{1}\"", spriteNameInternal, atlasInternal.name);
        }
    }

}
