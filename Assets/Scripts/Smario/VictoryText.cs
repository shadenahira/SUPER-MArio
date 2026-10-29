using UnityEngine;

// Muestra "¡GANASTE!" en pantalla al vencer a Bowser
public class VictoryText : MonoBehaviour
{
    private static bool show;
    private float t;

    public static void Show()
    {
        show = true;
        new GameObject("VictoryText").AddComponent<VictoryText>();
    }

    private void Update() { t += Time.deltaTime; }

    private void OnGUI()
    {
        if (!show) return;
        GUIStyle style = new GUIStyle(GUI.skin.label);
        style.alignment = TextAnchor.MiddleCenter;
        style.fontSize = Mathf.RoundToInt(Screen.height * 0.09f * Mathf.Clamp01(t * 2f));
        style.fontStyle = FontStyle.Bold;
        style.normal.textColor = Color.Lerp(Color.yellow, Color.white, Mathf.PingPong(t * 2f, 1f));
        GUI.Label(new Rect(0, 0, Screen.width, Screen.height), "¡GANASTE!\nRescataste el reino", style);
    }

    private void OnDestroy() { show = false; }
}
