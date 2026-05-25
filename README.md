# Sphere Merge

## Gameplay Preview

https://github.com/user-attachments/assets/5d69d86f-760d-4cbd-a448-4ee4c1ac1391

Sphere Merge is a 2D Unity puzzle game prototype where the player releases colored glass spheres, connects matching groups, and clears each level before its hazards cause failure.

This project was built as a technical case study with a focus on maintainable gameplay code, dependency injection, event-driven communication, configurable content, and a small custom level creation tool.

## Gameplay

- Tap an available sphere to release it into the level.
- A released sphere unlocks the next sphere above it in the same column.
- Three connected selected spheres of the same color merge and disappear.
- A level is completed when all required merges are finished.
- Rope-based levels can fail when every tracked rope becomes overloaded and breaks.
- Ice obstacles block a sphere until enough selections melt them.

## Technical Highlights

### Dependency Injection with VContainer

The project uses **VContainer** to keep systems connected without relying on global manager lookups.

- `GameSceneLifetimeScope` registers scene-level services such as game state, level management, and UI.
- `LevelManager` creates and disposes each level instance.
- `LevelContext` provides registrations for the child level scope, including `SpheresManager` and `SpheresMergeManager`.

This setup keeps level-owned systems alive only while their level is active and makes dependencies explicit.

### Typed Event Bus

`EventBus` is a lightweight typed publish/subscribe system for gameplay messages that do not need direct references.

Main event flows:

| Event | Published By | Used By |
| --- | --- | --- |
| `SphereSelectedEvent` | `GlassSphere2D` | Column activation and ice obstacle behavior |
| `SpheresMergedEvent` | `SpheresMergeManager` | Merge objective and rope load cleanup |
| `MergeProgressChangedEvent` | `LevelMergeObjective` | HUD merge progress text |

The event bus is used for gameplay notifications, while VContainer is used for stable service dependencies.

### Game State Flow

The project contains a small state-driven game flow through `IGameStateService` and `GameManager`.

Available states:

```text
Initializing -> Playing -> LevelCompleted
                       -> LevelFailed
                       -> Paused
```

- `LevelManager` changes the state to `Playing` after a level is successfully created.
- `LevelMergeObjective` reports `LevelCompleted`.
- `LevelRopeFailureObjective` reports `LevelFailed`.
- `UIManager` listens to state changes and presents the correct end-level panel.

### Configurable Data with ScriptableObjects

Visual and gameplay tuning data is kept in ScriptableObjects instead of being hardcoded into gameplay components.

Examples:

- Sphere color palettes
- Intro animation settings
- Merge animation settings
- Liquid motion settings
- Cannot-select feedback animation settings
- Available obstacle prefab catalog

These assets live under `Assets/SO/Spheres` and make iteration possible without editing code.

## Core Systems

### Sphere Grid and Selection

| Script | Responsibility |
| --- | --- |
| `SpheresManager` | Stores grid data, sphere placement, shared palette assignment, and layout calculations. |
| `SphereColumnActivationController` | Enables the next playable sphere in each column after selection. |
| `SpheresIntroAnimator` | Plays level-entry sphere animation and starts initial selection availability. |
| `GlassSphere2D` | Owns one sphere's state, selection rules, physics behavior, and color presentation. |
| `TapSelectController` | Converts tap input into `ISelectable` interactions through 2D raycasts. |

The responsibilities are separated so grid storage, gameplay activation rules, and visual intro behavior can change independently.

### Merge System

| Script | Responsibility |
| --- | --- |
| `SphereContactSensor2D` | Reports physical contacts between selected spheres. |
| `SpheresMergeManager` | Finds connected same-color groups of three and runs their merge animation. |
| `LevelMergeObjective` | Tracks completed merges and completes the level objective. |

The merge manager only handles merging; level completion logic stays in the objective component.

### Rope Mechanic

| Script | Responsibility |
| --- | --- |
| `RopeGenerator2D` | Builds a physics-based rope from segments and updates its visible line. |
| `RopeSegment2D` | Represents one connected physical rope part. |
| `RopeLoadSensor2D` | Counts selected spheres currently resting on a rope. |
| `BreakableRope` | Breaks the rope after its load reaches the configured limit. |
| `RopeCapacityView` | Displays remaining rope capacity and warning feedback. |
| `LevelRopeFailureObjective` | Fails a level after every tracked rope is broken. |

The rope is a standalone level mechanic, separate from sphere obstacles.

### Obstacles and Visual Feedback

| Script | Responsibility |
| --- | --- |
| `IceObstacle` | Blocks sphere selection until its melt counter reaches zero. |
| `ObstacleBaseAbstract` | Common type for obstacles placed on sphere cells. |
| `SpriteLiquid2D` | Drives the liquid shader response based on sphere movement and collisions. |
| `GlassSphereVisual2D` | Plays feedback when a sphere cannot be selected. |

## Custom Level Editor

The project includes an editor tool for building sphere grids directly inside Unity.

`SpheresManagerInspector` exposes an **Edit Level** button that opens `SpheresManagerEditorWindow`.

The editor supports:

- Setting grid size and tile spacing
- Painting sphere colors into cells
- Placing or clearing obstacle prefabs
- Resizing an existing grid
- Clearing a grid
- Undo support and prefab/scene dirty tracking

`SpheresGridEditorOperations` contains the prefab instantiation, deletion, obstacle editing, undo, and dirty-state operations. This keeps the editor window focused on its UI and user interaction flow.

## Project Structure

```text
Assets/
|-- Prefabs/
|   |-- Levels/                 # Playable level prefabs
|   |-- Obstacle/               # Sphere obstacle prefabs
|   `-- Rope/                   # Rope mechanic prefabs
|-- Scenes/
|   `-- GameScene.unity         # Main gameplay scene
|-- SO/
|   `-- Spheres/                # ScriptableObject configuration assets
`-- Scripts/
    |-- Bootstrap/DI/           # VContainer scene registration
    |-- Editor/Spheres/         # Custom sphere grid level editor
    |-- Gameplay/
    |   |-- GameFlow/           # Game state service
    |   |-- Input/              # Tap selection input
    |   |-- Level/              # Loading, scope, and objectives
    |   |-- Rope/               # Breakable rope mechanic
    |   `-- Spheres/            # Grid, merge, obstacles, visuals
    |-- Presentation/UI/        # HUD, end panels, UI animations
    `-- Shared/Messaging/       # Typed event bus
```

## Tools and Packages

- Unity `6000.3.13f1`
- Universal Render Pipeline
- VContainer `1.18.0`
- PrimeTween
- Odin Inspector / Odin Serializer
- Lean Touch
- TextMeshPro

## Running the Project

1. Open the project in Unity `6000.3.13f1` or a compatible Unity 6 version.
2. Open `Assets/Scenes/GameScene.unity`.
3. Enter Play Mode.
4. Tap selectable spheres to merge matches and complete the level objectives.
