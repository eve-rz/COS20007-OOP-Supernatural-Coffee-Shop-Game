# Supernatural Coffee Shop

A 2D management/simulation game built with C# and Raylib where you run a coffee shop serving both normal and supernatural customers.

You must prepare drinks under time pressure, manage stock and water usage, keep customers happy, and survive unusual events (ghost interference, supernatural moods, and special customer rules).

## Purpose Of This Project

This project demonstrates:

- A full game loop in C# using Raylib
- State-driven architecture (menu, intro, playing, paused, shop, game over, game complete)
- Object-oriented gameplay modeling (customers, drinks, ingredients, levels, orders)
- Real-time UI rendering and interaction
- Audio streaming and runtime resource loading
- Basic automated tests for customer ordering logic

## Core Features

- Multi-screen flow: Main menu, gameplay, workstation, dialogue, shop, pause, settings, end states
- Different customer categories:
  - Normal customers (office worker, tourist, student)
  - Supernatural customers (ghost, alien, fire monster, toyol)
- Recipe and ingredient system with unlock progression
- Level objectives (earnings target and timer)
- Patience and sanity mechanics
- Shop purchases between levels
- Audio tracks for gameplay/workstation and level variation

## Tech Stack And Libraries

- Language: C#
- Runtime/SDK: .NET 8 (`net8.0`)
- Rendering/Input/Audio API: `Raylib-cs` (NuGet)
- Testing: NUnit + .NET test runner packages in the same project

NuGet packages declared in the project:

- `Raylib-cs` `7.0.1`
- `NUnit` `3.9.0`
- `NUnit3TestAdapter` `3.9.0`
- `Microsoft.NET.Test.Sdk` `15.5.0`
- `NUnit.Console` `3.11.1`

## Requirements

1. Install .NET 8 SDK
2. A desktop OS supported by Raylib-cs (Windows/macOS/Linux)
3. Git (optional, for cloning)

## Getting Started

### 1. Clone

```bash
git clone <your-repo-url>
cd SuperNatural_Coffee_Shop_104382650
```

### 2. Restore Dependencies

```bash
dotnet restore CoffeeShop.sln
```

### 3. Build

```bash
dotnet build CoffeeShop.sln
```

### 4. Run

```bash
dotnet run --project CoffeeShop.csproj
```

This starts the Raylib window and enters the game flow.

## Tested Commands (Verified In This Workspace)

The following commands were executed successfully:

```bash
dotnet build CoffeeShop.sln
dotnet test CoffeeShop.sln
```

Test result in this workspace: 7 passed, 0 failed.

## Controls (Current Implementation)

- `P`: Pause game (when not in dialogue)
- `S`: Open shop (from customer/workstation gameplay view)
- Mouse input: Main UI interactions (buttons, dialogue, ingredient selection, serving/trashing, etc.)

Note: Input handling is distributed across game state and UI logic.

## Asset Notes

- `Images/` is copied to output via `.csproj` content settings.
- Audio is loaded at runtime from the output `Audio` directory:
  - `game_song.ogg`
  - `workstation_screen_audio.mp3`
  - `level4_song.ogg`
  - `level4_workstation.ogg`

If audio files are missing, the game logs warnings and continues without those tracks.

## Project Architecture

### High-Level Flow

1. `program.cs` initializes Raylib and audio device
2. `GameManager` is created and enters `MainMenuState`
3. Each frame:
   - update current state and systems
   - draw current view via `UIManager`
4. State transitions are handled through `GameManager.ChangeState(...)`

### Main Runtime Systems

- `GameManager`: central coordinator (state, level progression, transitions, score, active interaction)
- `UIManager`: all rendering, screen transitions, HUD, menus, workstation/shop interaction
- `ShopLayout`: customer queue, spawn logic, station/equipment placement
- `AudioManager`: music stream loading/playback/update/unload

## File-By-File Reference

### Entry Point

- `program.cs`: Application entry point. Initializes Raylib/audio, runs main update/draw loop, and handles shutdown.

