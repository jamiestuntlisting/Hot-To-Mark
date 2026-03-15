#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Build;

namespace HotToMark.Editor
{
    /// <summary>
    /// Editor tooling for quick scene setup and testing.
    /// Accessible via the Unity menu: HotToMark > ...
    /// </summary>
    public class HotToMarkEditorTools
    {
        [MenuItem("HotToMark/Setup Scene (One Click)", priority = 1)]
        public static void SetupScene()
        {
            // Create a new scene
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // Add the bootstrap GameObject
            var bootstrapObj = new GameObject("SceneBootstrap");
            bootstrapObj.AddComponent<Core.SceneBootstrap>();

            // Mark scene dirty so the user can save
            EditorSceneManager.MarkSceneDirty(scene);

            Debug.Log("[HotToMark] Scene setup complete. Hit Play to run the game!");
            Debug.Log("[HotToMark] Save the scene to Assets/Scenes/MainScene.unity");

            // Focus on the newly created object
            Selection.activeGameObject = bootstrapObj;
        }

        [MenuItem("HotToMark/Verify Project Settings", priority = 10)]
        public static void VerifySettings()
        {
            bool allGood = true;

            // Check target platform
            if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.iOS)
            {
                Debug.LogWarning("[HotToMark] Build target is not iOS. Switch via File > Build Settings.");
                allGood = false;
            }

            // Check frame rate
            if (Application.targetFrameRate != 60 && Application.targetFrameRate != -1)
            {
                Debug.LogWarning($"[HotToMark] Target frame rate is {Application.targetFrameRate}, expected 60.");
                allGood = false;
            }

            // Check color space
            if (PlayerSettings.colorSpace != ColorSpace.Linear)
            {
                Debug.LogWarning("[HotToMark] Color space should be Linear for URP. Change in Player Settings.");
                allGood = false;
            }

            // Check orientation
            if (PlayerSettings.defaultInterfaceOrientation != UIOrientation.LandscapeLeft)
            {
                Debug.LogWarning("[HotToMark] Default orientation should be Landscape Left for gameplay.");
                allGood = false;
            }

            if (allGood)
            {
                Debug.Log("[HotToMark] All project settings verified. Ready to build!");
            }
        }

        [MenuItem("HotToMark/Apply Recommended Player Settings", priority = 11)]
        public static void ApplyPlayerSettings()
        {
            PlayerSettings.companyName = "StuntGames";
            PlayerSettings.productName = "Hot To Mark";
            PlayerSettings.colorSpace = ColorSpace.Linear;
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.LandscapeLeft;

            // iOS specific
            PlayerSettings.iOS.targetOSVersionString = "15.0";
            PlayerSettings.iOS.requiresPersistentWiFi = false;
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.iOS,
                ScriptingImplementation.IL2CPP);
            PlayerSettings.SetApiCompatibilityLevel(NamedBuildTarget.iOS,
                ApiCompatibilityLevel.NET_Unity_4_8);
            PlayerSettings.iOS.appleEnableAutomaticSigning = true;

            // Bundle ID
            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.iOS,
                "com.stuntgames.hottomark");

            Debug.Log("[HotToMark] Player settings applied for iOS deployment.");
        }

        [MenuItem("HotToMark/Build iOS Xcode Project", priority = 20)]
        public static void BuildiOS()
        {
            string[] scenes = { "Assets/Scenes/MainScene.unity" };
            string buildPath = "Builds/iOS";

            var options = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = buildPath,
                target = BuildTarget.iOS,
                options = BuildOptions.None
            };

            var report = BuildPipeline.BuildPlayer(options);

            if (report.summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded)
            {
                Debug.Log($"[HotToMark] iOS build succeeded. Open {buildPath}/Unity-iPhone.xcodeproj in Xcode.");
            }
            else
            {
                Debug.LogError($"[HotToMark] iOS build failed: {report.summary.totalErrors} errors.");
            }
        }
    }
}
#endif
