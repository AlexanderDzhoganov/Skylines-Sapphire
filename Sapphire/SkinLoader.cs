using System.Collections.Generic;
using System.IO;
using ColossalFramework.Plugins;
using UnityEngine;

namespace Sapphire
{
    
    public class SkinLoader
    {

        public static Skin LoadSkin(string sapphirePath)
        {
            if (!Directory.Exists(sapphirePath))
            {
                Debug.LogErrorFormat("Failed to load skin at path \"{0}\", directory doesn't exist..", sapphirePath);
                return null;
            }

            return Skin.FromXmlFile(Path.Combine(sapphirePath, "skin.xml"));
        }

        public static List<string> FindAllSkins()
        {
            List<string> skinPaths = new List<string>();

            var plugins = PluginManager.instance.GetPluginsInfo();
            foreach (var plugin in plugins)
            {
                var path = plugin.modPath;
                var sapphirePath = Path.Combine(path, "_SapphireSkin");

                if (!Directory.Exists(sapphirePath))
                {
                    continue;
                }

                skinPaths.Add(sapphirePath);
            }

            return skinPaths;
        }

    }

}
