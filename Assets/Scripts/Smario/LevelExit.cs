using System.Collections;
using UnityEngine;

// Salida de nivel (tuberia al final): pasa al siguiente nivel
public class LevelExit : MonoBehaviour
{
    public int nextWorld = 1;
    public int nextStage = 1;
    public AudioClip enterSound;
    public AudioClip clearSound;
    private bool used;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (used || !other.CompareTag("Player")) return;
        used = true;
        StartCoroutine(Exit(other.transform));
    }

    private IEnumerator Exit(Transform player)
    {
        PlayerMovement movement = player.GetComponent<PlayerMovement>();
        if (movement != null) movement.enabled = false;
        AudioManager.StopMusic();
        AudioManager.Play(enterSound);

        Vector3 from = player.position;
        Vector3 to = from + Vector3.right * 1.5f;
        float e = 0f;
        while (e < 1f)
        {
            e += Time.deltaTime;
            player.position = Vector3.Lerp(from, to, e);
            player.localScale = Vector3.Lerp(Vector3.one, Vector3.one * 0.5f, e);
            yield return null;
        }
        player.gameObject.SetActive(false);
        AudioManager.Play(clearSound);
        yield return new WaitForSeconds(3f);
        GameManager.Instance.LoadLevel(nextWorld, nextStage);
    }
}
