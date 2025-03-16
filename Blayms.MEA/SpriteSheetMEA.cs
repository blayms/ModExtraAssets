using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Blayms.MEA
{
    /// <summary>
    /// A custom scriptable object specifically made to storage data about sprite sheets, since Unity Engine does not provide it through their API
    /// </summary>
    public class SpriteSheetMEA : ScriptableObject
    {
        internal Texture2D texture;
        internal Sprite[] sprites;
        internal Internal.SpriteSheetData internalData;
        internal AssetEntryMEA textureEntry, assetEntry;
        internal string rawJson;
        /// <summary>
        /// If set, acts as a default value for every sprite slice's pixelsPerUnit property (default = 100, json property inPath: "meta"."pixelsPerUnit")
        /// </summary>
        public float SheetPixelsPerUnit => internalData.meta.pixelsPerUnit;
        /// <summary>
        /// Main Texture's Width
        /// </summary>
        public int TotalWidth => texture.width;
        /// <summary>
        /// Main Texture's Height
        /// </summary>
        public int TotalHeight => texture.height;
        /// <summary>
        /// The amount of sprite slices
        /// </summary>
        public int Count => sprites.Length;
        /// <summary>
        /// Raw text of a sprite sheet configuration
        /// </summary>
        public string RawJson => rawJson;
        /// <summary>
        /// A reference to the Asset Entry of the main texture
        /// </summary>
        public AssetEntryMEA TextureEntry => textureEntry;
        /// <summary>
        /// ModExtraAssets' Asset Entry of the sheet
        /// </summary>
        public AssetEntryMEA AssetEntry => assetEntry;
        /// <summary>
        /// Main Texture
        /// </summary>
        public Texture2D Texture => texture;
        /// <summary>
        /// Indexer for picking the sprite slice by index
        /// </summary>
        /// <param name="i">Index</param>
        /// <returns></returns>
        public Sprite this[int i]
        {
            get { return sprites[i]; }
        }
        /// <summary>
        /// Converts sheet to the array of sprites
        /// </summary>
        public Sprite[] ToArray()
        {
            return sprites;
        }
        /// <summary>
        /// An enumerator to iterate through sprite slices
        /// </summary>
        public IEnumerator GetEnumerator()
        {
            return new Enumerator(this);
        }
        /// <summary>
        /// Converts sheet to the list of sprites
        /// </summary>
        public List<Sprite> ToList()
        {
            return sprites.ToList();
        }
        /// <summary>
        /// A quick access to the System.Linq "Where" function from the internal collection of sprites
        /// </summary>
        public IEnumerable<Sprite> Where(Func<Sprite, bool> predicate)
        {
            return sprites.Where(predicate);
        }

        internal void Internal_BakeAll()
        {
            if(sprites == null)
            {
                sprites = new Sprite[internalData.frames.Length];
                for (int i = 0; i < internalData.frames.Length; i++)
                {
                    float pixelsPerUnit = internalData.meta.pixelsPerUnit;
                    if (internalData.frames[i].overridePPU > -1)
                    {
                        pixelsPerUnit = internalData.frames[i].overridePPU;
                    }
                    Sprite sprite = Sprite.Create(texture, internalData.frames[i].frame.ToRect(), Vector2.one / 2, pixelsPerUnit);
                    sprite.name = $"{name}_{i}";
                    sprites[i] = sprite;
                }
            }
        }
        private class Enumerator : IEnumerator
        {
            internal Enumerator(SpriteSheetMEA outer)
            {
                this.outer = outer;
            }
            public object Current
            {
                get
                {
                    return outer[currentIndex];
                }
            }
            public bool MoveNext()
            {
                int childCount = outer.Count;
                int num = currentIndex + 1;
                currentIndex = num;
                return num < childCount;
            }
            public void Reset()
            {
                currentIndex = -1;
            }
            private SpriteSheetMEA outer;
            private int currentIndex = -1;
        }
    }
}
