namespace Infection.Interaction
{
    public interface IPickup
    {
        string ItemName { get; }
        void GrantPickup(PickupBehavior pickupBehavior);
    }
}
