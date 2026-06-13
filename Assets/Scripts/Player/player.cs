using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Player : MonoBehaviour, ISaveFuncs
{
    public static Player instance;
    public Action playerDied;
    string ISaveFuncs.id => "Player";
    GameObject tileFaller, Scorer;
    GameManager gameManager;

    [SerializeField] Transform cam, bgParticles, moveParticles;
    [SerializeField, Range(0, 100f)] float gravity, rotationSpeed;
    public int side = 0, temp_side = 0;
    float _speed = 0;
    Vector3 velocity, move, cameraOffset, playerMove;
    Quaternion rotation;
    [SerializeField] bool localMultiplayerMode, RightOrLeft, autoTurn;
    [SerializeField] LayerMask tileLayer;
    private int _lastSentSide = int.MinValue;
    bool isGrounded = true;

    void Awake()
    {
        if (instance != null) Destroy(instance.gameObject);
        instance = this;

        gameManager = GameManager.instance;
        tileFaller = transform.GetChild(0).gameObject;
        Scorer = transform.GetChild(1).gameObject;

        if (tileFaller != null) tileFaller.SetActive(false);
        if (Scorer != null) Scorer.SetActive(false);
        if (moveParticles != null) moveParticles.gameObject.SetActive(false);

        if (cam != null) cameraOffset = cam.position - transform.position;

        if (bgParticles != null)
        {
            ParticleSystemRenderer renderer = bgParticles.GetComponent<ParticleSystemRenderer>();
            renderer.material.renderQueue = 3000;
            renderer.sortingOrder = 1;
        }
    }

    void ISaveFuncs.LoadData(object data)
    {
        if (data is PlayerData d)
        {
            Debug.Log("Save Opened");
        }
    }

    object ISaveFuncs.SaveData()
    {
        return new PlayerData
        {
            posx = transform.position.x,
            posz = transform.position.z,
            speed = _speed
        };
    }

    class PlayerData
    {
        public float posx;
        public float posz;
        public float speed;
    }

    void AutoTurnRaycast()
    {
        float checkDistance = 1.5f;  // how far ahead to check
        float rayLength = 2f;        // downward ray length

        // three points ahead of player to check — cast DOWN onto tiles
        Vector3 checkForward = transform.position + new Vector3(0, 1f, checkDistance);
        Vector3 checkRight = transform.position + new Vector3(checkDistance, 1f, checkDistance);
        Vector3 checkLeft = transform.position + new Vector3(-checkDistance, 1f, checkDistance);

        bool tileForward = Physics.Raycast(checkForward, Vector3.down, rayLength, tileLayer);
        bool tileRight = Physics.Raycast(checkRight, Vector3.down, rayLength, tileLayer);
        bool tileLeft = Physics.Raycast(checkLeft, Vector3.down, rayLength, tileLayer);

        int newSide = side;

        if (tileRight && side != 1) newSide = 1;
        else if (tileLeft && side != -1) newSide = -1;

        // only apply + send if side actually changed
        if (newSide != side)
        {
            side = newSide;
            temp_side = newSide;

            // only send if this is a new direction we haven't sent yet
            if (side != _lastSentSide)
            {
                _lastSentSide = side;
                int networkSide = side == -1 ? 0 : 1;
                ConnectionManager.instance?.SendMoveInput(networkSide);
            }
        }
    }

    public void SetSpeed(float speed)
    {
        _speed = speed;
    }

    void FixedUpdate()
    {
        if (sceneManager.GameState != 1) return;
        if (gameManager.isGameOver) return;

        CheckGrounded();
        if (!isGrounded) velocity.y -= gravity * Time.fixedDeltaTime;
        else velocity.y = 0;

        move = new Vector3(side, 0f, 1f).normalized;
        transform.Translate(
            move * Time.fixedDeltaTime * _speed,
            Space.World
        );
        transform.Translate(new Vector3(0, velocity.y, 0) * Time.fixedDeltaTime, Space.World);

    }

    void Update()
    {
        if (sceneManager.GameState == 0)
        {
            if (tileFaller != null) tileFaller.SetActive(false);
            if (Scorer != null) Scorer.SetActive(false);

            return;
        }

        if (sceneManager.GameState == 1)
        {
            if (gameManager.isGameOver) return;
            if (autoTurn) AutoTurnRaycast();

            if (tileFaller != null) tileFaller.SetActive(true);
            if (Scorer != null) Scorer.SetActive(true);

            // if (transform.position.y < -1 && gameManager != null && !gameManager.isGameOver) playerDied?.Invoke();
            if (moveParticles != null) moveParticles.gameObject.SetActive(true);
            rotation = Quaternion.Euler(0, side * 45f, 0);
            transform.rotation = Quaternion.Lerp(transform.rotation, rotation, Time.deltaTime * rotationSpeed);

            if (cam != null) cam.position = transform.position + cameraOffset;
            if (bgParticles != null) bgParticles.position = new Vector3(transform.position.x, bgParticles.position.y, transform.position.z + 20);
        }
    }

    void CheckGrounded()
    {
        isGrounded = Physics.Raycast(transform.position, Vector3.down, .5f, tileLayer);
    }

    public float getSpeed()
    {
        return _speed;
    }

    public float getGravity()
    {
        return _speed;
    }

    public void turn()
    {
        if (sceneManager.GameState == 1)
        {
            temp_side *= -1;
            side = temp_side;
            ConnectionManager.instance.SendMoveInput(side);
        }
    }

    public void changeDir(int side)
    {
        if (side == 0) temp_side = -1;
        else if (side == 1) temp_side = 1;
    }
}
