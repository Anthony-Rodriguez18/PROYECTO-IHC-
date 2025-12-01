using UnityEngine;
using UnityEditor;

namespace Davigo.Davigedit
{
    [InitializeOnLoad]
    public class WelcomeScreen : EditorWindow
    {
        static WelcomeScreen()
        {
            EditorApplication.update += Update;
        }

        private static void Update()
        {
            EditorApplication.update -= Update;

            if (Application.unityVersion != About.SupportedUnityVersion)
            {
                string error = $"You are using the incorrect Unity Editor version ({Application.unityVersion}).\n\n" +
                    $"Please install {About.SupportedUnityVersion}. You can find a link to the Unity Download Archive page in the " +
                    $"Davigedit help page (Davigedit > Help...)";

                EditorUtility.DisplayDialog("Incorrect Unity Version", error, "Ok");
            }
            else if (RequireProjectSettingsModifications())
            {
                GetWindow<WelcomeScreen>("Validate Project Settings");
            }
        }

        [MenuItem("Davigedit/Validate Project Settings", priority = 50)]
        private static void DisplayWelcomeScreen()
        {
            if (RequireProjectSettingsModifications())
            {
                GetWindow<WelcomeScreen>("Validate Project Settings");
            }
            else
            {
                EditorUtility.DisplayDialog("Validate Project Settings", "All project settings are correct for Davigedit!", "Ok");
            }
        }

        private void OnGUI()
        {
            GUIStyle style = new GUIStyle
            {
                wordWrap = true,
                richText = true,
                padding = new RectOffset(10, 10, 10, 10)
            };

            EditorGUILayout.LabelField("Davigedit would like to make the following modifications to your project settings:", style);
            EditorGUILayout.LabelField($"• Change color space from {PlayerSettings.colorSpace} to <b>Linear</b>. (Davigo uses Linear color space; this will ensure your maps look same in game as they do in the editor.)", style);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Okay!"))
            {
                PlayerSettings.colorSpace = ColorSpace.Linear;
                Close();
                EditorUtility.DisplayDialog("", "All settings changed! Ready to go!", "Ok");
            }
            if (GUILayout.Button("No thanks"))
            {
                Close();
                EditorUtility.DisplayDialog("", "If you change your mind later, this window can be found in Davigedit / Validate Project Settings.", "Ok");
            }
            EditorGUILayout.EndHorizontal();
        }

        private static bool RequireProjectSettingsModifications()
        {
            return PlayerSettings.colorSpace != ColorSpace.Linear;
        }
    }
}
