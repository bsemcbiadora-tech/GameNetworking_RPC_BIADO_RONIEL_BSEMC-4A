using System.Collections;
using UnityEngine;
using Unity.Netcode;

// Server-side spawner for the coin collectibles. Bootstraps itself at startup
// so it works with no scene setup required, and loads the Coin prefab from
// Resources so it can be instantiated without an Inspector reference.
public class CollectibleSpawner : MonoBehaviour
{
    private static readonly Vector3[] SpawnPoints =
    {
        new Vector3(3f, 1f, 3f),
        new Vector3(-3f, 1f, 3f),
        new Vector3(3f, 1f, -3f),
        new Vector3(-3f, 1f, -3f),
        new Vector3(0f, 1f, 4f),
    };

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Bootstrap()
    {
        GameObject go = new GameObject("CollectibleSpawner");
        DontDestroyOnLoad(go);
        go.AddComponent<CollectibleSpawner>();
    }

    private void Awake()
    {
        StartCoroutine(WaitForNetworkManagerThenSubscribe());
    }

    private IEnumerator WaitForNetworkManagerThenSubscribe()
    {
        while (NetworkManager.Singleton == null) yield return null;
        NetworkManager.Singleton.OnServerStarted += SpawnCollectibles;
    }

    private void SpawnCollectibles()
    {
        GameObject coinPrefab = Resources.Load<GameObject>("Coin");
        if (coinPrefab == null)
        {
            Debug.LogError("CollectibleSpawner: Coin prefab not found at Assets/Resources/Coin.prefab");
            return;
        }

        foreach (Vector3 pos in SpawnPoints)
        {
            GameObject coin = Instantiate(coinPrefab, pos, Quaternion.identity);
            NetworkObject netObj = coin.GetComponent<NetworkObject>();
            if (netObj != null)
            {
                netObj.Spawn(true);
            }
        }
    }
}
