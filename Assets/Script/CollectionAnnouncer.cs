using UnityEngine;

// Shows a temporary banner at the top of the screen announcing who collected
// a collectible and how many points they earned. Bootstraps itself at startup
// so it works with no scene setup required.
public class CollectionAnnouncer : MonoBehaviour
{
    public static CollectionAnnouncer Instance { get; private set; }

    private string message = "";
    private float hideTime = -1f;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Bootstrap()
    {
        if (Instance != null) return;
        GameObject go = new GameObject("CollectionAnnouncer");
        DontDestroyOnLoad(go);
        go.AddComponent<CollectionAnnouncer>();
    }

    private void Awake()
    {
        Instance = this;
    }

    public void Show(string text, float duration = 3f)
    {
        message = text;
        hideTime = Time.time + duration;
    }

    private void OnGUI()
    {
        if (string.IsNullOrEmpty(message) || Time.time > hideTime) return;

        GUIStyle style = new GUIStyle(GUI.skin.box)
        {
            fontSize = 20,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter
        };
        style.normal.textColor = Color.yellow;

        float width = 480f;
        float height = 40f;
        GUI.Box(new Rect((Screen.width - width) / 2f, 20f, width, height), message, style);
    }
}
