   using UnityEngine;

   public class GameOverScreen : MonoBehaviour
   {
       public float autoRestart = 5f;

       void Start()
       {
           Invoke(nameof(Restart), autoRestart);
       }

       void Update()
       {
           if (Time.timeSinceLevelLoad > 1.5f && Input.anyKeyDown)
               Restart();
       }

       void Restart()
       {
           CancelInvoke();
           GameManager.Instance.NewGame();
       }
   }