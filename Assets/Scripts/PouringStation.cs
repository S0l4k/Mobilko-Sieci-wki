using Fusion;
using UnityEngine;

public class PouringStation : Table
{
    public override void Interact(Player player)
    {
        var heldKi = player.HeldItem?.GetComponent<KitchenItem>();
        var tableKi = GetKitchenItem();

        Debug.Log($"[PouringStation] Interact. Player held: {(heldKi != null ? heldKi.Variant.ToString() : "null")}, Table: {(tableKi != null ? tableKi.Variant.ToString() : "null")}");

        if (heldKi == null)
        {
            if (tableKi != null)
            {
                Debug.Log("[PouringStation] Podnoszenie glassa");
                player.RPC_Pickup(tableKi.Object);
                RemoveHeldItem();
            }
            return;
        }

        if (!heldKi.IsLiquid())
        {
            Debug.Log($"[PouringStation] Gracz nie trzyma płynu: {heldKi.Variant}");
            return;
        }

        if (tableKi == null)
        {
            Debug.Log("[PouringStation] Brak glassa na stole");
            return;
        }

        if (tableKi.Variant != ItemVariant.EmptyGlass)
        {
            Debug.Log($"[PouringStation] Glass nie jest pusty: {tableKi.Variant}");
            return;
        }

        Debug.Log($"[PouringStation] Nalewanie {heldKi.Variant} do glassa");
        player.RPC_PourLiquidToStation(tableKi.Object, heldKi.Object);
    }
}
