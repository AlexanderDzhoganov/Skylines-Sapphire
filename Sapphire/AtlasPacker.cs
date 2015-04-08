using System;
using System.Collections.Generic;
using ColossalFramework.UI;
using UnityEngine;

namespace Sapphire
{

    public class AtlasPacker
    {

        private List<KeyValuePair<string, Texture2D>> rawSprites = new List<KeyValuePair<string, Texture2D>>();

        public void AddSprite(string name, Texture2D texture)
        {
            rawSprites.Add(new KeyValuePair<string, Texture2D>(name, texture));
        }

        public UITextureAtlas GenerateAtlas()
        {
            SortSprites();

            var atlas = ScriptableObject.CreateInstance<UITextureAtlas>();
            atlas.material = new Material(Shader.Find("UI/Default UI Shader"));

            var atlasTexture = new Texture2D(2048, 2048);
            atlas.material.mainTexture = atlasTexture;

            for (int _x = 0; _x < atlasTexture.width; _x++)
            {
                for (int _y = 0; _y < atlasTexture.height; _y++)
                {
                    atlasTexture.SetPixel(_x, _y, Color.black);
                }
            }

            int x = 0;
            int y = 0;
            int maxY = 0;

            foreach (var item in rawSprites)
            {
                var name = item.Key;
                var texture = item.Value;

                if (x + texture.width <= atlasTexture.width)
                {
                    x = 0;
                    y += maxY;
                    maxY = 0;
                }

                float u = (float)x / atlasTexture.width;
                float v = (float) y/atlasTexture.height;
                float s = u + (float)(texture.width) / atlasTexture.width;
                float t = v + (float)(texture.height) / atlasTexture.height;
                
                var sprite = new UITextureAtlas.SpriteInfo();
                sprite.region = new Rect(u, v, s, t);
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

                x += texture.width;
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
                return areaA.CompareTo(areaB);
            });
        }

    }

}
