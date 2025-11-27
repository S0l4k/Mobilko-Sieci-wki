using Fusion;
using UnityEngine;

public enum ItemVariant
{
    None = 0,
    EmptyGlass = 1,
    Vodka = 26,
    Plazma = 27,
    Poison = 28,
    Blood = 29,
    Magma = 30,

    GlassWithVodka = 2,
    GlassWithPlazma = 3,
    GlassWithPoison = 4,
    GlassWithBlood = 5,
    GlassWithMagma = 6,

    Onion = 7,
    SlicedOnion = 8,

    Brain = 9,
    SlicedBrain = 10,

    Tojad = 11,
    SlicedTojad = 12,

    Pumpkin = 13,
    SlicedPumpkin = 14,

    Mandragora = 15,
    SlicedMandragora = 16,

    Eyes = 17,
    Fingers = 18,
    Worms = 19,
    Bone = 20,
    Dandruff = 21,

    Drink1 = 22,
    Drink2 = 23,
    Drink3 = 24,
    Drink4 = 25,
    Drink5 = 26
}

public class KitchenItem : NetworkBehaviour, IInteractable
{
    [Networked] public int VariantInt { get; set; }

    public ItemVariant Variant
    {
        get => (ItemVariant)VariantInt;
        set => VariantInt = (int)value;
    }

    public bool Is(ItemVariant v) => Variant == v;

    // ---- IInteractable ----
    public bool CanBePickedUp => true;

    public void Interact(Player player)
    {
        // pickup logic handled by Player, so nothing needed here
    }
}
