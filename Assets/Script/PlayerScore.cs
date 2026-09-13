using Unity.Netcode;

public class PlayerScore : NetworkBehaviour
{
    // Server writes it, everyone reads it - keeps Score in sync between Host and Client.
    public NetworkVariable<int> Score = new NetworkVariable<int>(
        0,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            Score.Value = 0;
        }
    }

    // Server-only. Called by PlayerHealth when this player's shot kills someone.
    public void ServerAddScore(int amount)
    {
        if (!IsServer) return;
        Score.Value += amount;
    }
}
