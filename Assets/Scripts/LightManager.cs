using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightManager : MonoBehaviour
{
    public static LightManager Instance { get; set; }

    public Light directionalLight;

    public float maxIntensity = 1.0f;
    public float minIntensity = 0.0f;
    
    public float transitionDuration = 3.0f;

    private float targetIntensity;
    private Coroutine transitionCoroutine;


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

    public void OnWaveChanged(int waveIndex)
    {
        if (waveIndex == 10)
        {
            SetTargetIntensity(maxIntensity); 
        }
        else if (waveIndex >= 8)
        {
            SetTargetIntensity(minIntensity);
        }
        else
        {
            float t = (waveIndex - 1) / 6.0f;
            float newIntensity = Mathf.Lerp(maxIntensity, minIntensity, t);
            SetTargetIntensity(newIntensity);
        }
    }

    private void SetTargetIntensity(float target)
    {
        targetIntensity = target;
        if (transitionCoroutine != null) StopCoroutine(transitionCoroutine);
        transitionCoroutine = StartCoroutine(TransitionLight());
    }

    private IEnumerator TransitionLight()
    {
        float startIntensity = directionalLight.intensity;

        float timer = 0f;

        while (timer < transitionDuration)
        {
            timer += Time.deltaTime;
            float t = timer / transitionDuration;
            
            directionalLight.intensity = Mathf.Lerp(startIntensity, targetIntensity, t);
            
            RenderSettings.ambientIntensity = Mathf.Lerp(startIntensity, targetIntensity, t);

            yield return null;
        }
        
        directionalLight.intensity = targetIntensity;
    }

    [ContextMenu("Test Wave 2")]
    public void TestWave2() { OnWaveChanged(2); }
    [ContextMenu("Test Wave 4 (Dark)")]
    public void TestWave4() { OnWaveChanged(4); }
    [ContextMenu("Test Wave 7 (Dark)")]
    public void TestWave7() { OnWaveChanged(7); }
        [ContextMenu("Test Wave 6 (Dark)")]
    public void TestWave6() { OnWaveChanged(6); }
    [ContextMenu("Test Wave 10 (Win)")]
    public void TestWave10() { OnWaveChanged(10); }
}
