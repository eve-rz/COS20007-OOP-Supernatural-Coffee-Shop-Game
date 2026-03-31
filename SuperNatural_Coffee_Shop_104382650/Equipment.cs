using System.Collections.Generic;


namespace CoffeeShop
{
    /// <summary>
    /// Abstract base class for all equipment in the coffee shop.
    /// All equipment is interactable.
    /// </summary>
    public abstract class Equipment : IInteractable
    {
        protected readonly string _equipmentName;
        protected bool _isBusy;
        protected float _condition;
        protected readonly EquipmentType _type;
        protected readonly string _interactionNameValue;

        /// <summary>
        /// Gets or sets the name of the equipment.
        /// </summary>
        public string EquipmentName
        {
            get { return _equipmentName; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the equipment is currently busy.
        /// </summary>
        public bool IsBusy
        {
            get { return _isBusy; }
        }

        /// <summary>
        /// Gets or sets the type of the equipment.
        /// </summary>
        public EquipmentType Type
        {
            get { return _type; }
        }

        /// <summary>
        /// Gets or sets the user-friendly name for interacting with this equipment.
        /// Implements <see cref="IInteractable.InteractionName"/>.
        /// </summary>
        public string InteractionName
        {
            get { return _interactionNameValue; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Equipment"/> abstract class.
        /// </summary>
        /// <param name="name">The name of the equipment.</param>
        /// <param name="type">The type of the equipment.</param>
        /// <param name="interactionName">The display name for interaction prompts.</param>
        protected Equipment(string name, EquipmentType type, string interactionName)
        {
            _equipmentName = string.IsNullOrWhiteSpace(name) ? "Unnamed Equipment" : name;
            _type = type;
            _interactionNameValue = string.IsNullOrWhiteSpace(interactionName) ? $"Use {name}" : interactionName;
            _isBusy = false;
            _condition = 100.0f; 
        }

        /// <summary>
        /// Sets the busy status of the equipment.
        /// </summary>
        /// <param name="status">True if the equipment is busy, false otherwise.</param>
        public virtual void SetBusy(bool status)
        {
            _isBusy = status;
            System.Console.WriteLine($"{EquipmentName} is now {(status ? "busy" : "available")}.");
        }

        /// <summary>
        /// Initiates an interaction with the equipment.
        /// Must be implemented by derived classes.
        /// Implements <see cref="IInteractable.InitiateInteraction"/>.
        /// </summary>
        /// <returns>An InteractionContext relevant to this equipment.</returns>
        public abstract InteractionContext InitiateInteraction();

        /// <summary>
        /// Performs a processing action using this equipment, typically to create a part of or a whole drink.
        /// Must be implemented by derived classes.
        /// </summary>
        /// <param name="recipe">The recipe guiding the process (can be null if not recipe-based).</param>
        /// <param name="ingredientsToProcess">The ingredients provided for processing.</param>
        /// <returns>A ProcessResult indicating the outcome of the operation.</returns>
        public abstract ProcessResult PerformProcess(Recipe? recipe, Dictionary<Ingredient, int> ingredientsToProcess, bool isForAlienDemand = false);
    }
}