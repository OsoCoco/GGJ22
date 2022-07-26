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
        public List<(Sprite image, Vector2 offset)> sprites;
        public BlockType type;
        public ActionType action;
        public Vector2 offset;

        public Sprite RandomSprite { get => sprites[UnityEngine.Random.Range(0, sprites.Count - 1)].image; }

        public List<Sprite> Sprites
        {
            get
            {
                List<Sprite> sprites = new List<Sprite>();

                foreach (var sprite in this.sprites)
                    sprites.Add(sprite.image);

                return sprites;
            }
        }
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
        Enemies,
        Background,
    }

    public enum ActionType
    {
        None,
        Platform,
        Collider,
        Coin,
        Enemy,
        FinalPoint,
    }

    public enum Offset
    {
        None,
        x2,
        x4,
    }
}
