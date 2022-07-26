using System;
using System.Collections.Generic;
using UnityEngine;

namespace Xolito.Utilities
{
    [CreateAssetMenu(fileName = "PlayerSettings", menuName = "XolitoSettings/Level Design", order = 0)]
    internal class LevelDesign : ScriptableObject
    {
        [Header("Settings")]
        [SerializeField] GridSprites images;
        [SerializeField] int rows = 0;
        [SerializeField] int columns = 0;


        BlockData[,] grid;

        public class ColliderData
        {
            public List<(int y, int x)> blocks;
            public bool isHorizontal = false;
        }
    }

    public class BlockData
    {
        public SpriteData sprite;
        public SpriteData backGround;
        public (int y, int x) position;
        public bool? isHorizontal = null;
        public Vector2 colliderSize = default;
        public Vector2 colliderPosition = default;

        public BlockData()
        {

        }

        public BlockData((int y, int x) position)
        {
            this.position = position;
        }

        public BlockData(SpriteData data)
        {
            sprite = data;
        }

        public BlockData(SpriteData sprite, SpriteData backGround) : this(sprite)
        {
            this.backGround = backGround;
        }
    }

    public class ColliderData
    {
        public Vector2 size;
        public Vector2 offset;
        public Vector2[] blocks;
    }
}
