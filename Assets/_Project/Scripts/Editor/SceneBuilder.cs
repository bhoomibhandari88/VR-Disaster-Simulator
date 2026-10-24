using System.Collections.Generic;
using RescueSim.Audio;
using RescueSim.Core;
using RescueSim.Hazards;
using RescueSim.Interaction;
using RescueSim.Player;
using RescueSim.Survivors;
using RescueSim.UI;
using TMPro;
using Unity.AI.Navigation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Movement;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Turning;
using UnityEngine.XR.Interaction.Toolkit.UI;
using static RescueSim.EditorTools.BuilderUtil;

namespace RescueSim.EditorTools
{
    /// <summary>
    /// One-click generator for every scene and asset the game needs. Safe to re-run: scenes and prefabs are regenerated,
    /// the original Tirgames asset pack and its sample scene are never modified.
    /// </summary>
    public static class SceneBuilder
    {
        const string PackScene = "Assets/TirgamesAssets/Factory/Scenes/FactoryNight.unity";
        const string RigPrefab = "Assets/Samples/XR Interaction Toolkit/3.6.1/Starter Assets/Prefabs/XR Origin (XR Rig).prefab";
        public const string MenuScenePath = ScenesDir + "/MainMenu.unity";
        public const string MissionScenePath = ScenesDir + "/FactoryRescue.unity";
        public const string MissionAssetPath = DataDir + "/Mission_NightShiftFactoryFire.asset";

        // Key world positions (the hall runs along Z, floor at Y=0).
        static readonly Vector3 PlayerStart = new Vector3(-11.4f, 0f, 24.5f);
        static readonly Vector3 BreakerPos = new Vector3(-2.95f, 1.15f, 22.6f);
        static readonly Vector3 GasPos = new Vector3(-13.5f, 0f, 11f);
        static readonly Vector3 CablePos = new Vector3(-7f, 0f, 4f);
        static readonly Vector3 FireAPos = new Vector3(-6.5f, 0f, -2.5f);
        static readonly Vector3 FireBPos = new Vector3(-14.5f, 0f, -10f);
        static readonly Vector3 GapCenter = new Vector3(-11.4f, 0f, -22f);
        static readonly Vector3 ExitPos = new Vector3(-11.4f, 0f, 29.5f);

        [MenuItem("Rescue Sim/Build/Everything")]
        public static void BuildEverything()
        {
            ResetCache();
            EnsureDir(ScenesDir); EnsureDir(DataDir); EnsureDir(PrefabsDir); EnsureDir(MaterialsDir);
            var mission = CreateMission();
            Prefabs.Fire(); Prefabs.GasLeak(); Prefabs.Survivor(); Prefabs.DebrisBlock();
            AssetDatabase.SaveAssets();
            BuildMenu(mission);
            BuildMission(mission);
            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(MenuScenePath, true),
                new EditorBuildSettingsScene(MissionScenePath, true)
            };
            AssetDatabase.SaveAssets();
            Debug.Log("[SceneBuilder] Done. Scenes: MainMenu, FactoryRescue (added to Build Settings).");
        }

        // ---- Mission data ---------------------------------------------------------------------

        static MissionDefinition CreateMission()
        {
            var m = AssetDatabase.LoadAssetAtPath<MissionDefinition>(MissionAssetPath);
            if (m == null) { m = ScriptableObject.CreateInstance<MissionDefinition>(); AssetDatabase.CreateAsset(m, MissionAssetPath); }
            m.missionId = "factory-fire-night-shift";
            m.title = "Night Shift: Factory Fire";
            m.tagline = "A transformer fire and a gas leak have trapped your crew in Hall 1.";
            m.briefing =
                "A transformer fire and a gas leak have started in Hall 1. Part of the partition wall has collapsed, and three workers are unaccounted for.\n\n" +
                "1. Cut main power at the breaker on the east wall before you spray the electrical fire.\n" +
                "2. Close the gas shutoff valve: hold GRIP on the valve wheel until it is fully shut.\n" +
                "3. Grab the extinguisher, squeeze TRIGGER and sweep the base of the flames.\n" +
                "4. Pull the fallen debris out of the collapsed passage.\n" +
                "5. Find every survivor, hold GRIP on them to check them, then lead them to the assembly point by the entrance.\n\n" +
                "Smoke and fumes build up exposure. Raise your left wrist to see time, exposure and the checklist.";
            m.timeLimitSeconds = 480f;
            m.exposureRecoveryPerSecond = 0.04f;
            m.objectives = new List<MissionDefinition.ObjectiveDefinition>
            {
                new MissionDefinition.ObjectiveDefinition { id = ObjectiveIds.Power, title = "Cut main power", hint = "Throw the main breaker on the east wall." },
                new MissionDefinition.ObjectiveDefinition { id = ObjectiveIds.Gas, title = "Close the gas valve", hint = "Hold grip on the valve wheel." },
                new MissionDefinition.ObjectiveDefinition { id = ObjectiveIds.Fire, title = "Extinguish both fires", hint = "Isolate power first for the electrical fire." },
                new MissionDefinition.ObjectiveDefinition { id = ObjectiveIds.Route, title = "Clear the collapsed passage", hint = "Grab debris and move it out of the gap." },
                new MissionDefinition.ObjectiveDefinition { id = ObjectiveIds.Locate, title = "Locate survivors", hint = "Look for the beacons and listen for whistles." },
                new MissionDefinition.ObjectiveDefinition { id = ObjectiveIds.Evacuate, title = "Evacuate everyone", hint = "Lead survivors to the assembly point." },
            };
            m.scoring = ScoreWeights.Default;
            EditorUtility.SetDirty(m);
            AssetDatabase.SaveAssetIfDirty(m);
            return m;
        }

        // ---- Common scene pieces --------------------------------------------------------------

        sealed class Rig
        {
            public GameObject root; public Camera cam; public Transform left, right;
            public ComfortVignette vignette; public ExposureOverlay overlay; public SceneTransitions transitions;
        }

        static Rig AddRig(Vector3 pos, float yaw)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(RigPrefab);
            if (prefab == null) throw new System.Exception("XRI Starter Assets rig prefab not found: " + RigPrefab);

            // Remove stray cameras/listeners from the source scene.
            foreach (var c in Object.FindObjectsByType<Camera>(FindObjectsInactive.Include, FindObjectsSortMode.None)) Object.DestroyImmediate(c.gameObject);

            var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            go.transform.SetPositionAndRotation(pos, Quaternion.Euler(0, yaw, 0));
            var r = new Rig { root = go, cam = go.GetComponentInChildren<Camera>(true) };
            foreach (var t in go.GetComponentsInChildren<Transform>(true))
            {
                if (t.name == "Left Controller" && r.left == null) r.left = t;
                if (t.name == "Right Controller" && r.right == null) r.right = t;
            }

