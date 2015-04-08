using System;
using ColossalFramework.UI;
using UnityEngine;

namespace Sapphire
{

    public static class TextureAtlasUtils
    {

        public static void ReplaceSprite(UITextureAtlas atlas, string spriteName, Texture2D replacement)
        {
            var texture = atlas.material.mainTexture as Texture2D;

            if (texture == null)
            {
                throw new AtlasMissingTextureException();
            }

            int spriteIndex = -1;
            for (int i = 0; i < atlas.spriteNames.Length; i++)
            {
                if (atlas.spriteNames[i] == spriteName)
                {
                    spriteIndex = i;
                    break;
                }
            }

            if (spriteIndex < 0 || spriteIndex >= atlas.sprites.Count)
            {
                throw new SpriteNotFoundException(spriteName, atlas);
            }

            var spriteInfo = atlas.sprites[spriteIndex];
            if (spriteInfo.width != replacement.width || spriteInfo.height != replacement.height)
            {
                throw new Exception("Sprite width/ height mismatch");
            }

            var startX = (int)Mathf.Floor(spriteInfo.region.x * texture.width);
            var startY = (int)Mathf.Floor(spriteInfo.region.y * texture.height);

            var replacementPixels = replacement.GetPixels();

            for (int x = 0; x < spriteInfo.pixelSize.x; x++)
            {
                for (int y = 0; y < spriteInfo.pixelSize.y; y++)
                {
                    texture.SetPixel(startX + x, startY + y, replacementPixels[x+y*replacement.width]);
                }
            }

            texture.Apply();
        }
    }

}
