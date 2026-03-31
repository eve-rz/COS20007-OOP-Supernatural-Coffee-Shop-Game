using Raylib_cs;

namespace CoffeeShop
{
    /// <summary>
    /// Represents the state where the player can access the in-game shop to buy items or upgrades.
    /// This state functions as an overlay, pausing the active gameplay state in the background.
    /// </summary>
    public class ShopState : GameStateBase
    {
        /// <summary>
        /// Gets the type of this game state.
        /// </summary>
        /// <remarks>
        /// This is considered a <see cref="GameState.Paused"/> state because it interrupts
        /// the normal flow of gameplay.
        /// </remarks>
        /// <value>
        /// Returns <see cref="GameState.Paused"/>.
        /// </value>
        public override GameState StateType => GameState.Paused;

        /// <summary>
        /// Stores a reference to the game state that was active before the shop was opened.
        /// </summary>
        private readonly GameStateBase _previousState;

        /// <summary>
        /// Stores the specific view that was active before entering the shop, to be restored on exit.
        /// </summary>
        private readonly GameView _viewToRestore;

        /// <summary>
        /// Initializes a new instance of the <see cref="ShopState"/> class.
        /// </summary>
        /// <param name="previousState">The state that was active before opening the shop.</param>
        public ShopState(GameManager gameManager)
        {
            _previousState = gameManager.GetCurrentStateForTesting(); 
            _viewToRestore = gameManager.CurrentGameView; 
        }

        /// <summary>
        /// Called when this state is first entered.
        /// It switches the active view to the shop screen.
        /// </summary>
        /// <param name="gameManager">The central game manager that controls the game's flow and components.</param>
        public override void Enter(GameManager gameManager)
        {
            gameManager.SwitchView(GameView.ShopScreen);
        }

        /// <summary>
        /// Called on every frame. Listens for the 'Escape' key press to close the shop menu.
        /// </summary>
        /// <param name="gameManager">The central game manager instance.</param>
        /// <param name="deltaTime">The time elapsed since the last frame.</param>
        public override void Update(GameManager gameManager, float deltaTime)
        {
            if (Raylib.IsKeyPressed(KeyboardKey.Escape))
            {
                gameManager.CloseShop();
            }
        }

        /// <summary>
        /// Called on every frame to draw the visuals.
        /// It first draws the underlying previous state as a background and then draws the
        /// shop UI overlay on top.
        /// </summary>
        /// <param name="gameManager">The central game manager, providing access to rendering and UI services.</param>
        public override void Draw(GameManager gameManager)
        {
            _previousState.Draw(gameManager);
            gameManager.UIManager.DrawShopScreen(gameManager);
        }

        /// <summary>
        /// Handles the logic for returning to the previous state from the shop.
        /// It changes the state back to the one that was active before the shop was opened.
        /// </summary>
        /// <param name="gameManager">The game manager instance that will handle the state change.</param>
        public void ReturnToPreviousState(GameManager gameManager)
        {
            gameManager.SwitchView(_viewToRestore); 
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
    }
}