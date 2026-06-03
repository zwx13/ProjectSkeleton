# Dog & Treat Catcher

A 2D arcade-style mini-game built with **C#, .NET, and Silk.NET (SDL)**. 

This project has acustom-built game loop, rendering pipeline, and physics system.

## Gameplay
The idea is that, to score points, we have to catch the falling treats. If a treat hits the ground and expires, you lose a heart (we start with 3). After losing 3 hearts, the game is over.

New for PR2: 
- Introduced falling chocolate bars; if the player collides with them, they lose a heart. The falling chocolate bars also stay longer on the ground. (They affect the dash mechanic, since the player could now dash in a chocolate bar and lose a heart.)
- Introduced levels with increasing difficulty: treats fall faster as levels go up (there is a cap, though, to not become impossible)
- Introduced highscore metrics; a highscore.txt file is saved after each run to persist the highest score.

### Controls
* **Move:** `Left Arrow` / `Right Arrow`
* **Jump:** `Up Arrow` or `Spacebar`
* **Dash:** `Double-Tap Left` or `Double-Tap Right`

## Content of files

* ** Separation of Concerns:** The game is split into:
  * `GameLogic.cs`: We have the game loop, object lifecycle, and collision detection.
  * `GameRenderer.cs`: This is where the visuals are rendered.
  * `GameWindow.cs`: Creates the window where we can play the game.
  * `InputLogic.cs`: This listens to keyboard. It tracks when and how fast we press keys.
  * `Program.cs`: Is the starting point of the game.
  * `GameCamera.cs`: Contains the logic for moving the camera.
* Assets: Contains all sprites, graphic, and the background music.
* Models: Contains the game entities that manage their own rules.


## How to run

[.NET SDK](https://dotnet.microsoft.com/download) should be installed

1. Clone this repository.
2. Open a terminal and navigate to the project folder (where the `.csproj` file is located).
3. Run the following command:
   ```bash
   dotnet run
   ```
