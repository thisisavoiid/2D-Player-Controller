using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    // Game manager objects
    private GameManager _gameManager;
    private CollisionManager _collisionManager;

    // Player behaviour values
    [SerializeField] public  float _speed = 10;
    [SerializeField] private float _jumpHeight = 10;
    [SerializeField] private float _movementAccel = 50;
    [SerializeField] private float _playerDieAfterHeight = -10;
    [SerializeField] private int _extraJumps = 1;

    public bool _canWallJump = true;
    public static bool _isGrounded { get; set; }
    public static bool _hasJumpRemaining { get; set; }
    public static int ExtraJumpCounter { get; set; }
    public static bool _hasUsedWallJump { get; set; }
    public static bool _hasLevelFinished { get; set; }


    // Input
    private InputMap _playerInput;
    private InputAction _move;
    private InputAction _jump;

    // Player components
    private Rigidbody2D _rigidbody;
    private SpriteRenderer _spriteRenderer;
    [SerializeField] private Animator _animator;
    [SerializeField] private SoundPlayer _soundPlayer; // Should be normalized!

    // Component fetching
    private void Awake()
    {
        _playerInput = new InputMap();
        _move = _playerInput.PlayerMovement.Move;
        _jump = _playerInput.PlayerMovement.Jump;
        _rigidbody = GetComponent<Rigidbody2D>();
        _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }


    // Enable and Disable event methods
    private void OnEnable()
    {
        _playerInput.Enable();
        Spawn();

    }

    private void OnDisable()
    {
        _playerInput.Disable();
    }

    // Collision detection
    private void OnCollisionStay2D(Collision2D collision)
    {
        CollisionManager.Instance.HandlePlayerCollisionStay(collision, this);
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        CollisionManager.Instance.HandlePlayerCollisionExit(collision);
    }

    // Movement
    private void Move()
    {
        Vector2 velocity = _rigidbody.linearVelocity;

        velocity.x = Mathf.MoveTowards(
            _rigidbody.linearVelocity.x,
            _move.ReadValue<float>() * _speed,
            _movementAccel * Time.deltaTime
            );

        _rigidbody.linearVelocity = velocity;
        _animator.SetFloat("Speed", Mathf.Abs(velocity.x));

        if (_rigidbody.transform.position.y < _playerDieAfterHeight)
        {
            Kill();
        }
    }

    private void Jump()
    {
        _hasUsedWallJump = true;
        _isGrounded = false;
        _soundPlayer.PlaySound("jump");
        _rigidbody.AddForce(Vector2.up * _jumpHeight / 10, ForceMode2D.Impulse);
    }

    // Flip character sprite
    private void FlipSprite()
    {
        if (_move.ReadValue<float>() < 0)
        {
            _spriteRenderer.flipX = false;
        }
        else if (_move.ReadValue<float>() > 0)
        {
            _spriteRenderer.flipX = true;
        }
    }


    // Endpoint check
    private bool IsLevelEndpointReached()
    {
        return _rigidbody.transform.position.x - GameManager.Instance.GetLevelEndAnchorPosition().x > 0;
    }

    // Update
    void Update()
    {
        FlipSprite();
        Move();

        if (_jump.WasPressedThisFrame())
        {
            if (_isGrounded)
            {
                Jump();
            }
            else if (_hasJumpRemaining)
            {
                Jump();
            }
            else if (ExtraJumpCounter > 0)
            {
                Jump();
                ExtraJumpCounter--;
            }
        }

        if (IsLevelEndpointReached() && !_hasLevelFinished)
        {
            _soundPlayer.PlaySound("level_finished");
            GameManager.Instance.LoadNextScene();
            Spawn();
        }
    }

    // Kill and Spawn methods
    private void Kill()
    {
        _soundPlayer.PlaySound("die");
        _rigidbody.linearVelocity = Vector2.zero;
        Spawn();
    }

    public void Spawn()
    {
        _hasLevelFinished = false;
        _rigidbody.transform.position = GameManager.Instance.GetLevelSpawnAnchorPosition();
        
    }

    public void ResetExtraJumps()
    {
        ExtraJumpCounter = _extraJumps;
    }

    // Gizmos
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(new Vector3(transform.position.x - 3, _playerDieAfterHeight, transform.position.z), new Vector3(transform.position.x + 3, _playerDieAfterHeight, transform.position.z));
    }

}
