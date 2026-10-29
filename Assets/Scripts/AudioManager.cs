   using UnityEngine;

   public class AudioManager : MonoBehaviour
   {
       public static AudioManager Instance { get; private set; }
       public AudioSource music;
       public AudioSource sfx;

       private void Awake()
       {
           Instance = this;

           // Conecta solos los Audio Source: el 1º = música, el 2º = efectos
           AudioSource[] sources = GetComponents<AudioSource>();
           if (music == null && sources.Length > 0) music = sources[0];
           if (sfx == null && sources.Length > 1) sfx = sources[1];
       }

       public static void Play(AudioClip clip)
       {
           if (Instance != null && clip != null) Instance.sfx.PlayOneShot(clip);
       }

       public static void StopMusic()
       {
           if (Instance != null) Instance.music.Stop();
       }
   }