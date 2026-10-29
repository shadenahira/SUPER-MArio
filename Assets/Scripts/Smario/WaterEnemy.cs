using UnityEngine;

// Cheep Cheep (nada a la izquierda ondulando) o Blooper (sube y baja persiguiendo a Mario)
public class WaterEnemy : MonoBehaviour
{
    public bool blooper = false;
    public float speed = 1.5f;
    public float waveHeight = 0.5f;
    public float waveSpeed = 2f;

    private Vector3 start;
    private Transform player;
    private bool active;

    private void Start()
    {
        start = transform.position;
        GameObject p = GameObject.FindWithTag("Player");
        if (p != null) player = p.transform;
    }

    private void OnBecameVisible() { active = true; }

    private void Update()
    {
        if (!active) return;

        if (blooper && player != null)
        {
            float cycle = Mathf.Repeat(Time.time, 1.6f);
            Vector3 pos = transform.position;
            if (cycle < 0.6f) {
                pos.y += 3f * Time.deltaTime;
                pos.x += Mathf.Sign(player.position.x - pos.x) * 2.5f * Time.deltaTime;
            } else {
                pos.y -= 1.2f * Time.deltaTime;
            }
            pos.y = Mathf.Clamp(pos.y, 2.5f, 11f);
            transform.position = pos;
        }
        else
        {
            start += Vector3.left * speed * Time.deltaTime;
            transform.position = start + Vector3.up * Mathf.Sin(Time.time * waveSpeed) * waveHeight;
        }
    }
}
