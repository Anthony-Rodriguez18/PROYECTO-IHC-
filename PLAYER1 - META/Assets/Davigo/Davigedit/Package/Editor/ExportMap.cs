using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.IO;
using System;
using System.Linq;
using System.Collections.Generic;

namespace Davigo.Davigedit
{
    public static class ExportMap
    {
        [MenuItem("Davigedit/Export Map", priority = 0)]
        private static void Export()
        {
            if (!Validate(out string error, out UnityEngine.Object errorObject))
            {
                EditorUtility.DisplayDialog("Error", $"Unable to save map. {error}", "Ok");

                if (errorObject != null)
                {
                    EditorGUIUtility.PingObject(errorObject);
                }

                return;
            }

            // TODO: This should be called from a unified API that both Davigo and Davigedit can access.
            string directoryPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Davigo", CustomMap.MapDirectory);

            if (!Directory.Exists(directoryPath))
                Directory.CreateDirectory(directoryPath);

            var scene = EditorSceneManager.GetActiveScene();

            string filename = $"{scene.name}.{CustomMap.MapExtension}";
            string path = Path.Combine(directoryPath, filename);

            if (File.Exists(path) && 
                !EditorUtility.DisplayDialog("Overwrite existing map?", 
                $"The map {filename} already exists in the {CustomMap.MapDirectory} directory. Do you want to replace it?", "Yes", "No"))
            {
                return;
            }

            EditorUtility.DisplayProgressBar("Saving Davimap", $"Saving out {filename} to Maps directory.", 0.1f);

            string scenePath = scene.path;
            string tempFileName = $"{scene.name}.temp";

            AssetBundleBuild[] bundles = new AssetBundleBuild[]
            {
                new AssetBundleBuild()
                {
                    assetBundleName = tempFileName,
                    assetNames = new string[] { scenePath }
                }
            };

            BuildPipeline.BuildAssetBundles(directoryPath, bundles, BuildAssetBundleOptions.None, BuildTarget.StandaloneWindows64);

            File.Delete(Path.Combine(directoryPath, CustomMap.MapDirectory));
            File.Delete(Path.Combine(directoryPath, $"{CustomMap.MapDirectory}.manifest"));
            File.Delete(Path.Combine(directoryPath, $"{tempFileName}.manifest"));

            byte[] assetBundle = File.ReadAllBytes(Path.Combine(directoryPath, tempFileName));

            File.Delete(Path.Combine(directoryPath, tempFileName));

            Guid guid = Guid.NewGuid();
            MapSettings settings = UnityEngine.Object.FindObjectOfType<MapSettingsComponent>().Settings;

            if (settings.Name == "")
                settings.Name = scene.name;

            if (settings.Description == "")
                settings.Description = "Just another Davigo custom map.";

            if (settings.PreviewImage != null)
            {
                string texturePath = AssetDatabase.GetAssetPath(settings.PreviewImage);
                TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(texturePath);

                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.isReadable = true;
                importer.SaveAndReimport();
            }

            try
            {
                CustomMap.Write(path, guid, settings, assetBundle);

                OpenMapsDirectory.Open();
            }
            catch (Exception e)
            {
                EditorUtility.ClearProgressBar();
                EditorUtility.DisplayDialog("Error", $"Unable to save map. {e}.", "Ok");

                return;
            }
            
            EditorUtility.ClearProgressBar();

            Debug.Log($"Successfully exported map {scene.name}.");
        }

        private static readonly HashSet<Type> componentBlacklist = new HashSet<Type>()
        {
            typeof(Camera),
            typeof(AudioListener)
        };

        public static bool Validate(out string error, out UnityEngine.Object errorObject)
        {
            errorObject = null;

            if (Application.unityVersion != About.SupportedUnityVersion)
            {
                error = $"You are using the incorrect Unity Editor version ({Application.unityVersion}).\n\n" +
                    $"Please install {About.SupportedUnityVersion}. You can find a link to the Unity Download Archive page in the " +
                    $"Davigedit help page (Davigedit > Help...)";

                return false;
            }

            var mapSettingsComponents = UnityEngine.Object.FindObjectsOfType<MapSettingsComponent>();

            if (mapSettingsComponents.Length < 1)
            {
                error = "No map settings prefab has been placed in the map. Please place a map settings prefab.";
                return false;
            }
            else if (mapSettingsComponents.Length > 1)
            {
                error = "More than one map settings prefab has been placed on the map. Only one map settings prefab is permitted.";
                return false;
            }

            if (UnityEngine.Object.FindObjectsOfType<StartPosition>().Where(s => s.Type == StartPosition.PositionType.Warrior).Count() < 1)
            {
                error = "No Warrior start position has been placed on the map. Please place a Warrior start position.";
                return false;
            }

            if (UnityEngine.Object.FindObjectsOfType<StartPosition>().Where(s => s.Type == StartPosition.PositionType.Giant).Count() < 1)
            {
                error = "No Giant start position has been placed on the map. Please place a Giant start position.";
                return false;
            }

            if (UnityEngine.Object.FindObjectOfType<KillzoneComponent>() == null)
            {
                error = "No Killzone has been placed on the map. Please place a Killzone.";
                return false;
            }

            foreach (Type type in componentBlacklist)
            {
                Component component = (Component)UnityEngine.Object.FindObjectOfType(type);

                if (component != null)
                {
                    error = $"Please delete the game object {component.gameObject.name}, or remove the {type} component from it.";
                    errorObject = component.gameObject;
                    return false;
                }
            }

            error = "";
            return true;
        }
    }
}