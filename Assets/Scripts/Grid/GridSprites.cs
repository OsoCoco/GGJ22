using System;
using System.Collections.Generic;
using UnityEngine;

namespace Xolito.Utilities
{
    [CreateAssetMenu(fileName = "PlayerSettings", menuName = "XolitoSettings/Grid Sprites", order = 0)]
    public class GridSprites : ScriptableObject
    {
        [Header("Blocks")]
        [SerializeField] bool applyChanges = false;
        [SerializeField] List<SpritesType> blackBlocks = new List<SpritesType>();
        [SerializeField] List<SpritesType> whiteBlocks = new List<SpritesType>();

        Dictionary<ColorType, Dictionary<BlockType, List<Sprite>>> sprites;

        public Sprite GetSprite(ColorType color, BlockType type, int index)
        {
            if (sprites == null || applyChanges)
                Initialize();

            return sprites[color][type][index];
        }

        public int Get_SpritesCount(ColorType color, BlockType type)
        {
            return sprites[color][type].Count;
        }

        private void Initialize()
        {
            sprites = new Dictionary<ColorType, Dictionary<BlockType, List<Sprite>>>();
            var bSprites = new Dictionary<BlockType, List<Sprite>>();
            var wSprites = new Dictionary<BlockType, List<Sprite>>();

            foreach (SpritesType data in blackBlocks)
            {
                bSprites.Add(data.type, data.sprites);
            }

            foreach (SpritesType data in whiteBlocks)
            {
                wSprites.Add(data.type, data.sprites);
            }

            bSprites.Add(BlockType.None, null);
            wSprites.Add(BlockType.None, null);
            sprites.Add(ColorType.None, null);
            sprites.Add(ColorType.Black, bSprites);
            sprites.Add(ColorType.White, wSprites);
        }
    }
}
