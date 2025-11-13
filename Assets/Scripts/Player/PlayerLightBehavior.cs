using UnityEngine;
using UnityEngine.Rendering.Universal;

public class PlayerLightBehavior : MonoBehaviour
{
    private Light2D _light;
    [SerializeField] private float _maxLightIntensity = 3f;
    [SerializeField] private float _minLightIntensity = 0.5f;
    [SerializeField] private float _lightFlickerSpeed = 5f;

    void Start()
    {
        _light = GetComponent<Light2D>();
    }

    void LateUpdate()
    {
        _light.intensity = Mathf.MoveTowards(_light.intensity, Random.Range(_minLightIntensity, _maxLightIntensity), _lightFlickerSpeed * Time.deltaTime);
    }
}
