using Fusion;
using UnityEngine;

public class Table : NetworkBehaviour, IInteractable
{
    [Networked] public NetworkObject HeldItem { get; set; }
    public bool CanBePickedUp => false;
    [SerializeField] private Transform placePoint;

    public void Interact(Player player)
    {
        // Wymagane przez interfejs, ale nie używamy
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_SetHeldItem(NetworkObject playerObj, NetworkObject item)
    {
        if (HeldItem != null || item == null || playerObj == null)
            return;

        Player player = playerObj.GetComponent<Player>();
        if (player == null) return;

        HeldItem = item;
        HeldItem.transform.position = placePoint.position;
        HeldItem.transform.rotation = placePoint.rotation;

        player.ClearHeldItem();
    }
}
