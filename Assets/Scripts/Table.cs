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
            if (Object.HasStateAuthority)
                HeldItem = null;
        }
    }

    public void RemoveHeldItem()
    {
        if (Object.HasStateAuthority)
            HeldItem = null;
    }

    public void ReceiveItem(NetworkObject item)
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
