using UnityEngine;
using Unity.Netcode;

public class NetworkBullet : NetworkBehaviour
{
    [SerializeField] private float speed = 25f;
    [SerializeField] private float lifeTime = 3f;
    [SerializeField] private int damage = 20;

    private Rigidbody rb;
    private ulong shooterClientId;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Set by PlayerController right after Instantiate, before NetworkObject.Spawn().
    public void SetShooter(ulong clientId)
    {
        shooterClientId = clientId;
    }

    public override void OnNetworkSpawn()
    {
        Debug.Log($"BULLET SPAWNED - IsServer: {IsServer}, HasAuthority: {HasAuthority}");

        if (IsServer)
        {
            rb.linearVelocity = transform.forward * speed;
            Invoke(nameof(DespawnBullet), lifeTime);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!IsServer) return;

        Debug.Log($"({NetworkObjectId}) BULLET: Hit: {collision.gameObject.name}");

        NetworkObject hitNetObj = collision.gameObject.GetComponentInParent<NetworkObject>();
        PlayerHealth hitHealth = collision.gameObject.GetComponentInParent<PlayerHealth>();

        // Ignore self-hits (a player's own bullet colliding with themselves).
        if (hitHealth != null && (hitNetObj == null || hitNetObj.OwnerClientId != shooterClientId))
        {
            hitHealth.ServerApplyDamage(damage, shooterClientId);
        }

        DespawnBullet();
    }

    private void DespawnBullet()
    {
        if (IsServer && NetworkObject != null && NetworkObject.IsSpawned)
        {
            NetworkObject.Despawn(true);
        }
    }
}
