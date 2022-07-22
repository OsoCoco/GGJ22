using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Xolito.Utilities
{
    [Serializable]
    public class SpritesType
    {
        public List<Sprite> sprites;
        public BlockType type;

        public Sprite RandomSprite { get => sprites[UnityEngine.Random.Range(0, sprites.Count - 1)]; }
    }

    public enum ColorType
    {
        None,
        Black,
        White
    }
    public enum BlockType
    {
        None,
        Platform,
        Ground,
        Corner,
        Special,
        Final,
        Enemies
    }
}
