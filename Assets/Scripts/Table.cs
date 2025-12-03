using Fusion;
using UnityEngine;

public class Table : NetworkBehaviour, IInteractable
{
    [Networked] public NetworkObject HeldItem { get; private set; }
    public bool CanBePickedUp => false;

    [SerializeField] private Transform placePoint;

    public virtual void Interact(Player player)
    {
        var heldKi = player.HeldItem?.GetComponent<KitchenItem>();
        var tableKi = GetKitchenItem();

        // jeśli stół ma przedmiot i gracz nic nie trzyma → podnieś
        if (HeldItem != null && heldKi == null)
        {
            player.RPC_Pickup(HeldItem);
            if (Object.HasStateAuthority)
                HeldItem = null;
        }
        // jeśli stół pusty i gracz trzyma coś → połóż
        else if (HeldItem == null && heldKi != null)
        {
            player.RPC_PlaceOnTable(Object, player.HeldItem);
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
