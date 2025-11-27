using System.Collections;
using UnityEngine;

public class Sun : MonoBehaviour
{
    public static Sun Instance { get; set; }

    public Light directionalLight;

    public float dayAngle = 90f;
    public float duskAngle = 10f;
    public float nightAngle = -20f;

    public float transitionDuration = 3.0f;
    
    public Gradient lightColor;
    public float maxIntensity = 1.5f;

    private Coroutine sunCoroutine;

    public void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    void Start()
    {
        SetSunAngle(dayAngle);
    }

    public void OnWaveChanged(int waveIndex)
    {
        float targetAngle = dayAngle;
        if (waveIndex >= 8)
        {
            targetAngle = nightAngle;
        }
        else
        {
            float t = (waveIndex - 1) / 7f; 
            
            targetAngle = Mathf.Lerp(dayAngle, duskAngle, t);
        }

        if (sunCoroutine != null) StopCoroutine(sunCoroutine);
        sunCoroutine = StartCoroutine(AnimateSun(targetAngle));
    }

    public void OnGameWin()
    {
        float targetAngle = dayAngle;
        
        if (sunCoroutine != null) StopCoroutine(sunCoroutine);
        sunCoroutine = StartCoroutine(AnimateSun(targetAngle));
    }

    private void SetSunAngle(float angle)
    {
        transform.rotation = Quaternion.Euler(angle, 0, 0);
        UpdateLight(angle);
    }

    private IEnumerator AnimateSun(float targetAngle)
    {
        float startAngle = transform.rotation.eulerAngles.x;
        if (startAngle > 180) startAngle -= 360;

        float timer = 0f;

        while (timer < transitionDuration)
        {
            timer += Time.deltaTime;
            float t = timer / transitionDuration;
            float currentAngle = Mathf.Lerp(startAngle, targetAngle, t);

            transform.rotation = Quaternion.Euler(currentAngle, 0, 0);
            
            UpdateLight(currentAngle);

            yield return null;
        }

        SetSunAngle(targetAngle);
    }

    private void UpdateLight(float angle)
    {
        float height01 = Mathf.InverseLerp(0f, 90f, angle);
        
        if (angle <= 0)
        {
            directionalLight.intensity = 0;
        }
        else
        {
            directionalLight.intensity = Mathf.Lerp(0.1f, maxIntensity, height01);
        }

        if (lightColor != null)
        {
            directionalLight.color = lightColor.Evaluate(1 - height01);
        }
        
        RenderSettings.ambientIntensity = Mathf.Lerp(0.1f, 1.0f, height01);
    }
}