# TestProjectJune

## Project Info

- Unity version: `6000.0.60f1`
- Main third-party/runtime packages: Addressables, Zenject, UniTask, DOTween, Eazy Sound Manager, URP 2D

## Project Structure

- `Assets/Scenes` - Unity scenes used by the project.
- `Assets/Scripts/Infrastructure` - application bootstrap, state machines, scene loading, services, UI roots, save/load, asset management, sound, and dependency injection setup.
- `Assets/Scripts/Gameplay/Level` - level catalog loading, level runtime models, figure creation, path components, tracing interaction, pointer logic, and presentation flow.
- `Assets/Scripts/Gameplay/LevelMenu` - level selection UI and category/level views.
- `Assets/Scripts/Gameplay/Tips` - gameplay tutorial/tip logic such as finger and sound hints.
- `Assets/Scripts/Utilities` - shared extensions, pooling, UI helpers, and camera/canvas fitting helpers.
- `Assets/AddressableAssets/InBuild` - addressable content used at runtime: levels catalog, UI, sounds, sprites/atlases, static data, tutorials, and figure assets.
- `Assets/Configs/Levels` - JSON level definitions grouped by type: `Letter`, `Number`, and `Shape`.
- `Assets/Graphics/InBuild/Gameplay/Levels` - source sprites for letters, numbers, and shapes.
- `Assets/Prefabs` - UI and gameplay prefabs.
- `Assets/Plugins` - imported plugins and third-party libraries.
- `ProjectSettings` and `Packages` - Unity project settings and package dependencies.

## Entry Points

- Start scene: `Assets/Scenes/BootstrapScene.unity`
- Build settings currently include:
  - `BootstrapScene` enabled
  - `LoadingScene` enabled
  - `Gameplay` present but disabled
  - `LevelsMenu` present but disabled
- Runtime bootstrap starts from `Assets/Scripts/Infrastructure/GameRunner.cs`.
- `GameRunner` creates `GameBootstrapper` if one does not already exist.
- `GameBootstrapper` registers the global game states and enters `GameBootstrapState`.
- `GameBootstrapState` initializes services, Addressables/static data, save/load, sound, sprite atlases, UI, and then opens the levels menu through `LevelsMenuLoadState`.
- Selecting a level moves the app through `GameplayLoadState` into the gameplay scene and loads the selected level through the level catalog/factory flow.

Main gameplay flow:

1. `BootstrapScene`
2. `GameRunner`
3. `GameBootstrapper`
4. `GameBootstrapState`
5. `LevelsMenuLoadState` / `LevelsMenuState`
6. `GameplayLoadState` / `GameplayState`
7. Scene-level gameplay states under `Assets/Scripts/Infrastructure/Gameplay/States`

## Level Creation Editor

The project includes a custom editor for creating and updating figure levels:

- Unity menu path: `Tools -> Level -> Figure Level Editor`
- Editor script: `Assets/Scripts/Gameplay/Level/Figure/Editor/FigureLevelEditorWindow.cs`

What the editor supports:

- Create levels for `Letter`, `Number`, and `Shape` figure types.
- Select a sprite and sync the `Figure ID` from the sprite name.
- Configure level ID, figure type, view color, and save path.
- Add multiple paths per figure.
- Use linear or bezier path types.
- Edit path points directly in the editor window or through Scene View handles/gizmos.
- Use helper generators for circles and evenly distributed linear points.
- Preview the generated figure/path setup in the scene.
- Import an existing level JSON.
- Save a level JSON into `Assets/Configs/Levels/<FigureType>`.
- Automatically update `Assets/AddressableAssets/InBuild/Levels/LevelCatalog.asset` after saving.

Typical level creation flow:

1. Open `Tools -> Level -> Figure Level Editor`.
2. Choose `Figure Type`.
3. Select the sprite for the figure.
4. Add and edit paths/points.
5. Use `Create Path Line With Gizmo` to preview and adjust points in Scene View.
6. Click `Save Level`.
7. The JSON level file is created/updated and registered in the level catalog.
