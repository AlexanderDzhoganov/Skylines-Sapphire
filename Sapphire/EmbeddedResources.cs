using System;
using System.Reflection;
using ColossalFramework.UI;
using UnityEngine;

namespace Sapphire
{
    public static class EmbeddedResources
    {

        private static UITextureAtlas atlas;

        public static UITextureAtlas GetSapphireAtlas()
        {
            if (atlas == null)
            {
                var atlasPacker = new AtlasPacker();
                atlasPacker.AddSprite("SapphireIcon", GetTextureResource("SapphireIcon.png"));
                atlasPacker.AddSprite("SapphireIconHover", GetTextureResource("SapphireIconHover.png"));
                atlasPacker.AddSprite("SapphireIconPressed", GetTextureResource("SapphireIconPressed.png"));
                atlasPacker.AddSprite("DefaultPanelBackground", GetTextureResource("DefaultPanelBackground.png"));
                atlas = atlasPacker.GenerateAtlas("SapphireIconsAtlas");
            }

            return atlas;
        }

        private static Texture2D GetTextureResource(string fileName)
        {
            var texture = new Texture2D(0, 0);
            texture.LoadImage(GetResource(String.Format("Sapphire.Resources.{0}", fileName)));
            return texture;
        }

        private static byte[] GetResource(string name)
        {
            var asm = Assembly.GetExecutingAssembly();
            var stream = asm.GetManifestResourceStream(name);
            byte[] data = new byte[stream.Length];
            stream.Read(data, 0, (int)stream.Length);
            return data;
        }

    }

}
