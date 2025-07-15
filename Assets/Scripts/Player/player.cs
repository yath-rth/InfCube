using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class player : MonoBehaviour, ISaveFuncs
{
    public static player instance;

    public Action playerDied;

    [SerializeField] string id;
    string ISaveFuncs.id => id;

    GameObject tileFaller, Scorer;
    Controls controls;
    CharacterController cc;
    GameManager gameManager;

    [SerializeField] Transform cam, bgParticles, moveParticles;
    [SerializeField, Range(0, 10f)] float minSpeed, maxSpeed;
    [SerializeField, Range(0, 1f)] float speedIncrement;
    [SerializeField, Range(0, 100f)] float gravity, rotationSpeed;

    int side = 0, temp_side = 0;
    float _speed = 0;
    Vector3 velocity, move, cameraOffset;
    Quaternion rotation;

    void Awake()
    {
        if (instance != null) Destroy(instance.gameObject);
        instance = this;

        cc = GetComponent<CharacterController>();
        gameManager = GameManager.instance;

        tileFaller = transform.GetChild(0).gameObject;
        Scorer = transform.GetChild(1).gameObject;

        if (tileFaller != null) tileFaller.SetActive(false);
        if (Scorer != null) Scorer.SetActive(false);
        if (moveParticles != null) moveParticles.gameObject.SetActive(false);

        controls = new Controls();

        controls.movement.turn.performed += ctx => turn();
        controls.movement.escape.performed += ctx => gameManager.close();
        controls.movement.mainMenu.performed += ctx => gameManager.mainMenu();
        controls.movement.space.performed += ctx => gameManager.restart();
        controls.movement.space.performed += ctx => sceneManager.instance.Game();
        controls.movement.leaderboard.performed += ctx => gameManager.showLeaderboard();
        controls.movement.shop.performed += ctx => gameManager.shop();

        if (cam != null) cameraOffset = cam.position - transform.position;

        if (bgParticles != null)
        {
            ParticleSystemRenderer renderer = bgParticles.GetComponent<ParticleSystemRenderer>();
            renderer.material.renderQueue = 3000;
            renderer.sortingOrder = 1;
        }

        _speed = minSpeed;
    }

    void OnEnable()
    {
        controls.Enable();
    }
    void OnDisable()
    {
        controls.Disable();
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

    void Update()
    {
        if (sceneManager.GameState == 0)
        {
            if (tileFaller != null) tileFaller.SetActive(false);
            if (Scorer != null) Scorer.SetActive(false);
        }

        if (cc != null && sceneManager.GameState == 1)
        {
            if (tileFaller != null) tileFaller.SetActive(true);
            if (Scorer != null) Scorer.SetActive(true);

            if (transform.position.y < -1 && gameManager != null && !gameManager.isGameOver) playerDied?.Invoke();

            if (!cc.isGrounded) velocity.y -= gravity * Time.deltaTime;
            else velocity.y = 0;

            if (!gameManager.isGameOver)
            {
                if (moveParticles != null) moveParticles.gameObject.SetActive(true);
                _speed += speedIncrement * Time.deltaTime;
                _speed = Mathf.Clamp(_speed, minSpeed, maxSpeed);

                move = new Vector3(side * _speed, velocity.y, _speed).normalized;
                cc.Move(move * Time.deltaTime * _speed);

                rotation = Quaternion.Euler(0, side * 45f, 0);
                transform.rotation = Quaternion.Lerp(transform.rotation, rotation, Time.deltaTime * rotationSpeed);

                if (cam != null) cam.position = transform.position + cameraOffset;
                if (bgParticles != null) bgParticles.position = new Vector3(transform.position.x, bgParticles.position.y, transform.position.z + 20);
            }
            else
            {
                move = new Vector3(0, velocity.y, 0);
                cc.Move(move * Time.deltaTime);
                if (moveParticles != null) moveParticles.gameObject.SetActive(false);

            }
        }
    }

    public void DecreaseSpeed(float decreaseAmt)
    {
        _speed -= decreaseAmt;
        _speed = Mathf.Clamp(_speed, minSpeed, maxSpeed);
    }

    void turn()
    {
        if (sceneManager.GameState == 1)
        {
            temp_side *= -1;
            side = temp_side;
        }
    }

    public void changeDir(int side)
    {
        if (side == 0) temp_side = -1;
        else if (side == 1) temp_side = 1;
    }
}
