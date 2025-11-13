
using System;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CollisionManager : MonoBehaviour
{
    public static CollisionManager Instance { get; private set; }
    private Rigidbody2D _playerRigidBody;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

    }

    private struct OverlapBox
    {
        public Vector2 position;
        public Vector2 size;
    }

    private OverlapBox _overlapBox = new OverlapBox { position = Vector2.zero, size = new Vector2(0.5f, 0.2f) };

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(_overlapBox.position, _overlapBox.size);
    }

    private void RefreshOverlayBoxPosition()
    {
        Vector3 newBoxPos = _playerRigidBody.transform.position;
        newBoxPos.y = _playerRigidBody.transform.position.y - _playerRigidBody.transform.localScale.y / 2 - _overlapBox.size.y / 2;
        _overlapBox.position = newBoxPos;
    }

    private void LateUpdate()
    {
        RefreshOverlayBoxPosition();
    }

    private bool IsCollisionAGroundCollision()
    {

        Collider2D hit = Physics2D.OverlapBox(_overlapBox.position, _overlapBox.size, 0f, LayerMask.GetMask("GroundObject"));

        if (hit == null)
        {
            return false;
        }

        return true;

    }

    public void HandlePlayerCollisionStay(Collision2D collision)
    {
        switch (collision.collider.tag.ToLower())
        {
            case "ground":
                {
                    if (IsCollisionAGroundCollision())
                    {
                        PlayerController._isGrounded = true;
                        PlayerController._hasJumpRemaining = true;
                    }
                    break;

                }

            case "wall":
                {
                    if (_playerRigidBody.GetComponentInParent<PlayerController>()._canWallJump)
                    {
                        if (!PlayerController._isGrounded && !PlayerController._hasJumpRemaining && !PlayerController._hasUsedWallJump)
                        {
                            PlayerController._hasJumpRemaining = true;
                            PlayerController._hasUsedWallJump = true;
                        }
                    }
                    break;
                }

            case "platform":
                {
                    if (IsCollisionAGroundCollision())
                    {
                        PlayerController._hasJumpRemaining = true;
                    }
                    break;
                }
        }
    }

    private void Start()
    {
        _playerRigidBody = FindFirstObjectByType<PlayerController>().GetComponentInParent<Rigidbody2D>();
    }

    public void HandlePlayerCollisionExit(Collision2D collision)
    {
        switch (collision.collider.tag.ToLower())
        {

            case "ground":
                {
                    PlayerController._isGrounded = false;
                    PlayerController._hasJumpRemaining = false;
                    break;
                }
            case "wall":
                if (_playerRigidBody.GetComponentInParent<PlayerController>()._canWallJump)
                {
                    PlayerController._hasJumpRemaining = false;
                }

                break;
            case "platform":
                {
                    PlayerController._hasJumpRemaining = false;
                    PlayerController._hasUsedWallJump = false;
                    break;
                }
        }
    }
}


