using Fusion;
using UnityEngine;

public class KitchenItem : NetworkBehaviour, IInteractable
{
    [Networked] public int VariantInt { get; set; }

    public ItemVariant Variant
    {
        get => (ItemVariant)VariantInt;
        set => VariantInt = (int)value;
    }

    public bool Is(ItemVariant v) => Variant == v;

    public bool IsLiquid()
    {
        return Variant == ItemVariant.Vodka ||
               Variant == ItemVariant.Plazma ||
               Variant == ItemVariant.Poison ||
               Variant == ItemVariant.Blood ||
               Variant == ItemVariant.Magma;
    }

    public bool CanBePickedUp => true;

    public void Interact(Player player)
    {
        // Pickup logic handled by Player
    }
}
