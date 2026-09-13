using UnityEngine;
using Unity.Netcode;

public class NetworkUI : MonoBehaviour
{
    private void OnGUI()
    {
        if (NetworkManager.Singleton == null) return;

        GUILayout.BeginArea(new Rect(20, 20, 180, 220));

        if (!NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsServer)
        {
            if (GUILayout.Button("Start Host", GUILayout.Height(35)))
            {
                NetworkManager.Singleton.StartHost();
            }

            if (GUILayout.Button("Start Client", GUILayout.Height(35)))
            {
                NetworkManager.Singleton.StartClient();
            }

            if (GUILayout.Button("Start Server", GUILayout.Height(35)))
            {
                NetworkManager.Singleton.StartServer();
            }
        }
        else
        {
            GUILayout.Label($"Role: {(NetworkManager.Singleton.IsHost ? "Host" : NetworkManager.Singleton.IsServer ? "Server" : "Client")}");

            if (GUILayout.Button("Shutdown", GUILayout.Height(30)))
            {
                NetworkManager.Singleton.Shutdown();
            }
        }

        GUILayout.EndArea();
    }
}