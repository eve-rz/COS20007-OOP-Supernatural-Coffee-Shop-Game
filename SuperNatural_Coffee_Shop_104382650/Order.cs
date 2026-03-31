using System; 

namespace CoffeeShop
{
    /// <summary>
    /// Represents a customer's request within the coffee shop.
    /// An order can be for a specific <see cref="Recipe"/> or based on an <see cref="AlienPropertyDemand"/>
    /// for special customer types. It tracks the lifecycle of the order via its status.
    /// </summary>
    public class Order
    {
        /// <summary>
        /// The recipe that has been assigned to this order.
        /// This will be <c>null</c> if the order is based on an <see cref="AlienPropertyDemand"/> or if no recipe is specified.
        /// This field is read-only after the order is created.
        /// </summary>
        private readonly Recipe? _assignedRecipe;
        /// <summary>
        /// The customer who initiated this order. This field is read-only after the order is created.
        /// </summary>
        private readonly Customer _requestingCustomer;
        /// <summary>
        /// The current processing status of this order (e.g., Pending, Served, Failed).
        /// </summary>
        private OrderStatus _currentStatus;

        /// <summary>
        /// The specific property-based demand made by an alien customer, if applicable to this order.
        /// This will be <c>null</c> for standard recipe-based orders.
        /// This field is read-only after the order is created.
        /// </summary>
        private readonly AlienPropertyDemand? _alienDemand;

        /// <summary>
        /// Gets the <see cref="Recipe"/> associated with this order, if any.
        /// Returns <c>null</c> if the order is an <see cref="AlienPropertyDemand"/> or has no specific recipe.
        /// </summary>
        public Recipe? AssignedRecipe
        {
            get { return _assignedRecipe; }
        }

        /// <summary>
        /// Gets the <see cref="Customer"/> who placed this order.
        /// </summary>
        public Customer RequestingCustomer
        {
            get { return _requestingCustomer; }
        }

        /// <summary>
        /// Gets the current <see cref="OrderStatus"/> of this order (e.g., Pending, Served, Failed).
        /// </summary>
        public OrderStatus CurrentStatus
        {
            get { return _currentStatus; }
        }

        /// <summary>
        /// Gets the <see cref="AlienPropertyDemand"/> for this order, if it's an alien customer's specific demand.
        /// Returns <c>null</c> if this order is based on a standard recipe.
        /// </summary>
        public AlienPropertyDemand? AlienDemand
        {
            get { return _alienDemand; }
        }

         /// <summary>
        /// Initializes a new instance of the <see cref="Order"/> class.
        /// Every order must be associated with a non-null customer.
        /// An order should ideally specify either a recipe or an alien demand.
        /// </summary>
        /// <param name="customer">The customer placing the order. This parameter cannot be null.</param>
        /// <param name="recipe">The specific <see cref="Recipe"/> for the order. Can be null if the order is an <see cref="AlienPropertyDemand"/>.</param>
        /// <param name="alienDemand">The <see cref="AlienPropertyDemand"/> for the order, typically for alien customers. Can be null if the order is recipe-based.</param>
        /// <exception cref="ArgumentNullException">Thrown if the provided <paramref name="customer"/> is null.</exception>
        public Order(Customer customer, Recipe? recipe, AlienPropertyDemand? alienDemand = null)
        {
            if (customer == null)
            {
                throw new ArgumentNullException(nameof(customer), "Requesting customer cannot be null.");
            }

            if (recipe == null && !alienDemand.HasValue)
            {
                Console.WriteLine($"WARNING: Order created for {customer.CustomerID} with NO recipe AND NO AlienDemand. This is likely an error in order creation logic.");
            }

            _requestingCustomer = customer;
            _assignedRecipe = recipe;
            _alienDemand = alienDemand;
            _currentStatus = OrderStatus.Pending; // New orders always start in a 'Pending' state.

            // Log details about the created order for debugging and tracking purposes.
            if (_alienDemand.HasValue)
            {
                Console.WriteLine($"Order created for {customer.CustomerID} - ALIEN DEMAND: {_alienDemand.Value.ToString()}. Status: {_currentStatus}");
            }
            else if (_assignedRecipe != null)
            {
                Console.WriteLine($"Order created for {customer.CustomerID} - Recipe: {_assignedRecipe.RecipeName} at time. Status: {_currentStatus}");
            }
        }

        /// <summary>
        /// Explicitly sets the current status of the order.
        /// If the new status differs from the current one, the status is updated, and the change is logged.
        /// </summary>
        /// <param name="status">The new <see cref="OrderStatus"/> to apply to this order.</param>
        public void SetCurrentStatus(OrderStatus status)
        {
            if (_currentStatus != status)
            {
                // Attempt to identify the order by recipe name or alien demand for clearer logging.
                string orderIdentifier = _assignedRecipe?.RecipeName ?? _alienDemand?.ToString() ?? "Unknown Order";
                _currentStatus = status;
                Console.WriteLine($"Order for {RequestingCustomer.CustomerID} ({orderIdentifier}) status explicitly set to: {status}");
            }
        }

        /// <summary>
        /// Updates the current status of the order to a new state.
        /// If the new status differs from the current one, the status is updated, and the change is logged.
        /// Note: The current implementation of this method is identical to <see cref="SetCurrentStatus"/>.
        /// </summary>
        /// <param name="newStatus">The new <see cref="OrderStatus"/> to apply to this order.</param>
        public void UpdateStatus(OrderStatus newStatus)
        {
            if (_currentStatus != newStatus)
            {
                string orderIdentifier = _assignedRecipe?.RecipeName ?? _alienDemand?.ToString() ?? "Unknown Order";
                _currentStatus = newStatus;
                Console.WriteLine($"Order for {RequestingCustomer.CustomerID} ({orderIdentifier}) status updated to: {newStatus}");
            }
        }
    }
}