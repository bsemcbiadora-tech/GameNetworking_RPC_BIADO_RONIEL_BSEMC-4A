using UnityEngine;
using Unity.Netcode;

public class Collectible : NetworkBehaviour
{
    public enum CollectibleType { Coin, Gem, Crystal }

    [Header("Collectible Settings")]
    [SerializeField] private CollectibleType collectibleType = CollectibleType.Coin;
    [SerializeField] private int points = 10;
    [SerializeField] private Color color = new Color(1f, 0.84f, 0f); // gold

    // Client-side guard: stops the owning client from firing more than one
    // collection request for this coin while it waits for the server to despawn it.
    private bool collectRequested = false;

    private void Awake()
    {
        // Tint the shared default material without needing a custom material asset.
        Renderer rend = GetComponent<Renderer>();
        if (rend != null)
        {
            rend.material.color = color;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (collectRequested) return;

        NetworkObject playerNetObj = other.GetComponentInParent<NetworkObject>();
        // Only the client that owns the colliding player reacts - otherwise every
        // connected client would independently fire a collection request for the
        // same trigger event.
        if (playerNetObj == null || !playerNetObj.IsOwner) return;

        collectRequested = true;
        CollectServerRpc();
    }

    // Player collects collectible -> RPC -> server processes the collection.
    [ServerRpc(RequireOwnership = false)]
    private void CollectServerRpc(ServerRpcParams rpcParams = default)
    {
        // Guards against a coin that another player already collected a moment earlier.
        if (NetworkObject == null || !NetworkObject.IsSpawned) return;

        ulong collectorClientId = rpcParams.Receive.SenderClientId;

        if (NetworkManager.Singleton.ConnectedClients.TryGetValue(collectorClientId, out var client) &&
            client.PlayerObject != null)
        {
            PlayerScore score = client.PlayerObject.GetComponent<PlayerScore>();
            if (score != null)
            {
                // Score increases -> NetworkVariable synchronizes the score.
                score.ServerAddScore(points);
            }
        }

        // Announce which player collected the object and how many points were awarded.
        AnnounceCollectionClientRpc(collectorClientId, collectibleType.ToString(), points);

        // Collectible is removed/despawned so it cannot be collected again.
        NetworkObject.Despawn(true);
    }

    [ClientRpc]
    private void AnnounceCollectionClientRpc(ulong collectorClientId, string collectibleName, int pointsAwarded)
    {
        string label = $"Player {collectorClientId + 1}";
        CollectionAnnouncer.Instance?.Show($"{label} collected a {collectibleName}! +{pointsAwarded} Points");
    }
}
