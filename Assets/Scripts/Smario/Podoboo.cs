using UnityEngine;

// Bola de lava que salta desde la lava
public class Podoboo : MonoBehaviour
{
    public float height = 6f;
    public float period = 3f;
    public float offset = 0f;
    private Vector3 start;
    private SpriteRenderer sr;

    private void Start()
    {
        start = transform.position;
        sr = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        float t = Mathf.Repeat(Time.time + offset, period) / period;
        float jump = t < 0.6f ? Mathf.Sin(t / 0.6f * Mathf.PI) : 0f;
        transform.position = start + Vector3.up * jump * height;
        if (sr != null) sr.flipY = t > 0.3f;
    }
}
