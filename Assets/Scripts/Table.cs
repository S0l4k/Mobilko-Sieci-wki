using Fusion;
using UnityEngine;

public class Table : NetworkBehaviour, IInteractable
{
    [Networked] public NetworkObject HeldItem { get; private set; }
    public bool CanBePickedUp => false;

    [SerializeField] private Transform placePoint;

    public virtual void Interact(Player player)
    {
        if (HeldItem != null && player.HeldItem == null)
        {
            player.RPC_Pickup(HeldItem);
            HeldItem = null;
        }
    }

    public void ReceiveItem(NetworkObject item)
    {
        if (item == null || HeldItem != null)
            return;

        HeldItem = item;

        item.transform.position = placePoint.position;
        item.transform.rotation = placePoint.rotation;

        if (item.TryGetComponent<Rigidbody>(out var rb))
            rb.isKinematic = true;
    }

    public KitchenItem GetKitchenItem()
    {
        if (HeldItem == null) return null;
        return HeldItem.GetComponent<KitchenItem>();
    }
}
