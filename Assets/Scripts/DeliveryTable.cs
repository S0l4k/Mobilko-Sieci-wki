using Fusion;
using UnityEngine;
using TMPro;

public class DeliveryTable : Table
{
    [Header("UI")]
    [SerializeField] private TextMeshProUGUI uiText;

    // Fusion 1.x — brak OnChanged
    [Networked] private int requiredDrink { get; set; }
    [Networked] private int playerScore { get; set; }

    public override void Spawned()
    {
        // Serwer ustawia wymagany drink
        if (Object.HasStateAuthority)
        {
            SetNewRequiredDrink();
        }

        // Klienci sami aktualizują UI z networked zmiennych
        UpdateUI();
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

        if (Object.HasStateAuthority)
        {
            if (drinkNumber == requiredDrink)
            {
                playerScore += 10;
            }
            else
            {
                playerScore -= 10;
            }

            // usuń drink
            Runner.Despawn(player.HeldItem);

            // gracz już nic nie trzyma
            player.ClearHeldItem();

            // nowe zlecenie
            SetNewRequiredDrink();

            // aktualizacja UI u wszystkich klientów
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

    // 🔹 aktualizacja UI u wszystkich
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_UpdateUI(int drink, int score)
    {
        if (uiText != null)
        {
            uiText.text = $"Wymagany drink: {drink}\nPunkty: {score}";
        }
    }

    // 🔹 Klienci odświeżają UI na podstawie networked pól
    public override void FixedUpdateNetwork()
    {
        if (!Object.HasStateAuthority)
        {
            UpdateUI();
        }
    }

    private void UpdateUI()
    {
        if (uiText != null)
        {
            uiText.text = $"Wymagany drink: {requiredDrink}\nPunkty: {playerScore}";
        }
    }
}
