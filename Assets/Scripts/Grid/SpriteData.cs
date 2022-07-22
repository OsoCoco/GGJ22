using UnityEngine;

namespace Xolito.Utilities
{
    public class SpriteData
    {
        public ColorType color;
        public BlockType type;

        public SpriteData(ColorType color, BlockType type)
        {
            this.color = color;
            this.type = type;
        }
    }
}
