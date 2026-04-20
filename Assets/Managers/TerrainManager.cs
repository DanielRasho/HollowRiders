using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.Tilemaps;

public class TerrainManager : MonoBehaviour
{
    [Header("Dungeon Map")]
    [SerializeField] private TextAsset dungeonFile;
    [SerializeField] private Tilemap tilemap;
    [SerializeField] private GameObject tilePrefab;
    [SerializeField] private Transform player;
    [SerializeField] private float tileHeight = 0.5f;

    [Header("Streaming")]
    [SerializeField] private int maxCells = 40;
    [SerializeField] private int playerRange = 3;
    [SerializeField] private bool refreshEveryFrame = true;

    private readonly HashSet<Vector2Int> dungeonCells = new();
    private readonly Dictionary<Vector2Int, GameObject> activeCells = new();
    private readonly HashSet<Vector2Int> desiredCells = new();
    private readonly List<Vector2Int> cellsToSpawn = new();
    private readonly List<Vector2Int> cellsToRelease = new();

    private ObjectPool<GameObject> cellPool;
    private Vector2Int currentPlayerCell = new(int.MinValue, int.MinValue);
    private Transform cellsParent;
    private int effectiveMaxCells;

    private void Awake()
    {
        cellsParent = tilemap != null ? tilemap.transform : transform;
        effectiveMaxCells = Mathf.Max(maxCells, GetSquareCellCount(playerRange));
        cellPool = new ObjectPool<GameObject>(
            CreateCell,
            OnGetCell,
            OnReleaseCell,
            OnDestroyCell,
            collectionCheck: false,
            defaultCapacity: Mathf.Max(1, effectiveMaxCells),
            maxSize: Mathf.Max(1, effectiveMaxCells));
    }

    private void Start()
    {
        if (dungeonFile == null)
            return;

        ParseDungeon(dungeonFile.text);
        RefreshVisibleCells(forceRefresh: true);
    }

    private void Update()
    {
        if (!refreshEveryFrame)
            return;

        RefreshVisibleCells();
    }

    public Vector3 GridToWorld(Vector2Int gridCoordinate)
    {
        Vector3 origin = cellsParent != null ? cellsParent.position : transform.position;
        return origin + new Vector3(gridCoordinate.x + 0.5f, tileHeight, -(gridCoordinate.y + 0.5f));
    }

    public Vector2Int WorldToGrid(Vector3 worldPosition)
    {
        Vector3 origin = cellsParent != null ? cellsParent.position : transform.position;
        Vector3 local = worldPosition - origin;
        return new Vector2Int(
            Mathf.FloorToInt(local.x),
            Mathf.FloorToInt(-local.z));
    }

    public bool HasCell(Vector2Int gridCoordinate)
    {
        return dungeonCells.Contains(gridCoordinate);
    }

    public void RefreshVisibleCells(bool forceRefresh = false)
    {
        Vector2Int playerCell = WorldToGrid(player.position);
        if (!forceRefresh && playerCell == currentPlayerCell)
            return;

        currentPlayerCell = playerCell;
        ComputeCellsInRange(playerCell, playerRange, desiredCells);
        cellsToRelease.Clear();

        foreach (KeyValuePair<Vector2Int, GameObject> pair in activeCells)
        {
            if (!desiredCells.Contains(pair.Key))
                cellsToRelease.Add(pair.Key);
        }

        for (int i = 0; i < cellsToRelease.Count; i++)
            ReleaseCell(cellsToRelease[i]);

        CollectCellsToSpawn(playerCell);
        int spawnCount = Mathf.Min(cellsToSpawn.Count, Mathf.Max(0, effectiveMaxCells - activeCells.Count));

        for (int i = 0; i < spawnCount; i++)
            SpawnCell(cellsToSpawn[i]);
    }

    private void ParseDungeon(string content)
    {
        dungeonCells.Clear();

        string[] lines = content.Replace("\r", string.Empty)
            .Split('\n', StringSplitOptions.RemoveEmptyEntries);

        for (int y = 0; y < lines.Length; y++)
        {
            for (int x = 0; x < lines[y].Length; x++)
            {
                if (lines[y][x] == '#')
                    dungeonCells.Add(new Vector2Int(x, y));
            }
        }
    }

    private void CollectCellsToSpawn(Vector2Int playerCell)
    {
        cellsToSpawn.Clear();

        foreach (Vector2Int coordinate in desiredCells)
        {
            if (!activeCells.ContainsKey(coordinate))
                cellsToSpawn.Add(coordinate);
        }

        cellsToSpawn.Sort((a, b) =>
        {
            int distanceA = GridDistanceSquared(a, playerCell);
            int distanceB = GridDistanceSquared(b, playerCell);
            return distanceA.CompareTo(distanceB);
        });
    }

    private void ComputeCellsInRange(Vector2Int center, int range, HashSet<Vector2Int> results)
    {
        results.Clear();

        for (int y = center.y - range; y <= center.y + range; y++)
        {
            for (int x = center.x - range; x <= center.x + range; x++)
            {
                Vector2Int coordinate = new Vector2Int(x, y);
                if (dungeonCells.Contains(coordinate))
                    results.Add(coordinate);
            }
        }
    }

    private void SpawnCell(Vector2Int coordinate)
    {
        if (activeCells.ContainsKey(coordinate))
            return;

        GameObject cell = cellPool.Get();
        cell.transform.SetPositionAndRotation(GridToWorld(coordinate), Quaternion.identity);
        activeCells.Add(coordinate, cell);
    }

    private void ReleaseCell(Vector2Int coordinate)
    {
        if (!activeCells.TryGetValue(coordinate, out GameObject cell))
            return;

        activeCells.Remove(coordinate);
        cellPool.Release(cell);
    }

    private GameObject CreateCell()
    {
        return Instantiate(tilePrefab, cellsParent);
    }

    private void OnGetCell(GameObject cell)
    {
        cell.transform.SetParent(cellsParent, false);
        cell.SetActive(true);
    }

    private void OnReleaseCell(GameObject cell)
    {
        cell.SetActive(false);
    }

    private void OnDestroyCell(GameObject cell)
    {
        Destroy(cell);
    }

    private static int GetSquareCellCount(int range)
    {
        int sideLength = range * 2 + 1;
        return sideLength * sideLength;
    }

    private static int GridDistanceSquared(Vector2Int a, Vector2Int b)
    {
        int dx = a.x - b.x;
        int dy = a.y - b.y;
        return dx * dx + dy * dy;
    }

    private void OnDrawGizmos()
    {
        if (player == null)
            return;

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(player.transform.position, playerRange);
    }
}
