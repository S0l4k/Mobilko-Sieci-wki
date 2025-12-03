using Fusion;
using UnityEngine;

public class PouringStation : Table
{
    public override void Interact(Player player)
    {
        var heldKi = player.HeldItem?.GetComponent<KitchenItem>();
        var tableKi = GetKitchenItem();

        // 1️⃣ Gracz trzyma płyn i stół ma EmptyGlass → nalewanie
        if (heldKi != null && heldKi.IsLiquid() && tableKi != null && tableKi.Variant == ItemVariant.EmptyGlass)
        {
            player.RPC_PourLiquidToStation(tableKi.Object, heldKi.Object);
            return;
        }

        // 2️⃣ Gracz nic nie trzyma → podnieś glass ze stołu
        if (heldKi == null && tableKi != null && tableKi.IsGlass())
        {
            player.RPC_Pickup(tableKi.Object);
            RemoveHeldItem();
            return;
        }

        // 3️⃣ Gracz trzyma EmptyGlass → połóż na stole
        if (heldKi != null && heldKi.Variant == ItemVariant.EmptyGlass && tableKi == null)
        {
            player.RPC_PlaceOnTable(Object, heldKi.Object);
            return;
        }

        // Wszystko inne → nic nie rób
        Debug.Log("[PouringStation] Brak możliwości interakcji");


        Debug.Log($"[PouringStation] Nalewanie {heldKi.Variant} do glassa");
        player.RPC_PourLiquidToStation(tableKi.Object, heldKi.Object);
    }

}
