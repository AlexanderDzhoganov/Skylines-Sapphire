using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using ColossalFramework.UI;
using UnityEngine;

namespace Sapphire
{
    public static class Util
    {

        public static List<UIComponent> FindComponentsInChildren(XmlNode node, UIComponent component, string childName, bool regex, bool recursive, bool optional, int depth = 0)
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
                    var childResults = FindComponentsInChildren(node, childComponent, childName, regex, true, optional, depth + 1);
                    results = results.Concat(childResults).ToList();
                }
            }

            if (depth == 0 && !results.Any() && !optional)
            {
                throw new MissingUIComponentException(childName, component, node);
            }

            return results;
        }

    }

}
