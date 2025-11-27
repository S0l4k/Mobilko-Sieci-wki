using Fusion;
using UnityEngine;

public class Player : NetworkBehaviour
{
    [Networked] public NetworkObject HeldItem { get; private set; }

    [SerializeField] private Transform holdPoint;
    [SerializeField] private float interactRange = 2f;
    [SerializeField] private float interactHeight = 1.2f;

    private Vector3 _forward;
    private NetworkCharacterController _cc;

    private NetworkButtons _previousButtons;

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

            bool interactPressed =
                data.buttons.WasPressed(_previousButtons, NetworkInputData.INTERACT);

            if (Object.HasInputAuthority && interactPressed)
                TryInteract();

            _previousButtons = data.buttons;
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

        Collider[] hits = Physics.OverlapSphere(start + _forward * interactRange, 0.4f);

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

                // PICKUP (KitchenItem or anything with CanBePickedUp=true)
                if (interactable.CanBePickedUp)
                {
                    NetworkObject netObj = hit.GetComponentInParent<NetworkObject>();
                    if (netObj != null)
                    {
                        RPC_Pickup(netObj);
                        return;
                    }

                    Debug.LogWarning($"No NetworkObject found on {hit.name}");
                }
            }
        }
    }


    public void Drop()
    {
        if (HeldItem == null) return;

        Vector3 origin = holdPoint.position;

        if (Physics.SphereCast(origin, 0.5f, _forward, out RaycastHit hit, 1.5f))
        {
            if (hit.collider.TryGetComponent<Table>(out var table))
            {
                RPC_PlaceOnTable(table.Object, HeldItem);
                return;
            }
        }

        Vector3 dropPos = transform.position + _forward * 1f + Vector3.up * 0.3f;
        RPC_DropItem(HeldItem, dropPos, Quaternion.identity);
    }


    // ------- RPCs ---------

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_Pickup(NetworkObject item)
    {
        if (HeldItem != null || item == null) return;

        HeldItem = item;

        if (item.TryGetComponent<Rigidbody>(out var rb))
        {
            rb.isKinematic = true;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        if (TryGetComponent(out Collider playerCol) &&
            item.TryGetComponent(out Collider itemCol))
        {
            Physics.IgnoreCollision(playerCol, itemCol, true);
        }
    }


    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_DropItem(NetworkObject item, Vector3 pos, Quaternion rot)
    {
        if (item == null) return;

        if (item.TryGetComponent<Rigidbody>(out var rb))
            rb.isKinematic = false;

        item.transform.position = pos;
        item.transform.rotation = rot;

        if (TryGetComponent(out Collider playerCol) &&
            item.TryGetComponent(out Collider itemCol))
        {
            Physics.IgnoreCollision(playerCol, itemCol, false);
        }

        if (HeldItem == item)
            HeldItem = null;
    }


    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    private void RPC_PlaceOnTable(NetworkObject tableObj, NetworkObject item)
    {
        if (tableObj == null || item == null) return;

        var table = tableObj.GetComponent<Table>();
        if (table == null || table.HeldItem != null) return;

        table.ReceiveItem(item);
        HeldItem = null;
    }
}