            if (Object.FindFirstObjectByType<XRInteractionManager>() == null)
                new GameObject("XR Interaction Manager").AddComponent<XRInteractionManager>();
            if (Object.FindFirstObjectByType<EventSystem>() == null)
            {
                var es = new GameObject("EventSystem");
                es.AddComponent<EventSystem>();
                es.AddComponent<XRUIInputModule>();
            }

            go.AddComponent<XRInputProbe>();

            // Comfort: applies saved settings to the rig's locomotion providers.
            var comfort = go.AddComponent<ComfortApplier>();

            // Screen-space overlays are clip-space quads parented to the camera.
            var vigMat = Overlay("Mat_ComfortVignette", 1f, 3800);
            var hazeMat = Overlay("Mat_ExposureHaze", -1f, 3900);
            var fadeMat = Overlay("Mat_ScreenFade", -1f, 4000);
            var vigQ = OverlayQuad("ComfortVignette", r.cam.transform, vigMat);
            var hazeQ = OverlayQuad("ExposureHaze", r.cam.transform, hazeMat);
            var fadeQ = OverlayQuad("ScreenFade", r.cam.transform, fadeMat);

            r.vignette = vigQ.gameObject.AddComponent<ComfortVignette>();
            Ref(r.vignette, "ring", vigQ); Ref(r.vignette, "rigRoot", go.transform);
            r.overlay = fadeQ.gameObject.AddComponent<ExposureOverlay>();
            Ref(r.overlay, "haze", hazeQ); Ref(r.overlay, "fader", fadeQ);

            Ref(comfort, "snapTurn", go.GetComponentInChildren<SnapTurnProvider>(true));
            Ref(comfort, "continuousTurn", go.GetComponentInChildren<ContinuousTurnProvider>(true));
            Ref(comfort, "move", go.GetComponentInChildren<ContinuousMoveProvider>(true));
            Ref(comfort, "vignette", r.vignette);

