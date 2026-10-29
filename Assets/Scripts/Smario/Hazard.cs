using UnityEngine;

// Daña a Mario al tocarlo. Si kill = true (lava) lo mata directamente.
public class Hazard : MonoBehaviour
{
    public bool kill = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        Player player = other.GetComponent<Player>();
        if (player == null) player = other.GetComponentInParent<Player>();
        if (player == null || player.dead) return;

        if (kill) player.Death();
        else player.Hit();
    }
}
