using System.Collections;
using System.Collections.Generic;
using UnityEngine;



/// <summary>
/// 战棋棋盘管理器。
/// 当前版本负责生成 7×7 棋盘。
/// </summary>
public class BoardManager : MonoBehaviour
{
    // 棋盘尺寸
    public const int BOARD_WIDTH = 7;
    public const int BOARD_HEIGHT = 7;

    // 每个格子的间距
    [SerializeField]
    private float tileSize = 1.0f;

    // Tile 预制体
    [SerializeField]
    private GameObject tilePrefab;

    // 棋盘格数组
    private Tile[,] tiles;

    private void Start()
    {
        GenerateBoard();
    }

    /// <summary>
    /// 生成 7×7 棋盘
    /// </summary>
    private void GenerateBoard()
    {
        tiles = new Tile[BOARD_WIDTH, BOARD_HEIGHT];

        for (int x = 0; x < BOARD_WIDTH; x++)
        {
            for (int y = 0; y < BOARD_HEIGHT; y++)
            {
                CreateTile(x, y);
            }
        }

        Debug.Log(
            $"棋盘生成完成：{BOARD_WIDTH} × {BOARD_HEIGHT}，" +
            $"共 {BOARD_WIDTH * BOARD_HEIGHT} 个格子"
        );
    }

    /// <summary>
    /// 创建单个棋盘格
    /// </summary>
    private void CreateTile(int x, int y)
    {
        GridPosition gridPosition = new GridPosition(x, y);

        // 根据棋盘坐标计算 Unity 世界坐标
        Vector3 worldPosition = new Vector3(
            x * tileSize,
            y * tileSize,
            0
        );

        GameObject tileObject;

        // 如果设置了 Tile Prefab，就使用 Prefab
        if (tilePrefab != null)
        {
            tileObject = Instantiate(
                tilePrefab,
                worldPosition,
                Quaternion.identity,
                transform
            );
        }
        else
        {
            // 如果没有 Prefab，就临时创建一个方块
            tileObject = GameObject.CreatePrimitive(
                PrimitiveType.Quad
            );

            tileObject.transform.position = worldPosition;
            tileObject.transform.SetParent(transform);

            tileObject.transform.localScale =
                Vector3.one * tileSize * 0.95f;
        }

        // 获取 Tile 组件
        Tile tile = tileObject.GetComponent<Tile>();

        // 如果 Prefab 上没有 Tile，就自动添加
        if (tile == null)
        {
            tile = tileObject.AddComponent<Tile>();
        }

        // 初始化 Tile
        tile.Initialize(gridPosition);

        // 保存到数组
        tiles[x, y] = tile;
    }

    /// <summary>
    /// 根据棋盘坐标获取 Tile
    /// </summary>
    public Tile GetTile(GridPosition position)
    {
        if (!IsValidPosition(position))
        {
            return null;
        }

        return tiles[position.x, position.y];
    }

    /// <summary>
    /// 判断坐标是否在 7×7 棋盘范围内
    /// </summary>
    public bool IsValidPosition(GridPosition position)
    {
        return
            position.x >= 0 &&
            position.x < BOARD_WIDTH &&
            position.y >= 0 &&
            position.y < BOARD_HEIGHT;
    }

    /// <summary>
    /// 棋盘坐标转换为 Unity 世界坐标
    /// </summary>
    public Vector3 GridToWorld(GridPosition position)
    {
        return new Vector3(
            position.x * tileSize,
            position.y * tileSize,
            0
        );
    }

    /// <summary>
    /// Unity 世界坐标转换为棋盘坐标
    /// </summary>
    public GridPosition WorldToGrid(Vector3 worldPosition)
    {
        int x = Mathf.RoundToInt(worldPosition.x / tileSize);
        int y = Mathf.RoundToInt(worldPosition.y / tileSize);

        return new GridPosition(x, y);
    }
}