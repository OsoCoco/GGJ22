using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

namespace Xolito.Utilities
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class GridController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] GridSprites sprites;
        [SerializeField] LevelDesign level;

        [Header("Settings")]
        [SerializeField] SpriteRenderer spriteInMouse;
        [SerializeField] int rows = 0;
        [SerializeField] int columns = 0;
        [SerializeField] bool drawLines = true;

        [Header("Edition")]
        [SerializeField] BlockType type;
        [SerializeField] ColorType color;
        [SerializeField] bool random = false;
        [SerializeField] bool showCursor = true;

        int spriteIndex = 0;
        (int y, int x) currentCell = default;
        (int y, int x, bool hasValue) lastCell = default;

        Block[,] grid;
        List<ColliderData> colliders = new List<ColliderData>();
        GameObject collidersParent;
        Vector3 corner;
        Vector2 blockSize = default;
        Vector2 blockExtends = default;

        Dictionary<BlockType, GameObject> parents;

        public class Block
        {
            public GameObject block;
            public GameObject backGround;
            public SpriteRenderer renderer;
            public SpriteRenderer bRenderer;
            public BlockData data;
            public int? collider;

            public Block(int row, int column)
            {
                block = new GameObject($"Block{row},{column}", typeof(SpriteRenderer));
                renderer = block.GetComponent<SpriteRenderer>();
                data = new BlockData(new Vector2(row, column));
            }

            public Sprite Sprite { get => renderer.sprite; set => renderer.sprite = value; }

            public Sprite BSprite
            {
                get => bRenderer?.sprite;
                set
                {
                    if (backGround is null)
                    {
                        backGround = new GameObject(block.name + " Back", typeof(SpriteRenderer));
                        backGround.transform.position = block.transform.position;
                        bRenderer = backGround.GetComponent<SpriteRenderer>();
                    }

                    if (value is null)
                        backGround = null;
                    else
                        bRenderer.sprite = value;
                }
            }
            public bool IsHorizontal { get => data.isHorizontal; set => data.isHorizontal = value; }
            public int? Collider
            {
                get => collider;
                set
                {
                    collider = value;
                    data.colliderPosition = new Vector2
                    {
                        x = Int32.Parse(block.name.Substring(5, 1)),
                        y = Int32.Parse(block.name.Substring(5, 1)),
                    };
                }
            }
            public Vector2 Position { get=> data.position; }

            public static bool operator true(Block block) => block != null;
            public static bool operator false(Block block) => block == null;
            public static bool operator ! (Block block)
            {
                if (block != null)
                    return true;
                else
                    return false;
            }
        }

        public class ColliderData
        {
            public GameObject item;
            public LinkedList<Block> blocks;
            public BoxCollider2D collider;

            public ColliderData(string name, Block block)
            {
                this.item = new GameObject(name, typeof(BoxCollider2D));
                collider = item.GetComponent<BoxCollider2D>();
                blocks = new LinkedList<Block>();

                blocks.AddFirst(block);
            }
        }

        void Start()
        {
            corner = Camera.main.ScreenToWorldPoint(Vector3.zero);
            blockSize = new Vector2(Camera.main.pixelWidth / columns, Camera.main.pixelHeight / rows);
            blockExtends = new Vector2(blockSize.x / 2, blockSize.y / 2);

            Initialize();
            Get_Sprite();
        }

        void Update()
        {
            Draw_Grid();

            if (Input.GetKeyDown(KeyCode.P)) Initialize();

            Change_CursorMode();

            if (Input.GetKeyDown(KeyCode.H))
            {
                var value = !showCursor;
                showCursor = value;

                if (value)
                    Get_Sprite();
                else
                    spriteInMouse.sprite = null;
            }

            if (Input.GetKeyDown(KeyCode.I))
            {
                if (color != ColorType.White)
                    color = ColorType.White;
                else
                    color = ColorType.Black;

                spriteIndex = 0;
                Get_Sprite();
            }

            if (Input.GetMouseButtonDown(1))
            {
                grid[currentCell.y, currentCell.x].Sprite = null;
                grid[currentCell.y, currentCell.x].block.transform.parent = parents[BlockType.None].transform;
                lastCell.hasValue = false;
            }

            if (Input.GetKeyDown(KeyCode.R))
            {
                random = !random;
            }

            if (!showCursor) return;

            Show_Cursor();

            if (Input.GetKey(KeyCode.N) && Input.GetMouseButton(0))
            {
                if (color == ColorType.None || type == BlockType.None)
                {
                    grid[currentCell.y, currentCell.x].Sprite = null;
                    return;
                }

                if (lastCell.hasValue)
                {
                    (int start, int end) points = default;
                    bool isX = true;

                    if (currentCell.x == lastCell.x && currentCell.y != lastCell.y)
                    {
                        if (currentCell.y > lastCell.y)
                        {
                            points.end = currentCell.y;
                            points.start = lastCell.y + 1;
                        }
                        else
                        {
                            points.end = lastCell.y - 1;
                            points.start = currentCell.y;
                        }
                    }
                    else if (currentCell.y == lastCell.y && currentCell.x != lastCell.x)
                    {
                        if (currentCell.x > lastCell.x)
                        {
                            points.end = currentCell.x;
                            points.start = lastCell.x + 1;
                        }
                        else
                        {
                            points.end = lastCell.x - 1;
                            points.start = currentCell.x;
                        }
                        isX = false;
                    }
                    else
                        return;


                    if (isX)
                        for (int i = points.start; i <= points.end; i++)
                        {
                            grid[i, lastCell.x].Sprite = spriteInMouse.sprite;
                            Get_Sprite();
                        }
                    else
                        for (int i = points.start; i <= points.end; i++)
                        {
                            grid[lastCell.y, i].Sprite = spriteInMouse.sprite;
                            Get_Sprite();
                        }

                    lastCell = (currentCell.y, currentCell.x, true);
                }
            }
            else if (Input.GetKey(KeyCode.C) && Input.GetMouseButton(0))
            {
                if (color == ColorType.None || type == BlockType.None)
                {
                    grid[currentCell.y, currentCell.x].Sprite = null;
                    return;
                }

                if (grid[currentCell.y, currentCell.x].Sprite != null) return;

                Color_Place(currentCell.x, currentCell.y, null);
            }
            else if (Input.GetMouseButtonDown(0))
            {
                if (color == ColorType.None || type == BlockType.None)
                {
                    grid[currentCell.y, currentCell.x].Sprite = null;
                    grid[currentCell.y, currentCell.x].data.sprite = null;
                    return;
                }

                grid[currentCell.y, currentCell.x].Sprite = sprites.GetSprite(color, type, spriteIndex);
                grid[currentCell.y, currentCell.x].data.sprite = new SpriteData(color, type);

                grid[currentCell.y, currentCell.x].block.transform.parent = parents[type].transform;
                //grid[currentCell.y, currentCell.x].next = 
                lastCell = (currentCell.y, currentCell.x, true);
                Get_Sprite();

                Find_Pairs(currentCell.y, currentCell.x);
            }

            if (Input.mouseScrollDelta.y > 0)
                Get_NextSprite(true);
            else if (Input.mouseScrollDelta.y < 0)
                Get_NextSprite(false);

            if (Input.GetKeyDown(KeyCode.J))
            {
                var values = Enum.GetValues(typeof(BlockType));
                var newValue = (int)type + 1;

                if (newValue > (int)values.GetValue(values.Length - 1))
                    newValue = 1;

                type = (BlockType)newValue;
                Get_Sprite();
            }
            else if (Input.GetKeyDown(KeyCode.K))
            {
                var values = Enum.GetValues(typeof(BlockType));
                var newValue = (int)type + 1;

                if (newValue > (int)values.GetValue(values.Length - 1))
                    newValue = 1;

                type = (BlockType)newValue;
                Get_Sprite();
            }

            if (Input.GetKeyDown(KeyCode.G))
            {
                print(grid[lastCell.y, lastCell.x].renderer.size);
                grid[lastCell.y, lastCell.x].block.transform.localScale *= .5f;
                print(grid[lastCell.y, lastCell.x].renderer.size);
                print(grid[lastCell.y, lastCell.x].block.name);
            }
        }

        private void Color_Place(int y, int x, int? pd)
        {
            if (grid[y, x].Sprite != null) return;

            grid[y, x].Sprite = spriteInMouse.sprite;
            Get_Sprite();

            var directions = new[]
            {
                new { X = -1, Y = 0, Limit = 0 },
                new { X = 1, Y = 0, Limit = columns },
                new { X = 0, Y = -1, Limit = 0 },
                new { X = 0, Y = 1, Limit = rows },
            };
            int j = x, k = y;

            for (int i = 0; i < directions.Length; i++)
            {
                if (i == pd) continue;

                if (k + directions[i].Y == directions[i].Limit || j + directions[i].X == directions[i].Limit)
                    continue;

                Color_Place(k + directions[i].Y, j + directions[i].X, i);
            }
        }

        private void Find_Pairs(int y, int x)
        {
            if (type == BlockType.Platform || type == BlockType.Enemies) return;

            var directions = new[]
            {
                new { X = -1, Y = 0, isH = true, Limit = 0 },
                new { X = 1, Y = 0, isH = true, Limit = columns },
                new { X = 0, Y = -1, isH = false, Limit = 0 },
                new { X = 0, Y = 1, isH = false, Limit = rows },
            };

            bool founded = false;

            for (int i = 0; i < directions.Length; i++)
            {
                if (y + directions[i].Y == directions[i].Limit || x + directions[i].X == directions[i].Limit)
                    continue;

                ref var idx = ref directions[i];
                ref Block cur = ref grid[y + idx.Y, x + idx.X];

                if (cur.Sprite && cur.data.sprite.type != BlockType.Platform && cur.data.sprite.type != BlockType.Enemies)
                {
                    if (cur.collider.HasValue && (cur.data.isHorizontal != idx.isH))
                        continue;

                    switch (i)
                    {
                        case 0:
                        case 1:

                            Create_Collider(true, Check_IsRight(directions[i]), (y, x), (y + idx.Y, x + idx.X));
                            goto default;

                        case 2:
                        case 3:

                            Create_Collider(false, Check_IsRight(directions[i]), (y, x), (y + idx.Y, x + idx.X));
                            goto default;

                        default:
                            
                            founded = true;
                        break;
                    }
                }
            }

            if (!founded)
                Create_Collider(true, true, (y, x));

            bool Check_IsRight(dynamic id) => id.x + id.y > 0 ? true : false;
        }

        private void Create_Collider(bool horizontal, bool atRight, (int y, int x) first, params (int y, int x)[] points)
        {
            ColliderData col = null;
            ref var cur = ref grid[first.y, first.x];

            if (!cur.collider.HasValue)
            {
                var data = new ColliderData($"Col{colliders.Count}", cur);

                colliders.Add(data);
                cur.collider = colliders.Count - 1;
                cur.data.isHorizontal = horizontal;
                col = colliders[cur.collider.Value];

                col.collider.transform.parent = collidersParent.transform;
                colliders[grid[first.y, first.x].collider.Value].item.transform.position = cur.block.transform.position;
            }
            else
                col = colliders[grid[first.y, first.x].collider.Value];

            if (!atRight) InvertArrayOrder();

            foreach (var point in points)
            {
                ref var idx = ref grid[point.y, point.x];

                if (idx.collider.HasValue)
                {
                    Destroy(colliders[idx.collider.Value].item);
                    colliders.RemoveAt(idx.collider.Value);
                }

                idx.Collider = cur.collider.Value;
                idx.data.isHorizontal = cur.data.isHorizontal;

                if (atRight)
                    col.blocks.AddLast(idx);
                else
                    col.blocks.AddFirst(idx);
            }

            if (horizontal)
            {
                col.collider.offset = new Vector2(.445f * (col.blocks.Count - 1), 0);
                col.collider.size = new Vector2(.89f * col.blocks.Count, .83f); 
            }
            else
            {
                col.collider.offset = new Vector2(0, .445f * (col.blocks.Count - 1));
                col.collider.size = new Vector2(.89f, .83f * col.blocks.Count);
            }

            void InvertArrayOrder()
            {
                var values = ((int y, int x)[])points.Clone();
                points = new (int y, int x)[values.Length];

                for (int i = values.Length, j = 0; i < values.Length; i--, j++)
                {
                    points[j] = values[i];
                }
            }
        }

        private static void Change_CursorMode()
        {
            if (Input.GetKeyDown(KeyCode.M))
            {
                if (Cursor.lockState == CursorLockMode.None)
                    Cursor.lockState = CursorLockMode.Confined;
                else if (Cursor.lockState == CursorLockMode.Confined)
                    Cursor.lockState = CursorLockMode.None;

                print(Cursor.lockState);
            }
        }

        private void Show_Cursor()
        {
            currentCell = ((int)(Input.mousePosition.y / blockSize.y), (int)(Input.mousePosition.x / blockSize.x));

            Vector3 position = Camera.main.ScreenToWorldPoint(new Vector2
            {
                x = blockSize.x * currentCell.x + (blockExtends.x),
                y = blockSize.y * currentCell.y + (blockExtends.y),
            });

            spriteInMouse.transform.position = new Vector3
            {
                x = position.x,
                y = position.y,
                z = 0
            };
        }

        private void Get_NextSprite(bool isForward)
        {
            if (color == ColorType.None || type == BlockType.None) return;

            var index = 0;
            if (isForward)
            {
                index = spriteIndex + 1;
                if (index >= sprites.Get_SpritesCount(color, type))
                    index = 0;
            }
            else
            {
                index = spriteIndex - 1;
                if (index < 0)
                    index = sprites.Get_SpritesCount(color, type) - 1;
            }

            spriteInMouse.sprite = sprites.GetSprite(color, type, index);
            spriteIndex = index;
        }

        private void Initialize()
        {
            parents = new Dictionary<BlockType, GameObject>();
            var values = System.Enum.GetValues(typeof(BlockType));

            foreach (var value in values)
            {
                BlockType type = (BlockType)value;

                var parent = new GameObject(type.ToString());
                parent.transform.parent = transform;

                parents.Add(type, parent);
            }

            collidersParent = new GameObject("Colliders");
            collidersParent.transform.parent = transform;

            grid = new Block[rows, columns];

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < columns; j++)
                {
                    grid[i, j] = new Block(i, j);
                    grid[i, j].block.transform.parent = parents[BlockType.None].transform;

                    grid[i, j].block.transform.position = ScreenToWorldPoint((i, j));
                    grid[i, j].renderer.size = spriteInMouse.size;
                }
            }
        }

        private Vector3 ScreenToWorldPoint((int y, int x) point)
        {
            Vector3 newPosition = Camera.main.ScreenToWorldPoint(new Vector2
            {
                x = point.x * blockSize.x + blockExtends.x,
                y = point.y * blockSize.y + blockExtends.y,
            });

            return new Vector3
            {
                x = newPosition.x,
                y = newPosition.y,
                z = 0
            };
        }

        private void Get_Sprite()
        {
            if (color == ColorType.None || type == BlockType.None)
            {
                spriteInMouse.sprite = null;
                return;
            }

            if (random)
            {
                var rand = new System.Random();
                var value = rand.Next(sprites.Get_SpritesCount(color, type));
                spriteInMouse.sprite = sprites.GetSprite(color, type, value);
                spriteIndex = value;
            }
            else
            {
                spriteInMouse.sprite = sprites.GetSprite(color, type, 0);
                spriteIndex = 0;
            }
        }

        private void Draw_Grid()
        {
            if (drawLines)
            {
                for (int i = 1; i < rows; i++)
                {
                    Debug.DrawLine(Camera.main.ScreenToWorldPoint(new Vector3(0, blockSize.y * i, 58f)),
                        Camera.main.ScreenToWorldPoint(new Vector3(Camera.main.pixelWidth, blockSize.y * i, 58f)), Color.red);
                }

                for (int i = 1; i < columns; i++)
                {
                    Debug.DrawLine(Camera.main.ScreenToWorldPoint(new Vector3(blockSize.x * i, 0, 58f)),
                        Camera.main.ScreenToWorldPoint(new Vector3(blockSize.x * i, Camera.main.pixelHeight, 58f)), Color.red);
                }
            }
        }
    } 
}
