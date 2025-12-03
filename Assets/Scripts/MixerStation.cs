using Fusion;
using System.Collections.Generic;
using UnityEngine;

public class MixerTable : Table
{
    [Networked] private string ingredientString { get; set; }

    private List<int> currentIngredients = new List<int>();

    public override void Interact(Player player)
    {
        var heldKi = player.HeldItem?.GetComponent<KitchenItem>();
        var tableKi = GetKitchenItem();

        if (heldKi == null)
        {
            if (tableKi != null && IsGlass(tableKi.Variant))
            {
                player.RPC_Pickup(tableKi.Object);
                RemoveHeldItem();
                currentIngredients.Clear();
                ingredientString = "";
                Debug.Log("[MixerTable] Podniesiono szklankę/drink");
            }
            return;
        }

        if (IsGlass(heldKi.Variant) && tableKi == null)
        {
            player.RPC_PlaceOnTable(Object, heldKi.Object);
            Debug.Log("[MixerTable] Położono szklankę na stole");
            return;
        }

        if (tableKi != null && IsGlass(tableKi.Variant) && IsValidIngredient(heldKi.Variant))
        {
            int ingredientId = (int)heldKi.Variant;

            if (!currentIngredients.Contains(ingredientId))
            {
                currentIngredients.Add(ingredientId);
                UpdateIngredientString();
                Debug.Log($"[MixerTable] Dodano składnik: {heldKi.Variant}");

                Runner.Despawn(heldKi.Object);
                player.ClearHeldItem();
            }

            bool drinkCreated = TryMakeDrink(tableKi);

            if (drinkCreated)
            {
                player.RPC_Pickup(tableKi.Object);
                RemoveHeldItem();
                Debug.Log("[MixerTable] Drink gotowy i podniesiony");
            }
            return;
        }

        Debug.Log("[MixerTable] Brak możliwości interakcji");
    }

    private void UpdateIngredientString()
    {
        ingredientString = string.Join(",", currentIngredients);
    }

    private bool IsGlass(ItemVariant variant)
    {
        return variant == ItemVariant.EmptyGlass ||
               variant == ItemVariant.GlassWithVodka ||
               variant == ItemVariant.GlassWithPlazma ||
               variant == ItemVariant.GlassWithPoison ||
               variant == ItemVariant.GlassWithBlood ||
               variant == ItemVariant.GlassWithMagma ||
               variant == ItemVariant.Drink1 ||
               variant == ItemVariant.Drink2 ||
               variant == ItemVariant.Drink3 ||
               variant == ItemVariant.Drink4 ||
               variant == ItemVariant.Drink5;
    }

    private bool IsValidIngredient(ItemVariant variant)
    {
        return variant == ItemVariant.Eyes ||
               variant == ItemVariant.Fingers ||
               variant == ItemVariant.Worms ||
               variant == ItemVariant.Bone ||
               variant == ItemVariant.Dandruff ||
               variant == ItemVariant.SlicedOnion ||
               variant == ItemVariant.SlicedBrain ||
               variant == ItemVariant.SlicedTojad ||
               variant == ItemVariant.SlicedPumpkin ||
               variant == ItemVariant.SlicedMandragora;
    }

    private bool TryMakeDrink(KitchenItem glass)
    {
        List<ItemVariant> ingredients = new List<ItemVariant>();
        foreach (int i in currentIngredients)
            ingredients.Add((ItemVariant)i);

        ItemVariant result = ItemVariant.None;

        switch (glass.Variant)
        {
            case ItemVariant.GlassWithBlood:
                if (ingredients.Contains(ItemVariant.SlicedTojad) &&
                    ingredients.Contains(ItemVariant.Fingers))
                    result = ItemVariant.Drink1;
                break;

            case ItemVariant.GlassWithPoison:
                if (ingredients.Contains(ItemVariant.SlicedBrain) &&
                    ingredients.Contains(ItemVariant.Dandruff))
                    result = ItemVariant.Drink2;
                break;

            case ItemVariant.GlassWithVodka:
                if (ingredients.Contains(ItemVariant.SlicedOnion) &&
                    ingredients.Contains(ItemVariant.Eyes))
                    result = ItemVariant.Drink3;
                break;

            case ItemVariant.GlassWithMagma:
                if (ingredients.Contains(ItemVariant.SlicedPumpkin) &&
                    ingredients.Contains(ItemVariant.Bone))
                    result = ItemVariant.Drink4;
                break;

            case ItemVariant.GlassWithPlazma:
                if (ingredients.Contains(ItemVariant.SlicedMandragora) &&
                    ingredients.Contains(ItemVariant.Worms))
                    result = ItemVariant.Drink5;
                break;
        }

        if (result != ItemVariant.None)
        {
            glass.Variant = result;
            currentIngredients.Clear();
            ingredientString = "";
            Debug.Log($"[MixerTable] Stworzono drink: {result}");
            return true;
        }

        return false;
    }

    public override void ReceiveItem(NetworkObject item)
    {
        base.ReceiveItem(item);
        currentIngredients.Clear();
        ingredientString = "";
    }
}
