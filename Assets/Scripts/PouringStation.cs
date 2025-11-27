using Fusion;
using UnityEngine;

public class PouringStation : Table
{
    [SerializeField] private ItemVariant liquidVariant;

    public override void Interact(Player player)
    {
        // Jeśli gracz ma pustą rękę → NIE pozwalamy zabrać glassa (bo Table normalnie to robi)
        if (player.HeldItem == null)
        {
            Debug.Log("PouringStation: gracz nie trzyma płynu");
            return;
        }

        // Gracz trzyma płyn
        if (!player.HeldItem.TryGetComponent<KitchenItem>(out var heldKi))
            return;

        // Na stole musi stać glass
        var glassKi = GetKitchenItem();
        if (glassKi == null)
        {
            Debug.Log("PouringStation: na stole nie ma glass");
            return;
        }

        // Glass musi być pusty
        if (glassKi.Variant != ItemVariant.EmptyGlass)
        {
            Debug.Log("PouringStation: glass nie jest pusty");
            return;
        }

        Debug.Log("Nalewanie!");

        RPC_PourLiquid(glassKi.Object, heldKi.Object);
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    private void RPC_PourLiquid(NetworkObject glassObj, NetworkObject liquidObj)
    {
        var glass = glassObj.GetComponent<KitchenItem>();
        var liquid = liquidObj.GetComponent<KitchenItem>();

        if (glass == null || liquid == null) return;

        glass.Variant = liquid.Variant;

        Runner.Despawn(liquidObj);
    }
}
