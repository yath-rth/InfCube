using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerGhost : MonoBehaviour
{
    CharacterController cc;
    int side = 0, count = 0, pathIndex = 0;
    Vector3 velocity;
    GameObject tileFaller, Scorer, moveParticles;
    List<PathSpawnData> path;
    float distanceTravelled;
    float tileDistance;
    float straightDistance;
    float diagonalDistance;

    private void Awake()
    {
        cc = GetComponent<CharacterController>();

        tileFaller = transform.GetChild(0).gameObject;
        Scorer = transform.GetChild(1).gameObject;
        moveParticles = transform.GetChild(1).gameObject;

        if (tileFaller != null) tileFaller.SetActive(false);
        if (Scorer != null) Scorer.SetActive(false);
        if (moveParticles != null) moveParticles.SetActive(false);
    }

    void OnEnable()
    {
        pathSpawner.pathInfoEvent += getPath;
    }

    void OnDisable()
    {
        pathSpawner.pathInfoEvent -= getPath;
    }

    void Update()
    {
        if (player.instance == null) return;

        if (sceneManager.GameState == 1)
        {
            if (tileFaller != null) tileFaller.SetActive(true);
            if (moveParticles != null) moveParticles.SetActive(true);

            if (!cc.isGrounded) velocity.y -= player.instance.getGravity() * Time.deltaTime;
            else velocity.y = 0;

            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(0, side * 45f, 0), Time.deltaTime * .2f);

            Vector3 move = new Vector3(side, velocity.y, 1f).normalized;
            Vector3 displacement = move * Time.deltaTime * player.instance.getSpeed();

            cc.Move(displacement);

            Vector3 horizontal = new Vector3(displacement.x, 0, displacement.z);
            distanceTravelled += horizontal.magnitude;

            if (distanceTravelled >= tileDistance)
            {
                distanceTravelled -= tileDistance;
                count--;

                if (count <= 0 && pathIndex < path.Count - 1)
                {
                    pathIndex++;
                    side = getSide(path[pathIndex].side);
                    count = path[pathIndex].count;
                }
            }
        }
    }

    void getPath(List<PathSpawnData> data)
    {
        path = data;

        pathIndex = 0;
        side = getSide(path[0].side);
        count = path[0].count + 3;

        tileDistance = pathSpawner.instance.tileSize;

        distanceTravelled = 0f;
    }

    int getSide(int s)
    {
        if (s == -1) return 0;
        else if (s == 0) return 1;
        else if (s == 1) return -1;
        else return 0;
    }
}