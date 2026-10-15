using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace StealthSandbox.Editor
{
    public static class SandboxSetup
    {
        private const string ScenePath = "Assets/Scenes/StealthSandbox.unity";
        private static Sprite square;
        private static Material material;

        [InitializeOnLoadMethod]
        private static void ScheduleFirstSetup()
        {
            if (Application.isBatchMode) return;
            EditorApplication.delayCall += () =>
            {
                if (File.Exists(ScenePath) || EditorApplication.isPlayingOrWillChangePlaymode) return;
                Create();
            };
        }

        [MenuItem("Stealth Sandbox/Create Foundation Scene")]
        public static void CreateFromMenu()
        {
            if (File.Exists(ScenePath))
            {
                EditorSceneManager.OpenScene(ScenePath);
                return;
            }
            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) Create();
        }

        public static void Create()
        {
            Directory.CreateDirectory("Assets/Art/Foundation");
            Directory.CreateDirectory("Assets/Prefabs");
            Directory.CreateDirectory("Assets/Scenes");
            var texture = new Texture2D(16, 16);
            var pixels = new Color[256];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = Color.white;
            texture.SetPixels(pixels); texture.Apply();
            const string spritePath = "Assets/Art/Foundation/Square.png";
            File.WriteAllBytes(spritePath, texture.EncodeToPNG());
            Object.DestroyImmediate(texture);
            AssetDatabase.ImportAsset(spritePath);
            var importer = (TextureImporter)AssetImporter.GetAtPath(spritePath);
            importer.textureType = TextureImporterType.Sprite;
            importer.spritePixelsPerUnit = 16;
            importer.filterMode = FilterMode.Point;
            importer.SaveAndReimport();
            square = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
            material = new Material(Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default"));
            AssetDatabase.CreateAsset(material, "Assets/Art/Foundation/Unlit.mat");
            SetLayer(8, "Walls"); SetLayer(9, "Player"); SetLayer(10, "Guard");
            EditorSettings.defaultBehaviorMode = EditorBehaviorMode.Mode2D;
            var previousScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
            UnityEngine.SceneManagement.SceneManager.SetActiveScene(scene);
            var floor = new GameObject("Floor");
            for (int x = -12; x < 12; x++)
                for (int y = -9; y < 9; y++)
                    Visual("Tile", new Vector2(x + 0.5f, y + 0.5f), Vector2.one * 0.98f,
                        (x + y) % 2 == 0 ? new Color(0.16f, 0.21f, 0.25f) : new Color(0.18f, 0.23f, 0.27f), -10, floor.transform);
            var walls = new GameObject("Walls");
            Wall("North", new Vector2(0, 9), new Vector2(25, 1), walls.transform);
            Wall("South", new Vector2(0, -9), new Vector2(25, 1), walls.transform);
            Wall("West", new Vector2(-12, 0), new Vector2(1, 18), walls.transform);
            Wall("East", new Vector2(12, 0), new Vector2(1, 18), walls.transform);
            Wall("Cover A", new Vector2(-2, 1), new Vector2(1, 6), walls.transform);
            Wall("Cover B", new Vector2(4, -2), new Vector2(5, 1), walls.transform);
            Wall("Cover C", new Vector2(5, 4), new Vector2(3, 1), walls.transform);
            var player = Actor("Player", new Vector2(-7, -4), new Color(0.2f, 0.85f, 0.95f), 9);
            player.tag = "Player";
            player.AddComponent<PlayerMovement2D>();
            PrefabUtility.SaveAsPrefabAsset(player, "Assets/Prefabs/Player.prefab");
            var guard = Actor("Guard", new Vector2(7, 6), new Color(1, 0.48f, 0.25f), 10);
            guard.GetComponent<ActorMotor2D>().Speed = 2.5f;
            var references = guard.AddComponent<GuardReferences>();
            references.wallMask = 1 << 8;
            PrefabUtility.SaveAsPrefabAsset(guard, "Assets/Prefabs/Guard.prefab");
            references.player = player.transform;
            references.wallMask = 1 << 8;
            var patrolRoot = new GameObject("Patrol Points (positions only)");
            var positions = new[] { new Vector2(7, 6), new Vector2(-7, 6), new Vector2(-7, -6), new Vector2(7, -6) };
            references.patrolPoints = new Transform[positions.Length];
            for (int i = 0; i < positions.Length; i++)
            {
                var point = new GameObject("Patrol Point " + (i + 1));
                point.transform.SetParent(patrolRoot.transform);
                point.transform.position = positions[i];
                references.patrolPoints[i] = point.transform;
            }
            var cameraObject = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener), typeof(CameraFollow2D));
            cameraObject.tag = "MainCamera";
            cameraObject.transform.position = new Vector3(-7, -4, -10);
            var camera = cameraObject.GetComponent<Camera>();
            camera.orthographic = true; camera.orthographicSize = 6;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.08f, 0.11f, 0.15f);
            cameraObject.GetComponent<CameraFollow2D>().target = player.transform;
            var navigation = new GameObject("Navigation Grid", typeof(NavigationGrid2D), typeof(Astar));
            EditorSceneManager.SaveScene(scene, ScenePath);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
            AssetDatabase.SaveAssets();
            if (previousScene.IsValid())
            {
                UnityEngine.SceneManagement.SceneManager.SetActiveScene(previousScene);
                EditorSceneManager.CloseScene(scene, true);
            }
            Debug.Log("Foundation ready: " + ScenePath + ". WASD / arrows / gamepad. Guard AI is yours to build.");
        }

        private static GameObject Visual(string name, Vector2 position, Vector2 size, Color color, int order, Transform parent = null)
        {
            var obj = new GameObject(name, typeof(SpriteRenderer));
            obj.transform.SetParent(parent);
            obj.transform.position = position;
            obj.transform.localScale = new Vector3(size.x, size.y, 1);
            var renderer = obj.GetComponent<SpriteRenderer>();
            renderer.sprite = square; renderer.color = color;
            renderer.sharedMaterial = material; renderer.sortingOrder = order;
            return obj;
        }
        private static void Wall(string name, Vector2 position, Vector2 size, Transform parent)
        {
            var obj = Visual(name, position, size, new Color(0.44f, 0.51f, 0.57f), 0, parent);
            obj.layer = 8; obj.AddComponent<BoxCollider2D>();
        }
        private static GameObject Actor(string name, Vector2 position, Color color, int layer)
        {
            var obj = new GameObject(name, typeof(Rigidbody2D), typeof(CircleCollider2D), typeof(ActorMotor2D));
            obj.layer = layer; obj.transform.position = position;
            var body = obj.GetComponent<Rigidbody2D>();
            body.gravityScale = 0; body.constraints = RigidbodyConstraints2D.FreezeRotation;
            body.interpolation = RigidbodyInterpolation2D.Interpolate;
            body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            obj.GetComponent<CircleCollider2D>().radius = 0.38f;
            var visual = Visual("Body", position, Vector2.one * 0.75f, color, 10, obj.transform);
            var marker = Visual("Facing", position + Vector2.up * 0.28f, new Vector2(0.18f, 0.18f), Color.white, 11, obj.transform);
            obj.GetComponent<ActorMotor2D>().SetFacingMarker(marker.transform);
            return obj;
        }
        private static void SetLayer(int index, string name)
        {
            var settings = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            settings.FindProperty("layers").GetArrayElementAtIndex(index).stringValue = name;
            settings.ApplyModifiedProperties();
        }
    }
}
