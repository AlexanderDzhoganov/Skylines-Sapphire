using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml;
using ColossalFramework.UI;
using UnityEngine;

namespace Sapphire
{

    public class SkinModule
    {

        private string sourcePath;

        public string SourcePath
        {
            get {  return sourcePath; }
        }

        private XmlDocument document;

        public static SkinModule FromXmlFile(string path)
        {
            SkinModule skinModule = null;

            try
            {
                var document = new XmlDocument();
                document.LoadXml(File.ReadAllText(path));
                skinModule = new SkinModule(path, document);
            }
            catch (XmlNodeException ex)
            {
                ErrorLogger.LogErrorFormat("{0} while parsing skin module \"{1}\" at node \"{2}\": {3}", 
                    ex.GetType(), path, XmlUtil.XmlNodeInfo(ex.Node), ex.ToString());
            }
            catch (XmlException ex)
            {
                ErrorLogger.LogErrorFormat("XmlException while parsing skin module \"{0}\" at line {1}, col {2}: {3}",
                    path, ex.LineNumber, ex.LinePosition, ex.Message);
            }
            catch (Exception ex)
            {
                ErrorLogger.LogErrorFormat("{0} while parsing skin module \"{1}\": {2}", ex.GetType(),
                    path, ex.ToString());
            }
           
            return skinModule;
        }

        private SkinModule(string path, XmlDocument _document)
        {
            sourcePath = path;
            document = _document;
        }

        public delegate void ModuleWalkCallback(XmlNode node, UIComponent component);

        public void WalkModule(ModuleWalkCallback visitor)
        {
            var rootNode = document.SelectSingleNode("/UIView");
            if (rootNode == null)
            {
                throw new Exception("SkinModule missing root UIView node");
            }

            WalkModuleInternalRecursive(visitor, rootNode, null);
        }

        private void WalkModuleInternalRecursive(ModuleWalkCallback visitor, XmlNode node, UIComponent component)
        {
            foreach (XmlNode childNode in node.ChildNodes)
            {
                if (childNode.Attributes == null)
                {
                    Debug.LogWarningFormat("Child with null attributes: \"{0}\"", XmlUtil.XmlNodeInfo(childNode));
                    continue;
                }

                if (childNode.Name == "Component")
                {
                    ApplyComponentSelector(visitor, childNode, component);
                }
                else if (component != null)
                {
                    visitor(childNode, component);
                }
                else
                {
                    throw new ParseException("Setting properties on the UIView object is not allowed!", childNode);
                }
            }
        }

        private void ApplyComponentSelector(ModuleWalkCallback visitor, XmlNode node, UIComponent component)
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

                    if (Regex.IsMatch(componentHash, hash))
                    {
                        WalkModuleInternalRecursive(visitor, node, childComponent);
                        break;
                    }

                    continue;
                }

                WalkModuleInternalRecursive(visitor, node, childComponent);
            }
        }

    }

}
