namespace CoffeeShop
{
    /// <summary>
    /// Represents a station in the coffee shop where a piece of equipment can be placed
    /// and where the player can interact. Stations are fixed points in the shop layout.
    /// </summary>
    public class Station
    {
        /// <summary>
        /// The unique identifier for this station. This is assigned at creation and does not change.
        /// </summary>
        private readonly string _stationID;
        /// <summary>
        /// The 2D coordinates representing the position of this station within the shop.
        /// This is assigned at creation and does not change.
        /// </summary>
        private readonly Vector2D _position;
        /// <summary>
        /// The piece of equipment currently assigned to this station. It can be null if no equipment is present.
        /// </summary>
        private Equipment? _assignedEquipment;
        // private bool _isOccupiedByPlayer; // This field was commented out in the provided code.

        /// <summary>
        /// Gets the unique identifier for this station.
        /// </summary>
        public string StationID => _stationID;

        /// <summary>
        /// Gets the 2D position (coordinates) of this station within the shop layout.
        /// </summary>
        public Vector2D Position => _position;

        /// <summary>
        /// Gets the equipment currently assigned to this station.
        /// Returns <c>null</c> if no equipment is currently placed at this station.
        /// </summary>
        public Equipment? AssignedEquipment => _assignedEquipment;

        /// <summary>
        /// Initializes a new instance of the <see cref="Station"/> class.
        /// A station is defined by its unique ID and its position in the shop.
        /// </summary>
        /// <param name="id">The unique identifier for the station. This value cannot be null or consist only of white-space characters.</param>
        /// <param name="position">The <see cref="Vector2D"/> representing the position of the station. This value cannot be null.</param>
        /// <exception cref="System.ArgumentException">Thrown if the provided <paramref name="id"/> is null or whitespace.</exception>
        /// <exception cref="System.ArgumentNullException">Thrown if the provided <paramref name="position"/> is null.</exception>
        public Station(string id, Vector2D position)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new System.ArgumentException("Station ID cannot be null or whitespace.", nameof(id));
            }
            if (position == null)
            {
                throw new System.ArgumentNullException(nameof(position), "Position cannot be null.");
            }

            _stationID = id;
            _position = position;
            _assignedEquipment = null; 
            System.Console.WriteLine($"Station '{_stationID}' created at ({_position.X}, {_position.Y}).");
        }

        /// <summary>
        /// Assigns a piece of <see cref="Equipment"/> to this station.
        /// If <c>null</c> is passed, any existing equipment is removed from the station.
        /// </summary>
        /// <param name="equipment">The equipment to assign to this station, or <c>null</c> to clear the station's equipment.</param>
        public void AssignEquipment(Equipment? equipment) 
        {
            _assignedEquipment = equipment;
            if (equipment != null)
            {
                System.Console.WriteLine($"Equipment '{equipment.EquipmentName}' assigned to station '{StationID}'.");
            }
            else
            {
                System.Console.WriteLine($"Equipment removed from station '{StationID}'.");
            }
        }
    }
}
