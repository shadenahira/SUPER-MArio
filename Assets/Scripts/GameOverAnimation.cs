   using UnityEngine;
   using TMPro;

   public class GameOverAnimation : MonoBehaviour
   {
       public RectTransform title;     // texto GAME OVER
       public TMP_Text prompt;         // texto "Presiona cualquier tecla"
       public float growTime = 1f;
       public float blinkSpeed = 4f;

       void Update()
       {
           float t = Time.timeSinceLevelLoad;

           // GAME OVER crece desde 0 hasta su tamaño normal
           float s = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t / growTime));
           title.localScale = Vector3.one * s;

           // "Presiona cualquier tecla" aparece y parpadea
           Color c = prompt.color;
           c.a = t < 1.5f ? 0f : (Mathf.Sin(t * blinkSpeed) + 1f) / 2f;
           prompt.color = c;
       }
   }