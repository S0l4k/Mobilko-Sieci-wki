using Fusion;
using UnityEngine;

public class Table : NetworkBehaviour, IInteractable
{
    [Networked] public NetworkObject HeldItem { get; private set; }
    public bool CanBePickedUp => false;

    [SerializeField] private Transform placePoint;

    public void Interact(Player player)
    {
        if (HeldItem != null && player.HeldItem == null)
        {
            player.RPC_Pickup(HeldItem);
            HeldItem = null;
        }
    }

    public void ReceiveItem(NetworkObject item)
    {
        if (HeldItem != null || item == null) return;

        HeldItem = item;
        item.transform.position = placePoint.position + Vector3.up * 0.05f;
        item.transform.rotation = placePoint.rotation;

        Rigidbody rb = item.GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = true;
    }
}
