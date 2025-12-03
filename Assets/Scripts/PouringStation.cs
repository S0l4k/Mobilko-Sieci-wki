using Fusion;
using UnityEngine;

public class PouringStation : Table
{
    public override void Interact(Player player)
    {
        var heldKi = player.HeldItem?.GetComponent<KitchenItem>();
        var tableKi = GetKitchenItem();

        if (heldKi != null && heldKi.IsLiquid() && tableKi != null && tableKi.Variant == ItemVariant.EmptyGlass)
        {
            player.RPC_PourLiquidToStation(tableKi.Object, heldKi.Object);
            return;
        }

        if (heldKi == null && tableKi != null && tableKi.IsGlass())
        {
            player.RPC_Pickup(tableKi.Object);
            RemoveHeldItem();
            return;
        }

        if (heldKi != null && heldKi.Variant == ItemVariant.EmptyGlass && tableKi == null)
        {
            player.RPC_PlaceOnTable(Object, heldKi.Object);
            return;
        }

        Debug.Log("[PouringStation] Brak możliwości interakcji");
    }
}
