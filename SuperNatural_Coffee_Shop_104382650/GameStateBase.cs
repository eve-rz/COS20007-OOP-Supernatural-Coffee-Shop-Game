namespace CoffeeShop
{
    /// <summary>
    /// Defines the base structure for all game states in the game.
    /// This abstract class provides a common interface for state transitions, updating, and rendering,
    /// implementing the State design pattern.
    /// </summary>
    public abstract class GameStateBase
    {
        /// <summary>
        /// Gets the specific type of the game state, represented by the <see cref="GameState"/> enum.
        /// Each derived class must implement this to identify itself.
        /// </summary>
        public abstract GameState StateType { get; }

        /// <summary>
        /// Called when this game state becomes the active state.
        /// This method can be overridden by derived classes to perform initialization logic,
        /// such as setting up UI, starting music, or resetting variables.
        /// </summary>
        /// <param name="gameManager">The central game manager that controls the game's flow and components.</param>
        public virtual void Enter(GameManager gameManager)
        { 
            // Base implementation is empty. Derived states can override this.
        }

        /// <summary>
        /// Called on every frame to update the game logic for the current state.
        /// This must be implemented by all derived state classes.
        /// </summary>
        /// <param name="gameManager">The central game manager instance.</param>
        /// <param name="deltaTime">The time in seconds that has passed since the last frame. Used for frame-rate independent logic.</param>
        public abstract void Update(GameManager gameManager, float deltaTime);

        /// <summary>
        /// Called on every frame to draw the visual elements of the current state.
        /// This must be implemented by all derived state classes.
        /// </summary>
        /// <param name="gameManager">The central game manager, providing access to rendering services.</param>
        public abstract void Draw(GameManager gameManager);

        /// <summary>
        /// Called just before the game transitions away from this state.
        /// This method can be overridden by derived classes to perform cleanup tasks,
        /// such as saving data, stopping sounds, or hiding UI elements.
        /// </summary>
        /// <param name="gameManager">The central game manager that controls the game's flow and components.</param>
        public virtual void Exit(GameManager gameManager)
        { 
            // Base implementation is empty. Derived states can override this.
        }

        /// <summary>
        /// Gets the enumeration value that represents the current concrete state.
        /// This is required by all derived classes.
        /// </summary>
        /// <returns>The <see cref="GameState"/> enum value corresponding to this state.</returns>
        public abstract GameState GetState();
    }
}