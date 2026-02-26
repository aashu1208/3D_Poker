# Poker Architecture Overview
graph TD
    subgraph Core_Logic
        GM[GameManager] --> SMC[GameStateController]
        GM --> Deck[DeckService]
        GM --> Chips[ChipManagerService]
        GM --> Eval[HandEvaluator]
        GM --> NA[INetworkAdapter]
    end

    subgraph States
        SMC --> S1[PreFlop]
        SMC --> S2[Flop]
        SMC --> S3[Turn]
        SMC --> S4[River]
        SMC --> S5[Showdown]
    end

    subgraph Communication
        GM -- Broadcasts --> EB[EventBus]
    end

    subgraph Visuals_and_UI
        EB -- Listens --> UI[UIManager]
        EB -- Listens --> TV[Table3DVisuals]
        UI -- "Action Request" --> NA
    end

    subgraph Data_Configuration
        SO1[GameSettingsSO] --> GM
        SO2[CardDatabaseSO] --> UI
    end
```

## Scalability Notes:
1. **Network**: Swap `LocalNetworkAdapter` with `PhotonNetworkAdapter` to go online.
2. **Visuals**: `Table3DVisuals` is decoupled; can be swapped for a 2D view by just changing the listener.
3. **Rules**: `GameSettingsSO` allows for different poker variants (e.g. adjust Blinds).
