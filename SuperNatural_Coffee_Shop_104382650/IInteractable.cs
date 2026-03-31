// IInteractable.cs

namespace CoffeeShop
{
    /// <summary>
    /// Defines a contract for objects that the player can interact with.
    /// </summary>
    public interface IInteractable
    {
        /// <summary>
        /// Gets the user-friendly name of the interaction (e.g., "Use Coffee Machine", "Grind Beans").
        /// </summary>
        string InteractionName { get; }

        /// <summary>
        /// Initiates an interaction with the object.
        /// </summary>
        /// <returns>
        /// An <see cref="InteractionContext"/> object containing information about
        /// the state of the interactable and available actions.
        /// </returns>
        InteractionContext InitiateInteraction();
    }
}