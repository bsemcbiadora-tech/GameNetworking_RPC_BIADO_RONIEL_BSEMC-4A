using UnityEngine;
using Unity.Netcode;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerHealth))]
public class PlayerController : NetworkBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private float turnSpeed = 180f;

    [Header("Shooting Settings")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firePoint;

    [Header("Animation")]
    [SerializeField] private Animator animator;

    private PlayerHealth playerHealth;

    private void Awake()
    {
        playerHealth = GetComponent<PlayerHealth>();
        if (animator == null) animator = GetComponent<Animator>();
    }

    public override void OnNetworkSpawn()
    {
        Debug.Log($"PLAYER SPAWNED - IsOwner: {IsOwner}, IsServer: {IsServer}, HasAuthority: {HasAuthority}");

        if (IsServer)
        {
            transform.position = new Vector3(Random.Range(-5f, 5f), 1f, Random.Range(-5f, 5f));
        }
    }

    private void Update()
    {
        if (!IsOwner) return;

        // Dead players can no longer move or shoot.
        if (playerHealth != null && playerHealth.IsDead.Value)
        {
            if (animator != null) animator.SetBool("IsRunning", false);
            return;
        }

        HandleInput();
    }

    private void HandleInput()
    {
        float moveInput = 0f;
        float rotateInput = 0f;

        if (Keyboard.current != null)
        {
            if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) moveInput += 1f;
            if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) moveInput -= 1f;
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) rotateInput -= 1f;
            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) rotateInput += 1f;
        }
        else
        {
            moveInput = Input.GetAxisRaw("Vertical");
            rotateInput = Input.GetAxisRaw("Horizontal");
        }

        bool isMoving = moveInput != 0f || rotateInput != 0f;
        if (animator != null) animator.SetBool("IsRunning", isMoving);

        if (isMoving)
        {
            MoveServerRpc(moveInput, rotateInput, Time.deltaTime);
        }
        bool shoot = false;
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame) shoot = true;
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame) shoot = true;
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0)) shoot = true;

        if (shoot)
        {
            Transform spawn = firePoint != null ? firePoint : transform;
            FireServerRpc(spawn.position, spawn.rotation);
        }
    }

    [ServerRpc]
    private void MoveServerRpc(float moveInput, float rotateInput, float deltaTime)
    {
        transform.Rotate(Vector3.up * rotateInput * turnSpeed * deltaTime);
        Vector3 moveDir = transform.forward * moveInput * moveSpeed * deltaTime;
        transform.position += moveDir;
    }

    [ServerRpc]
    private void FireServerRpc(Vector3 spawnPosition, Quaternion spawnRotation, ServerRpcParams rpcParams = default)
    {
        if (bulletPrefab == null) return;

        GameObject bulletInstance = Instantiate(bulletPrefab, spawnPosition, spawnRotation);

        NetworkBullet bullet = bulletInstance.GetComponent<NetworkBullet>();
        if (bullet != null)
        {
            bullet.SetShooter(rpcParams.Receive.SenderClientId);
        }

        NetworkObject netObj = bulletInstance.GetComponent<NetworkObject>();
        if (netObj != null)
        {
            netObj.Spawn(true);
        }
    }
}