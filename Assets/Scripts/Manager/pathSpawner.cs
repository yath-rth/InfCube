using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class pathSpawner : MonoBehaviour, ISaveFuncs
{
    public static pathSpawner instance;

    ObjectPooler pool;
    [SerializeField] Player player;
    [Range(0, 2f)] public float tileSize;
    [SerializeField, Range(0f, 10f)] float tilesAhead;
    [SerializeField] AnimationCurve spawnTimeCurve;
    [SerializeField] Transform startTile;
    [SerializeField] bool ghost;
    int count, side = -1, startCount = 10, startMirrorCount = 10, tileCount = 0, mirrorCount, mirrorSide = -1;
    double coinChance = 0f;
    GameObject spawnedTile;
    Vector3 spawnPos, mirrorPos;
    bool sent = false;
    Queue<PathInfo> path, mirroredPath;
    List<Transform> spawnedTiles = new List<Transform>();
    List<Transform> spawnedMirrorTiles = new List<Transform>();
    List<PathSpawnData> currentPath = new List<PathSpawnData>();
    List<PathSpawnData> previousPath = new List<PathSpawnData>();
    Transform remotePlayer;
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
        if (player == null) player = Player.instance;

        spawnPos = startTile != null ? startTile.position : Vector3.zero;
        mirrorPos = spawnPos;

        currentPath.Add(new PathSpawnData(-1, startCount));

        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.RegisterObject(this);
            SaveManager.Instance.LoadDataForObject(this);
            pathInfoEvent?.Invoke(previousPath);
        }
    }

    // ─────────────────────────────────────────────
    // Update
    // ─────────────────────────────────────────────

    public void Initialize(Queue<PathInfo> path, Transform remote)
    {
        spawnStartTile();
        spawnMirrorStartTiles();

        remotePlayer = remote;
        this.path = new Queue<PathInfo>(path);
        mirroredPath = new Queue<PathInfo>(path);

        PathInfo info = this.path.Dequeue();
        side = info.side;
        count = info.count;

        info = mirroredPath.Dequeue();
        mirrorSide = info.side;
        mirrorCount = info.count;
        currentPath.Add(new PathSpawnData(side, count));

        if (player != null) player.changeDir(side);
    }

    public void UpdatePath(Queue<PathInfo> extension)
    {
        sent = false;
        foreach (PathInfo info in extension)
        {
            path.Enqueue(info);
            mirroredPath.Enqueue(info);
        }
    }

    void Update()
    {
        if (sceneManager.GameState != 1) return;
        if (pool == null || GameManager.instance.isGameOver) return;

        if (path.Count <= 50 && !sent)
        {
            ConnectionManager.instance.SendMapOver();
            sent = true;
        }

        if (count <= 0)
        {
            PathInfo info = path.Dequeue();
            side = info.side;
            count = info.count;
            currentPath.Add(new PathSpawnData(side, count));
        }

        if (mirrorCount <= 0)
        {
            PathInfo info = mirroredPath.Dequeue();
            mirrorSide = info.side;
            mirrorCount = info.count;
        }

        float dist =
            Vector3.Distance(
                player.transform.position,
                spawnedTiles[spawnedTiles.Count - 1].position
            );

        if (dist < tilesAhead * tileSize)
        {
            if (startCount > 0)
            {
                spawnStartTile();
            }
            else
            {
                spawnPos.z += tileSize / 1.41f;
                if (side == 0) spawnPos.x += tileSize / 1.41f;
                else if (side == 1) spawnPos.x -= tileSize / 1.41f;
                spawnTile(0);
                count--;

                // Coin on real path
                // coinChance = random.Next(0f, 1f);
                if (coinChance < 0.0f)
                {
                    GameObject coin = pool.GetObject(1);
                    coin.transform.position = new Vector3(spawnedTile.transform.position.x, 0.5f, spawnedTile.transform.position.z);
                    coin.SetActive(true);
                    // spawnedObjs.Add(coin.transform);
                }
            }
        }

        if (remotePlayer == null) return;

        float remoteDist =
            Vector3.Distance(
                remotePlayer.position,
                spawnedMirrorTiles[spawnedMirrorTiles.Count - 1].position
            );

        if (remoteDist < tilesAhead * tileSize)
        {
            if (startMirrorCount > 0)
            {
                spawnMirrorStartTiles();
            }
            else
            {
                mirrorPos.z += tileSize / 1.41f;
                if (mirrorSide == 0) mirrorPos.x -= tileSize / 1.41f;
                else if (mirrorSide == 1) mirrorPos.x += tileSize / 1.41f;

                spawnMirrorTile(0);
                mirrorCount--;
            }
        }
    }

    public int getCurrentTile()
    {
        return tileCount;
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
        tileCount++;
    }

    void spawnMirrorStartTiles()
    {
        mirrorPos.z += tileSize;
        mirrorPos.y = -1f;
        GameObject mirrorTile = pool.GetObject(0);
        mirrorTile.transform.position = mirrorPos;
        mirrorTile.SetActive(true);
        mirrorTile.transform.localScale = new Vector3(tileSize, 0.2f, tileSize);
        spawnedMirrorTiles.Add(mirrorTile.transform);
        startMirrorCount--;
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
        tileCount++;
    }

    void spawnMirrorTile(int poolIndex)
    {
        mirrorPos.y = -1f;
        GameObject mirrorTile = pool.GetObject(poolIndex);
        mirrorTile.transform.position = mirrorPos;
        mirrorTile.SetActive(true);
        mirrorTile.transform.localScale = new Vector3(tileSize, 0.2f, tileSize);
        mirrorTile.transform.eulerAngles = new Vector3(0, -45, 0);
        spawnedMirrorTiles.Add(mirrorTile.transform);
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