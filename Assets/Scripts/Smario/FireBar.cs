using UnityEngine;

// Barra de fuego giratoria
public class FireBar : MonoBehaviour
{
    public float degreesPerSecond = 90f;

    private void Update() { transform.Rotate(0f, 0f, degreesPerSecond * Time.deltaTime); }
}
