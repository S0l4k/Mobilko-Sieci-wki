public interface IInteractable
{
    bool CanBePickedUp { get; }
    void Interact(Player player);
    
}
