using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    // ---------------------------------------------------------
    //  FIELDS & SETTINGS
    // ---------------------------------------------------------

    /// <summary>
    /// Reference to the central game manager controlling level flow.
    /// </summary>
    private GameManager _gameManager;

    // Player behaviour values
    [SerializeField] public float _speed = 10;
    [SerializeField] private float _jumpHeight = 10;
    [SerializeField] private float _movementAccel = 50;
    [SerializeField] private float _playerDieAfterHeight = -10;

    [SerializeField] private int _extraJumps = 1;
    [SerializeField] private bool _canWallJump = true;
    [SerializeField] private bool _canDoubleJump = true;

    // Player state flags
    private bool _isGrounded;
    private bool _hasJumpRemaining;
    private int _extraJumpCounter;
    private bool _hasUsedWallJump;
    private bool _hasLevelFinished;

    // Input system actions
    private InputMap _playerInput;
    private InputAction _move;
    private InputAction _jump;

    // OverlapBox used for ground detection
    private struct OverlapBox
    {
        public Vector2 position;
        public Vector2 size;
    }

    // Default ground-check box
    private OverlapBox _overlapBox = new OverlapBox
    {
        position = Vector2.zero,
        size = new Vector2(0.5f, 0.2f)
    };

    // Player components
    private Rigidbody2D _rigidbody;
    private SpriteRenderer _spriteRenderer;

    [SerializeField] private Animator _animator;
    [SerializeField] private SoundPlayer _soundPlayer;


    // ---------------------------------------------------------
    //  INITIALIZATION
    // ---------------------------------------------------------

    /// <summary>
    /// Initializes input actions and component references.
    /// </summary>
    private void Awake()
    {
        _playerInput = new InputMap();
        _move = _playerInput.PlayerMovement.Move;
        _jump = _playerInput.PlayerMovement.Jump;

        _rigidbody = GetComponent<Rigidbody2D>();
        _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    /// <summary>
    /// Enables player input and spawns the player at the start position.
    /// </summary>
    private void OnEnable()
    {
        _playerInput.Enable();
        Spawn();
    }

    /// <summary>
    /// Disables input when the object is disabled.
    /// </summary>
    private void OnDisable()
    {
        _playerInput.Disable();
    }


    // ---------------------------------------------------------
    //  COLLISION HANDLING
    // ---------------------------------------------------------

    /// <summary>
    /// Called when the player stops colliding with something.
    /// Resets ground/wall/platform state.
    /// </summary>
    private void OnCollisionExit2D(Collision2D collision)
    {
        switch (collision.collider.tag.ToLower())
        {
            case "ground":
                _isGrounded = false;
                _hasJumpRemaining = false;
                break;

            case "wall":
                if (_canWallJump) _hasJumpRemaining = false;
                break;

            case "platform":
                _hasJumpRemaining = false;
                _hasUsedWallJump = false;
                break;
        }
    }

    /// <summary>
    /// Called every frame while colliding with an object.
    /// Handles grounded check, wall jump conditions, and platform interactions.
    /// </summary>
    private void OnCollisionStay2D(Collision2D collision)
    {
        switch (collision.collider.tag.ToLower())
        {
            case "ground":
                if (IsCollisionAGroundCollision())
                {
                    _isGrounded = true;
                    _hasJumpRemaining = true;

                    // Reset double jump if enabled
                    if (_canDoubleJump) ResetExtraJumps();
                }
                break;

            case "wall":
                // Allow wall jump only once until grounded again
                if (_canWallJump)
                {
                    if (!_isGrounded &&
                        !_hasJumpRemaining &&
                        !_hasUsedWallJump)
                    {
                        _hasJumpRemaining = true;
                        _hasUsedWallJump = true;
                        _extraJumpCounter = 0;
                    }
                }
                break;

            case "platform":
                if (IsCollisionAGroundCollision())
                {
                    _hasJumpRemaining = true;
                    if (_canDoubleJump) ResetExtraJumps();
                }
                break;
        }
    }


    // ---------------------------------------------------------
    //  GROUND CHECKING
    // ---------------------------------------------------------

    /// <summary>
    /// Uses an OverlapBox to check if the player is standing on ground layers.
    /// </summary>
    private bool IsCollisionAGroundCollision()
    {
        Collider2D hit = Physics2D.OverlapBox(
            _overlapBox.position,
            _overlapBox.size,
            0f,
            LayerMask.GetMask("GroundObject")
        );
        return hit != null;
    }

    /// <summary>
    /// Updates the ground check box position based on the player's current position.
    /// </summary>
    private void RefreshOverlayBoxPosition()
    {
        Vector3 newBoxPos = _rigidbody.transform.position;
        newBoxPos.y = _rigidbody.transform.position.y
                      - _rigidbody.transform.localScale.y / 2
                      - _overlapBox.size.y / 2;

        _overlapBox.position = newBoxPos;
    }

    private void LateUpdate()
    {
        RefreshOverlayBoxPosition();
    }


    // ---------------------------------------------------------
    //  MOVEMENT & ACTIONS
    // ---------------------------------------------------------

    /// <summary>
    /// Handles horizontal motion, acceleration smoothing and fall-death detection.
    /// </summary>
    private void Move()
    {
        Vector2 velocity = _rigidbody.linearVelocity;

        // Smooth horizontal acceleration
        velocity.x = Mathf.MoveTowards(
            _rigidbody.linearVelocity.x,
            _move.ReadValue<float>() * _speed,
            _movementAccel * Time.deltaTime
        );

        _rigidbody.linearVelocity = velocity;
        _animator.SetFloat("Speed", Mathf.Abs(velocity.x));

        // Kill the player if they fall too low
        if (_rigidbody.transform.position.y < _playerDieAfterHeight)
        {
            Kill();
        }
    }

    /// <summary>
    /// Performs a jump with sound and upward force application.
    /// </summary>
    private void Jump()
    {
        _hasUsedWallJump = true;
        _isGrounded = false;

        _soundPlayer.PlaySound("jump");

        _rigidbody.AddForce(Vector2.up * _jumpHeight / 10, ForceMode2D.Impulse);
    }

    /// <summary>
    /// Flips the player sprite depending on movement direction.
    /// </summary>
    private void FlipSprite()
    {
        if (_move.ReadValue<float>() < 0) _spriteRenderer.flipX = false;
        else if (_move.ReadValue<float>() > 0) _spriteRenderer.flipX = true;
    }


    // ---------------------------------------------------------
    //  LEVEL END CHECK
    // ---------------------------------------------------------

    /// <summary>
    /// Returns true if the player moved past the level's designated endpoint.
    /// </summary>
    private bool IsLevelEndpointReached()
    {
        return _rigidbody.transform.position.x
               - GameManager.Instance.GetLevelEndAnchorPosition().x > 0;
    }


    // ---------------------------------------------------------
    //  UPDATE LOOP
    // ---------------------------------------------------------

    /// <summary>
    /// Handles input, movement, jumping logic and level completion.
    /// </summary>
    void Update()
    {
        FlipSprite();
        Move();

        // Jump input (ground, wall or double jump)
        if (_jump.WasPressedThisFrame())
        {
            if (_isGrounded) Jump();
            else if (_hasJumpRemaining) Jump();
            else if (_extraJumpCounter > 0)
            {
                Jump();
                _extraJumpCounter--;
            }
        }

        // Level finish check
        if (IsLevelEndpointReached() && !_hasLevelFinished)
        {
            _soundPlayer.PlaySound("level_finished");
            GameManager.Instance.LoadNextScene();
            Spawn();
        }
    }


    // ---------------------------------------------------------
    //  SPAWN / DEATH
    // ---------------------------------------------------------

    /// <summary>
    /// Kills the player, plays sound and respawns them.
    /// </summary>
    private void Kill()
    {
        _soundPlayer.PlaySound("die");
        _rigidbody.linearVelocity = Vector2.zero;
        Spawn();
    }

    /// <summary>
    /// Respawns the player at the level's spawn anchor.
    /// </summary>
    public void Spawn()
    {
        _hasLevelFinished = false;
        _rigidbody.transform.position = GameManager.Instance.GetLevelSpawnAnchorPosition();
    }

    /// <summary>
    /// Resets the extra jump counter back to the configured amount.
    /// </summary>
    public void ResetExtraJumps()
    {
        _extraJumpCounter = _extraJumps;
    }


    // ---------------------------------------------------------
    //  GIZMOS
    // ---------------------------------------------------------

    /// <summary>
    /// Draws death line and ground-check gizmos in the editor.
    /// </summary>
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;

        // Draw death height
        Gizmos.DrawLine(
            new Vector3(transform.position.x - 3, _playerDieAfterHeight, transform.position.z),
            new Vector3(transform.position.x + 3, _playerDieAfterHeight, transform.position.z)
        );

        // Draw ground check box
        Gizmos.DrawWireCube(_overlapBox.position, _overlapBox.size);
    }
}
