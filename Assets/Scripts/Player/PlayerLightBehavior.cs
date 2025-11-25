using UnityEngine;
using UnityEngine.Rendering.Universal;

public class PlayerLightBehavior : MonoBehaviour
{
    // ---------------------------------------------------------
    //  FIELDS & SETTINGS
    // ---------------------------------------------------------

    /// <summary>
    /// Reference to the 2D light component attached to the player.
    /// </summary>
    private Light2D _light;

    /// <summary>
    /// Maximum light intensity the flicker can reach.
    /// </summary>
    [SerializeField] private float _maxLightIntensity = 3f;

    /// <summary>
    /// Minimum light intensity the flicker can reach.
    /// </summary>
    [SerializeField] private float _minLightIntensity = 0.5f;

    /// <summary>
    /// Speed at which the light flickers.
    /// </summary>
    [SerializeField] private float _lightFlickerSpeed = 5f;


    // ---------------------------------------------------------
    //  INITIALIZATION
    // ---------------------------------------------------------

    /// <summary>
    /// Fetches the Light2D component on Start.
    /// </summary>
    void Start()
    {
        _light = GetComponent<Light2D>();
    }


    // ---------------------------------------------------------
    //  UPDATE LOOP
    // ---------------------------------------------------------

    /// <summary>
    /// Updates the light intensity every frame to create a flickering effect.
    /// Moves intensity toward a random value between min and max each frame.
    /// </summary>
    void LateUpdate()
    {
        _light.intensity = Mathf.MoveTowards(
            _light.intensity,
            Random.Range(_minLightIntensity, _maxLightIntensity),
            _lightFlickerSpeed * Time.deltaTime
        );
    }
}
