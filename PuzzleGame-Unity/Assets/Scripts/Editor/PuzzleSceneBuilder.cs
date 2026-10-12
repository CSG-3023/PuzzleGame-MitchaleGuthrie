/************************************************************
 * COPYRIGHT:    2026
 * PROJECT:      Puzzle Game
 * FILE NAME:    PuzzleSceneBuilder.cs
 * DESCRIPTION:  Editor tool that generates the materials, prefabs, and
 *               scenes (Main Menu placeholder, Level01, EndScene) for the
 *               puzzle prototype and registers them in Build Settings.
 *               Run from: Tools > Puzzle Game > Build All Scenes.
 *
 * REVISION HISTORY:
 * Date [YYYY/MM/DD] | Author | Comments
 * ------------------------------------------------------------
 * 2026/10/12 | Mitchale Guthrie | Created class
 *
 ************************************************************/

using System.IO;
using System.Linq;
using CSG.Puzzle.Behaviours;
using CSG.Puzzle.Components;
using CSG.Puzzle.Player;
using ProfessorAkram.SimpleUnityHierarchyFolder;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace CSG.Puzzle.EditorTools
{
    /// <summary>
    /// Builds the puzzle prototype's assets and scenes from code so the level
    /// can be regenerated or tweaked consistently.
    /// </summary>
    public static class PuzzleSceneBuilder
    {
        #region Constants

        /// <summary>Menu path for the builder commands.</summary>
        private const string MENU_PATH = "Tools/Puzzle Game/Build All Scenes";

        /// <summary>Folder for generated scenes.</summary>
        private const string SCENES_FOLDER = "Assets/Scenes";

        /// <summary>Folder for generated materials.</summary>
        private const string MATERIALS_FOLDER = "Assets/Materials";

        /// <summary>Folder for generated physics materials.</summary>
        private const string PHYSICS_FOLDER = "Assets/Materials/Physics";

        /// <summary>Folder for generated prefabs.</summary>
        private const string PREFABS_FOLDER = "Assets/Prefabs";

        /// <summary>Input actions asset containing Player/Move.</summary>
        private const string INPUT_ACTIONS_PATH = "Assets/InputSystem_Actions.inputactions";

        /// <summary>Scene names (must match Build Settings).</summary>
        private const string LEVEL_SCENE = "Level01";
        private const string END_SCENE = "EndScene";
        private const string DEFAULT_MENU_SCENE = "MainMenu";

        /// <summary>Displayed game title.</summary>
        private const string GAME_TITLE = "Pressure Point";

        /// <summary>Built-in font used for all text.</summary>
        private const string FONT_NAME = "LegacyRuntime.ttf";

        /// <summary>Wall height and thickness in world units.</summary>
        private const float WALL_HEIGHT = 2f;
        private const float WALL_THICKNESS = 1f;

        /// <summary>Arena interior bounds (x: -HALF_WIDTH..HALF_WIDTH, z: MIN_Z..MAX_Z).</summary>
        private const float ARENA_HALF_WIDTH = 10f;
        private const float ARENA_MIN_Z = -14f;
        private const float ARENA_MAX_Z = 20f;

        /// <summary>Z positions of the two dividing walls that contain the gates.</summary>
        private const float GATE_ONE_Z = -4f;
        private const float GATE_TWO_Z = 14f;

        /// <summary>Gate dimensions.</summary>
        private const float GATE_WIDTH = 4f;
        private const float GATE_THICKNESS = 0.6f;

        /// <summary>Default pressure plate width.</summary>
        private const float PLATE_SIZE = 2.5f;

        /// <summary>Scale applied to the heavy plate so it reads as "bigger / needs more".</summary>
        private const float HEAVY_PLATE_SCALE = 1.3f;

        /// <summary>Crate sizes and masses.</summary>
        private const float LIGHT_CRATE_SIZE = 1.2f;
        private const float LIGHT_CRATE_MASS = 1f;
        private const float HEAVY_CRATE_SIZE = 1.6f;
        private const float HEAVY_CRATE_MASS = 2f;

        /// <summary>Damping that makes crates stop soon after being pushed.</summary>
        private const float CRATE_DAMPING = 4f;

        /// <summary>Player capsule scale and mass.</summary>
        private const float PLAYER_SCALE = 0.8f;
        private const float PLAYER_MASS = 1f;

        /// <summary>TextMesh sizing: world height is roughly characterSize * fontSize / 10.</summary>
        private const float TEXT_CHARACTER_SIZE = 0.1f;
        private const int LABEL_FONT_SIZE = 70;
        private const int HINT_FONT_SIZE = 55;

        /// <summary>Lifts floor text slightly to avoid z-fighting.</summary>
        private const float FLOOR_TEXT_HEIGHT = 0.03f;

        /// <summary>Reference resolution for UI scaling.</summary>
        private static readonly Vector2 UI_REFERENCE_RESOLUTION = new Vector2(1920f, 1080f);

        #endregion

        #region Fields & Properties

        /// <summary>
        /// Generated materials shared by the scene builders.
        /// </summary>
        private class MaterialSet
        {
            public Material Floor, Wall, Player, CrateLight, CrateHeavy, Plate, Gate, Exit;
            public PhysicsMaterial Slide, Frictionless;
        } //end MaterialSet

        /// <summary>
        /// Generated prefabs used to populate Level01.
        /// </summary>
        private class PrefabSet
        {
            public GameObject Player, CrateLight, CrateHeavy, Plate, Gate;
        } //end PrefabSet

        /// <summary>
        /// Built-in font used by every generated text element.
        /// </summary>
        private static Font BuiltinFont => Resources.GetBuiltinResource<Font>(FONT_NAME);

        #endregion

        #region Feature Logic

        /// <summary>
        /// Generates all assets and scenes, then registers the scenes in Build Settings.
        /// </summary>
        [MenuItem(MENU_PATH)]
        public static void BuildAllScenes()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

            bool confirmed = EditorUtility.DisplayDialog(
                "Build Puzzle Scenes",
                $"This will create/overwrite {LEVEL_SCENE}, {END_SCENE}, and the generated materials and prefabs.\n\n" +
                $"A placeholder {DEFAULT_MENU_SCENE} is only created if no Main Menu scene exists.",
                "Build", "Cancel");
            if (!confirmed) return;

            EnsureFolder(SCENES_FOLDER);
            EnsureFolder(MATERIALS_FOLDER);
            EnsureFolder(PHYSICS_FOLDER);
            EnsureFolder(PREFABS_FOLDER);

            MaterialSet materials = CreateMaterials();
            PrefabSet prefabs = CreatePrefabs(materials);

            string menuPath = FindOrCreateMainMenu();
            string menuName = Path.GetFileNameWithoutExtension(menuPath);

            string levelPath = BuildLevelScene(materials, prefabs);
            string endPath = BuildEndScene(menuName);

            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(menuPath, true),
                new EditorBuildSettingsScene(levelPath, true),
                new EditorBuildSettingsScene(endPath, true)
            };

            AssetDatabase.SaveAssets();
            EditorSceneManager.OpenScene(levelPath);
            Debug.Log($"Puzzle scenes built: {menuPath}, {levelPath}, {endPath}");
        } //end BuildAllScenes()

        #endregion

        #region Helpers - Assets

        /// <summary>
        /// Creates (or updates) every material used by the level.
        /// </summary>
        /// <returns>The generated material set.</returns>
        private static MaterialSet CreateMaterials()
        {
            return new MaterialSet
            {
                Floor = CreateMaterial("M_Floor", new Color(0.78f, 0.80f, 0.84f)),
                Wall = CreateMaterial("M_Wall", new Color(0.35f, 0.38f, 0.46f)),
                Player = CreateMaterial("M_Player", new Color(0.20f, 0.55f, 1.00f)),
                CrateLight = CreateMaterial("M_CrateLight", new Color(0.86f, 0.66f, 0.40f)),
                CrateHeavy = CreateMaterial("M_CrateHeavy", new Color(0.45f, 0.30f, 0.20f)),
                Plate = CreateMaterial("M_PressurePlate", new Color(0.85f, 0.25f, 0.25f)),
                Gate = CreateMaterial("M_Gate", new Color(0.85f, 0.25f, 0.25f)),
                Exit = CreateMaterial("M_Exit", new Color(0.30f, 0.90f, 0.45f)),
                Slide = CreatePhysicsMaterial("PM_Slide", 0.2f),
                Frictionless = CreatePhysicsMaterial("PM_Frictionless", 0f)
            };
        } //end CreateMaterials()

        /// <summary>
        /// Loads or creates a URP Lit material with the given color.
        /// </summary>
        private static Material CreateMaterial(string assetName, Color color)
        {
            string path = $"{MATERIALS_FOLDER}/{assetName}.mat";
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);

            if (material == null)
            {
                Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
                material = new Material(shader);
                AssetDatabase.CreateAsset(material, path);
            } //end if(material == null)

            material.color = color;
            if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", color);
            EditorUtility.SetDirty(material);
            return material;
        } //end CreateMaterial(string, Color)

        /// <summary>
        /// Loads or creates a physics material with the given friction.
        /// </summary>
        private static PhysicsMaterial CreatePhysicsMaterial(string assetName, float friction)
        {
            string path = $"{PHYSICS_FOLDER}/{assetName}.physicsMaterial";
            PhysicsMaterial material = AssetDatabase.LoadAssetAtPath<PhysicsMaterial>(path);

            if (material == null)
            {
                material = new PhysicsMaterial(assetName);
                AssetDatabase.CreateAsset(material, path);
            } //end if(material == null)

            material.staticFriction = friction;
            material.dynamicFriction = friction;
            material.bounciness = 0f;
            material.frictionCombine = PhysicsMaterialCombine.Minimum;
            EditorUtility.SetDirty(material);
            return material;
        } //end CreatePhysicsMaterial(string, float)

        /// <summary>
        /// Creates every prefab used in Level01.
        /// </summary>
        private static PrefabSet CreatePrefabs(MaterialSet materials)
        {
            return new PrefabSet
            {
                Player = CreatePlayerPrefab(materials),
                CrateLight = CreateCratePrefab("PF_CrateLight", LIGHT_CRATE_SIZE, LIGHT_CRATE_MASS, materials.CrateLight, materials.Slide),
                CrateHeavy = CreateCratePrefab("PF_CrateHeavy", HEAVY_CRATE_SIZE, HEAVY_CRATE_MASS, materials.CrateHeavy, materials.Slide),
                Plate = CreatePlatePrefab(materials),
                Gate = CreateGatePrefab(materials)
            };
        } //end CreatePrefabs(MaterialSet)

        /// <summary>
        /// Builds the player prefab: capsule body, facing marker, Rigidbody, and controller.
        /// </summary>
        private static GameObject CreatePlayerPrefab(MaterialSet materials)
        {
            GameObject player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            player.name = "Player";
            player.tag = "Player";
            player.transform.localScale = Vector3.one * PLAYER_SCALE;
            player.GetComponent<Renderer>().sharedMaterial = materials.Player;
            player.GetComponent<Collider>().sharedMaterial = materials.Frictionless;

            // Small "visor" so players can see which way they face
            CreateVisual(PrimitiveType.Cube, "Visor", player.transform,
                new Vector3(0f, 0.45f, 0.4f), new Vector3(0.6f, 0.25f, 0.3f), materials.Wall);

            Rigidbody body = player.AddComponent<Rigidbody>();
            body.mass = PLAYER_MASS;
            body.constraints = RigidbodyConstraints.FreezeRotation;
            body.interpolation = RigidbodyInterpolation.Interpolate;
            body.collisionDetectionMode = CollisionDetectionMode.Continuous;

            PlayerController controller = player.AddComponent<PlayerController>();
            SetObject(controller, "_moveActionReference", FindMoveActionReference());

            return SavePrefab(player, "PF_Player");
        } //end CreatePlayerPrefab(MaterialSet)

        /// <summary>
        /// Builds a pushable crate prefab with its weight printed on top.
        /// </summary>
        private static GameObject CreateCratePrefab(string assetName, float size, float mass, Material material, PhysicsMaterial physicsMaterial)
        {
            GameObject crate = GameObject.CreatePrimitive(PrimitiveType.Cube);
            crate.name = assetName.Replace("PF_", string.Empty);
            crate.transform.localScale = Vector3.one * size;
            crate.GetComponent<Renderer>().sharedMaterial = material;
            crate.GetComponent<Collider>().sharedMaterial = physicsMaterial;

            Rigidbody body = crate.AddComponent<Rigidbody>();
            body.mass = mass;
            body.linearDamping = CRATE_DAMPING;
            body.constraints = RigidbodyConstraints.FreezeRotation;
            body.interpolation = RigidbodyInterpolation.Interpolate;

            // Weight label on the top face; scale is divided out so text size is consistent
            TextMesh label = CreateTextMesh("WeightLabel", crate.transform, mass.ToString("0"), LABEL_FONT_SIZE, Color.white);
            label.transform.localPosition = new Vector3(0f, 0.51f, 0f);
            label.characterSize = TEXT_CHARACTER_SIZE / size;

            return SavePrefab(crate, assetName);
        } //end CreateCratePrefab(...)

        /// <summary>
        /// Builds the pressure plate prefab: trigger sensor, flat visual, label, and feedback.
        /// </summary>
        private static GameObject CreatePlatePrefab(MaterialSet materials)
        {
            GameObject plate = new GameObject("PressurePlate");

            BoxCollider trigger = plate.AddComponent<BoxCollider>();
            trigger.isTrigger = true;
            trigger.center = new Vector3(0f, 0.75f, 0f);
            trigger.size = new Vector3(PLATE_SIZE * 0.9f, 1.5f, PLATE_SIZE * 0.9f);
            plate.AddComponent<WeightSensor>();

            // Flat visual with no collider so crates slide over it freely
            GameObject visual = CreateVisual(PrimitiveType.Cube, "Visual", plate.transform,
                new Vector3(0f, 0.01f, 0f), new Vector3(PLATE_SIZE, 0.04f, PLATE_SIZE), materials.Plate);

            TextMesh label = CreateTextMesh("WeightLabel", plate.transform, "0 / 1", LABEL_FONT_SIZE, Color.white);
            label.transform.localPosition = new Vector3(0f, FLOOR_TEXT_HEIGHT, -(PLATE_SIZE * 0.5f + 0.5f));

            ColorFeedback feedback = plate.AddComponent<ColorFeedback>();
            SetObjectArray(feedback, "_renderers", visual.GetComponent<Renderer>());

            TonePlayer tones = AddTonePlayer(plate);

            PressurePlate pressurePlate = plate.AddComponent<PressurePlate>();
            SetObject(pressurePlate, "_colorFeedback", feedback);
            SetObject(pressurePlate, "_tonePlayer", tones);
            SetObject(pressurePlate, "_weightLabel", label);

            return SavePrefab(plate, "PF_PressurePlate");
        } //end CreatePlatePrefab(MaterialSet)

        /// <summary>
        /// Builds the gate prefab: a wall segment that sinks into the floor when opened.
        /// </summary>
        private static GameObject CreateGatePrefab(MaterialSet materials)
        {
            GameObject gate = GameObject.CreatePrimitive(PrimitiveType.Cube);
            gate.name = "Gate";
            gate.transform.localScale = new Vector3(GATE_WIDTH, WALL_HEIGHT, GATE_THICKNESS);
            gate.GetComponent<Renderer>().sharedMaterial = materials.Gate;

            Rigidbody body = gate.AddComponent<Rigidbody>();
            body.isKinematic = true;
            body.interpolation = RigidbodyInterpolation.Interpolate;

            SlidingMover mover = gate.AddComponent<SlidingMover>();
            SetVector3(mover, "_openOffset", new Vector3(0f, -(WALL_HEIGHT + 0.05f), 0f));

            ColorFeedback feedback = gate.AddComponent<ColorFeedback>();
            TonePlayer tones = AddTonePlayer(gate);

            Gate gateBehaviour = gate.AddComponent<Gate>();
            SetObject(gateBehaviour, "_colorFeedback", feedback);
            SetObject(gateBehaviour, "_tonePlayer", tones);

            return SavePrefab(gate, "PF_Gate");
        } //end CreateGatePrefab(MaterialSet)

        /// <summary>
        /// Saves a temporary scene object as a prefab asset and destroys the temporary object.
        /// </summary>
        private static GameObject SavePrefab(GameObject source, string assetName)
        {
            string path = $"{PREFABS_FOLDER}/{assetName}.prefab";
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(source, path);
            Object.DestroyImmediate(source);
            return prefab;
        } //end SavePrefab(GameObject, string)

        /// <summary>
        /// Finds the Player/Move InputActionReference inside the project's input actions asset.
        /// </summary>
        private static InputActionReference FindMoveActionReference()
        {
            return AssetDatabase.LoadAllAssetsAtPath(INPUT_ACTIONS_PATH)
                .OfType<InputActionReference>()
                .FirstOrDefault(reference => reference.action != null
                                             && reference.action.name == "Move"
                                             && reference.action.actionMap?.name == "Player");
        } //end FindMoveActionReference()

        #endregion

        #region Helpers - Scenes

        /// <summary>
        /// Uses an existing Main Menu scene (e.g. from the provided package) or creates a placeholder.
        /// </summary>
        /// <returns>Asset path of the menu scene.</returns>
        private static string FindOrCreateMainMenu()
        {
            string existing = AssetDatabase.FindAssets("t:Scene Menu")
                .Select(AssetDatabase.GUIDToAssetPath)
                .Where(path => path.StartsWith("Assets/"))
                .FirstOrDefault(path => Path.GetFileNameWithoutExtension(path).Contains("Menu"));
            if (!string.IsNullOrEmpty(existing)) return existing;

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            CreateUICamera();
            CreateEventSystem();

            GameObject loaderObject = new GameObject("SceneLoader");
            SceneLoader loader = loaderObject.AddComponent<SceneLoader>();

            Canvas canvas = CreateCanvas("MainMenuCanvas");
            CreateUIText(canvas.transform, "Title", GAME_TITLE, 110, new Vector2(0f, 220f), new Vector2(1400f, 160f));
            CreateUIText(canvas.transform, "Tagline", "Push. Weigh. Discover.", 44, new Vector2(0f, 110f), new Vector2(1400f, 80f));
            CreateButton(canvas.transform, "PlayButton", "Play", new Vector2(0f, -40f), loader, LEVEL_SCENE);
            CreateUIText(canvas.transform, "Note", "(Placeholder menu - replace with the provided Main Menu package)", 24,
                new Vector2(0f, -400f), new Vector2(1400f, 50f));

            string path = $"{SCENES_FOLDER}/{DEFAULT_MENU_SCENE}.unity";
            EditorSceneManager.SaveScene(scene, path);
            return path;
        } //end FindOrCreateMainMenu()

        /// <summary>
        /// Builds the playable puzzle level.
        /// </summary>
        /// <returns>Asset path of the saved scene.</returns>
        private static string BuildLevelScene(MaterialSet materials, PrefabSet prefabs)
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.5f, 0.5f, 0.55f);

            Transform managers = CreateFolder("Managers", HierarchyFolder.ColorName.Red);
            Transform environment = CreateFolder("Environment", HierarchyFolder.ColorName.Green);
            Transform puzzle = CreateFolder("Puzzle", HierarchyFolder.ColorName.Orange);
            Transform actors = CreateFolder("Actors", HierarchyFolder.ColorName.Blue);
            Transform lights = CreateFolder("Lights", HierarchyFolder.ColorName.Yellow);
            Transform cameras = CreateFolder("Cameras", HierarchyFolder.ColorName.Cyan);
            Transform ui = CreateFolder("UI", HierarchyFolder.ColorName.Purple);

            CreateDirectionalLight(lights);
            BuildArena(environment, materials);

            // ---- Room 1: teaches "weight on a plate opens a gate" ----
            PressurePlate plateOne = PlacePlate(prefabs, puzzle, "PressurePlate_01", new Vector3(5f, 0f, -9f), LIGHT_CRATE_MASS, 1f);
            PlacePrefab(prefabs.CrateLight, puzzle, "CrateLight_01", new Vector3(-5f, LIGHT_CRATE_SIZE * 0.5f, -9f));
            Gate gateOne = PlaceGate(prefabs, puzzle, "Gate_01", GATE_ONE_Z, plateOne);
            CreateFloorText(environment, "Hint_Room1", "Plates open gates.", new Vector3(0f, FLOOR_TEXT_HEIGHT, -6f));

            // ---- Room 2: two plates, one needing double weight; several valid solutions ----
            PressurePlate plateTwo = PlacePlate(prefabs, puzzle, "PressurePlate_02", new Vector3(-6f, 0f, 9f), LIGHT_CRATE_MASS, 1f);
            PressurePlate plateHeavy = PlacePlate(prefabs, puzzle, "PressurePlate_03_Heavy", new Vector3(6f, 0f, 9f), HEAVY_CRATE_MASS, HEAVY_PLATE_SCALE);
            PlacePrefab(prefabs.CrateHeavy, puzzle, "CrateHeavy_01", new Vector3(-4f, HEAVY_CRATE_SIZE * 0.5f, 1.5f));
            PlacePrefab(prefabs.CrateLight, puzzle, "CrateLight_02", new Vector3(4.5f, LIGHT_CRATE_SIZE * 0.5f, 1.5f));
            PlacePrefab(prefabs.CrateLight, puzzle, "CrateLight_03", new Vector3(0f, LIGHT_CRATE_SIZE * 0.5f, 6f));
            Gate gateTwo = PlaceGate(prefabs, puzzle, "Gate_02", GATE_TWO_Z, plateTwo, plateHeavy);
            CreateFloorText(environment, "Hint_Room2", "Some plates need more weight...", new Vector3(0f, FLOOR_TEXT_HEIGHT, 12f));

            // ---- Exit ----
            TriggerZone exitZone = CreateExit(puzzle, materials);

            // ---- Player & camera ----
            GameObject player = PlacePrefab(prefabs.Player, actors, "Player", new Vector3(0f, PLAYER_SCALE + 0.05f, -11f));
            CreateFollowCamera(cameras, player.transform);

            // ---- Managers ----
            GameObject managerObject = new GameObject("PuzzleManager");
            managerObject.transform.SetParent(managers);
            SceneLoader endLoader = managerObject.AddComponent<SceneLoader>();
            SetString(endLoader, "_targetSceneName", END_SCENE);
            TonePlayer victoryTones = AddTonePlayer(managerObject);
            PuzzleManager manager = managerObject.AddComponent<PuzzleManager>();
            SetObjectArray(manager, "_requiredGates", gateOne, gateTwo);
            SetObject(manager, "_exitZone", exitZone);
            SetObject(manager, "_tonePlayer", victoryTones);

            GameObject restarterObject = new GameObject("LevelRestarter");
            restarterObject.transform.SetParent(managers);
            restarterObject.AddComponent<SceneLoader>();
            restarterObject.AddComponent<LevelRestarter>();

            // ---- HUD ----
            Canvas hud = CreateCanvas("HUD");
            hud.transform.SetParent(ui);
            Text goal = CreateUIText(hud.transform, "GoalText", "Goal: open the gates and reach the EXIT", 40, new Vector2(0f, -50f), new Vector2(1600f, 70f));
            AnchorTo(goal.rectTransform, new Vector2(0.5f, 1f));
            Text controls = CreateUIText(hud.transform, "ControlsText", "WASD  Move      Mouse  Look around      R  Restart", 30, new Vector2(0f, 40f), new Vector2(1600f, 60f));
            AnchorTo(controls.rectTransform, new Vector2(0.5f, 0f));

            string path = $"{SCENES_FOLDER}/{LEVEL_SCENE}.unity";
            EditorSceneManager.SaveScene(scene, path);
            return path;
        } //end BuildLevelScene(MaterialSet, PrefabSet)

        /// <summary>
        /// Builds the End Scene with a success message and restart / menu buttons.
        /// </summary>
        /// <param name="menuSceneName">Name of the Main Menu scene for the menu button.</param>
        /// <returns>Asset path of the saved scene.</returns>
        private static string BuildEndScene(string menuSceneName)
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            CreateUICamera();
            CreateEventSystem();

            GameObject loaderObject = new GameObject("SceneLoader");
            SceneLoader loader = loaderObject.AddComponent<SceneLoader>();

            Canvas canvas = CreateCanvas("EndCanvas");
            CreateUIText(canvas.transform, "Title", "Puzzle Solved!", 110, new Vector2(0f, 240f), new Vector2(1400f, 160f));
            CreateUIText(canvas.transform, "Subtitle", "You figured out how weight opens the way.", 44, new Vector2(0f, 130f), new Vector2(1400f, 80f));
            Text time = CreateUIText(canvas.transform, "CompletionTime", string.Empty, 36, new Vector2(0f, 50f), new Vector2(1400f, 60f));
            time.gameObject.AddComponent<CompletionTimeDisplay>();

            CreateButton(canvas.transform, "RestartButton", "Play Again", new Vector2(0f, -80f), loader, LEVEL_SCENE);
            CreateButton(canvas.transform, "MainMenuButton", "Main Menu", new Vector2(0f, -190f), loader, menuSceneName);

            string path = $"{SCENES_FOLDER}/{END_SCENE}.unity";
            EditorSceneManager.SaveScene(scene, path);
            return path;
        } //end BuildEndScene(string)

        /// <summary>
        /// Builds the floor, outer walls, and the two dividing walls with gate gaps.
        /// </summary>
        private static void BuildArena(Transform parent, MaterialSet materials)
        {
            float width = ARENA_HALF_WIDTH * 2f;
            float depth = ARENA_MAX_Z - ARENA_MIN_Z;
            float centerZ = (ARENA_MAX_Z + ARENA_MIN_Z) * 0.5f;
            float wallY = WALL_HEIGHT * 0.5f;
            float halfThick = WALL_THICKNESS * 0.5f;

            CreateBlock("Floor", parent, new Vector3(0f, -0.5f, centerZ), new Vector3(width + WALL_THICKNESS * 2f, 1f, depth + WALL_THICKNESS * 2f), materials.Floor);

            CreateBlock("Wall_West", parent, new Vector3(-ARENA_HALF_WIDTH - halfThick, wallY, centerZ), new Vector3(WALL_THICKNESS, WALL_HEIGHT, depth + WALL_THICKNESS * 2f), materials.Wall);
            CreateBlock("Wall_East", parent, new Vector3(ARENA_HALF_WIDTH + halfThick, wallY, centerZ), new Vector3(WALL_THICKNESS, WALL_HEIGHT, depth + WALL_THICKNESS * 2f), materials.Wall);
            CreateBlock("Wall_South", parent, new Vector3(0f, wallY, ARENA_MIN_Z - halfThick), new Vector3(width, WALL_HEIGHT, WALL_THICKNESS), materials.Wall);
            CreateBlock("Wall_North", parent, new Vector3(0f, wallY, ARENA_MAX_Z + halfThick), new Vector3(width, WALL_HEIGHT, WALL_THICKNESS), materials.Wall);

            CreateDividerWall(parent, "Divider_01", GATE_ONE_Z, materials.Wall);
            CreateDividerWall(parent, "Divider_02", GATE_TWO_Z, materials.Wall);
        } //end BuildArena(Transform, MaterialSet)

        /// <summary>
        /// Creates a dividing wall across the arena with a gap in the middle for a gate.
        /// </summary>
        private static void CreateDividerWall(Transform parent, string name, float z, Material material)
        {
            float halfGap = GATE_WIDTH * 0.5f;
            float segmentLength = ARENA_HALF_WIDTH - halfGap;
            float segmentCenter = halfGap + segmentLength * 0.5f;
            Vector3 size = new Vector3(segmentLength, WALL_HEIGHT, WALL_THICKNESS);

            CreateBlock($"{name}_West", parent, new Vector3(-segmentCenter, WALL_HEIGHT * 0.5f, z), size, material);
            CreateBlock($"{name}_East", parent, new Vector3(segmentCenter, WALL_HEIGHT * 0.5f, z), size, material);
        } //end CreateDividerWall(...)

        /// <summary>
        /// Places a pressure plate and sets its required weight.
        /// </summary>
        private static PressurePlate PlacePlate(PrefabSet prefabs, Transform parent, string name, Vector3 position, float requiredWeight, float scale)
        {
            GameObject plate = PlacePrefab(prefabs.Plate, parent, name, position);
            plate.transform.localScale = new Vector3(scale, 1f, scale);
            SetFloat(plate.GetComponent<WeightSensor>(), "_requiredWeight", requiredWeight);

            // Pre-fill the label so the scene view matches what players will see
            plate.GetComponentInChildren<TextMesh>().text = $"0 / {requiredWeight:0.#}";
            return plate.GetComponent<PressurePlate>();
        } //end PlacePlate(...)

        /// <summary>
        /// Places a gate in a divider wall and links its required plates.
        /// </summary>
        private static Gate PlaceGate(PrefabSet prefabs, Transform parent, string name, float z, params PressurePlate[] plates)
        {
            GameObject gate = PlacePrefab(prefabs.Gate, parent, name, new Vector3(0f, WALL_HEIGHT * 0.5f, z));
            Gate gateBehaviour = gate.GetComponent<Gate>();
            SetObjectArray(gateBehaviour, "_requiredPlates", plates.Cast<Object>().ToArray());
            return gateBehaviour;
        } //end PlaceGate(...)

        /// <summary>
        /// Creates the glowing exit pad and its trigger zone behind the final gate.
        /// </summary>
        private static TriggerZone CreateExit(Transform parent, MaterialSet materials)
        {
            float exitZ = (GATE_TWO_Z + ARENA_MAX_Z) * 0.5f + 0.5f;

            GameObject exit = new GameObject("Exit");
            exit.transform.SetParent(parent);
            exit.transform.position = new Vector3(0f, 0f, exitZ);

            BoxCollider trigger = exit.AddComponent<BoxCollider>();
            trigger.isTrigger = true;
            trigger.center = new Vector3(0f, 1f, 0f);
            trigger.size = new Vector3(4f, 2f, 3f);
            TriggerZone zone = exit.AddComponent<TriggerZone>();

            CreateVisual(PrimitiveType.Cube, "ExitPad", exit.transform, new Vector3(0f, 0.01f, 0f), new Vector3(4f, 0.04f, 3f), materials.Exit);
            CreateFloorText(exit.transform, "ExitLabel", "EXIT", new Vector3(0f, FLOOR_TEXT_HEIGHT * 2f, exitZ));
            return zone;
        } //end CreateExit(Transform, MaterialSet)

        /// <summary>
        /// Creates the top-down follow camera.
        /// </summary>
        private static void CreateFollowCamera(Transform parent, Transform target)
        {
            GameObject cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            cameraObject.transform.SetParent(parent);

            Camera camera = cameraObject.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.10f, 0.12f, 0.18f);
            cameraObject.AddComponent<AudioListener>();

            TopDownCamera follow = cameraObject.AddComponent<TopDownCamera>();
            SetObject(follow, "_target", target);

            // Match the runtime framing so the Scene/Game view looks right before Play
            Vector3 offset = new Vector3(0f, 14f, -10f);
            cameraObject.transform.position = target.position + offset;
            cameraObject.transform.LookAt(target.position);
        } //end CreateFollowCamera(Transform, Transform)

        /// <summary>
        /// Creates the main directional light.
        /// </summary>
        private static void CreateDirectionalLight(Transform parent)
        {
            GameObject lightObject = new GameObject("Directional Light");
            lightObject.transform.SetParent(parent);
            lightObject.transform.rotation = Quaternion.Euler(55f, -30f, 0f);

            Light light = lightObject.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.2f;
            light.color = new Color(1f, 0.96f, 0.88f);
            light.shadows = LightShadows.Soft;
        } //end CreateDirectionalLight(Transform)

        /// <summary>
        /// Creates a simple camera for UI-only scenes.
        /// </summary>
        private static void CreateUICamera()
        {
            GameObject cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            Camera camera = cameraObject.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.10f, 0.12f, 0.18f);
            cameraObject.AddComponent<AudioListener>();
        } //end CreateUICamera()

        /// <summary>
        /// Creates an EventSystem that uses the New Input System UI module.
        /// </summary>
        private static void CreateEventSystem()
        {
            GameObject eventSystem = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            eventSystem.GetComponent<InputSystemUIInputModule>().AssignDefaultActions();
        } //end CreateEventSystem()

        #endregion

        #region Helpers - Objects

        /// <summary>
        /// Creates an empty hierarchy folder object.
        /// </summary>
        private static Transform CreateFolder(string name, HierarchyFolder.ColorName color)
        {
            GameObject folder = new GameObject(name);
            folder.AddComponent<HierarchyFolder>().folderColor = color;
            return folder.transform;
        } //end CreateFolder(string, HierarchyFolder.ColorName)

        /// <summary>
        /// Creates a static cube block (floor or wall).
        /// </summary>
        private static GameObject CreateBlock(string name, Transform parent, Vector3 position, Vector3 scale, Material material)
        {
            GameObject block = GameObject.CreatePrimitive(PrimitiveType.Cube);
            block.name = name;
            block.transform.SetParent(parent);
            block.transform.position = position;
            block.transform.localScale = scale;
            block.GetComponent<Renderer>().sharedMaterial = material;
            block.isStatic = true;
            return block;
        } //end CreateBlock(...)

        /// <summary>
        /// Creates a child primitive used only for visuals (its collider is removed).
        /// </summary>
        private static GameObject CreateVisual(PrimitiveType type, string name, Transform parent, Vector3 localPosition, Vector3 localScale, Material material)
        {
            GameObject visual = GameObject.CreatePrimitive(type);
            visual.name = name;
            Object.DestroyImmediate(visual.GetComponent<Collider>());
            visual.transform.SetParent(parent, false);
            visual.transform.localPosition = localPosition;
            visual.transform.localScale = localScale;
            visual.GetComponent<Renderer>().sharedMaterial = material;
            return visual;
        } //end CreateVisual(...)

        /// <summary>
        /// Instantiates a prefab (keeping the prefab link) at a position.
        /// </summary>
        private static GameObject PlacePrefab(GameObject prefab, Transform parent, string name, Vector3 position)
        {
            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
            instance.name = name;
            instance.transform.position = position;
            return instance;
        } //end PlacePrefab(...)

        /// <summary>
        /// Adds an AudioSource and TonePlayer to an object.
        /// </summary>
        private static TonePlayer AddTonePlayer(GameObject target)
        {
            AudioSource source = target.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.spatialBlend = 0f;
            return target.AddComponent<TonePlayer>();
        } //end AddTonePlayer(GameObject)

        /// <summary>
        /// Creates a flat, upward-facing TextMesh.
        /// </summary>
        private static TextMesh CreateTextMesh(string name, Transform parent, string text, int fontSize, Color color)
        {
            GameObject textObject = new GameObject(name);
            textObject.transform.SetParent(parent, false);
            textObject.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);

            TextMesh mesh = textObject.AddComponent<TextMesh>();
            mesh.text = text;
            mesh.font = BuiltinFont;
            mesh.fontSize = fontSize;
            mesh.characterSize = TEXT_CHARACTER_SIZE;
            mesh.anchor = TextAnchor.MiddleCenter;
            mesh.alignment = TextAlignment.Center;
            mesh.color = color;
            textObject.GetComponent<MeshRenderer>().sharedMaterial = BuiltinFont.material;
            return mesh;
        } //end CreateTextMesh(...)

        /// <summary>
        /// Creates hint text lying on the floor at a world position.
        /// </summary>
        private static void CreateFloorText(Transform parent, string name, string text, Vector3 position)
        {
            TextMesh mesh = CreateTextMesh(name, parent, text, HINT_FONT_SIZE, new Color(0.15f, 0.18f, 0.25f));
            mesh.transform.position = position;
        } //end CreateFloorText(...)

        #endregion

        #region Helpers - UI

        /// <summary>
        /// Creates a screen-space canvas that scales with resolution.
        /// </summary>
        private static Canvas CreateCanvas(string name)
        {
            GameObject canvasObject = new GameObject(name, typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = UI_REFERENCE_RESOLUTION;
            scaler.matchWidthOrHeight = 0.5f;
            return canvas;
        } //end CreateCanvas(string)

        /// <summary>
        /// Creates a centered UI Text element with a drop shadow for readability.
        /// </summary>
        private static Text CreateUIText(Transform parent, string name, string content, int fontSize, Vector2 anchoredPosition, Vector2 size)
        {
            GameObject textObject = new GameObject(name, typeof(RectTransform), typeof(Text), typeof(Shadow));
            textObject.transform.SetParent(parent, false);

            RectTransform rect = textObject.GetComponent<RectTransform>();
            rect.sizeDelta = size;
            rect.anchoredPosition = anchoredPosition;

            Text text = textObject.GetComponent<Text>();
            text.text = content;
            text.font = BuiltinFont;
            text.fontSize = fontSize;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.white;
            text.raycastTarget = false;
            return text;
        } //end CreateUIText(...)

        /// <summary>
        /// Anchors and pivots a RectTransform to a point on the screen, keeping its offset.
        /// </summary>
        private static void AnchorTo(RectTransform rect, Vector2 anchor)
        {
            Vector2 position = rect.anchoredPosition;
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
        } //end AnchorTo(RectTransform, Vector2)

        /// <summary>
        /// Creates a UI button that loads a scene through a SceneLoader (persistent listener).
        /// </summary>
        private static void CreateButton(Transform parent, string name, string label, Vector2 anchoredPosition, SceneLoader loader, string sceneName)
        {
            DefaultControls.Resources resources = new DefaultControls.Resources
            {
                standard = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd")
            };

            GameObject buttonObject = DefaultControls.CreateButton(resources);
            buttonObject.name = name;
            buttonObject.transform.SetParent(parent, false);

            RectTransform rect = buttonObject.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(380f, 84f);
            rect.anchoredPosition = anchoredPosition;

            Text text = buttonObject.GetComponentInChildren<Text>();
            text.text = label;
            text.font = BuiltinFont;
            text.fontSize = 38;
            text.color = new Color(0.12f, 0.14f, 0.2f);

            Button button = buttonObject.GetComponent<Button>();
            UnityEventTools.AddStringPersistentListener(button.onClick, new UnityAction<string>(loader.LoadScene), sceneName);
        } //end CreateButton(...)

        #endregion

        #region Helpers - Serialized Fields

        /// <summary>Folder creation that also creates missing parents.</summary>
        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;

            string parent = Path.GetDirectoryName(path)?.Replace('\\', '/');
            if (!string.IsNullOrEmpty(parent)) EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, Path.GetFileName(path));
        } //end EnsureFolder(string)

        /// <summary>Sets a private serialized object reference field.</summary>
        private static void SetObject(Object target, string field, Object value)
        {
            SerializedObject serialized = new SerializedObject(target);
            serialized.FindProperty(field).objectReferenceValue = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        } //end SetObject(...)

        /// <summary>Sets a private serialized object reference array field.</summary>
        private static void SetObjectArray(Object target, string field, params Object[] values)
        {
            SerializedObject serialized = new SerializedObject(target);
            SerializedProperty property = serialized.FindProperty(field);
            property.arraySize = values.Length;
            for (int i = 0; i < values.Length; i++)
            {
                property.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
            } //end for
            serialized.ApplyModifiedPropertiesWithoutUndo();
        } //end SetObjectArray(...)

        /// <summary>Sets a private serialized float field.</summary>
        private static void SetFloat(Object target, string field, float value)
        {
            SerializedObject serialized = new SerializedObject(target);
            serialized.FindProperty(field).floatValue = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        } //end SetFloat(...)

        /// <summary>Sets a private serialized string field.</summary>
        private static void SetString(Object target, string field, string value)
        {
            SerializedObject serialized = new SerializedObject(target);
            serialized.FindProperty(field).stringValue = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        } //end SetString(...)

        /// <summary>Sets a private serialized Vector3 field.</summary>
        private static void SetVector3(Object target, string field, Vector3 value)
        {
            SerializedObject serialized = new SerializedObject(target);
            serialized.FindProperty(field).vector3Value = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        } //end SetVector3(...)

        #endregion

    } //end PuzzleSceneBuilder
} //end namespace CSG.Puzzle.EditorTools
