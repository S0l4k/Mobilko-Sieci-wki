using Fusion;
using UnityEngine;

public class KitchenItem : NetworkBehaviour, IInteractable
{
    [Networked] public int VariantInt { get; set; }

    public ItemVariant Variant
    {
        get => (ItemVariant)VariantInt;
        set
        {
            VariantInt = (int)value;
            UpdateModel(); // zmiana modelu od razu
        }
    }

    [Header("Models")]
    [SerializeField] private GameObject emptyGlassModel;
    [SerializeField] private GameObject glassWithVodkaModel;
    [SerializeField] private GameObject glassWithPlazmaModel;
    [SerializeField] private GameObject glassWithPoisonModel;
    [SerializeField] private GameObject glassWithBloodModel;
    [SerializeField] private GameObject glassWithMagmaModel;
    [SerializeField] private GameObject drink1;
    [SerializeField] private GameObject drink2;
    [SerializeField] private GameObject drink3;
    [SerializeField] private GameObject drink4;
    [SerializeField] private GameObject drink5;

    private void UpdateModel()
    {
        // Wy³¹cz wszystkie
        emptyGlassModel.SetActive(false);
        glassWithVodkaModel.SetActive(false);
        glassWithPlazmaModel.SetActive(false);
        glassWithPoisonModel.SetActive(false);
        glassWithBloodModel.SetActive(false);
        glassWithMagmaModel.SetActive(false);
        drink1.SetActive(false);
        drink2.SetActive(false);
        drink3.SetActive(false);
        drink4.SetActive(false);
        drink5.SetActive(false);

        // W³¹cz tylko odpowiedni
        switch (Variant)
        {
            case ItemVariant.EmptyGlass: emptyGlassModel.SetActive(true); break;
            case ItemVariant.GlassWithVodka: glassWithVodkaModel.SetActive(true); break;
            case ItemVariant.GlassWithPlazma: glassWithPlazmaModel.SetActive(true); break;
            case ItemVariant.GlassWithPoison: glassWithPoisonModel.SetActive(true); break;
            case ItemVariant.GlassWithBlood: glassWithBloodModel.SetActive(true); break;
            case ItemVariant.GlassWithMagma: glassWithMagmaModel.SetActive(true); break;
            case ItemVariant.Drink1: drink1.SetActive(true); break;
            case ItemVariant.Drink2: drink2.SetActive(true); break;
            case ItemVariant.Drink3: drink3.SetActive(true); break;
            case ItemVariant.Drink4: drink4.SetActive(true); break;
            case ItemVariant.Drink5: drink5.SetActive(true); break;
        }
    }

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
