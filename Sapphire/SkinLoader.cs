using System.Collections.Generic;
using System.IO;
using ColossalFramework.Plugins;

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
                var path = plugin.modPath;
                var sapphirePath = Path.Combine(path, "_SapphireSkin");

                if (!Directory.Exists(sapphirePath))
                {
                    continue;
                }

                var metadata = Skin.MetadataFromXmlFile(sapphirePath);
                if (metadata != null)
                {
                    skins.Add(metadata);
                }
            }

            return skins;
        }

    }

}
