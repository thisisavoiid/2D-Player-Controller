using System.Linq;
using Unity.VisualScripting;
using Unity.VisualScripting.InputSystem;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CameraManager : MonoBehaviour
{

    [SerializeField] private bool _changeCameraSizeToPlayerHeight = true;

    [SerializeField] private GameObject Camera;

    public float CameraSmoothness = 7.0f;

    private Rigidbody2D _playerRigidBody;
    private PlayerController _playerController;

    private Vector3 _currentplayerPosition;
    private Vector3 _currentCameraPosition;

    void Start()
    {
        RefreshReferences();
    }

    public void RefreshReferences()
    {
        _playerController = FindFirstObjectByType<PlayerController>();
        _playerRigidBody = _playerController.GetComponentInParent<Rigidbody2D>();

        Camera = GameManager.Instance.GetAllGameObjectsInScene(SceneManager.GetActiveScene().buildIndex).Where(obj => obj.gameObject.CompareTag("MainCamera")).First();
        Camera.GetComponent<Camera>().orthographicSize = 5;
    }

    private void OnEnable()
    {
        RefreshReferences();
    }

    void Update()
    {
        _currentplayerPosition = _playerRigidBody.transform.position;
        _currentCameraPosition = Camera.transform.position;

        Camera.transform.position = Vector3.Lerp(
            _currentCameraPosition,
            new Vector3(
                _currentplayerPosition.x,
                _currentplayerPosition.y,
                _currentCameraPosition.z
            ),
            CameraSmoothness * Time.deltaTime
        );

        if (_changeCameraSizeToPlayerHeight )
        {
            Camera.GetComponent<Camera>().orthographicSize = Mathf.Lerp(
            Camera.GetComponent<Camera>().orthographicSize,
            Mathf.Clamp(
                _playerRigidBody.transform.position.y, 5, 25
                ),
            CameraSmoothness * Time.deltaTime
            );
        }
    }
}
