using Raylib_cs;

namespace CoffeeShop
{
    /// <summary>
    /// Represents the main menu state of the game.
    /// This is typically the first state the player interacts with, allowing them
    /// to start a new game, access options, or exit the application.
    /// </summary>
    public class MainMenuState : GameStateBase
    {
        /// <summary>
        /// Gets the type of this game state.
        /// </summary>
        /// <value>
        /// Always returns <see cref="GameState.MainMenu"/>.
        /// </value>
        public override GameState StateType => GameState.MainMenu;

        /// <summary>
        /// Called when this state is first entered.
        /// It switches the view to the main menu screen and starts playing the main theme music on a loop.
        /// </summary>
        /// <param name="gameManager">The central game manager that controls the game's flow and components.</param>
        public override void Enter(GameManager gameManager)
        {
            gameManager.SwitchView(GameView.MainMenuScreen);
            gameManager.AudioManager.PlayMusicLooping(gameManager.AudioManager.GetGameSong(), "game_song");
        }

        /// <summary>
        /// Called on every frame to update the state's logic.
        /// For the main menu, input handling and button logic are typically managed
        /// by the UIManager, so this method is empty.
        /// </summary>
        /// <param name="gameManager">The central game manager instance.</param>
        /// <param name="deltaTime">The time elapsed since the last frame.</param>
        public override void Update(GameManager gameManager, float deltaTime)
        { 
            // Input and updates are handled by the UIManager for the main menu screen.
        }

        /// <summary>
        /// Called on every frame to draw the visual elements of the state.
        /// This method delegates the drawing of the main menu to the UIManager.
        /// </summary>
        /// <param name="gameManager">The central game manager, providing access to rendering and UI services.</param>
        public override void Draw(GameManager gameManager)
        {
            gameManager.UIManager.DrawCurrentView(gameManager);
        }
        
        /// <summary>
        /// Gets the enumeration value representing the current state.
        /// </summary>
        /// <returns>The <see cref="GameState"/> enumeration value, which is <see cref="GameState.MainMenu"/>.</returns>
        public override GameState GetState()
        {
            return GameState.MainMenu;
        }
    }
}