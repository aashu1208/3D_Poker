# 3D Poker Project - Advanced Architecture Assessment

## 1. Overview
This project is a highly modular, event-driven 3D Poker game built in Unity. It demonstrates a scalable architecture ready for real-time multiplayer integration, a robust state machine for game flow, and data-driven design using ScriptableObjects.

## 2. Key Architectural Decisions

### A. Service-Manager Separation
- **Services**: Pure C# classes (no MonoBehaviour) handling specific logic.
  - `HandEvaluator`: Combines cards and determines hand strength/rank. Also contains AI decision logic.
  - `DeckService`: Manages card shuffling and dealing.
  - `ChipManagerService`: Handles pot logic, betting deductions, and winner distribution.
- **Managers**: MonoBehaviours that coordinate services and handle Unity-specific lifecycles.
  - `GameManager`: The central bootstrapper that wires everything together.
  - `TurnTimer`: Manages turn timing and advances the game state.

### B. State Machine (GameStateController)
The game flow is managed by a structured State Machine:
- `PreFlopState`: Blinds, dealing hole cards.
- `FlopState`: 3 community cards.
- `TurnState`: 1 additional card.
- `RiverState`: Final community card.
- `ShowdownState`: Reveal hands, distribute pot, and reset.

### C. Multiplayer Readiness (Network Abstraction)
- **INetworkAdapter**: An interface that abstracts all player actions.
- **LocalNetworkAdapter**: Currently used for local simulation.
- **Action Flow**: All UI inputs (Fold, Call, Raise) are sent to the `INetworkAdapter`, which then forwards them to the Game Logic. This mirrors exactly how a real networked game works.
- **Scalability**: To convert this to online multiplayer (e.g., Photon), you only need to create a `PhotonNetworkAdapter` and swap it in the `GameManager` initialization. No game logic changes required.

### D. Event-Driven Communication (EventBus)
The UI and 3D Visuals are completely decoupled from the game logic through the `EventBus`.
- Logic classes broadcast events (e.g., `OnHoleCardsDealt`).
- UI (`UIManager`) and Visuals (`Table3DVisuals`) listen to these events.
- **Benefit**: You can completely replace the UI or 3D view without breaking the core game.

### E. Data-Driven Design (ScriptableObjects)
- **GameSettingsSO**: Allows changing match rules (Small Blind, Starting Chips, etc.) through Unity assets instead of code.
- **CardDatabaseSO**: Maps card data (Rank/Suit) to UI Sprites, allowing for easy visual updates.

## 3. Mandatory Requirements Fulfilled
- [x] **Multiplayer-ready architecture**: Abstracted via `INetworkAdapter`. All player actions (AI and Human) flow through this layer.
- [x] **Local multiplayer simulation**: Implemented turn-based logic with local loopback.
- [x] **UI/Logic Separation**: Achieved via `EventBus`.
- [x] **State Machine**: Full round-flow implemented.
- [x] **Timer System**: 15s per player. Human players can act before the timer expires.
- [x] **Pot Logic**: Correct deduction and distribution.
- [x] **Restart Logic**: Reset state functionality without scene reload.
- [x] **Improved AI**: Decision-making based on Hand Strength and Pot Odds.
- [x] **Human Input**: UI buttons for Fold, Call/Check, and Raise.

## 4. Setup Instructions
1. Open the scene `PokerScene`.
2. Locate `GameSettings` asset in `Assets/Settings`.
3. Locate `CardDatabase` asset in `Assets/Settings`.
4. Ensure these are assigned to `GameManager` and `UIManager` in the Inspector.
5. Hit Play!

---
*Created as part of the Unity Developer Assessment.*
