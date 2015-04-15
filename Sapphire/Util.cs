using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using ColossalFramework.UI;
using UnityEngine;

namespace Sapphire
{
    public enum AspectRatio
    {
        R16_9,
        R16_10,
        R4_3,
        R21_9
    }

    public static class Util
    {

        public static AspectRatio AspectRatioFromResolution(float width, float height, AspectRatio defaultAspect = AspectRatio.R16_9)
        {
            float aspect = width/height;
            
            if (DeltaCompare(aspect, 1.777f))
            {
                return AspectRatio.R16_9;
            }
            
            if (DeltaCompare(aspect, 1.6f))
            {
                return AspectRatio.R16_10;
            }

            if (DeltaCompare(aspect, 1.333f))
            {
                return AspectRatio.R4_3;
            }

            if (DeltaCompare(aspect, 2.333f))
            {
                return AspectRatio.R16_9;
            }

            return defaultAspect;
        }

        public static AspectRatio AspectRatioFromString(string aspect)
        {
            if (aspect == null)
            {
                return AspectRatio.R16_9;
            }

            switch (aspect)
            {
                case "16:9":
                    return AspectRatio.R16_9;
                case "16:10":
                    return AspectRatio.R16_10;
                case "4:3":
                    return AspectRatio.R4_3;
                case "21:9":
                    return AspectRatio.R21_9;
            }

            return AspectRatio.R16_9;
        }

        public static bool DeltaCompare(float a, float b, float delta = 0.01f)
        {
            if (Mathf.Abs(a - b) < delta)
            {
                return true;
            }

            return false;
        }

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
