# Monopoly Game

A digital implementation of the classic Monopoly board game built with C# and Windows Forms.

## Features

- 🎮 **Single Player & Multiplayer**: Play locally with up to 8 players or online with friends
- 🎲 **Classic Monopoly Gameplay**: Buy properties, build houses, and bankrupt your opponents
- 💾 **Save & Load**: Save your game progress and continue later
- 🌐 **Online Multiplayer**: Host or join online games with real-time synchronization
- 🎵 **Audio Support**: Background music and sound effects using NAudio
- 📊 **SQLite Database**: Persistent game state storage

## Requirements

- .NET 9.0 or later
- Windows operating system
- Visual Studio 2022 or later (for development)

## Installation

1. Clone the repository:
   ```bash
   git clone https://github.com/VuVjp/Monopoly.git
   cd Monopoly
   ```

2. Open the solution file `Monopoly.sln` in Visual Studio

3. Restore NuGet packages:
   - NAudio (v2.2.1)
   - Newtonsoft.Json (v13.0.4)
   - System.Data.SQLite.Core (v1.0.119)

4. Build and run the project (F5)

## How to Play

### Offline Mode
1. Launch the game
2. Select "Offline Mode" from the main menu
3. Configure the number of players (2-8)
4. Customize player names and avatars
5. Click "Start Game" to begin

### Online Mode
**Host a Game:**
1. Select "Online Mode" → "Host Game"
2. Configure game settings
3. Wait for players to join
4. Start the game when ready

**Join a Game:**
1. Select "Online Mode" → "Find Server"
2. Enter the host's IP address
3. Choose your avatar and name
4. Wait for the host to start the game

## Game Controls

- **Roll Dice**: Click the "Roll" button to move your player
- **Buy Property**: Click "Buy" when landing on an unowned property
- **Build Houses**: Right-click owned properties to upgrade
- **Trade**: Use the trade interface to negotiate with other players
- **End Turn**: Click "End Turn" when finished with your actions

## Project Structure

```
Monopoly/
├── Data/               # Game data files (Board.json)
├── Properties/         # Application properties and resources
├── Resources/          # Images, icons, and audio files
├── Screens/           # UI forms and user controls
│   ├── MainMenu.cs
│   ├── Game.cs
│   ├── GameConfig.cs
│   └── ...
├── Scripts/           # Game logic and networking
│   ├── Board.cs       # Board and property definitions
│   ├── PlayerManager.cs  # Player state management
│   ├── Data.cs        # Network message types
│   └── SocketManager.cs  # Network communication
├── Monopoly.cs        # Main form
└── Program.cs         # Application entry point
```

## Development

### Building the Project

```bash
dotnet build
```

### Running Tests

Currently, this project does not have automated tests. Manual testing is required.

## Technologies Used

- **C# / .NET 9.0**: Core application framework
- **Windows Forms**: User interface
- **NAudio**: Audio playback
- **Newtonsoft.Json**: JSON serialization
- **SQLite**: Game state persistence
- **TCP Sockets**: Online multiplayer networking

## License

This project is provided as-is for educational purposes.

## Contributors

See the About Us section in the game for contributor information.

## Support

For issues or questions, please open an issue on the GitHub repository.
