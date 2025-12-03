using Fusion;
using UnityEngine;

public class KitchenItem : NetworkBehaviour, IInteractable
{
    [Networked] public int VariantInt { get; set; }
    [SerializeField] private ItemVariant initialVariant = ItemVariant.None;


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


   

    [Header("Cutting Models")]
    [SerializeField] private GameObject onionModel;
    [SerializeField] private GameObject slicedOnionModel;
    [SerializeField] private GameObject brainModel;
    [SerializeField] private GameObject slicedBrainModel;
    [SerializeField] private GameObject tojadModel;
    [SerializeField] private GameObject slicedTojadModel;
    [SerializeField] private GameObject pumpkinModel;
    [SerializeField] private GameObject slicedPumpkinModel;
    [SerializeField] private GameObject mandragoraModel;
    [SerializeField] private GameObject slicedMandragoraModel;

    public override void Spawned()
    {
        base.Spawned();

        if (Object.HasStateAuthority)
        {
            Variant = initialVariant;
        }
    }
    private void UpdateModel()
    {
        // Wy³¹cz wszystkie istniej¹ce modele
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

        onionModel.SetActive(false);
        slicedOnionModel.SetActive(false);
        brainModel.SetActive(false);
        slicedBrainModel.SetActive(false);
        tojadModel.SetActive(false);
        slicedTojadModel.SetActive(false);
        pumpkinModel.SetActive(false);
        slicedPumpkinModel.SetActive(false);
        mandragoraModel.SetActive(false);
        slicedMandragoraModel.SetActive(false);

        // W³¹cz odpowiedni model wg Variant
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

            // Cutting items
            case ItemVariant.Onion: onionModel.SetActive(true); break;
            case ItemVariant.SlicedOnion: slicedOnionModel.SetActive(true); break;
            case ItemVariant.Brain: brainModel.SetActive(true); break;
            case ItemVariant.SlicedBrain: slicedBrainModel.SetActive(true); break;
            case ItemVariant.Tojad: tojadModel.SetActive(true); break;
            case ItemVariant.SlicedTojad: slicedTojadModel.SetActive(true); break;
            case ItemVariant.Pumpkin: pumpkinModel.SetActive(true); break;
            case ItemVariant.SlicedPumpkin: slicedPumpkinModel.SetActive(true); break;
            case ItemVariant.Mandragora: mandragoraModel.SetActive(true); break;
            case ItemVariant.SlicedMandragora: slicedMandragoraModel.SetActive(true); break;
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
