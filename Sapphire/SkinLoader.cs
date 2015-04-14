using System.Collections.Generic;
using System.IO;
using ColossalFramework.Plugins;
using UnityEngine;

namespace Sapphire
{

  
    public class SkinLoader
    {

        public static List<SkinMetadata> FindAllSkins()
        {
            List<SkinMetadata> skins = new List<SkinMetadata>();

            var plugins = PluginManager.instance.GetPluginsInfo();
            foreach (var plugin in plugins)
            {
                if (!plugin.isEnabled)
                {
                    continue;
                }

                var path = plugin.modPath;
                var sapphirePath = Path.Combine(path, "_SapphireSkin");

                if (!Directory.Exists(sapphirePath))
                {
                    continue;
                }

                if (!File.Exists(Path.Combine(sapphirePath, "skin.xml")))
                {
                    Debug.LogWarningFormat("\"skin.xml\" not found in \"{0}\", skipping", sapphirePath);
                    continue;
                }

                var metadata = Skin.MetadataFromXmlFile(Path.Combine(sapphirePath, "skin.xml"));
                if (metadata != null)
                {
                    skins.Add(metadata);
                }
            }

            return skins;
        }

    }

}
