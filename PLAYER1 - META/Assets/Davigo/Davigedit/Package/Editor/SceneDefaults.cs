using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace Davigo.Davigedit
{
#if !DAVIREPO
    [InitializeOnLoad]
#endif
    public class SceneDefaults
    {
#if !DAVIREPO
        static SceneDefaults()
        {
            EditorApplication.update += Update;
        }

        private static void Update()
        {    
            EditorSceneManager.newSceneCreated += NewSceneCreated;
            EditorSceneManager.sceneOpened += SceneOpened;

            EditorApplication.update -= Update;
        }
#endif
        [MenuItem("Davigedit/Apply Lighting Defaults", priority = 50)]
        private static void ApplyLightingDefaults()
        {
            TryApplyDefaults();
        }

        private static void TryApplyDefaults()
        {
            if (EditorUtility.DisplayDialog("Davigedit", "Would you like to apply the Davigo lighting defaults to this map?", "Ok", "No thanks"))
            {
                SetDavigoLightingDefaults();
            }
        }

        private static void NewSceneCreated(Scene scene, NewSceneSetup setup, NewSceneMode mode)
        {
            if (!BuildPipeline.isBuildingPlayer && IsCurrentSceneBrandNew())
                TryApplyDefaults();
        }

        private static void SceneOpened(Scene scene, OpenSceneMode mode)
        {
            if (!BuildPipeline.isBuildingPlayer && IsCurrentSceneBrandNew())
                TryApplyDefaults();
        }

        private static void SetDavigoLightingDefaults()
        {
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.5023f, 0.516622f, 0.58f, 1);
            RenderSettings.skybox = (Material)AssetDatabase.LoadAssetAtPath("Assets/Davigo/Common/Materials/Level/GreySkybox.mat", typeof(Material));
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogDensity = 0.005f;
            RenderSettings.fogColor = new Color(0.5686275f, 0.7529413f, 0.7686275f, 1);
        }

        private static bool IsCurrentSceneBrandNew()
        {
            Scene scene = EditorSceneManager.GetActiveScene();

            if (RenderSettings.ambientMode != UnityEngine.Rendering.AmbientMode.Skybox)
                return false;

            GameObject[] roots = scene.GetRootGameObjects();

            if (roots.Length != 2)
                return false;

            for (int i = 0; i < roots.Length; i++)
            {
                if (roots[i].transform.childCount > 0)
                    return false;
            }

            if (roots[0].name != "Main Camera")
                return false;

            if (roots[1].name != "Directional Light")
                return false;

            if (Vector3.Distance(roots[0].transform.position, new Vector3(0, 1, -10)) > 0.001f)
                return false;

            if (Vector3.Distance(roots[1].transform.position, new Vector3(0, 3, 0)) > 0.001f)
                return false;

            return true;
        }
    }
}
