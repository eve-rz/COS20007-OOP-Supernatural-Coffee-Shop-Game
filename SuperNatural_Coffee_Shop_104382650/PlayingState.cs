using Raylib_cs;

namespace CoffeeShop
{
    /// <summary>
    /// Represents the primary state where active gameplay occurs.
    /// This state manages the core game loop, including player input,
    /// character interactions, and game logic updates.
    /// </summary>
    public class PlayingState : GameStateBase
    {
        /// <summary>
        /// Gets the type of this game state.
        /// </summary>
        /// <value>
        /// Always returns <see cref="GameState.Playing"/>.
        /// </value>
        public override GameState StateType => GameState.Playing;
        
        // /// <summary>
        // /// Called when this state is first entered.
        // /// It sets the initial game view to the customer screen.
        // /// </summary>
        // /// <param name="gameManager">The central game manager that controls the game's flow and components.</param>
        // public override void Enter(GameManager gameManager)
        // {
        //     gameManager.SwitchView(GameView.CustomerScreen);
        // }

        /// <summary>
        /// Called on every frame to update the game's logic and handle player input.
        /// </summary>
        /// <param name="gameManager">The central game manager instance.</param>
        /// <param name="deltaTime">The time elapsed since the last frame.</param>
        public override void Update(GameManager gameManager, float deltaTime)
        {
            gameManager.UpdateCoreGameplay(deltaTime);

            if (Raylib.IsKeyPressed(KeyboardKey.P) && gameManager.CurrentGameView != GameView.DialogueScreen)
            {
                gameManager.PauseGame();
            }
            
            if (Raylib.IsKeyPressed(KeyboardKey.S))
            {
                if (gameManager.CurrentGameView == GameView.CustomerScreen || gameManager.CurrentGameView == GameView.WorkstationScreen)
                {
                    gameManager.OpenShop();
                }
            }
        }

        /// <summary>
        /// Called on every frame to draw the visual elements of the state.
        /// This method delegates the drawing of the current game view (e.g., customer screen, workstation)
        /// to the UIManager.
        /// </summary>
        /// <param name="gameManager">The central game manager, providing access to rendering and UI services.</param>
        public override void Draw(GameManager gameManager)
        {
            gameManager.UIManager.DrawCurrentView(gameManager);
        }

        /// <summary>
        /// Gets the enumeration value representing the current state.
        /// </summary>
        /// <returns>The <see cref="GameState"/> enumeration value, which is <see cref="GameState.Playing"/>.</returns>
        public override GameState GetState()
        {
            return GameState.Playing;
        }

        /// <summary>
        /// Gets the primary view associated with this state.
        /// </summary>
        /// <remarks>
        /// This method currently always returns the CustomerScreen. It might be used
        /// to determine a default or starting view for the playing state.
        /// </remarks>
        /// <returns>The <see cref="GameView.CustomerScreen"/>.</returns>
        public GameView GetCurrentView()
        {
            return GameView.CustomerScreen; 
        }
    }
}