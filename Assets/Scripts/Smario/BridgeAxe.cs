using System.Collections;
using UnityEngine;

// Hacha al final del puente: rompe el puente, Bowser cae y se gana el juego
public class BridgeAxe : MonoBehaviour
{
    public GameObject[] bridgePieces;
    public Bowser bowser;
    public AudioClip breakSound;
    public AudioClip winSound;
    private bool used;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (used || !other.CompareTag("Player")) return;
        used = true;
        StartCoroutine(Win(other.GetComponent<PlayerMovement>()));
    }

    private IEnumerator Win(PlayerMovement mario)
    {
        AudioManager.StopMusic();
        if (mario != null) mario.enabled = false;
        GetComponent<SpriteRenderer>().enabled = false;

        for (int i = bridgePieces.Length - 1; i >= 0; i--)
        {
            if (bridgePieces[i] != null) Destroy(bridgePieces[i]);
            AudioManager.Play(breakSound);
            yield return new WaitForSeconds(0.08f);
        }

        if (bowser != null) bowser.Fall();
        yield return new WaitForSeconds(1.5f);

        AudioManager.Play(winSound);
        VictoryText.Show();
        yield return new WaitForSeconds(6f);

        GameManager.Instance.NewGame();
    }
}
