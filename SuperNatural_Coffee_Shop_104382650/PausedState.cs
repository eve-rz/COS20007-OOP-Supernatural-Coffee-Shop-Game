using Raylib_cs;

namespace CoffeeShop
{
    /// <summary>
    /// Represents the paused state of the game.
    /// This state is entered when the player pauses the game, typically from the 'Playing' state.
    /// It preserves the state of the game that was paused and draws a pause menu over it.
    /// </summary>
    public class PausedState : GameStateBase
    {
        /// <summary>
        /// Gets the type of this game state.
        /// </summary>
        /// <value>
        /// Always returns <see cref="GameState.Paused"/>.
        /// </value>
        public override GameState StateType => GameState.Paused;

        /// <summary>
        /// Stores a reference to the game state that was active before the game was paused.
        /// This allows the game to resume seamlessly.
        /// </summary>
        private readonly GameStateBase _previousState;

        /// <summary>
        /// Initializes a new instance of the <see cref="PausedState"/> class.
        /// </summary>
        /// <param name="previousState">The state that was active when the game was paused. This will be resumed later.</param>
        public PausedState(GameStateBase previousState)
        {
            _previousState = previousState;
        }

        /// <summary>
        /// Called on every frame. Checks for input to resume the game.
        /// If the 'P' or 'Escape' key is pressed, the game will unpause.
        /// </summary>
        /// <param name="gameManager">The central game manager instance.</param>
        /// <param name="deltaTime">The time elapsed since the last frame (not used in this state).</param>
        public override void Update(GameManager gameManager, float deltaTime)
        {
            if (Raylib.IsKeyPressed(KeyboardKey.P) || Raylib.IsKeyPressed(KeyboardKey.Escape))
            {
                Resume(gameManager);
            }
        }

        /// <summary>
        /// Called on every frame to draw the state's visuals.
        /// It first draws the underlying state (the game as it was when paused)
        /// and then draws the pause menu UI over it.
        /// </summary>
        /// <param name="gameManager">The central game manager, providing access to rendering and UI services.</param>
        public override void Draw(GameManager gameManager)
        {
            // Draw the screen of the state that was paused.
            _previousState.Draw(gameManager);
            // Draw the pause menu on top.
            gameManager.UIManager.DrawPauseMenu(gameManager);
        }
        
        /// <summary>
        /// Resumes the game by changing the state back to the one that was active before pausing.
        /// </summary>
        /// <param name="gameManager">The game manager instance that will handle the state change.</param>
        public void Resume(GameManager gameManager)
        {
            gameManager.ChangeState(_previousState);
        }

        /// <summary>
        /// Gets the enumeration value representing the current state.
        /// </summary>
        /// <returns>The <see cref="GameState"/> enumeration value, which is <see cref="GameState.Paused"/>.</returns>
        public override GameState GetState()
        {
            return GameState.Paused;
        }

        /// <summary>
        /// Determines the appropriate background view based on the type of the previous state.
        /// This can be used by the UI to know what context it is being displayed in.
        /// </summary>
        /// <returns>
        /// <see cref="GameView.CustomerScreen"/> if the game was paused during active gameplay;
        /// otherwise, <see cref="GameView.MainMenuScreen"/>.
        /// </returns>
        public GameView GetPreviousStateView()
        {
            if (_previousState is PlayingState) return GameView.CustomerScreen;
            return GameView.MainMenuScreen; 
        }
    }
}