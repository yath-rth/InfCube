using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class pathSpawner : MonoBehaviour, ISaveFuncs
{
    public static pathSpawner instance;
    ObjectPooler pool;
    [SerializeField] player player;
    [Range(0, 2f)] public float tileSize;
    [SerializeField, Range(0, 20)] int minLength_min, maxLength_max;
    [SerializeField] AnimationCurve spawnTimeCurve;
    [SerializeField] float maxSpawnTime, minSpawnTime;
    float timeBTWspawns, timer = 0f;
    int count, side = -1, startCount = 8;
    double lastTime = 0f, coinChance = 0f;
    GameObject spawnedTile;
    Vector3 spawnPos;
    [SerializeField] Transform startTile;
    [SerializeField] bool ghost;
    List<PathSpawnData> currentPath = new List<PathSpawnData>();
    List<PathSpawnData> previousPath = new List<PathSpawnData>();
    string ISaveFuncs.id => "PathSpawner";
    public static event Action<List<PathSpawnData>> pathInfoEvent;

    //Previous path data
    int pSide = 0, pCount = 0, pIndex = 0;
    Vector3 pPosition = Vector3.zero;

    private void Awake() {
        if(instance != null) Destroy(this);
        instance = this;
    }

    void Start()
    {
        startCount = 5;
        currentPath.Add(new PathSpawnData(-1, startCount));

        pool = ObjectPooler.instance;
        spawnPos = Vector3.zero;
        if (startTile != null) spawnPos = new Vector3(startTile.position.x, startTile.position.y, startTile.position.z + tileSize);

        pPosition = spawnPos;

        if (player == null) player = player.instance;

        //if (player != null) player.position = new Vector3(0, player.position.y, spawnPos.z);

        count = UnityEngine.Random.Range(minLength_min, maxLength_max);

        if (side == -1) side = UnityEngine.Random.Range(0, 2);

        currentPath.Add(new PathSpawnData(side, count));

        if (player != null) player.changeDir(side); //To make the player move in the right direction the first time they click or else they might go in the wrong direction only use when keeping the input using 1 button if u give 2 button input no point

        timeBTWspawns = minSpawnTime;
        timer = 0;

        Debug.Log(Time.fixedDeltaTime);

        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.RegisterObject(this);
            SaveManager.Instance.LoadDataForObject(this);
            Debug.Log("Loaded data for path spawner");

            pathInfoEvent?.Invoke(previousPath);
        }
    }

    void Update()
    {
        if (sceneManager.GameState == 1)
        {
            lastTime += Time.deltaTime;
            timer += Time.fixedDeltaTime * 0.001f;
            timeBTWspawns = minSpawnTime + spawnTimeCurve.Evaluate(timer) * (maxSpawnTime - minSpawnTime);

            if (count <= 0)
            {
                count = UnityEngine.Random.Range(minLength_min, maxLength_max);

                int rand = UnityEngine.Random.Range(0, 20);
                if(rand < 4) count += 4;

                if (side == -1) side = UnityEngine.Random.Range(0, 2);
                else side = (side == 0) ? 1 : 0;

                currentPath.Add(new PathSpawnData(side, count));
            }

            if (pCount <= 0 && previousPath.Count > 0 && pIndex < previousPath.Count)
            {
                pCount = previousPath[pIndex].count;
                pSide = previousPath[pIndex].side;
                pIndex++;
            }

            if (lastTime > timeBTWspawns && sceneManager.GameState == 1)
            {
                lastTime = 0.0;

                if (pool != null)
                {
                    if (!GameManager.instance.isGameOver)
                    {
                        if (startCount <= 0)
                        {
                            spawnTile(0, spawnPos);

                            spawnPos.z += tileSize / 1.41f;
                            if (side == 0) spawnPos.x += tileSize / 1.41f;
                            else if (side == 1) spawnPos.x -= tileSize / 1.41f;

                            count--;

                            coinChance = UnityEngine.Random.Range(0f, 1f);
                            if (coinChance < 0.1f) //Make the -1f to 0.1f if you want to start spawning coins again
                            {
                                GameObject coin = pool.GetObject(1);
                                coin.transform.position = new Vector3(spawnedTile.transform.position.x, 0.5f, spawnedTile.transform.position.z);
                                coin.SetActive(true);
                            }
                        }
                        else
                        {
                            spawnedTile = pool.GetObject(0);
                            spawnedTile.transform.position = spawnPos;
                            spawnedTile.SetActive(true);
                            spawnedTile.transform.localScale = new Vector3(tileSize, 0.2f, tileSize);

                            spawnPos.z += tileSize;
                            startCount--;
                        }
                    }

                    if (previousPath.Count > 0 && ghost)
                    {
                        spawnTile(2, pPosition);
                        if (pSide != -1) pPosition.z += tileSize / 1.41f;
                        else pPosition.z += tileSize;
                        if (pSide == 0) pPosition.x += tileSize / 1.41f;
                        else if (pSide == 1) pPosition.x -= tileSize / 1.41f;

                        pCount--;
                    }
                }
            }
        }
    }

    void spawnTile(int poolIndex, Vector3 position)
    {
        spawnedTile = pool.GetObject(poolIndex);
        spawnedTile.transform.position = position;
        spawnedTile.SetActive(true);
        spawnedTile.transform.localScale = new Vector3(tileSize, 0.2f, tileSize);
        spawnedTile.transform.eulerAngles = new Vector3(0, -45, 0);
    }

    public void LoadData(object data)
    {
        if (data is PathData p)
        {
            previousPath = p._path;
        }
    }

    public object SaveData()
    {
        return new PathData
        {
            _path = currentPath
        };
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
