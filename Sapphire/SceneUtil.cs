using System;
using System.IO;
using ColossalFramework.UI;
using UnityEngine;

namespace Sapphire
{
    public static class SceneUtil
    {

        public static void DumpSceneToXML(string path)
        {
            var contents = DumpScene();
            File.WriteAllText(path, contents);
        }

        public static string DumpScene()
        {
            string sceneString = "<UIVIew>\n";

            var uiView = GameObject.FindObjectOfType<UIView>();
            for (int i = 0; i < uiView.transform.childCount; i++)
            {
                var child = uiView.transform.GetChild(i).GetComponent<UIComponent>();
                if (child == null)
                {
                    continue;
                }

                sceneString += DumpComponent(child);
            }

            sceneString += "</UIVIew>";
            return sceneString;
        }

        public static string DumpComponent(UIComponent component, int ident = 0)
        {
            string identString = "";
            for (int i = 0; i < ident; i++)
            {
                identString += "  ";
            }

            string result = "";

            result += String.Format("\n{1}<Component name=\"{0}\">\n", component.name, identString);

            var properties = component.GetType().GetProperties();
            foreach (var property in properties)
            {
                if (!property.CanRead)
                {
                    continue;
                }

                var name = property.Name;
                string value = "[ERROR]";
                try
                {
                    var rawValue = property.GetValue(component, null);
                    if (rawValue == null)
                    {
                        value = "null";
                    }
                    else
                    {
                        value = rawValue.ToString();
                    }
                }
                catch (Exception ex)
                {
                    value = "[ERROR] " + ex.ToString();
                }
                
                result += String.Format("{2}  <{0}>{1}</{0}>\n", name, value, identString);
            }
            
            for (int i = 0; i < component.transform.childCount; i++)
            {
                try
                {
                    result += DumpComponent(component.transform.GetChild(i).GetComponent<UIComponent>(), ident + 1);
                }
                catch (Exception)
                {
                }
            }

            result += String.Format("{0}</Component>\n", identString);
            return result;
        }

    }

}
