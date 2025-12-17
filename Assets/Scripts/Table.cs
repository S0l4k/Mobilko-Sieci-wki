using Fusion;
using UnityEngine;

public class Table : NetworkBehaviour, IInteractable
{
    [Networked] public NetworkObject HeldItem { get; private set; }
    public bool CanBePickedUp => false;

    [SerializeField] protected Transform placePoint;

    public virtual void Interact(Player player)
    {
        if (!Runner.IsServer) return;

        var heldKi = player.HeldItem?.GetComponent<KitchenItem>();
        var tableKi = GetKitchenItem();

        if (tableKi != null && heldKi == null)
        {
            player.RPC_Pickup(tableKi.Object);
            RemoveHeldItem();
            return;
        }

        if (tableKi == null && heldKi != null)
        {
            player.RPC_PlaceOnTable(Object, heldKi.Object);
            return;
        }
    }

    public void RemoveHeldItem()
    {
        if (Object.HasStateAuthority)
            HeldItem = null;
    }

    public virtual void ReceiveItem(NetworkObject item)
    {
        if (item == null || HeldItem != null) return;

        HeldItem = item;

        if (placePoint != null)
        {
            item.transform.position = placePoint.position;
            item.transform.rotation = placePoint.rotation;
        }

        if (item.TryGetComponent<Rigidbody>(out var rb))
            rb.isKinematic = true;

        Debug.Log($"[Table] ReceiveItem - {item.name} na {gameObject.name}");
    }

    public KitchenItem GetKitchenItem()
    {
        if (HeldItem == null) return null;
        return HeldItem.GetComponent<KitchenItem>();
    }
}
