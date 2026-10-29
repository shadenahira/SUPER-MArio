using System.Collections;
using UnityEngine;

// Jefe Bowser: camina de lado a lado, salta y lanza fuego
public class Bowser : MonoBehaviour
{
    public float moveRange = 2f;
    public float moveSpeed = 1f;
    public float jumpHeight = 1.5f;
    public float fireInterval = 2.5f;
    public Sprite[] fireSprites;
    public AudioClip fireSound;
    public AudioClip fallSound;

    private Vector3 start;
    private bool falling;
    private float fireTimer;

    private void Start()
    {
        start = transform.position;
        fireTimer = fireInterval;
    }

    private void Update()
    {
        if (falling) return;

        float x = start.x + Mathf.Sin(Time.time * moveSpeed) * moveRange;
        float jump = Mathf.Max(0f, Mathf.Sin(Time.time * 1.3f)) ;
        jump = jump > 0.9f ? (jump - 0.9f) * 10f * jumpHeight : 0f;
        transform.position = new Vector3(x, start.y + jump, start.z);

        fireTimer -= Time.deltaTime;
        if (fireTimer <= 0f)
        {
            fireTimer = fireInterval;
            Fire();
        }
    }

    private void Fire()
    {
        GameObject fire = new GameObject("BowserFire");
        fire.transform.position = transform.position + new Vector3(-1.2f, 0.3f, 0f);
        fire.transform.localScale = new Vector3(2f, 1f, 1f);
        SpriteRenderer sr = fire.AddComponent<SpriteRenderer>();
        sr.sortingOrder = 10;
        if (fireSprites != null && fireSprites.Length > 0) sr.sprite = fireSprites[0];
        AnimatedSprite anim = fire.AddComponent<AnimatedSprite>();
        anim.sprites = fireSprites;
        anim.framerate = 0.1f;
        CircleCollider2D col = fire.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.2f;
        fire.AddComponent<Hazard>();
        fire.AddComponent<Projectile>();
        AudioManager.Play(fireSound);
    }

    public void Fall()
    {
        if (falling) return;
        falling = true;
        AudioManager.Play(fallSound);
        StartCoroutine(FallRoutine());
    }

    private IEnumerator FallRoutine()
    {
        GetComponent<Collider2D>().enabled = false;
        float v = 0f;
        float t = 0f;
        while (t < 3f)
        {
            t += Time.deltaTime;
            v -= 20f * Time.deltaTime;
            transform.position += Vector3.up * v * Time.deltaTime;
            yield return null;
        }
        Destroy(gameObject);
    }
}
