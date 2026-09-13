using UnityEngine;
using Unity.Netcode;

public class PlayerHUD : NetworkBehaviour
{
    [SerializeField] private Vector3 worldOffset = new Vector3(0, 1.2f, 0);

    private PlayerHealth health;
    private PlayerScore score;

    private void Awake()
    {
        health = GetComponent<PlayerHealth>();
        score = GetComponent<PlayerScore>();
    }

    private void OnGUI()
    {
        if (health == null || Camera.main == null) return;

        // Floating health bar above every player's head, visible on every client.
        Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position + worldOffset);
        if (screenPos.z > 0f)
        {
            float guiY = Screen.height - screenPos.y;
            float barWidth = 80f;
            float barHeight = 10f;
            float pct = Mathf.Clamp01(health.MaxHealth > 0 ? (float)health.Health.Value / health.MaxHealth : 0f);

            Rect bg = new Rect(screenPos.x - barWidth / 2f, guiY, barWidth, barHeight);
            GUI.Box(bg, "");

            Color prevColor = GUI.color;
            GUI.color = Color.red;
            GUI.DrawTexture(new Rect(bg.x, bg.y, barWidth * pct, barHeight), Texture2D.whiteTexture);
            GUI.color = prevColor;

            GUI.Label(new Rect(bg.x, bg.y - 16, barWidth, 16), $"HP: {health.Health.Value}");
        }

        // Local player's own Score/HP readout, top-left corner.
        if (IsOwner && score != null)
        {
            GUILayout.BeginArea(new Rect(20, 250, 200, 60));
            GUILayout.Label($"Score: {score.Score.Value}");
            GUILayout.Label(health.IsDead.Value ? "You are DEAD" : $"HP: {health.Health.Value}");
            GUILayout.EndArea();
        }
    }
}
