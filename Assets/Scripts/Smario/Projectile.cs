using UnityEngine;

// Fuego que avanza en linea recta (fuego de Bowser)
public class Projectile : MonoBehaviour
{
    public Vector3 direction = Vector3.left;
    public float speed = 5f;
    public float lifetime = 6f;

    private void Start() { Destroy(gameObject, lifetime); }

    private void Update() { transform.position += direction * speed * Time.deltaTime; }
}
