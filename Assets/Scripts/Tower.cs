using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Tower : MonoBehaviour
{
    public float maxHP = 100f;
    public float currentHP;
    public float decayRate = 1f;
    
    public Light towerLight;
    public float maxLightRange = 20f;
    public float minLightRange = 5f;

    public Image hpBarFill;
    public Canvas hpCanvas;

    private Vector3 initialScale;
    public float minScale = 0.5f;
    public float maxScale = 1.2f;

    void Start()
    {
        initialScale = transform.localScale;
        currentHP = maxHP;
        if (towerLight == null) towerLight = GetComponentInChildren<Light>();
    }

    void Update()
    {
        if (currentHP > 0)
        {
            currentHP -= decayRate * Time.deltaTime;
        }

        float hpPercent = currentHP / maxHP;
        float targetRange = Mathf.Lerp(minLightRange, maxLightRange, hpPercent); 
        towerLight.range = Mathf.Lerp(towerLight.range, targetRange, Time.deltaTime * 2f);


        hpBarFill.fillAmount = hpPercent;

        hpBarFill.color = Color.Lerp(Color.red, Color.green, hpPercent); 
        
        hpCanvas.transform.LookAt(hpCanvas.transform.position + Camera.main.transform.rotation * Vector3.forward, Camera.main.transform.rotation * Vector3.up);

        Vector3 targetScale = initialScale * Mathf.Lerp(minScale, maxScale, hpPercent);
        
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * 2f);
    }

    public void AddEnergy(float amount)
    {
        currentHP += amount;
        if (currentHP > maxHP) currentHP = maxHP;
    }

    public bool IsInsideLight(Vector3 position)
    {
        float distance = Vector3.Distance(transform.position, position);
        return distance <= towerLight.range;
    }
}
