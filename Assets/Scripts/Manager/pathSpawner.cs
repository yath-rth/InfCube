using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class pathSpawner : MonoBehaviour, ISaveFuncs
{
    public static pathSpawner instance;

    ObjectPooler pool;
    [SerializeField] player player;
    [Range(0, 2f)] public float tileSize;
    [SerializeField, Range(0, 20)] int minLength_min, maxLength_max;
    [SerializeField, Range(0f, 10f)] float tilesAhead;
    [SerializeField] AnimationCurve spawnTimeCurve;
    [SerializeField] Transform startTile;
    [SerializeField] bool ghost;
    float timer = 0f;
    int count, side = -1, startCount = 10;
    double coinChance = 0f;
    GameObject spawnedTile;
    Vector3 spawnPos, mirrorPos;

    List<Transform> spawnedTiles = new List<Transform>();
    List<Transform> spawnedObjs = new List<Transform>();
    List<Transform> spawnedGhostTiles = new List<Transform>();
    List<PathSpawnData> currentPath = new List<PathSpawnData>();
    List<PathSpawnData> previousPath = new List<PathSpawnData>();

    int pSide = 0, pCount = 0, pIndex = 0;
    Vector3 pPosition = Vector3.zero;

    string ISaveFuncs.id => "PathSpawner";
    public static event Action<List<PathSpawnData>> pathInfoEvent;

    // ─────────────────────────────────────────────
    // Lifecycle
    // ─────────────────────────────────────────────

    private void Awake()
    {
        if (instance != null) Destroy(this);
        instance = this;
    }

    void Start()
    {
        pool = ObjectPooler.instance;
        if (player == null) player = player.instance;

        spawnPos = startTile != null ? startTile.position : Vector3.zero;
        mirrorPos = spawnPos;
        pPosition = spawnPos;
        spawnStartTile();

        currentPath.Add(new PathSpawnData(-1, startCount));

        side = UnityEngine.Random.Range(0, 2);
        count = UnityEngine.Random.Range(minLength_min, maxLength_max);
        currentPath.Add(new PathSpawnData(side, count));

        if (player != null) player.changeDir(side);

        timer = 0;

        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.RegisterObject(this);
            SaveManager.Instance.LoadDataForObject(this);
            pathInfoEvent?.Invoke(previousPath);
        }
    }

    // ─────────────────────────────────────────────
    // World movement (called from player.cs)
    // ─────────────────────────────────────────────

    public void move(Vector3 delta)
    {
        foreach (Transform tile in spawnedTiles)
            tile.position -= delta;

        foreach (Transform obj in spawnedObjs)
            obj.position -= delta;

        foreach (Transform tile in spawnedGhostTiles)
            tile.position -= delta;

        // Anchor real spawn pos to last real tile
        if (spawnedTiles.Count > 0)
        {
            spawnPos = spawnedTiles[spawnedTiles.Count - 1].position;
            spawnPos.y = startTile.position.y;
        }

        // Anchor mirror spawn pos to last mirror tile
        if (spawnedObjs.Count > 0)
        {
            mirrorPos = spawnedObjs[spawnedObjs.Count - 1].position;
            mirrorPos.y = startTile.position.y;
        }

        if (spawnedGhostTiles.Count > 0)
        {
            pPosition = spawnedGhostTiles[spawnedGhostTiles.Count - 1].position;
            pPosition.y = -1f;
        }
        else
        {
            pPosition -= delta;
        }
    }

    // ─────────────────────────────────────────────
    // Update
    // ─────────────────────────────────────────────

    void Update()
    {
        if (sceneManager.GameState != 1) return;
        timer += Time.deltaTime * 0.001f;

        if (count <= 0)
        {
            count = UnityEngine.Random.Range(minLength_min, maxLength_max);
            if (UnityEngine.Random.Range(0, 30) < 3) count += 2;

            side = (side == 0) ? 1 : 0;
            currentPath.Add(new PathSpawnData(side, count));
        }

        if (pCount <= 0 && previousPath.Count > 0 && pIndex < previousPath.Count)
        {
            pCount = previousPath[pIndex].count;
            pSide = previousPath[pIndex].side;
            pIndex++;
        }

        float dist =
            Vector3.Distance(
                player.transform.position,
                spawnedTiles[spawnedTiles.Count - 1].position
            );

        if (dist > tilesAhead * tileSize) return;

        if (pool == null || GameManager.instance.isGameOver) return;

        // ── Real + mirror tiles ──
        if (startCount > 0)
        {
            spawnStartTile();
        }
        else
        {
            spawnPos.z += tileSize / 1.41f;
            if (side == 0) spawnPos.x += tileSize / 1.41f;
            else if (side == 1) spawnPos.x -= tileSize / 1.41f;

            mirrorPos.z += tileSize / 1.41f;
            if (side == 0) mirrorPos.x -= tileSize / 1.41f;
            else if (side == 1) mirrorPos.x += tileSize / 1.41f;

            spawnTile(0);
            count--;

            // Coin on real path
            coinChance = UnityEngine.Random.Range(0f, 1f);
            if (coinChance < 0.0f)
            {
                GameObject coin = pool.GetObject(1);
                coin.transform.position = new Vector3(spawnedTile.transform.position.x, 0.5f, spawnedTile.transform.position.z);
                coin.SetActive(true);
                // spawnedObjs.Add(coin.transform);
            }
        }

        // ── Ghost tiles ──
        if (ghost)
        {
            pPosition.z += (side != -1) ? tileSize / 1.41f : tileSize;
            if (side == 0) pPosition.x -= tileSize / 1.41f;
            else if (side == 1) pPosition.x += tileSize / 1.41f;

            spawnGhostTile(pPosition);
        }
    }

    // ─────────────────────────────────────────────
    // Tile helpers
    // ─────────────────────────────────────────────

    void spawnStartTile()
    {
        spawnPos.z += tileSize;
        spawnedTile = pool.GetObject(0);
        spawnedTile.transform.position = spawnPos;
        spawnedTile.SetActive(true);
        spawnedTile.transform.localScale = new Vector3(tileSize, 0.2f, tileSize);
        spawnedTiles.Add(spawnedTile.transform);
        startCount--;

        // Mirror start tile
        mirrorPos.z += tileSize;
        mirrorPos.y = startTile.position.y - 1f;
        GameObject mirrorTile = pool.GetObject(0);
        mirrorTile.transform.position = mirrorPos;
        mirrorTile.SetActive(true);
        mirrorTile.transform.localScale = new Vector3(tileSize, 0.2f, tileSize);
        spawnedObjs.Add(mirrorTile.transform);
    }

    void spawnTile(int poolIndex)
    {
        // Real tile
        spawnPos.y = startTile.position.y;
        spawnedTile = pool.GetObject(poolIndex);
        spawnedTile.transform.position = spawnPos;
        spawnedTile.SetActive(true);
        spawnedTile.transform.localScale = new Vector3(tileSize, 0.2f, tileSize);
        spawnedTile.transform.eulerAngles = new Vector3(0, -45, 0);
        spawnedTiles.Add(spawnedTile.transform);

        // Mirror tile — tracks its own position, x direction flipped
        mirrorPos.y = startTile.position.y - 1f;
        GameObject mirrorTile = pool.GetObject(poolIndex);
        mirrorTile.transform.position = mirrorPos;
        mirrorTile.SetActive(true);
        mirrorTile.transform.localScale = new Vector3(tileSize, 0.2f, tileSize);
        mirrorTile.transform.eulerAngles = new Vector3(0, -45, 0);
        spawnedObjs.Add(mirrorTile.transform);
    }

    void spawnGhostTile(Vector3 position)
    {
        position.y = -1f;
        GameObject ghostTile = pool.GetObject(2);
        ghostTile.transform.position = position;
        ghostTile.SetActive(true);
        ghostTile.transform.localScale = new Vector3(tileSize, 0.2f, tileSize);
        ghostTile.transform.eulerAngles = new Vector3(0, -45, 0);
        spawnedGhostTiles.Add(ghostTile.transform);
    }

    // ─────────────────────────────────────────────
    // Save / Load
    // ─────────────────────────────────────────────

    public void LoadData(object data)
    {
        if (data is PathData p)
            previousPath = p._path;
    }

    public object SaveData()
    {
        return new PathData { _path = currentPath };
    }

    class PathData
    {
        public List<PathSpawnData> _path;
    }
}

[Serializable]
public class PathSpawnData
{
    public int side;
    public int count;

    public PathSpawnData(int side, int count)
    {
        this.side = side;
        this.count = count;
    }
}