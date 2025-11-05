using Fusion;
using UnityEngine;

public class Player : NetworkBehaviour
{
    [Networked] public NetworkObject HeldItem { get; private set; }

    [SerializeField] private Transform holdPoint;
    [SerializeField] private float interactRange = 2f;
    [SerializeField] private float interactHeight = 1.5f;

    private Vector3 _forward;
    private NetworkCharacterController _cc;

    private void Awake()
    {
        _cc = GetComponent<NetworkCharacterController>();
        _forward = transform.forward;
    }

    public override void FixedUpdateNetwork()
    {
        if (GetInput(out NetworkInputData data))
        {
            data.direction.Normalize();
            _cc.Move(5 * data.direction * Runner.DeltaTime);

            if (data.direction.sqrMagnitude > 0)
                _forward = data.direction;

            if (Object.HasInputAuthority && data.buttons.IsSet(NetworkInputData.INTERACT))
                TryInteract();
        }


        if (HeldItem != null && Object.HasStateAuthority)
        {
            HeldItem.transform.position = holdPoint.position;
            HeldItem.transform.rotation = holdPoint.rotation;
        }

    }

    private void TryInteract()
    {
        Vector3 start = transform.position + Vector3.up * interactHeight;
        Collider[] hits = Physics.OverlapSphere(start + _forward * interactRange, 0.5f);

        foreach (var hit in hits)
        {
            if (hit.TryGetComponent(out IInteractable interactable))
            {
                if (HeldItem != null)
                {
                    Drop();
                    return;
                }

                interactable.Interact(this);

                if (interactable.CanBePickedUp && hit.TryGetComponent(out NetworkObject netObj))
                {
                    RPC_Pickup(netObj);
                    return;
                }
            }
        }
    }

    public void Drop()
    {
        if (HeldItem == null) return;

        Vector3 origin = holdPoint.position;
        float radius = 0.5f;
        float distance = 2f;

        if (Physics.SphereCast(origin, radius, _forward, out RaycastHit hit, distance))
        {
            if (hit.collider.TryGetComponent<Table>(out Table table))
            {
                RPC_PlaceOnTable(table.Object, HeldItem);
                return;
            }
        }

        Vector3 dropPos = transform.position + _forward * 1f + Vector3.up * 0.3f;
        RPC_DropItem(HeldItem, dropPos, Quaternion.identity);
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_Pickup(NetworkObject item)
    {
        if (HeldItem != null || item == null) return;

        HeldItem = item;

        Rigidbody rb = item.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        Collider playerCol = GetComponent<Collider>();
        Collider itemCol = item.GetComponent<Collider>();
        if (playerCol && itemCol)
            Physics.IgnoreCollision(playerCol, itemCol, true);

        // ❌ USUŃ: item.AssignInputAuthority(Object.InputAuthority);
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_DropItem(NetworkObject itemObj, Vector3 pos, Quaternion rot)
    {
        if (itemObj == null) return;

        Rigidbody rb = itemObj.GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = false;

        itemObj.transform.position = pos;
        itemObj.transform.rotation = rot;

        Collider playerCol = GetComponent<Collider>();
        Collider itemCol = itemObj.GetComponent<Collider>();
        if (playerCol && itemCol)
            Physics.IgnoreCollision(playerCol, itemCol, false);

        if (HeldItem == itemObj)
            HeldItem = null;
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    private void RPC_PlaceOnTable(NetworkObject tableObj, NetworkObject itemObj)
    {
        if (tableObj == null || itemObj == null) return;

        Table table = tableObj.GetComponent<Table>();
        if (table == null || table.HeldItem != null) return;

        table.ReceiveItem(itemObj);
        HeldItem = null;

        Rigidbody rb = itemObj.GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = true;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Vector3 start = transform.position + Vector3.up * interactHeight;
        Gizmos.DrawWireSphere(start + _forward * interactRange, 0.4f);
    }
}
