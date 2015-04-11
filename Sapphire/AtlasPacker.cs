using System;
using System.Collections.Generic;
using ColossalFramework.UI;
using UnityEngine;

namespace Sapphire
{

    public class AtlasPacker
    {

        public class TooManySprites : Exception
        {
        }

        private readonly List<KeyValuePair<string, Texture2D>> rawSprites = new List<KeyValuePair<string, Texture2D>>();

        public void AddSprite(string name, Texture2D texture)
        {
            rawSprites.Add(new KeyValuePair<string, Texture2D>(name, texture));
        }

        public UITextureAtlas GenerateAtlas(string atlasName)
        {
            SortSprites();

            var atlas = ScriptableObject.CreateInstance<UITextureAtlas>();
            atlas.material = new Material(Shader.Find("UI/Default UI Shader"));

            var atlasTexture = new Texture2D(2048, 2048, TextureFormat.ARGB32, false, true);
            atlasTexture.filterMode = FilterMode.Point;
            atlas.material.mainTexture = atlasTexture;

            var transparent = new Color(0, 0, 0, 0);
            for (int _x = 0; _x < atlasTexture.width; _x++)
            {
                for (int _y = 0; _y < atlasTexture.height; _y++)
                {
                    atlasTexture.SetPixel(_x, _y, transparent);
                }
            }

            int x = 2;
            int y = 2;
            int maxY = 0;

            foreach (var item in rawSprites)
            {
                var name = item.Key;
                var texture = item.Value;

                if (x + texture.width >= atlasTexture.width)
                {
                    x = 0;
                    y += maxY + 2;
                    maxY = 0;

                    if (y >= atlasTexture.height)
                    {
                        throw new TooManySprites();
                    }
                }

                float u = (float)x/atlasTexture.width;
                float v = (float)y/atlasTexture.height;
                float s = (float)(texture.width) / atlasTexture.width;
                float t = (float)(texture.height) / atlasTexture.height;

                float pixelSize = 1.0f/atlasTexture.width;

                var sprite = new UITextureAtlas.SpriteInfo();
                sprite.region = new Rect(u + pixelSize, v + pixelSize, s - pixelSize * 2.0f, t - pixelSize * 2.0f);
                sprite.name = name;
                sprite.texture = texture;
                atlas.AddSprite(sprite);

                for (int _x = 0; _x < texture.width; _x++)
                {
                    for (int _y = 0; _y < texture.height; _y++)
                    {
                        atlasTexture.SetPixel(x+_x, y+_y, texture.GetPixel(_x, _y));
                    }
                }

                x += texture.width + 2;
                maxY = Mathf.Max(maxY, texture.height);
            }

            atlasTexture.Apply();
            return atlas;
        }


        private void SortSprites()
        {
            rawSprites.Sort((a, b) =>
            {
                var areaA = a.Value.width*a.Value.height;
                var areaB = b.Value.width*b.Value.height;
                return areaB.CompareTo(areaA);
            });
        }

    }

}
