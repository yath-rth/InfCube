using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class pathSpawner : MonoBehaviour
{
    ObjectPooler pool;

    [Range(0, 2f)]public float tileSize;
    [SerializeField, Range(0, 20)] int minLength_min, maxLength_min, minLength_max, maxLength_max;
    [SerializeField] double maxSpawnTime, minSpawnTime;
    double timeBTWspawns;
    int count, side = -1, minLength, maxLength, startCount = 8;
    double lastTime = 0f, coinChance = 0f;
    GameObject spawnedTile;
    Vector3 spawnPos;

    void Start()
    {
        startCount = 5;
        pool = ObjectPooler.instance;
        spawnPos = Vector3.zero;
        spawnPos.z = -tileSize * startCount;

        //if (player != null) player.position = new Vector3(0, player.position.y, spawnPos.z);

        minLength = Random.Range(minLength_min, minLength_max);
        maxLength = Random.Range(maxLength_min, maxLength_max);
        count = Random.Range(minLength, maxLength);
        if (side == -1) side = Random.Range(0, 2);
        if (player.instance != null) player.instance.changeDir(side); //To make the player move in the right direction the first time they click or else they might go in the wrong direction

        timeBTWspawns = minSpawnTime;
    }

    void Update()
    {
        if (sceneManager.GameState == 1)
        {
            lastTime += Time.deltaTime;

            timeBTWspawns += Time.deltaTime;
            if (timeBTWspawns > maxSpawnTime) timeBTWspawns = maxSpawnTime;

            if (count <= 0)
            {
                minLength = Random.Range(minLength_min, minLength_max);
                maxLength = Random.Range(maxLength_min, maxLength_max);
                count = Random.Range(minLength, maxLength);
                if (side == -1) side = Random.Range(0, 2);
                else side = (side == 0) ? 1 : 0;
            }

            if (lastTime > timeBTWspawns && !GameManager.instance.isGameOver && sceneManager.GameState == 1)
            {
                lastTime = 0.0;

                if (pool != null)
                {
                    if (startCount <= 0)
                    {
                        spawnedTile = pool.GetObject(0);
                        spawnedTile.transform.position = spawnPos;

                        spawnPos.z += tileSize / 1.41f;

                        if (side == 0) spawnPos.x += tileSize / 1.41f;
                        else if (side == 1) spawnPos.x -= tileSize / 1.41f;

                        spawnedTile.SetActive(true);

                        spawnedTile.transform.localScale = new Vector3(tileSize, 0.2f, tileSize);
                        spawnedTile.transform.eulerAngles = new Vector3(0, -45, 0);

                        count--;

                        coinChance = Random.Range(0f, 1f);
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
            }
        }
    }
}
