using IPSDataAcquisition.Domain.Common;

namespace IPSDataAcquisition.Domain.Entities;

public class ButtonPress : BaseEntity
{
    public string SessionId { get; set; } = string.Empty;
    public string? UserId { get; set; }
    public string Action { get; set; } = string.Empty;
    public long Timestamp { get; set; }
    public bool IsSynced { get; set; } = true;

    // Navigation property
    public virtual Session? Session { get; set; }
}

public static class ButtonAction
{
    public const string EnteredRestaurantBuilding = "ENTERED_RESTAURANT_BUILDING";
    public const string EnteredElevator = "ENTERED_ELEVATOR";
    public const string ClimbingStairs3Floors = "CLIMBING_STAIRS_3_FLOORS";
    public const string GoingUp8FloorsInLift = "GOING_UP_8_FLOORS_IN_LIFT";
    public const string ReachedRestaurantCorridor = "REACHED_RESTAURANT_CORRIDOR";
    public const string ReachedRestaurant = "REACHED_RESTAURANT";
    public const string LeftRestaurant = "LEFT_RESTAURANT";
    public const string ComingDown3Floors = "COMING_DOWN_3_FLOORS";
    public const string LeftRestaurantBuilding = "LEFT_RESTAURANT_BUILDING";
    public const string EnteredDeliveryBuilding = "ENTERED_DELIVERY_BUILDING";
    public const string ReachedDeliveryCorridor = "REACHED_DELIVERY_CORRIDOR";
    public const string ReachedDoorstep = "REACHED_DOORSTEP";
    public const string LeftDoorstep = "LEFT_DOORSTEP";
    public const string GoingDown8FloorsInLift = "GOING_DOWN_8_FLOORS_IN_LIFT";
    public const string LeftDeliveryBuilding = "LEFT_DELIVERY_BUILDING";

    public static readonly string[] ValidActions = new[]
    {
        EnteredRestaurantBuilding, EnteredElevator, ClimbingStairs3Floors,
        GoingUp8FloorsInLift, ReachedRestaurantCorridor, ReachedRestaurant,
        LeftRestaurant, ComingDown3Floors, LeftRestaurantBuilding,
        EnteredDeliveryBuilding, ReachedDeliveryCorridor, ReachedDoorstep,
        LeftDoorstep, GoingDown8FloorsInLift, LeftDeliveryBuilding
    };
}