### State Management

- `GameStateBase.cs`: Abstract base contract for all game states.
- `GameState.cs`: Enum of logical state identifiers.
- `GameView.cs`: Enum of visual/gameplay screens.
- `MainMenuState.cs`: Main menu logic and transitions.
- `LevelIntroState.cs`: Level intro setup/briefing state.
- `PlayingState.cs`: Main gameplay update loop state.
- `PausedState.cs`: Pause menu flow and resume/exit options.
- `ShopState.cs`: In-between-level shop state.
- `SettingsState.cs`: Settings screen state and return flow.
- `GameOverState.cs`: Failure/end screen behavior.
- `GameCompleteState.cs`: Win/completion screen behavior.

### Core Orchestration And Presentation

- `GameManager.cs`: Central game controller and state machine owner.
- `UIManager.cs`: Handles all screen drawing, user interaction, HUD/workstation/shop UI.
- `AudioManager.cs`: Loads and controls background music streams.

### Player, Level, Progression

- `Player.cs`: Player stats and current active order context.
- `Level.cs`: Level configuration (targets, time, allowed types, unlocked content).
- `ShopLayout.cs`: Queue/station management and customer spawning.
- `Station.cs`: Physical station model where equipment is assigned.
- `Vector2D.cs`: Simple 2D coordinate helper class.

### Interaction And Equipment

- `IInteractable.cs`: Interface for interactable game entities.
- `Equipment.cs`: Abstract base for usable equipment.
- `CoffeeMachine.cs`: Coffee machine implementation (brewing/water process behavior).
- `EquipmentType.cs`: Enum for equipment categories.
- `InteractionContext.cs`: Status/actions payload returned by interactions.
- `ProcessResult.cs`: Result object for operations like brewing.

### Menu, Recipes, And Items

- `Item.cs`: Abstract base item model.
- `Ingredient.cs`: Ingredient definition and gameplay properties.
- `IngredientType.cs`: Enum for ingredient identity.
- `Drink.cs`: Final drink model composed from recipe/ingredients.
- `Recipe.cs`: Recipe definition including required ingredients and constraints.

### Customers, Dialogue, And Orders

- `Customer.cs`: Abstract customer base (state, patience, textures, ordering hooks).
- `NormalCustomer.cs`: Standard customer ordering behavior.
- `SupernaturalCustomer.cs`: Special customer behavior/effects and unique order rules.
- `CustomerType.cs`: Enum for normal customer categories.
- `SupernaturalCustomerType.cs`: Enum for supernatural variants.
- `CustomerMood.cs`: Enum for mood/temperament states.
- `CustomerState.cs`: Enum for customer lifecycle states.
- `Dialogue.cs`: Dialogue line storage and retrieval.
- `Order.cs`: Order object linking customer and requested recipe.
- `OrderStatus.cs`: Enum for order progress/status.
- `AlienPropertyDemand.cs`: Struct defining alien flavor-property demands.
- `MessageType.cs`: Enum for UI message categories.

### Testing

- `TestUnit.cs`: NUnit tests focused on customer drink-order selection behavior.

### Project/Build Files

- `CoffeeShop.csproj`: .NET project file, package references, image content copy rules.
- `CoffeeShop.sln`: Solution file for IDE/build tooling.

### Runtime/Generated Folders

- `Images/`: Source game textures/sprites used by UI and customers.
- `Sprites/`: Additional sprite assets.
- `bin/`: Build output (executables, copied assets, runtime dependencies).
- `obj/`: Intermediate build artifacts.

## Known Notes

- Audio file loading depends on expected filenames in output `Audio/` path.
- Build and tests currently run from the single project setup.
- The codebase includes extensive XML documentation comments, which helps maintenance and onboarding.

## Suggested Future Improvements

- Separate tests into a dedicated test project
- Add save/load system for progression and settings
- Add CI workflow for build + test on GitHub Actions
- Add gameplay GIF/screenshot section to this README for repository presentation
