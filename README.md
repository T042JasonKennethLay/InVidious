# 👻 InVidious - Multiplayer Horror Game (Unity)

> *Rise to banish the darkness, or embrace it.*

**InVidious** is an asymmetric multiplayer horror game built in Unity and set in a
forgotten, fog-covered port. Players join as either an **Exorcist** or a **Ghost**
from Indonesian folklore (**Kuntilanak**, **Tuyul**, or **Wewe Gombel**).

- 🕯️ **Exorcists** search the map for clue papers, work out which ghost is haunting
  the port, and place the right banishment items on the ritual plates before dawn.
- 👻 **Ghosts** stay invisible, pass through walls, and haunt, disrupt, and hunt the
  Exorcists until the ritual fails.

## ✨ Features

### Game Modes
- **Singleplayer**: play as the Exorcist against a Ghost AI. The AI learns where you
  like to go, then hunts you with A* pathfinding.
- **Multiplayer (2–5 players)**: public and private lobbies, join by code, host kick,
  and a choice of 1 or 2 ghosts.

### Time Phases
Each match lasts 12 real minutes (11:00 PM → 5:00 AM in-game):

| Phase | In-game time | Ghost abilities |
|---|---|---|
| 🌫️ Wandering | 11 PM – 1 AM | Teleport, appear, scream |
| 🚪 Disturbing | 1 AM – 3 AM | Turn off lights, open and close doors and gates, knock items off the plates, special skill |
| 🩸 Hunting | 3 AM – 5 AM | Attack Exorcists, all skills (1-minute countdown starts at 4:30 AM) |

### Ghosts
| Ghost | Passive | Special Skill |
|---|---|---|
| Kuntilanak | Slows nearby players | Fly |
| Tuyul | Faster movement | Steal an Exorcist's held item |
| Wewe Gombel | Double damage | Kidnap an Exorcist |

### Exorcist Mechanics
- First- and third-person camera with collision handling
- Torch, 2-slot inventory, picking up and throwing items
- Climbing ladders, swimming with a breath meter, fall damage
- Med-kit healing for yourself and teammates
- Animations that change with health, plus a blood-splatter overlay

### Communication
- Proximity voice chat (Exorcists hear Exorcists, Ghosts hear Ghosts)
- Special items for talking to the ghost:
  - **Notebook** for text
  - **Spiritbox** for voice
  - **Ouija Board**, where the ghost spells out answers letter by letter

### Progression & Save
- EXP and levels; the Notebook, Spiritbox, and Ouija Board unlock at levels 4, 8, and 14
- Match and win statistics for each mode
- Encrypted JSON save file, autosaved every 4 minutes

## 🛠 Tech Stack
| Area | Tech |
|---|---|
| Engine | Unity (C#) |
| Networking | Netcode for GameObjects, Unity Transport (UTP) |
| Online services | Unity Gaming Services: Authentication, Lobby, Relay |
| UI | TextMeshPro, Unity UI |
| Rendering | Post-processing on every scene |

## 📂 Featured Scripts
| Script | Purpose |
|---|---|
| `Lobby - Copy.cs` | Create, list, and join lobbies (public/private, join by code), host kick, heartbeat and polling, Relay allocation |
| `StartHost - Copy.cs` | Host start-up and connection approval |
| `PlayerSpawner - Copy.cs` | Spawns every connected player's character on the network |
| `GameSessionManager - Copy.cs` | Keeps session state across scenes |
| `PlayerHealth.cs` | Networked health (NetworkVariable + ServerRpc), damage, healing, blood overlay |
| `ClientNetwork - Copy.cs` | Client-authoritative network transform |
| `GenderPrefab - Copy.cs` | Picks the male or female Exorcist prefab |
| `Data Manager.cs`, `DataPlayerLoad.cs`, `PassDataSource.cs` | Player data persistence and passing data between scenes |
| `Name.cs`, `nameLobby.cs` | Player nametags and lobby name display |
| `role - Copy.cs`, `value_setting.cs` | Role data and settings value display |
