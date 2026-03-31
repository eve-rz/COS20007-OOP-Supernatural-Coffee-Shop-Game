using Raylib_cs;

namespace CoffeeShop
{
    /// <summary>
    /// Represents the state where the player can configure game settings.
    /// This state acts as an overlay, preserving the previous game state (like MainMenu or Paused)
    /// to return to it after settings are adjusted.
    /// </summary>
    public class SettingsState : GameStateBase
    {
        /// <summary>
        /// Gets the type of this game state.
        /// </summary>
        /// <remarks>
        /// This is considered a <see cref="GameState.Paused"/> state because it interrupts
        /// the normal flow of the game that was running before it was entered.
        /// </remarks>
        /// <value>
        /// Returns <see cref="GameState.Paused"/>.
        /// </value>
        public override GameState StateType => GameState.Paused;

        /// <summary>
        /// Stores a reference to the game state that was active before the settings menu was opened.
        /// </summary>
        private readonly GameStateBase _previousState;

        /// <summary>
        /// Stores the specific view that should be restored when exiting the settings menu.
        /// </summary>
        private readonly GameView _viewToRestore; 

        /// <summary>
        /// Initializes a new instance of the <see cref="SettingsState"/> class.
        /// It determines which view to return to based on the state the game was in previously.
        /// </summary>
        /// <param name="previousState">The state that was active before opening settings.</param>
        public SettingsState(GameStateBase previousState)
        {
            _previousState = previousState;

            // Determine the correct view to restore after closing settings.
            if (previousState is PlayingState) _viewToRestore = GameView.CustomerScreen; 
            else if (previousState is PausedState p) _viewToRestore = p.GetPreviousStateView();
            else _viewToRestore = GameView.MainMenuScreen;
        }

        /// <summary>
        /// Called when this state is first entered.
        /// It switches the active view to the settings screen.
        /// </summary>
        /// <param name="gameManager">The central game manager that controls the game's flow and components.</param>
        public override void Enter(GameManager gameManager)
        {
            gameManager.SwitchView(GameView.SettingsScreen);
        }

        /// <summary>
        /// Called on every frame. Listens for the 'Escape' key press to close the settings menu.
        /// </summary>
        /// <param name="gameManager">The central game manager instance.</param>
        /// <param name="deltaTime">The time elapsed since the last frame.</param>
        public override void Update(GameManager gameManager, float deltaTime)
        {
            if (Raylib.IsKeyPressed(KeyboardKey.Escape))
            {
                gameManager.CloseSettings();
            }
        }

        /// <summary>
        /// Called on every frame to draw the visuals.
        /// It first draws the underlying previous state as a background, then draws the
        /// settings UI overlay on top.
        /// </summary>
        /// <param name="gameManager">The central game manager, providing access to rendering and UI services.</param>
        public override void Draw(GameManager gameManager)
        {
            _previousState.Draw(gameManager);
            gameManager.UIManager.DrawSettingsScreen(gameManager);
        }

        /// <summary>
        /// Handles the logic for returning to the previous state from the settings menu.
        /// It restores the original view and then changes the state back.
        /// </summary>
        /// <param name="gameManager">The game manager instance that will handle the view and state change.</param>
        public void ReturnToPreviousState(GameManager gameManager)
        {
            gameManager.SwitchView(_viewToRestore);
            gameManager.ChangeState(_previousState);
        }

        /// <summary>
        /// Gets the enumeration value representing the current state.
        /// </summary>
        /// <returns>The <see cref="GameState"/> enumeration value, which is <see cref="GameState.Paused"/>.</returns>
        public override GameState GetState() => GameState.Paused;
    }
}