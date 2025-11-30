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

            bool interactPressed = data.buttons.WasPressed(_previousButtons, NetworkInputData.INTERACT);
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
        RaycastHit[] hits = Physics.RaycastAll(start, _forward, interactRange);

        foreach (var hit in hits)
        {
            if (!hit.collider.TryGetComponent<IInteractable>(out var interactable))
                continue;

            if (HeldItem != null)
            {
                var heldKi = HeldItem.GetComponent<KitchenItem>();

                if (heldKi.IsLiquid() && interactable is PouringStation)
                {
                    interactable.Interact(this);
                    return;
                }

                Drop();
                return;
            }

            interactable.Interact(this);

            if (interactable.CanBePickedUp)
            {
                NetworkObject netObj = hit.collider.GetComponentInParent<NetworkObject>();
                if (netObj != null) RPC_Pickup(netObj);
            }

            return;
        }
    }


    public void Drop()
    {
        if (HeldItem == null) return;

        // Spróbuj najpierw położyć na stół w zasięgu
        Collider[] colliders = Physics.OverlapSphere(transform.position + _forward, 1.5f);
        foreach (var col in colliders)
        {
            if (col.TryGetComponent<Table>(out var table))
            {
                RPC_PlaceOnTable(table.Object, HeldItem);
                return;
            }
        }

        // Jeśli brak stołu → upuść w przód
        Vector3 dropPos = transform.position + _forward * 1f + Vector3.up * 0.3f;
        RPC_DropItem(HeldItem, dropPos, Quaternion.identity);
    }


    // ---------------- RPCs ----------------

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

        if (TryGetComponent(out Collider playerCol) && item.TryGetComponent(out Collider itemCol))
        {
            Physics.IgnoreCollision(playerCol, itemCol, true);
        }

        Debug.Log($"[Player] Podniesiono {item.name}");
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_DropItem(NetworkObject item, Vector3 pos, Quaternion rot)
    {
        if (item == null) return;

        if (item.TryGetComponent<Rigidbody>(out var rb))
            rb.isKinematic = false;

        item.transform.position = pos;
        item.transform.rotation = rot;

        if (TryGetComponent(out Collider playerCol) && item.TryGetComponent(out Collider itemCol))
        {
            Physics.IgnoreCollision(playerCol, itemCol, false);
        }

        if (HeldItem == item)
            HeldItem = null;

        Debug.Log($"[Player] Upuszczono {item.name}");
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    private void RPC_PlaceOnTable(NetworkObject tableObj, NetworkObject item)
    {
        if (item == null) return;

        if (tableObj != null)
        {
            var table = tableObj.GetComponent<Table>();
            if (table != null && table.HeldItem == null)
            {
                table.ReceiveItem(item);
                HeldItem = null;
                Debug.Log($"[Player] Odłożono {item.name} na stół {table.name}");
                return;
            }
        }

        // fallback: zwykły drop w świecie
        Vector3 dropPos = transform.position + _forward * 1f + Vector3.up * 0.3f;
        RPC_DropItem(item, dropPos, Quaternion.identity);
    }


    // ---------------- Nalewanie RPC ----------------

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_PourLiquidToStation(NetworkObject glassObj, NetworkObject liquidObj)
    {
        var glass = glassObj.GetComponent<KitchenItem>();
        var liquid = liquidObj.GetComponent<KitchenItem>();
        if (glass == null || liquid == null) return;

        Debug.Log($"[Player] Nalewanie: glass={glass.Variant}, liquid={liquid.Variant}");

        switch (liquid.Variant)
        {
            case ItemVariant.Vodka: glass.Variant = ItemVariant.GlassWithVodka; break;
            case ItemVariant.Plazma: glass.Variant = ItemVariant.GlassWithPlazma; break;
            case ItemVariant.Poison: glass.Variant = ItemVariant.GlassWithPoison; break;
            case ItemVariant.Blood: glass.Variant = ItemVariant.GlassWithBlood; break;
            case ItemVariant.Magma: glass.Variant = ItemVariant.GlassWithMagma; break;
            default:
                Debug.LogWarning($"[Player] Nieznany płyn: {liquid.Variant}");
                return;
        }

        Runner.Despawn(liquidObj);

        var table = glassObj.GetComponentInParent<PouringStation>();
        table?.ReceiveItem(glassObj);

        Debug.Log($"[Player] Nalewanie zakończone: glass={glass.Variant}");
    }
}
