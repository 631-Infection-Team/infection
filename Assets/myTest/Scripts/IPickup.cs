namespace myTest
{
    public interface IPickup
    {
        string ItemName { get; }
        void GrantPickup(PickupBehavior pickupBehavior);
    }
}
