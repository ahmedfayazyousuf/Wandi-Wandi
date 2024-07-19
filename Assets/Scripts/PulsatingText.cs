using UnityEngine;
using TMPro;

public class PulsatingText : MonoBehaviour
{
    public TextMeshProUGUI textToPulsate;
    public float pulseSpeed = 1.0f;
    public float maxScale = 1.2f;
    public float minScale = 0.8f;

    private Vector3 originalScale;

    void Start()
    {
        if (textToPulsate == null)
        {
            textToPulsate = GetComponent<TextMeshProUGUI>();
        }
        originalScale = textToPulsate.transform.localScale;
    }

    void Update()
    {
        float scale = minScale + Mathf.PingPong(Time.time * pulseSpeed, maxScale - minScale);
        textToPulsate.transform.localScale = originalScale * scale;
    }
}
