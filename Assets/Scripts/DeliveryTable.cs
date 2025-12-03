using Fusion;
using UnityEngine;
using TMPro;

public class DeliveryTable : Table
{
    [SerializeField] private TextMeshProUGUI uiText;

    [Networked] private int requiredDrink { get; set; }
    [Networked] private int playerScore { get; set; }

    public override void Spawned()
    {
        if (Object.HasStateAuthority)
        {
            SetNewRequiredDrink();
            RPC_UpdateUI(requiredDrink, playerScore); // 🔹 aktualizujemy UI od razu
        }
    }

    public override void Interact(Player player)
    {
        var heldKi = player.HeldItem?.GetComponent<KitchenItem>();
        if (heldKi == null) return;

        if (!IsDrink(heldKi.Variant))
        {
            Debug.Log("[DeliveryTable] Ten stół przyjmuje tylko drinki!");
            return;
        }

        int drinkNumber = GetDrinkNumber(heldKi.Variant);

        if (drinkNumber == requiredDrink)
        {
            playerScore += 10;
            Debug.Log($"[DeliveryTable] Poprawny drink! +10 punktów. Suma: {playerScore}");
        }
        else
        {
            playerScore -= 10;
            Debug.Log($"[DeliveryTable] Zły drink! -10 punktów. Suma: {playerScore}");
        }

        // 🔹 Despawnujemy drink po oddaniu
        if (Object.HasStateAuthority)
        {
            Runner.Despawn(player.HeldItem);
        }

        player.ClearHeldItem();

        // Serwer ustawia nowy wymagany drink i informuje wszystkich
        if (Object.HasStateAuthority)
        {
            SetNewRequiredDrink();
            RPC_UpdateUI(requiredDrink, playerScore);
        }
    }

    private bool IsDrink(ItemVariant variant)
    {
        return variant == ItemVariant.Drink1 ||
               variant == ItemVariant.Drink2 ||
               variant == ItemVariant.Drink3 ||
               variant == ItemVariant.Drink4 ||
               variant == ItemVariant.Drink5;
    }

    private int GetDrinkNumber(ItemVariant variant)
    {
        switch (variant)
        {
            case ItemVariant.Drink1: return 1;
            case ItemVariant.Drink2: return 2;
            case ItemVariant.Drink3: return 3;
            case ItemVariant.Drink4: return 4;
            case ItemVariant.Drink5: return 5;
        }
        return 0;
    }

    private void SetNewRequiredDrink()
    {
        requiredDrink = Random.Range(1, 6);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_UpdateUI(int drink, int score)
    {
        if (uiText != null)
        {
            uiText.text = $"Wymagany drink: {drink}\nPunkty: {score}";
        }
    }
}
