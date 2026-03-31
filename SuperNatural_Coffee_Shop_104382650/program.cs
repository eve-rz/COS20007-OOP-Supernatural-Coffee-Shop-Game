using Raylib_cs;
using CoffeeShop;
using System;    

class Program 
{
    public static void Main(string[] args)
    {
        const int screenWidth = 1280;
        const int screenHeight = 720;

        Raylib.InitWindow(screenWidth, screenHeight, "Supernatural Coffee Shop");
        Raylib.SetExitKey(KeyboardKey.Null); 
        Raylib.SetTargetFPS(60);

        Raylib.InitAudioDevice(); 
        if (!Raylib.IsAudioDeviceReady())
        {
            Console.WriteLine("CRITICAL WARNING: Raylib audio device FAILED to initialize!");
        }

        GameManager? gameManager = null; 

        try
        {
            gameManager = new GameManager(screenWidth, screenHeight);

            while (!Raylib.WindowShouldClose() && (gameManager != null && !gameManager!.QuitProgramRequested))
            {
                // 1. Update Game Logic
                gameManager.Update(Raylib.GetFrameTime());

                // 2. Draw Everything
                Raylib.BeginDrawing();
                Raylib.ClearBackground(Color.Black);

                gameManager.Draw();

                Raylib.EndDrawing();
            }
        }
        catch (Exception e)
        {
            Console.WriteLine("CRITICAL ERROR in game loop: " + e.ToString());
            Raylib.BeginDrawing();
            Raylib.ClearBackground(Color.Black);
            Raylib.DrawText($"FATAL ERROR: {e.Message}", 10, 10, 20, Color.Red);
            Raylib.DrawText("Please check the console output for more details.", 10, 40, 20, Color.White);
            Raylib.EndDrawing();

            while (!Raylib.WindowShouldClose()) { /* Wait for user to close */ }
        }
        finally
        {
            Console.WriteLine("Cleaning up and closing application...");
            
            gameManager?.UIManager?.UnloadAssets();
            gameManager?.AudioManager?.UnloadAllMusic(); 

            if (Raylib.IsAudioDeviceReady()) 
            {
                Raylib.CloseAudioDevice();
            }
            Raylib.CloseWindow();
            Console.WriteLine("Application closed.");
        }
    }
}