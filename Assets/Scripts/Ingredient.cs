using Fusion;
using UnityEngine;

public class Ingredient : NetworkBehaviour, IInteractable
{
    public bool CanBePickedUp => true;
    public void Interact(Player player)
    {
        Debug.Log($"{player.name} picked up {name}");
        
    }
}
