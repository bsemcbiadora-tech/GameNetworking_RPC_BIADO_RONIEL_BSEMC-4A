using UnityEngine;
using Unity.Netcode;

public class PlayerHealth : NetworkBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private int maxHealth = 100;

    public int MaxHealth => maxHealth;

    // Server writes it, everyone reads it - this is what keeps Health in sync
    // between Host and Client.
    public NetworkVariable<int> Health = new NetworkVariable<int>(
        100,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);

    public NetworkVariable<bool> IsDead = new NetworkVariable<bool>(
        false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);

    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            Health.Value = maxHealth;
            IsDead.Value = false;
        }

        IsDead.OnValueChanged += OnDeadChanged;

        // Late-joining clients: if this player already died before we connected,
        // play the death animation immediately instead of waiting for a change event.
        if (IsDead.Value)
        {
            OnDeadChanged(false, true);
        }
    }

    public override void OnNetworkDespawn()
    {
        IsDead.OnValueChanged -= OnDeadChanged;
    }

    private void OnDeadChanged(bool previous, bool current)
    {
        if (!current) return;

        // Only the owner triggers its own Animator - NetworkAnimator (owner authoritative)
        // takes care of replicating that trigger out to every other connected client.
        if (IsOwner && animator != null)
        {
            animator.SetTrigger("Die");
        }
    }

    // Server-only. Called by NetworkBullet when this player's collider is hit.
    public void ServerApplyDamage(int amount, ulong attackerClientId)
    {
        if (!IsServer || IsDead.Value) return;

        Health.Value = Mathf.Max(0, Health.Value - amount);

        if (Health.Value == 0)
        {
            IsDead.Value = true;
            AwardKillScore(attackerClientId);
        }
    }

    private void AwardKillScore(ulong attackerClientId)
    {
        if (NetworkManager.Singleton == null) return;

        if (NetworkManager.Singleton.ConnectedClients.TryGetValue(attackerClientId, out var attacker) &&
            attacker.PlayerObject != null)
        {
            PlayerScore attackerScore = attacker.PlayerObject.GetComponent<PlayerScore>();
            if (attackerScore != null)
            {
                attackerScore.ServerAddScore(1);
            }
        }
    }
}
