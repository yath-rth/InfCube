using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class NetworkPlayer : MonoBehaviour
{
    CharacterController cc;
    [SerializeField] Transform bgParticles, moveParticles;
    [SerializeField, Range(0, 100f)] float gravity, rotationSpeed;
    public int side = 0;
    float _speed = 0;
    Vector3 velocity, move;

    void Awake()
    {
        cc = GetComponent<CharacterController>();
    }

    public void SetInitialSide(int networkSide)
    {
        side = networkSide == 0 ? -1 : 1;
    }

    public void HandleTurn(InputPayload turn)
    {
        side = turn.side == 0 ? 1 : -1;
        transform.position = new Vector3(-turn.posX, transform.position.y, turn.posZ);
    }

    public void SetSpeed(float speed)
    {
        _speed = speed;
    }

    void FixedUpdate()
    {
        if (sceneManager.GameState != 1) return;
        if (GameManager.instance.isGameOver) return;

        if (!cc.isGrounded) velocity.y -= gravity * Time.fixedDeltaTime;
        else velocity.y = 0;
        move = new Vector3(side, 0f, 1f).normalized;
        transform.Translate(
            move * Time.fixedDeltaTime * _speed,
            Space.World
        );
    }

    void Update()
    {
        if (sceneManager.GameState != 1) return;
        if (GameManager.instance.isGameOver)
        {
            if (moveParticles != null) moveParticles.gameObject.SetActive(false);
            return;
        }

        ConnectionManager.instance.ProcessTurn(this);

        if (moveParticles != null) moveParticles.gameObject.SetActive(true);

        Quaternion rotation = Quaternion.Euler(0, side * 45f, 0);
        transform.rotation = Quaternion.Lerp(
            transform.rotation, rotation, Time.deltaTime * rotationSpeed
        );

        if (bgParticles != null) bgParticles.position = new Vector3(
            transform.position.x, bgParticles.position.y, transform.position.z + 20
        );
    }
}