            var systems = new GameObject("Systems");
            r.transitions = systems.AddComponent<SceneTransitions>();
            Ref(r.transitions, "overlay", r.overlay);
            return r;
        }

        static Renderer OverlayQuad(string name, Transform cam, Material mat)
        {
            var q = Prim(PrimitiveType.Quad, name, cam, new Vector3(0, 0, 0.3f), Vector3.one, mat);
            var rend = q.GetComponent<Renderer>();
            rend.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            rend.receiveShadows = false;
            rend.enabled = false;
            return rend;
        }

        static void SetSkybox()
        {
            foreach (var guid in AssetDatabase.FindAssets("SkyBoxIndustrial01Night t:Material"))
            {
                RenderSettings.skybox = AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(guid));
                break;
            }
        }

        static SpatialSound Ambient(string name, SpatialSound.Cue cue, float volume, Vector3 pos, bool is3D, float min = 5f, float max = 40f)
        {
            var go = new GameObject(name);
            go.transform.position = pos;
            var s = Sound(go, cue, volume, min, max);
            Val(s, "playOnAwake", true);
            Val(s, "spatialBlend", is3D ? 1f : 0f);
            return s;
        }

        // ---- Main menu ------------------------------------------------------------------------

        static void BuildMenu(MissionDefinition mission)
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            mission = AssetDatabase.LoadAssetAtPath<MissionDefinition>(MissionAssetPath);
            SetSkybox();
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.12f, 0.13f, 0.16f);

            var rig = AddRig(Vector3.zero, 0f);

            // Stage: floor, back wall, a few pack props and warm light.
            var floorMat = Lit("Mat_MenuFloor", new Color(0.13f, 0.13f, 0.14f), 0.35f);
            var wallMat = Lit("Mat_MenuWall", new Color(0.2f, 0.2f, 0.22f), 0.1f);
            var stage = new GameObject("Stage").transform;
            Prim(PrimitiveType.Cube, "Floor", stage, new Vector3(0, -0.05f, 4f), new Vector3(24f, 0.1f, 24f), floorMat, true);
            Prim(PrimitiveType.Cube, "BackWall", stage, new Vector3(0, 3f, 9f), new Vector3(24f, 6f, 0.3f), wallMat);
            Prim(PrimitiveType.Cube, "LeftWall", stage, new Vector3(-8f, 3f, 4f), new Vector3(0.3f, 6f, 24f), wallMat);
            Prim(PrimitiveType.Cube, "RightWall", stage, new Vector3(8f, 3f, 4f), new Vector3(0.3f, 6f, 24f), wallMat);
            PlaceProp(stage, "Barrel01a", new Vector3(-3.2f, 0f, 6.5f), 20f);
            PlaceProp(stage, "Barrel01b", new Vector3(-2.4f, 0f, 7.2f), 70f);
            PlaceProp(stage, "Barrel01c", new Vector3(3.4f, 0f, 6.8f), 120f);
            PlaceProp(stage, "Barrel01a", new Vector3(4.1f, 0f, 6.0f), 10f);
            PlaceProp(stage, "MetalCabinet01_1", new Vector3(6.2f, 0f, 7.6f), 190f);
            PlaceProp(stage, "PowerBox01_1", new Vector3(-6.3f, 0f, 7.9f), 180f);

            var warm = new GameObject("KeyLight").AddComponent<Light>();
            warm.type = LightType.Point; warm.color = new Color(1f, 0.7f, 0.4f); warm.intensity = 4f; warm.range = 14f; warm.shadows = LightShadows.None;
            warm.transform.position = new Vector3(0, 3.6f, 3.5f);
            var red = new GameObject("AlarmLight").AddComponent<Light>();
            red.type = LightType.Point; red.color = new Color(1f, 0.15f, 0.1f); red.intensity = 2.5f; red.range = 9f; red.shadows = LightShadows.None;
            red.transform.position = new Vector3(0, 2.6f, 8f);

            // Title on the back wall.
            var titleCanvas = UIFactory.CreateWorldCanvas("Title", new Vector2(1600, 300), stage, 0.004f, false);
            titleCanvas.transform.position = new Vector3(0, 4.3f, 8.8f);
            UIFactory.AddText(titleCanvas.transform, "FACTORY RESCUE VR", 160f, new Vector2(0, -10), new Vector2(1560, 200), TextAlignmentOptions.Center, UIFactory.Accent, FontStyles.Bold);
            UIFactory.AddText(titleCanvas.transform, "Fire and collapse response simulator", 60f, new Vector2(0, -210), new Vector2(1560, 80), TextAlignmentOptions.Center, UIFactory.TextDim);

            // Main panel.
            var uiRoot = new GameObject("UI").transform;
            var main = UIFactory.CreateWorldCanvas("MainPanel", new Vector2(1000, 760), uiRoot);
            var mainPanel = main.gameObject.AddComponent<VRPanel>();
            UIFactory.AddBackground(main.transform, UIFactory.Panel);
            UIFactory.AddText(main.transform, "MISSION", 30f, new Vector2(0, -30), new Vector2(900, 40), TextAlignmentOptions.Center, UIFactory.TextDim, FontStyles.Bold);
            var title = UIFactory.AddText(main.transform, "", 58f, new Vector2(0, -75), new Vector2(920, 80), TextAlignmentOptions.Center, UIFactory.Accent, FontStyles.Bold);
            var tag = UIFactory.AddText(main.transform, "", 34f, new Vector2(0, -170), new Vector2(880, 110), TextAlignmentOptions.Center, UIFactory.TextMain);
            UIFactory.AddText(main.transform, "Aim with either controller and squeeze the trigger to select.\nGrip grabs and holds. Left stick moves, right stick turns.", 27f,
                new Vector2(0, -300), new Vector2(880, 90), TextAlignmentOptions.Center, UIFactory.TextDim);
            var start = UIFactory.AddButton(main.transform, "Start Mission", new Vector2(0, -420), new Vector2(560, 100), null, UIFactory.AccentDim, 42f);
            var settingsBtn = UIFactory.AddButton(main.transform, "Settings", new Vector2(0, -540), new Vector2(560, 90), null, UIFactory.Button, 38f);
            var quit = UIFactory.AddButton(main.transform, "Quit", new Vector2(0, -650), new Vector2(560, 80), null, UIFactory.Button, 34f);

            var sc = UIFactory.CreateWorldCanvas("SettingsPanel", new Vector2(1000, 760), uiRoot);
            var settings = SettingsPanel.Build(sc);

            var menu = rig.transitions.gameObject.AddComponent<MainMenuController>();
            Ref(menu, "mission", mission); Ref(menu, "head", rig.cam.transform); Ref(menu, "transitions", rig.transitions);
            Ref(menu, "mainPanel", mainPanel); Ref(menu, "settingsPanel", settings);
            Ref(menu, "startButton", start); Ref(menu, "settingsButton", settingsBtn); Ref(menu, "quitButton", quit);
            Ref(menu, "missionTitle", title); Ref(menu, "missionTagline", tag);

            Ambient("AmbientHum", SpatialSound.Cue.AmbientHum, 0.22f, Vector3.zero, false);

            EditorSceneManager.SaveScene(scene, MenuScenePath);
        }

        static void PlaceProp(Transform parent, string prefabName, Vector3 pos, float yaw)
        {
            var guids = AssetDatabase.FindAssets(prefabName + " t:Prefab", new[] { "Assets/TirgamesAssets/Factory/Prefabs" });
            foreach (var g in guids)
            {
                string p = AssetDatabase.GUIDToAssetPath(g);
                if (System.IO.Path.GetFileNameWithoutExtension(p) != prefabName) continue;
                var go = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(p), parent);
                go.transform.SetPositionAndRotation(pos, Quaternion.Euler(0, yaw, 0));
                return;
            }
            Debug.LogWarning("[SceneBuilder] Pack prefab not found: " + prefabName);
        }

        // ---- Mission scene --------------------------------------------------------------------

        static void BuildMission(MissionDefinition def)
        {
            AssetDatabase.DeleteAsset(MissionScenePath);
            if (!AssetDatabase.CopyAsset(PackScene, MissionScenePath)) throw new System.Exception("Could not copy " + PackScene);
            var scene = EditorSceneManager.OpenScene(MissionScenePath, OpenSceneMode.Single);
            def = AssetDatabase.LoadAssetAtPath<MissionDefinition>(MissionAssetPath);

            StripPostProcessing();
            ReduceLighting();
            var boundary = AddBoundary();

            // Bake navigation before adding movable debris / the partition wall.
            var surfaceGo = new GameObject("NavMesh");
            var surface = surfaceGo.AddComponent<NavMeshSurface>();
            surface.collectObjects = CollectObjects.All;
            surface.useGeometry = NavMeshCollectGeometry.PhysicsColliders;
            Physics.SyncTransforms();
            surface.BuildNavMesh();
            if (surface.navMeshData != null)
            {
                string navPath = ScenesDir + "/FactoryRescue_NavMesh.asset";
                AssetDatabase.DeleteAsset(navPath);
                AssetDatabase.CreateAsset(surface.navMeshData, navPath);
                surface.navMeshData = AssetDatabase.LoadAssetAtPath<NavMeshData>(navPath);
            }
            else Debug.LogError("[SceneBuilder] NavMesh bake produced no data.");

            var rig = AddRig(PlayerStart, 180f);
            var head = rig.cam.transform;

            var gameplay = new GameObject("Gameplay").transform;

            // Core controllers.
            var vitalsGo = new GameObject("PlayerVitals");
            vitalsGo.transform.SetParent(rig.root.transform, false);
            var vitals = vitalsGo.AddComponent<PlayerVitals>();
            Ref(vitals, "head", head);
            var feedback = vitalsGo.AddComponent<ExposureFeedback>();
            var coughGo = new GameObject("Cough"); coughGo.transform.SetParent(head, false);
            var cough = Sound(coughGo, SpatialSound.Cue.None, 0.7f, 1f, 6f);
            Val(cough, "spatialBlend", 0f);
            Ref(feedback, "vitals", vitals); Ref(feedback, "cough", cough);
            Ref(rig.overlay, "vitals", vitals);

            var missionGo = new GameObject("MissionController");
            var mission = missionGo.AddComponent<MissionController>();
            Ref(mission, "mission", def); Ref(mission, "vitals", vitals);

            // Props.
            var breaker = BuildBreaker(gameplay);
            var gas = BuildGas(gameplay, vitals);
            var cable = BuildCable(gameplay, breaker, mission, vitals);

            var firePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabsDir + "/Fire.prefab");
            var fireA = PlaceFire(firePrefab, gameplay, "Fire_Electrical", "Electrical fire", FireAPos, mission, vitals, breaker);
            var fireB = PlaceFire(firePrefab, gameplay, "Fire_Barrels", "Barrel fire", FireBPos, mission, vitals, null);

            var extinguisher = BuildExtinguisher(gameplay, new List<FireHazard> { fireA, fireB });

            var debris = BuildCollapse(gameplay, mission);

            // Exit.
            var exitGo = new GameObject("AssemblyPoint");
            exitGo.transform.SetParent(gameplay, false);
            exitGo.transform.position = ExitPos + Vector3.up * 1.5f;
            var exit = exitGo.AddComponent<ExitZone>();
            exit.Size = new Vector3(8f, 3f, 6f);
            BuildAssemblySign(exitGo.transform);

            // Survivors.
            var survivorPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabsDir + "/Survivor.prefab");
            var survivors = new List<Survivor>
            {
                PlaceSurvivor(survivorPrefab, gameplay, "Survivor_Dana", "Dana", new Vector3(-16.8f, 0f, -7f), 90f, "", mission, vitals, head, exit),
                PlaceSurvivor(survivorPrefab, gameplay, "Survivor_Marcus", "Marcus", new Vector3(-15.5f, 0f, -28.5f), 20f, ObjectiveIds.Route, mission, vitals, head, exit),
                PlaceSurvivor(survivorPrefab, gameplay, "Survivor_Priya", "Priya", new Vector3(-6.8f, 0f, -31f), 200f, ObjectiveIds.Route, mission, vitals, head, exit),
            };

            // Hazard registration with the mission.
            var hazards = new List<Hazard> { breaker.Electrical, gas.Leak, fireA, fireB };
            Refs(mission, "hazards", hazards);
            Refs(mission, "survivors", survivors);

            // UI.
            var ui = BuildMissionUI(rig, mission, vitals, head);
            BuildWristHud(rig, mission, vitals, head);
            BuildToast(mission, head);
            BuildMarkers(mission, head);

            // Audio ambience.
            Ambient("AmbientHum", SpatialSound.Cue.AmbientHum, 0.2f, Vector3.zero, false);
            Ambient("FireAlarm", SpatialSound.Cue.Alarm, 0.28f, new Vector3(-11.4f, 6f, 14f), true, 8f, 50f);

            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogColor = new Color(0.07f, 0.065f, 0.06f);
            RenderSettings.fogDensity = 0.012f;

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, MissionScenePath);
        }

        // ---- Environment tweaks ---------------------------------------------------------------

        static void StripPostProcessing()
        {
            int removed = 0;
            foreach (var go in Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (go == null) continue;
                foreach (var c in go.GetComponents<Component>())
                {
                    if (c == null) continue;
                    if (c.GetType().Name.Contains("PostProcess")) { Object.DestroyImmediate(c); removed++; }
                }
            }
            foreach (var go in Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (go == null) continue;
                removed += GameObjectUtility.RemoveMonoBehavioursWithMissingScript(go);
            }
            foreach (var go in Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                if (go != null && go.name.Contains("PostProcess") && go.GetComponents<Component>().Length <= 1) Object.DestroyImmediate(go);
            Debug.Log($"[SceneBuilder] Removed {removed} legacy post-processing / missing-script component(s) from the scene copy.");
        }

        static void ReduceLighting()
        {
            // The pack's baked lighting data does not match the copied scene; use cheap realtime lights without shadows (Quest budget).
            Lightmapping.lightingDataAsset = null;
            int n = 0;
            foreach (var l in Object.FindObjectsByType<Light>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                l.shadows = LightShadows.None;
                l.lightmapBakeType = LightmapBakeType.Realtime;
                n++;
            }
            foreach (var guid in new[] { new Vector3(-11.4f, 3.4f, 34f), new Vector3(-11.4f, 3.4f, -34f) })
            {
                var go = new GameObject("EmergencyLight");
                go.transform.position = guid;
                var l = go.AddComponent<Light>();
                l.type = LightType.Point; l.color = new Color(1f, 0.2f, 0.12f); l.intensity = 2f; l.range = 13f; l.shadows = LightShadows.None;
            }
            RenderSettings.ambientIntensity = 1.1f;
            Debug.Log($"[SceneBuilder] Disabled shadows on {n} pack light(s); added 2 emergency lights.");
        }

        static GameObject AddBoundary()
        {
            var root = new GameObject("Boundary (invisible colliders)");
            void Box(string name, Vector3 c, Vector3 s)
            {
                var g = new GameObject(name); g.transform.SetParent(root.transform, false);
                g.transform.position = c; g.AddComponent<BoxCollider>().size = s;
            }
            Box("Floor", new Vector3(-11.4f, -0.25f, 0f), new Vector3(24f, 0.5f, 86f));
            Box("WallWest", new Vector3(-20.9f, 4f, 0f), new Vector3(0.5f, 10f, 86f));
            Box("WallEast", new Vector3(-1.9f, 4f, 0f), new Vector3(0.5f, 10f, 86f));
            Box("WallSouth", new Vector3(-11.4f, 4f, -37.3f), new Vector3(24f, 10f, 0.5f));
            Box("WallNorth", new Vector3(-11.4f, 4f, 37.3f), new Vector3(24f, 10f, 0.5f));
            return root;
        }

        // ---- Props ----------------------------------------------------------------------------

        sealed class BreakerRefs { public MainBreaker Breaker; public ElectricalHazard Electrical; }
        sealed class GasRefs { public GasLeakHazard Leak; public GasValve Valve; }

        static BreakerRefs BuildBreaker(Transform parent)
        {
            var root = new GameObject("MainBreaker");
            root.transform.SetParent(parent, false);
            root.transform.SetPositionAndRotation(BreakerPos, Quaternion.Euler(0, -90f, 0));

            Prim(PrimitiveType.Cube, "Panel", root.transform, Vector3.zero, new Vector3(0.7f, 1.0f, 0.16f), Lit("Mat_BreakerPanel", new Color(0.18f, 0.2f, 0.22f), 0.4f, 0.6f, true));
            Prim(PrimitiveType.Cube, "Warning", root.transform, new Vector3(0, 0.3f, 0.085f), new Vector3(0.5f, 0.12f, 0.01f), Unlit("Mat_WarningStripe", new Color(1f, 0.8f, 0.05f)));
            var pivot = new GameObject("LeverPivot");
            pivot.transform.SetParent(root.transform, false);
            pivot.transform.localPosition = new Vector3(0, -0.12f, 0.1f);
            var leverMat = Lit("Mat_Lever", new Color(0.9f, 0.15f, 0.1f), 0.3f, 0f, true);
            Prim(PrimitiveType.Cube, "Lever", pivot.transform, new Vector3(0, 0.16f, 0), new Vector3(0.07f, 0.34f, 0.07f), leverMat);
            Prim(PrimitiveType.Sphere, "Knob", pivot.transform, new Vector3(0, 0.34f, 0), Vector3.one * 0.12f, leverMat);
            var ind = Prim(PrimitiveType.Sphere, "Indicator", root.transform, new Vector3(0.24f, 0.4f, 0.09f), Vector3.one * 0.08f, Unlit("Mat_BreakerIndicator", Color.green));

            var labelCanvas = UIFactory.CreateWorldCanvas("Label", new Vector2(360, 90), root.transform, 0.0018f, false);
            labelCanvas.transform.localPosition = new Vector3(0, 0.1f, 0.09f);
            labelCanvas.transform.localRotation = Quaternion.Euler(0, 180f, 0);
            UIFactory.AddText(labelCanvas.transform, "MAIN POWER", 56f, new Vector2(0, 0), new Vector2(350, 80), TextAlignmentOptions.Center, UIFactory.TextMain, FontStyles.Bold);

            var box = root.AddComponent<BoxCollider>();
            box.isTrigger = true; box.size = new Vector3(0.85f, 1.15f, 0.7f); box.center = new Vector3(0, 0, 0.25f);
            var snd = Sound(root, SpatialSound.Cue.None, 0.8f, 1.5f, 20f);
            var mb = root.AddComponent<MainBreaker>();
            Ref(mb, "lever", pivot.transform); Ref(mb, "indicator", ind.GetComponent<Renderer>()); Ref(mb, "sound", snd);
            root.AddComponent<HoverHighlight>();

            // Live cable is built by BuildCable (needs the breaker).
            return new BreakerRefs { Breaker = mb };
        }

        static BreakerRefs BuildCable(Transform parent, BreakerRefs b, MissionController mission, PlayerVitals vitals)
        {
            var root = new GameObject("LiveCable");
            root.transform.SetParent(parent, false);
            root.transform.position = new Vector3(CablePos.x, 0f, CablePos.z);
            Prim(PrimitiveType.Cylinder, "Cable", root.transform, new Vector3(0, 2.8f, 0), new Vector3(0.05f, 1.2f, 0.05f), Lit("Mat_Cable", new Color(0.04f, 0.04f, 0.04f), 0.3f));
            Prim(PrimitiveType.Sphere, "Tip", root.transform, new Vector3(0, 1.58f, 0), Vector3.one * 0.08f, Lit("Mat_CableTip", new Color(0.7f, 0.5f, 0.2f), 0.6f, 0.9f));

            var sparkMat = Particle("Fx_Spark", new Color(1f, 0.85f, 0.4f, 1f), true);
            var sparks = MakeParticles(root.transform, "Sparks", new PS
            {
                material = sparkMat, lifetime = 0.5f, speed = 2.6f, size = 0.05f, startSizeMax = 0.1f, rate = 24f, gravity = 1.2f,
                coneAngle = 55f, coneRadius = 0.03f, max = 40, color = Flame(), loop = true, playOnAwake = true
            }, new Vector3(0, 1.55f, 0), new Vector3(90, 0, 0));
            var lightGo = new GameObject("ArcLight");
            lightGo.transform.SetParent(root.transform, false);
            lightGo.transform.localPosition = new Vector3(0, 1.55f, 0);
            var arc = lightGo.AddComponent<Light>();
            arc.type = LightType.Point; arc.color = new Color(0.55f, 0.75f, 1f); arc.range = 4f; arc.intensity = 3f; arc.shadows = LightShadows.None;
            var buzzGo = new GameObject("Buzz"); buzzGo.transform.SetParent(root.transform, false);
            buzzGo.transform.localPosition = new Vector3(0, 1.55f, 0);
            var buzz = Sound(buzzGo, SpatialSound.Cue.None, 0.6f, 1.5f, 18f);

            var hz = root.AddComponent<ElectricalHazard>();
            Val(hz, "hazardId", "live-cable"); Val(hz, "displayName", "Live cable"); Val(hz, "objectiveId", ObjectiveIds.Power);
            Ref(hz, "breaker", b.Breaker); Ref(hz, "mission", mission); Ref(hz, "vitals", vitals);
            Ref(hz, "sparks", sparks); Ref(hz, "arcLight", arc); Ref(hz, "buzz", buzz);
            b.Electrical = hz;
            return b;
        }

        static GasRefs BuildGas(Transform parent, PlayerVitals vitals)
        {
            var root = new GameObject("GasCylinder");
            root.transform.SetParent(parent, false);
            root.transform.position = GasPos;
            var bodyMat = Lit("Mat_GasCylinder", new Color(0.9f, 0.75f, 0.1f), 0.5f, 0.3f);
            var body = Prim(PrimitiveType.Cylinder, "Body", root.transform, new Vector3(0, 0.65f, 0), new Vector3(0.32f, 0.65f, 0.32f), bodyMat, true);
            body.GetComponent<Collider>().isTrigger = false;
            Prim(PrimitiveType.Sphere, "Cap", root.transform, new Vector3(0, 1.3f, 0), new Vector3(0.32f, 0.2f, 0.32f), bodyMat);
            Prim(PrimitiveType.Cylinder, "Stripe", root.transform, new Vector3(0, 0.9f, 0), new Vector3(0.325f, 0.05f, 0.325f), Unlit("Mat_GasStripe", new Color(0.1f, 0.1f, 0.1f)));

            var leakPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabsDir + "/GasLeak.prefab");
            var leakGo = (GameObject)PrefabUtility.InstantiatePrefab(leakPrefab, root.transform);
            leakGo.transform.localPosition = new Vector3(0.15f, 1.35f, 0);
            var leak = leakGo.GetComponent<GasLeakHazard>();
            Val(leak, "hazardId", "gas-leak"); Val(leak, "displayName", "Gas leak"); Val(leak, "objectiveId", ObjectiveIds.Gas);
            Ref(leakGo.GetComponent<ExposureSource>(), "vitals", vitals);

            var valve = new GameObject("GasValve");
            valve.transform.SetParent(root.transform, false);
            valve.transform.localPosition = new Vector3(0, 1.48f, 0);
            var wheel = new GameObject("Wheel");
            wheel.transform.SetParent(valve.transform, false);
            var red = Lit("Mat_ValveWheel", new Color(0.85f, 0.12f, 0.1f), 0.4f, 0.4f, true);
            Prim(PrimitiveType.Cylinder, "Disc", wheel.transform, Vector3.zero, new Vector3(0.3f, 0.015f, 0.3f), red);
            Prim(PrimitiveType.Cube, "SpokeA", wheel.transform, Vector3.zero, new Vector3(0.32f, 0.03f, 0.04f), red);
            Prim(PrimitiveType.Cube, "SpokeB", wheel.transform, Vector3.zero, new Vector3(0.04f, 0.03f, 0.32f), red);
            var sc = valve.AddComponent<SphereCollider>();
            sc.isTrigger = true; sc.radius = 0.28f;
            var snd = Sound(valve, SpatialSound.Cue.None, 0.8f, 1.5f, 20f);
            var gv = valve.AddComponent<GasValve>();
            Ref(gv, "gasLeak", leak); Ref(gv, "wheel", wheel.transform); Ref(gv, "sound", snd);
            valve.AddComponent<HoverHighlight>();

            var labelCanvas = UIFactory.CreateWorldCanvas("Label", new Vector2(420, 90), root.transform, 0.0016f, false);
            labelCanvas.transform.localPosition = new Vector3(0, 0.9f, -0.17f);
            labelCanvas.gameObject.AddComponent<FaceCamera>();
            UIFactory.AddText(labelCanvas.transform, "GAS SHUTOFF", 56f, new Vector2(0, 0), new Vector2(410, 80), TextAlignmentOptions.Center, new Color(0.1f, 0.1f, 0.1f), FontStyles.Bold);
            return new GasRefs { Leak = leak, Valve = gv };
        }

        static FireHazard PlaceFire(GameObject prefab, Transform parent, string name, string display, Vector3 pos, MissionController mission, PlayerVitals vitals, BreakerRefs breaker)
        {
            var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
            go.name = name;
            go.transform.position = pos;
            var fire = go.GetComponent<FireHazard>();
            Val(fire, "hazardId", name.ToLowerInvariant()); Val(fire, "displayName", display); Val(fire, "objectiveId", ObjectiveIds.Fire);
            Ref(fire, "mission", mission); Ref(fire, "vitals", vitals);
            if (breaker != null) Ref(fire, "liveSource", breaker.Breaker);
            Ref(go.GetComponent<ExposureSource>(), "vitals", vitals);

            // Burning fuel: a charred barrel and a scorch mark so the fire has a visible source.
            var burnt = Lit("Mat_Scorch", new Color(0.03f, 0.03f, 0.03f), 0.05f);
            Prim(PrimitiveType.Cylinder, "Scorch", go.transform, new Vector3(0, 0.005f, 0), new Vector3(1.6f, 0.005f, 1.6f), burnt);
            return fire;
        }

        static Extinguisher BuildExtinguisher(Transform parent, List<FireHazard> fires)
        {
            var crate = Prim(PrimitiveType.Cube, "Bench", parent, new Vector3(-9f, 0.4f, 22.5f), new Vector3(1.0f, 0.8f, 0.6f), Lit("Mat_Bench", new Color(0.25f, 0.2f, 0.15f), 0.15f), true);
            var root = new GameObject("Extinguisher");
            root.transform.SetParent(parent, false);
            root.transform.position = new Vector3(-9f, 0.93f, 22.5f);
            var red = Lit("Mat_Extinguisher", new Color(0.8f, 0.06f, 0.05f), 0.5f, 0.3f, true);
            var dark = Lit("Mat_ExtinguisherDark", new Color(0.05f, 0.05f, 0.05f), 0.3f, 0.2f, true);
            Prim(PrimitiveType.Cylinder, "Tank", root.transform, Vector3.zero, new Vector3(0.18f, 0.22f, 0.18f), red).transform.localRotation = Quaternion.Euler(90, 0, 0);
            Prim(PrimitiveType.Cube, "Handle", root.transform, new Vector3(0, 0.12f, 0.02f), new Vector3(0.04f, 0.05f, 0.2f), dark);
            Prim(PrimitiveType.Cube, "Valve", root.transform, new Vector3(0, 0.09f, 0.24f), new Vector3(0.06f, 0.08f, 0.1f), dark);
            var nozzle = new GameObject("Nozzle");
            nozzle.transform.SetParent(root.transform, false);
            nozzle.transform.localPosition = new Vector3(0, 0.02f, 0.4f);
            Prim(PrimitiveType.Cylinder, "NozzleTip", nozzle.transform, new Vector3(0, 0, -0.06f), new Vector3(0.05f, 0.07f, 0.05f), dark).transform.localRotation = Quaternion.Euler(90, 0, 0);

            var sprayMat = Particle("Fx_Spray", new Color(0.92f, 0.95f, 1f, 0.8f), false);
            var spray = MakeParticles(nozzle.transform, "Spray", new PS
            {
                material = sprayMat, lifetime = 0.55f, speed = 7f, size = 0.12f, startSizeMax = 0.3f, rate = 90f, gravity = 0.08f,
                coneAngle = 8f, coneRadius = 0.02f, max = 80, color = Fade(new Color(0.95f, 0.97f, 1f), 0.65f), loop = true, playOnAwake = false
            }, Vector3.zero, Vector3.zero);

            var cap = root.AddComponent<CapsuleCollider>();
            cap.direction = 2; cap.radius = 0.11f; cap.height = 0.85f; cap.center = new Vector3(0, 0, 0.1f);
            var rb = root.AddComponent<Rigidbody>();
            rb.mass = 3f;
            var snd = Sound(root, SpatialSound.Cue.None, 0.7f, 1.5f, 15f);
            var grab = root.AddComponent<XRGrabInteractable>();
            grab.throwOnDetach = false;
            var ex = root.AddComponent<Extinguisher>();
            Ref(ex, "nozzle", nozzle.transform); Ref(ex, "sprayParticles", spray); Ref(ex, "sound", snd);
            Refs(ex, "fires", fires);
            root.AddComponent<HoverHighlight>();

            // Wall sign above the bench.
            var sign = UIFactory.CreateWorldCanvas("Sign", new Vector2(520, 130), parent, 0.0016f, false);
            sign.transform.position = new Vector3(-9f, 1.6f, 22.5f);
            sign.gameObject.AddComponent<FaceCamera>();
            UIFactory.AddText(sign.transform, "FIRE EXTINGUISHER", 42f, new Vector2(0, 0), new Vector2(500, 70), TextAlignmentOptions.Center, new Color(1f, 0.45f, 0.4f), FontStyles.Bold);
            UIFactory.AddText(sign.transform, "Grip to pick up. Trigger to spray.", 28f, new Vector2(0, -58), new Vector2(500, 50), TextAlignmentOptions.Center, UIFactory.TextDim);
            return ex;
        }

        static List<DebrisItem> BuildCollapse(Transform parent, MissionController mission)
        {
            var root = new GameObject("CollapsedPassage").transform;
            root.SetParent(parent, false);
            var wallMat = Lit("Mat_PartitionWall", new Color(0.46f, 0.45f, 0.43f), 0.08f);
            float z = GapCenter.z;
            void Wall(string name, float x0, float x1)
            {
                var w = Prim(PrimitiveType.Cube, name, root, new Vector3((x0 + x1) * 0.5f, 2.75f, z), new Vector3(x1 - x0, 5.5f, 0.5f), wallMat, true);
                var ob = w.AddComponent<NavMeshObstacle>();
                ob.carving = true; ob.shape = NavMeshObstacleShape.Box; ob.size = Vector3.one;
            }
            Wall("WallWest", -20.6f, -13.1f);
            Wall("WallEast", -9.7f, -2.2f);
            var lintel = Prim(PrimitiveType.Cube, "BrokenLintel", root, new Vector3(-11.4f, 4.6f, z), new Vector3(3.9f, 0.6f, 0.5f), wallMat, true);
            lintel.transform.localRotation = Quaternion.Euler(0, 0, 5f);

            var tapeMat = Lit("Mat_Tape", new Color(1f, 0.75f, 0.05f), 0.2f, 0f, true);
            var tape = Prim(PrimitiveType.Cube, "HazardTape", root, new Vector3(-11.4f, 1.45f, z + 1.1f), new Vector3(3.4f, 0.08f, 0.02f), tapeMat);

            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabsDir + "/DebrisBlock.prefab");
            var items = new List<DebrisItem>();
            var rng = new System.Random(7);
            float[] layerY = { 0.36f, 1.08f, 1.8f };
            int[] counts = { 4, 4, 3 };
            for (int layer = 0; layer < 3; layer++)
            {
                for (int i = 0; i < counts[layer]; i++)
                {
                    var b = (GameObject)PrefabUtility.InstantiatePrefab(prefab, root);
                    b.name = $"Debris_{layer}_{i}";
                    float x = -12.7f + i * 0.84f + (layer % 2) * 0.4f;
                    float s = 0.62f + (float)rng.NextDouble() * 0.16f;
                    b.transform.localScale = new Vector3(s, 0.7f, s * 0.95f);
                    b.transform.position = new Vector3(x, layerY[layer], z + ((float)rng.NextDouble() - 0.5f) * 0.25f);
                    b.transform.rotation = Quaternion.Euler(0, (float)rng.NextDouble() * 40f - 20f, 0);
                    items.Add(b.GetComponent<DebrisItem>());
                }
            }

            var monitorGo = new GameObject("PassageMonitor");
            monitorGo.transform.SetParent(root, false);
            monitorGo.transform.position = new Vector3(GapCenter.x, 1.5f, z);
            var pm = monitorGo.AddComponent<PassageMonitor>();
            Ref(pm, "mission", mission);
            Val(pm, "size", new Vector3(3.4f, 3f, 2.6f));
            Refs(pm, "debris", items);
            Refs(pm, "hideWhenCleared", new List<GameObject> { tape });
            var snd = Sound(monitorGo, SpatialSound.Cue.None, 0.8f, 2f, 25f);
            Ref(pm, "sound", snd);
            return items;
        }

        static void BuildAssemblySign(Transform exit)
        {
            var mat = Unlit("Mat_AssemblyGreen", new Color(0.2f, 0.9f, 0.4f, 1f));
            Prim(PrimitiveType.Cube, "FloorMarkA", exit, new Vector3(0, -1.48f, 0), new Vector3(8f, 0.02f, 0.12f), mat);
            Prim(PrimitiveType.Cube, "FloorMarkB", exit, new Vector3(0, -1.48f, 3f), new Vector3(8f, 0.02f, 0.12f), mat);
            Prim(PrimitiveType.Cube, "FloorMarkC", exit, new Vector3(-4f, -1.48f, 1.5f), new Vector3(0.12f, 0.02f, 3f), mat);
            Prim(PrimitiveType.Cube, "FloorMarkD", exit, new Vector3(4f, -1.48f, 1.5f), new Vector3(0.12f, 0.02f, 3f), mat);
            var c = UIFactory.CreateWorldCanvas("Sign", new Vector2(900, 200), exit, 0.003f, false);
            c.transform.localPosition = new Vector3(0, 0.4f, 0);
            c.gameObject.AddComponent<FaceCamera>();
            UIFactory.AddText(c.transform, "ASSEMBLY POINT", 100f, new Vector2(0, 0), new Vector2(880, 130), TextAlignmentOptions.Center, UIFactory.Good, FontStyles.Bold);
            var l = new GameObject("SafeLight").AddComponent<Light>();
            l.transform.SetParent(exit, false); l.transform.localPosition = new Vector3(0, 0.8f, 0);
            l.type = LightType.Point; l.color = new Color(0.3f, 1f, 0.5f); l.intensity = 2.5f; l.range = 9f; l.shadows = LightShadows.None;
        }

        static Survivor PlaceSurvivor(GameObject prefab, Transform parent, string objName, string display, Vector3 pos, float yaw, string requires,
            MissionController mission, PlayerVitals vitals, Transform head, ExitZone exit)
        {
            var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
            go.name = objName;
            go.transform.SetPositionAndRotation(pos, Quaternion.Euler(0, yaw, 0));
            var s = go.GetComponent<Survivor>();
            Val(s, "survivorName", display);
            Val(s, "requiresObjective", requires);
            s.Configure(vitals, head, exit, mission);
            return s;
        }

        // ---- UI -------------------------------------------------------------------------------

        static MissionUI BuildMissionUI(Rig rig, MissionController mission, PlayerVitals vitals, Transform head)
        {
            var uiRoot = new GameObject("UI").transform;

            var briefing = UIFactory.CreateWorldCanvas("BriefingPanel", new Vector2(1100, 860), uiRoot);
            var briefingPanel = briefing.gameObject.AddComponent<VRPanel>();
            UIFactory.AddBackground(briefing.transform, UIFactory.Panel);
            UIFactory.AddText(briefing.transform, "MISSION BRIEFING", 30f, new Vector2(0, -26), new Vector2(1000, 40), TextAlignmentOptions.Center, UIFactory.TextDim, FontStyles.Bold);
            var bTitle = UIFactory.AddText(briefing.transform, "", 54f, new Vector2(0, -66), new Vector2(1020, 80), TextAlignmentOptions.Center, UIFactory.Accent, FontStyles.Bold);
            var bBody = UIFactory.AddText(briefing.transform, "", 30f, new Vector2(0, -160), new Vector2(980, 540), TextAlignmentOptions.TopLeft, UIFactory.TextMain);
            var begin = UIFactory.AddButton(briefing.transform, "Begin Mission", new Vector2(0, -740), new Vector2(560, 100), null, UIFactory.AccentDim, 42f);

            var pause = UIFactory.CreateWorldCanvas("PausePanel", new Vector2(800, 640), uiRoot);
            var pausePanel = pause.gameObject.AddComponent<VRPanel>();
            UIFactory.AddBackground(pause.transform, UIFactory.Panel);
            UIFactory.AddText(pause.transform, "PAUSED", 64f, new Vector2(0, -40), new Vector2(700, 90), TextAlignmentOptions.Center, UIFactory.Accent, FontStyles.Bold);
            var resume = UIFactory.AddButton(pause.transform, "Resume", new Vector2(0, -160), new Vector2(520, 90), null, UIFactory.AccentDim, 40f);
            var pSettings = UIFactory.AddButton(pause.transform, "Settings", new Vector2(0, -270), new Vector2(520, 90), null, UIFactory.Button, 38f);
            var pRestart = UIFactory.AddButton(pause.transform, "Restart Mission", new Vector2(0, -380), new Vector2(520, 90), null, UIFactory.Button, 38f);
            var pMenu = UIFactory.AddButton(pause.transform, "Main Menu", new Vector2(0, -490), new Vector2(520, 90), null, UIFactory.Button, 38f);

            var settingsCanvas = UIFactory.CreateWorldCanvas("SettingsPanel", new Vector2(1000, 760), uiRoot);
            var settings = SettingsPanel.Build(settingsCanvas);

            var results = UIFactory.CreateWorldCanvas("ResultsPanel", new Vector2(1100, 900), uiRoot);
            var resultsPanel = results.gameObject.AddComponent<VRPanel>();
            UIFactory.AddBackground(results.transform, UIFactory.Panel);
            var rTitle = UIFactory.AddText(results.transform, "", 64f, new Vector2(0, -40), new Vector2(1000, 90), TextAlignmentOptions.Center, UIFactory.Accent, FontStyles.Bold);
            var rBody = UIFactory.AddText(results.transform, "", 34f, new Vector2(0, -150), new Vector2(980, 560), TextAlignmentOptions.TopLeft, UIFactory.TextMain);
            var replay = UIFactory.AddButton(results.transform, "Replay", new Vector2(-280, -760), new Vector2(480, 100), null, UIFactory.AccentDim, 40f);
            var rMenu = UIFactory.AddButton(results.transform, "Main Menu", new Vector2(280, -760), new Vector2(480, 100), null, UIFactory.Button, 40f);

            var sys = rig.transitions.gameObject;
            var mui = sys.AddComponent<MissionUI>();
            Ref(mui, "mission", mission); Ref(mui, "head", head); Ref(mui, "transitions", rig.transitions);
            Ref(mui, "briefingPanel", briefingPanel); Ref(mui, "pausePanel", pausePanel); Ref(mui, "settingsPanel", settings); Ref(mui, "resultsPanel", resultsPanel);
            Ref(mui, "briefingTitle", bTitle); Ref(mui, "briefingBody", bBody); Ref(mui, "beginButton", begin);
            Ref(mui, "resumeButton", resume); Ref(mui, "pauseSettingsButton", pSettings); Ref(mui, "pauseRestartButton", pRestart); Ref(mui, "pauseMenuButton", pMenu);
            Ref(mui, "resultsTitle", rTitle); Ref(mui, "resultsBody", rBody); Ref(mui, "replayButton", replay); Ref(mui, "resultsMenuButton", rMenu);

            var pi = sys.AddComponent<PauseInput>();
            pi.Configure(mui);
            return mui;
        }

        static void BuildWristHud(Rig rig, MissionController mission, PlayerVitals vitals, Transform head)
        {
            var go = new GameObject("WristHud");
            var canvas = UIFactory.CreateWorldCanvas("Canvas", new Vector2(520, 400), go.transform, 0.0004f, false);
            var group = canvas.GetComponent<CanvasGroup>();
            group.alpha = 0f;
            UIFactory.AddBackground(canvas.transform, new Color(0.04f, 0.06f, 0.09f, 0.9f));
            var timer = UIFactory.AddText(canvas.transform, "08:00", 70f, new Vector2(0, -6), new Vector2(480, 80), TextAlignmentOptions.Center, UIFactory.TextMain, FontStyles.Bold);
            UIFactory.AddText(canvas.transform, "EXPOSURE", 22f, new Vector2(0, -86), new Vector2(480, 28), TextAlignmentOptions.Center, UIFactory.TextDim, FontStyles.Bold);
            var bar = UIFactory.AddBar(canvas.transform, new Vector2(0, -116), new Vector2(460, 22), UIFactory.Good, "Exposure");
            var obj = UIFactory.AddText(canvas.transform, "", 27f, new Vector2(0, -150), new Vector2(480, 240), TextAlignmentOptions.TopLeft, UIFactory.TextMain);
            var hud = go.AddComponent<WristHud>();
            hud.Configure(mission, vitals, head, rig.left != null ? rig.left : head, group, timer, obj, bar);
        }

        static void BuildToast(MissionController mission, Transform head)
        {
            var go = new GameObject("FeedbackToast");
            var canvas = UIFactory.CreateWorldCanvas("Canvas", new Vector2(1000, 150), go.transform, 0.0012f, false);
            var group = canvas.GetComponent<CanvasGroup>();
            UIFactory.AddBackground(canvas.transform, new Color(0.04f, 0.06f, 0.09f, 0.85f));
            var label = UIFactory.AddText(canvas.transform, "", 40f, new Vector2(0, -20), new Vector2(940, 110), TextAlignmentOptions.Center);
            var snd = Sound(go, SpatialSound.Cue.None, 0.7f, 1f, 10f);
            Val(snd, "spatialBlend", 0f);
            var follow = go.AddComponent<SoftFollow>();
            Ref(follow, "head", head);
            Val(follow, "offset", new Vector3(0f, -0.2f, 1.5f));
            var toast = go.AddComponent<FeedbackToast>();
            toast.Configure(mission, group, label, snd);
        }

        static void BuildMarkers(MissionController mission, Transform head)
        {
            var root = new GameObject("ObjectiveMarkers").transform;
            Marker(root, mission, head, ObjectiveIds.Power, "Cut power", new Vector3(-3.4f, 2.5f, 22.6f));
            Marker(root, mission, head, ObjectiveIds.Gas, "Gas valve", GasPos + new Vector3(0, 2.2f, 0));
            Marker(root, mission, head, ObjectiveIds.Fire, "Fire", Vector3.Lerp(FireAPos, FireBPos, 0.5f) + new Vector3(0, 3.0f, 0));
            Marker(root, mission, head, ObjectiveIds.Route, "Clear passage", new Vector3(GapCenter.x, 4.0f, GapCenter.z + 1.2f));
            Marker(root, mission, head, ObjectiveIds.Locate, "Search here", new Vector3(-11.4f, 3.2f, -29f));
            Marker(root, mission, head, ObjectiveIds.Evacuate, "Assembly point", ExitPos + new Vector3(0, 3.0f, 0));
        }

        static void Marker(Transform parent, MissionController mission, Transform head, string id, string text, Vector3 pos)
        {
            var c = UIFactory.CreateWorldCanvas("Marker_" + id, new Vector2(420, 200), parent, 0.003f, false);
            c.transform.position = pos;
            var diamond = UIFactory.NewUI<UnityEngine.UI.Image>("Diamond", c.transform);
            diamond.color = UIFactory.Accent;
            diamond.raycastTarget = false;
            var rt = diamond.rectTransform;
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 1f); rt.pivot = new Vector2(0.5f, 1f);
            rt.anchoredPosition = new Vector2(0, -20); rt.sizeDelta = new Vector2(70, 70);
            rt.localRotation = Quaternion.Euler(0, 0, 45f);
            var label = UIFactory.AddText(c.transform, text.ToUpperInvariant(), 46f, new Vector2(0, -110), new Vector2(410, 70), TextAlignmentOptions.Center, UIFactory.TextMain, FontStyles.Bold);
            var m = c.gameObject.AddComponent<ObjectiveMarker>();
            m.Configure(mission, head, id, c.GetComponent<CanvasGroup>(), diamond.transform, label);
        }
    }
}
// 1792858733
// rebuild 1792858873
