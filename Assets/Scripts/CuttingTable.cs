using Fusion;
using UnityEngine;

public class CuttingTable : Table
{
    [SerializeField] private int clicksToCut = 3;
    private int _currentClicks = 0;

    public override void Interact(Player player)
    {
        var heldKi = player.HeldItem?.GetComponent<KitchenItem>();
        var tableKi = GetKitchenItem();

        if (tableKi != null && CanBeCut(tableKi.Variant))
        {
            _currentClicks++;
            Debug.Log($"[CuttingTable] Kliknięto {tableKi.Variant}. Kliknięć: {_currentClicks}/{clicksToCut}");

            if (_currentClicks >= clicksToCut)
            {
                tableKi.Variant = GetSlicedVariant(tableKi.Variant);
                _currentClicks = 0;
                Debug.Log($"[CuttingTable] {tableKi.Variant} zostało pocięte!");
            }

            if (heldKi == null && IsSliced(tableKi.Variant))
            {
                player.RPC_Pickup(tableKi.Object);
                RemoveHeldItem();
                Debug.Log("[CuttingTable] Podniesiono pocięty przedmiot");
            }
            return;
        }

        if (heldKi != null)
        {
            if (!CanBeCut(heldKi.Variant))
            {
                Debug.Log($"[CuttingTable] Nie można kroić tego przedmiotu: {heldKi.Variant}");
                return;
            }

            if (tableKi == null)
            {
                player.RPC_PlaceOnTable(Object, heldKi.Object);
                _currentClicks = 0;
                Debug.Log($"[CuttingTable] Położono {heldKi.Variant} na stole do krojenia");
                return;
            }

            Debug.Log("[CuttingTable] Stół zajęty czymś nie do krojenia");
            return;
        }

        Debug.Log("[CuttingTable] Stół pusty lub przedmiot nie do krojenia");
    }

    private bool CanBeCut(ItemVariant variant)
    {
        return variant == ItemVariant.Onion ||
               variant == ItemVariant.Brain ||
               variant == ItemVariant.Tojad ||
               variant == ItemVariant.Pumpkin ||
               variant == ItemVariant.Mandragora;
    }

    private ItemVariant GetSlicedVariant(ItemVariant variant)
    {
        return variant switch
        {
            ItemVariant.Onion => ItemVariant.SlicedOnion,
            ItemVariant.Brain => ItemVariant.SlicedBrain,
            ItemVariant.Tojad => ItemVariant.SlicedTojad,
            ItemVariant.Pumpkin => ItemVariant.SlicedPumpkin,
            ItemVariant.Mandragora => ItemVariant.SlicedMandragora,
            _ => variant
        };
    }

    private bool IsSliced(ItemVariant variant)
    {
        return variant == ItemVariant.SlicedOnion ||
               variant == ItemVariant.SlicedBrain ||
               variant == ItemVariant.SlicedTojad ||
               variant == ItemVariant.SlicedPumpkin ||
               variant == ItemVariant.SlicedMandragora;
    }
}
