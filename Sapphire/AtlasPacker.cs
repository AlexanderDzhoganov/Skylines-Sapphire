using System;
using System.Collections.Generic;
using System.IO;
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
        private readonly List<KeyValuePair<string, string>> pathSprites = new List<KeyValuePair<string, string>>();

        private readonly Dictionary<string, KeyValuePair<Texture2D, Rect>> spriteCache = new Dictionary<string, KeyValuePair<Texture2D, Rect>>();

        public void AddSprite(string name, Texture2D texture)
        {
            rawSprites.Add(new KeyValuePair<string, Texture2D>(name, texture));
        }
        public void AddSprite(string name, string pngPath)
        {
            pathSprites.Add(new KeyValuePair<string, string>(name, pngPath));
        }

        public UITextureAtlas GenerateAtlas(string atlasName)
        {
            SortSprites();

            var atlas = ScriptableObject.CreateInstance<UITextureAtlas>();
            atlas.material = new Material(Shader.Find("UI/Default UI Shader"));

            var atlasTexture = new Texture2D(2048, 2048, TextureFormat.ARGB32, false, false);
            atlasTexture.filterMode = FilterMode.Bilinear;
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

                CopySpriteToAtlas(atlas, x, y, name, texture);

                x += texture.width + 2;
                maxY = Mathf.Max(maxY, texture.height);
            }

            foreach (var item in pathSprites)
            {
                var name = item.Key;
                var pngPath = item.Value;

                if (spriteCache.ContainsKey(pngPath))
                {
                    var cachedSprite = new UITextureAtlas.SpriteInfo();
                    cachedSprite.name = name;
                    cachedSprite.texture = spriteCache[pngPath].Key;
                    cachedSprite.region = spriteCache[pngPath].Value;
                    atlas.AddSprite(cachedSprite);

                    Debug.LogWarningFormat("Texture for sprite \"{0}\" already exists in atlas, reusing cached copy..", name);
                    continue;
                }

                var texture = new Texture2D(0, 0, TextureFormat.ARGB32, false, true);
                texture.LoadImage(File.ReadAllBytes(pngPath));

                x += texture.width + 2;
                maxY = Mathf.Max(maxY, texture.height);

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

                var spriteRect = CopySpriteToAtlas(atlas, x, y, name, texture);
                spriteCache.Add(pngPath, new KeyValuePair<Texture2D, Rect>(texture, spriteRect));
            }

            atlasTexture.Apply();
            return atlas;
        }

        private Rect CopySpriteToAtlas(UITextureAtlas atlas, int x, int y, string name, Texture2D texture)
        {
            var atlasTexture = atlas.material.mainTexture as Texture2D;

            for (int _x = 0; _x < texture.width; _x++)
            {
                for (int _y = 0; _y < texture.height; _y++)
                {
                    atlasTexture.SetPixel(x + _x, y + _y, texture.GetPixel(_x, _y));
                }
            }

            float u = (float)x / atlasTexture.width;
            float v = (float)y / atlasTexture.height;
            float s = (float)(texture.width) / atlasTexture.width;
            float t = (float)(texture.height) / atlasTexture.height;

            var sprite = new UITextureAtlas.SpriteInfo();
            sprite.region = new Rect(u, v, s, t);
            sprite.name = name;
            sprite.texture = texture;

            atlas.AddSprite(sprite);
            return sprite.region;
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
