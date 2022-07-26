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
        #region Variables
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
        Vector2 platformSize = default;

        Dictionary<BlockType, GameObject> parents;

        [Flags]
        public enum Directions
        {
            None = 0,
            Left = 1,
            Right = 1 << 1,
            Down = 1 << 2,
            Up = 1 << 3,
            Horizontal = Left | Right,
            Vertical = Down | Up,
            All = Left | Right | Down | Up,
        }
        public class Block
        {
            public GameObject block;
            public GameObject backGround;
            public SpriteRenderer renderer;
            public SpriteRenderer bRenderer;
            public BlockData data;
            public int? collider = null;

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
            public bool? IsHorizontal { get => data.isHorizontal; set => data.isHorizontal = value; }
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
            public (int y, int x) Position { get => data.position; }

            public static bool operator true(Block block) => block != null;
            public static bool operator false(Block block) => block == null;
            public static bool operator !(Block block)
            {
                if (block != null)
                    return true;
                else
                    return false;
            }

            public Block(int row, int column)
            {
                block = new GameObject($"Block{row},{column}", typeof(SpriteRenderer));
                renderer = block.GetComponent<SpriteRenderer>();
                data = new BlockData((row, column));
            }
        }
        public class ColliderData
        {
            public GameObject item;
            public List<Block> blocks;
            public BoxCollider2D collider;
            public bool? isHorizontal = null;

            public Block First => blocks == null || blocks.Count == 0 ? null : blocks[0];
            public Block Last => blocks == null || blocks.Count == 0 ? null : blocks[blocks.Count - 1];

            public ColliderData(string name, Block block)
            {
                this.item = new GameObject(name, typeof(BoxCollider2D));
                collider = item.GetComponent<BoxCollider2D>();
                blocks = new List<Block>();
                isHorizontal = block.IsHorizontal;

                AddFirst(block);
            }

            public void AddFirst(Block block) => blocks.Insert(0, block);
            public void AddLast(Block block) => blocks.Insert(blocks.Count, block);
            public void RemoveFirst()
            {
                blocks[0].collider = null;
                blocks.RemoveAt(0);
            }
            public void RemoveLast()
            {
                blocks[blocks.Count - 1].collider = null;
                blocks.RemoveAt(blocks.Count - 1);
            }

            public void Clear()
            {
                foreach (var block in blocks)
                    block.collider = null;

                blocks.Clear();
                Destroy(item);
            }
        }
        public struct Point
        {
            public int x;
            public int y;

            public Point(int y, int x) => (this.y, this.x) = (y, x);

            public Point(Vector2 vector)
            {
                x = (int)vector.x;
                y = (int)vector.y;
            }

            public Point((int y, int x) point) => (y, x) = point;
        }

        #endregion

        #region Unity methods
        void Start()
        {
            corner = Camera.main.ScreenToWorldPoint(Vector3.zero);
            blockSize = new Vector2(Camera.main.pixelWidth / columns, Camera.main.pixelHeight / rows);
            platformSize = new Vector2(Camera.main.pixelWidth / columns, Camera.main.pixelHeight / (rows * 2));
            blockExtends = new Vector2(blockSize.x / 2, blockSize.y / 2);

            Initialize();
            Get_Sprite();
        }

        void Update()
        {
            Draw_Grid();

            Edit_Settings();

            if (!showCursor) return;

            Show_Cursor();
            Painting_Blocks();

            Change_Sprite();
            Change_BlockType();
        }

        
        #endregion

        #region Public methods
        public void RemoveInList(in Point point, int colliderIndex)
        {
            if (colliders[colliderIndex].First.Position == (point.y, point.x))
            {
                colliders[colliderIndex].RemoveFirst();
                Resize_Collider(colliderIndex);
                return;
            }
            else if (colliders[colliderIndex].Last.Position == (point.y, point.x))
            {
                colliders[colliderIndex].RemoveLast();
                Resize_Collider(colliderIndex);
                return;
            }

            var points = new List<List<Point>>();
            points.Add(new List<Point>());

            Split_Collider(in point);

            for (int i = 0; i < points.Count; i++)
            {
                Point first = points[i][0];
                points[i].RemoveAt(0);

                Create_Collider(grid[point.y, point.x].IsHorizontal, false, first, points[i].ToArray());
            }

            void Resize_Collider(int index)
            {
                List<Point> newPoints = new List<Point>();
                foreach (var item in colliders[index].blocks)
                    newPoints.Add(new Point(item.Position));

                Point first = newPoints[0];
                newPoints.RemoveAt(0);

                Create_Collider(colliders[index].isHorizontal, true, first, newPoints.ToArray());
            }

            void Split_Collider(in Point point)
            {
                foreach (var block in colliders[colliderIndex].blocks)
                {
                    if (block.Position == (point.y, point.x))
                    {
                        points.Add(new List<Point>());
                        continue;
                    }

                    points[points.Count - 1].Add(new Point(block.Position));
                }

                colliders[colliderIndex].Clear();
                colliders.RemoveAt(colliderIndex);
            }
        }
        #endregion

        #region Private methods

        #region Inputs
        private void Painting_Blocks()
        {
            try
            {
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

                        int first = default;
                        List<Point> pointsList = new List<Point>();
                        bool atRitgh = false;

                        if (isX)
                        {
                            atRitgh = lastCell.y > currentCell.y ? true : false;

                            for (int i = points.start, j = 0; i <= points.end; i++, j++)
                            {
                                grid[i, lastCell.x].Sprite = spriteInMouse.sprite;
                                grid[i, lastCell.x].data.sprite = new SpriteData(color, type);
                                Get_Sprite();

                                pointsList.Add(new Point(i, lastCell.x));
                            }

                            first = atRitgh ? pointsList.Count - 1 : 0;

                            Find_Pairs(pointsList.ToArray(), atRitgh ? Directions.Up : Directions.Down, first);
                        }
                        else
                        {
                            atRitgh = lastCell.x > currentCell.x ? true : false;

                            for (int i = points.start; i <= points.end; i++)
                            {
                                grid[lastCell.y, i].Sprite = spriteInMouse.sprite;
                                grid[lastCell.y, i].data.sprite = new SpriteData(color, type);
                                Get_Sprite();

                                pointsList.Add(new Point(lastCell.y, i));
                            }

                            first = atRitgh ? pointsList.Count - 1 : 0;

                            Find_Pairs(pointsList.ToArray(), atRitgh ? Directions.Right : Directions.Left, first);
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

                    lastCell = (currentCell.y, currentCell.x, true);
                    Get_Sprite();

                    Point[] points = new Point[]
                    {
                    new Point(currentCell)
                    };
                    Find_Pairs(points);
                }
            }
            catch (IndexOutOfRangeException) { }
        }

        private void Change_BlockType()
        {
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
                var newValue = (int)type - 1;

                if (newValue > (int)values.GetValue(values.Length - 1))
                    newValue = 1;

                type = (BlockType)newValue;
                Get_Sprite();
            }
        }

        private void Change_Sprite()
        {
            if (Input.mouseScrollDelta.y > 0)
                Get_NextSprite(true);
            else if (Input.mouseScrollDelta.y < 0)
                Get_NextSprite(false);
        }

        private void Edit_Settings()
        {
            if (Input.GetKeyDown(KeyCode.P)) Initialize();

            if (Input.GetKeyDown(KeyCode.M))
            {
                if (Cursor.lockState == CursorLockMode.None)
                    Cursor.lockState = CursorLockMode.Confined;
                else if (Cursor.lockState == CursorLockMode.Confined)
                    Cursor.lockState = CursorLockMode.None;

                print(Cursor.lockState);
            }

            if (Input.GetKeyDown(KeyCode.H))
            {
                var value = !showCursor;
                showCursor = value;

                if (value)
                    Get_Sprite();
                else
                    spriteInMouse.sprite = null;
            }

            if (Input.GetKeyDown(KeyCode.G))
            {
                drawLines = !drawLines;
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
                try
                {
                    grid[currentCell.y, currentCell.x].Sprite = null;
                    grid[currentCell.y, currentCell.x].data.sprite = new SpriteData(color, type);
                    grid[currentCell.y, currentCell.x].block.transform.parent = parents[BlockType.None].transform;
                    Remove_Collider(new Point(currentCell.y, currentCell.x));
                    lastCell.hasValue = false;
                }
                catch (IndexOutOfRangeException) { }
            }

            if (Input.GetKeyDown(KeyCode.R)) random = !random;
        }
        #endregion

        private void Remove_Collider(in Point blockInd)
        {
            ref var block = ref grid[blockInd.y, blockInd.x];

            if (colliders[block.Collider.Value].blocks.Count > 1)
                RemoveInList(blockInd, block.Collider.Value);
            else
            {
                Destroy(colliders[block.Collider.Value].item);
                colliders.RemoveAt(block.collider.Value);
                block.collider = null;
            }
        }

        private void Color_Place(int y, int x, int? pd)
        {
            if (grid[y, x].Sprite != null) return;

            grid[y, x].Sprite = spriteInMouse.sprite;
            grid[y, x].data.sprite = new SpriteData(color, type);
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

        private void Find_Pairs(Point[] points, Directions direction = Directions.All, int firstIdx = 0)
        {
            if (type == BlockType.Platform || type == BlockType.Enemies || points == null) return;

            ref Point cp = ref points[firstIdx];

            var directions = new[]
            {
                new { X = -1, Y = 0, isH = true, Limit = -1, Direction = Directions.Left},
                new { X = 1, Y = 0, isH = true, Limit = columns, Direction = Directions.Right},
                new { X = 0, Y = -1, isH = false, Limit = -1, Direction = Directions.Down},
                new { X = 0, Y = 1, isH = false, Limit = rows, Direction = Directions.Up},
            };

            bool founded = false;

            for (int i = 0; i < directions.Length; i++)
            {
                if (cp.y + directions[i].Y == directions[i].Limit || cp.x + directions[i].X == directions[i].Limit)
                    continue;

                ref var idx = ref directions[i];
                ref var cur = ref grid[cp.y + idx.Y, cp.x + idx.X];

                if (cur.Sprite && cur.data.sprite.type != BlockType.Platform && cur.data.sprite.type != BlockType.Enemies)
                {
                    if ((direction & directions[i].Direction) == 0) continue;

                    if (cur.collider.HasValue && (cur.data.isHorizontal.HasValue && cur.data.isHorizontal != idx.isH))
                        continue;

                    switch (i)
                    {
                        case 0:
                        case 1:

                            if (founded)
                                Create_Collider(true, Check_IsRight(directions[i]), cp, new Point(cp.y + idx.Y, cp.x + idx.X));
                            else
                                Create_Collider(true, Check_IsRight(directions[i]), new Point(cp.y + idx.Y, cp.x + idx.X), points);
                            goto default;

                        case 2:
                        case 3:

                            if (founded)
                                Create_Collider(false, Check_IsRight(directions[i]), cp, new Point(cp.y + idx.Y, cp.x + idx.X));
                            else
                                Create_Collider(false, Check_IsRight(directions[i]), new Point(cp.y + idx.Y, cp.x + idx.X), points);
                            goto default;

                        default:

                            founded = true;
                            break;
                    }
                }
            }

            if (!founded)
            {
                if (points.Length -1 > 1)
                {
                    Point[] lastBlocks = new Point[points.Length - 1];

                    for (int i = 0; i < lastBlocks.Length - 1; i++)
                    {
                        lastBlocks[i] = points[i + 1];
                    }

                    bool? isH = null;

                    if (points[0].x == points[1].x)
                        isH = false;
                    else if (points[0].y == points[1].y)
                        isH = true;

                    Create_Collider(isH, true, cp, lastBlocks); 
                }
                else
                    Create_Collider(null, true, cp);
            }

            bool Check_IsRight(dynamic id) => id.X + id.Y > 0 ? true : false;
        }

        private void Create_Collider(bool? horizontal, bool atRight, Point first, params Point[] points)
        {
            if (points == null) points = new Point[0];

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
                col.item.transform.position = cur.block.transform.position;
            }
            else
                col = colliders[cur.collider.Value];

            cur.IsHorizontal = horizontal;

            if (!atRight)
            {
                InvertArrayOrder(ref points);
            }
            else if (points != null && points.Length > 0)
                col.item.transform.position = grid[points[0].y, points[0].x].block.transform.position;
            else
                col.item.transform.position = cur.block.transform.position;

            Set_BlocksCollider(ref cur);

            if (!horizontal.HasValue || horizontal.Value)
            {
                col.collider.offset = new Vector2(.445f * (col.blocks.Count - 1), 0);
                col.collider.size = new Vector2(.89f * col.blocks.Count, .83f);
            }
            else
            {
                col.collider.offset = new Vector2(0, .42f * (col.blocks.Count - 1));
                col.collider.size = new Vector2(.89f, .83f * col.blocks.Count);
            }

            void Set_BlocksCollider(ref Block cur)
            {
                foreach (var point in points)
                {
                    ref var idx = ref grid[point.y, point.x];

                    if (idx.collider.HasValue) 
                        Remove_Collider(point);

                    idx.Collider = cur.collider.Value;
                    idx.data.isHorizontal = cur.data.isHorizontal;

                    if (!atRight)
                        col.AddLast(idx);
                    else
                        col.AddFirst(idx);
                }
            }
        }

        private void InvertArrayOrder(ref Point[] points)
        {
            var values = (Point[])points.Clone();
            points = new Point[values.Length];

            for (int i = values.Length - 1, j = 0; i >= 0; i--, j++)
            {
                points[j] = values[i];
            }
        }

        private void Show_Cursor()
        {
            Vector3 position = default;

            if (type == BlockType.Platform)
            {
                currentCell = ((int)(Input.mousePosition.y / platformSize.y), (int)(Input.mousePosition.x / platformSize.x));

                position = Camera.main.ScreenToWorldPoint(new Vector2
                {
                    x = platformSize.x * currentCell.x + (blockExtends.x),
                    y = platformSize.y * currentCell.y + (blockExtends.y / 2),
                });
            }
            else
            {
                currentCell = ((int)(Input.mousePosition.y / blockSize.y), (int)(Input.mousePosition.x / blockSize.x));

                position = Camera.main.ScreenToWorldPoint(new Vector2
                {
                    x = blockSize.x * currentCell.x + (blockExtends.x),
                    y = blockSize.y * currentCell.y + (blockExtends.y),
                });
            }

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
        #endregion
    } 
}
