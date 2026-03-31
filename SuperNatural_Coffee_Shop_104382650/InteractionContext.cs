using System.Collections.Generic;
using System.Linq;

namespace CoffeeShop
{
    /// <summary>
    /// Represents the context of an interaction with an object, detailing its current readiness,
    /// a status message, and a list of actions currently available to the player.
    /// This object is typically returned when the player attempts to interact with an <see cref="IInteractable"/> item.
    /// </summary>
    public class InteractionContext
    {
        /// <summary>
        /// Backing field indicating if the interaction target is ready for its primary action(s).
        /// </summary>
        private readonly bool _isReady;
        /// <summary>
        /// Backing field for the status message.
        /// </summary>
        private readonly string _statusMessage;
        /// <summary>
        /// Internal list of available actions.
        /// </summary>
        private readonly List<string> _internalAvailableActions;

        /// <summary>
        /// Gets a message describing the current status of the interactable object
        /// (e.g., "Ready to brew", "Needs more water!", "Needs maintenance!").
        /// </summary>
        public string StatusMessage
        {
            get { return _statusMessage; }
        }

        /// <summary>
        /// Gets a list of string descriptions for actions the player can currently take
        /// with the interactable object (e.g., "Brew Drink", "Refill Water", "Check Status").
        /// This property returns a new list instance to prevent external modification
        /// of the internal collection of actions.
        /// </summary>
        public List<string> AvailableActions
        {
            get { return new List<string>(_internalAvailableActions); }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InteractionContext"/> class.
        /// </summary>
        /// <param name="isReady">A boolean value indicating whether the interactable object is ready
        /// for its primary functions. For example, <c>true</c> if a coffee machine can brew, <c>false</c> if it needs water.</param>
        /// <param name="statusMessage">A user-friendly message describing the current status of the interactable object.
        /// If null, defaults to an empty string.</param>
        /// <param name="actions">A list of string descriptions for the actions available to the player in this context.
        /// If null, defaults to an empty list.</param>
        public InteractionContext(bool isReady, string statusMessage, List<string> actions)
        {
            _isReady = isReady;
            _statusMessage = statusMessage ?? string.Empty; 
            _internalAvailableActions = actions ?? new List<string>();
        }
    }
}