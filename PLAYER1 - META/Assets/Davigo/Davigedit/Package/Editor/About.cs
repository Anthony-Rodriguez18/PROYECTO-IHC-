using UnityEditor;

namespace Davigo.Davigedit
{
    public static class About
    {
        public const string Version = "Davigedit Initial Release F";
        public const string SupportedUnityVersion = "2019.4.10f1";

        [MenuItem("Davigedit/About Davigedit", priority = 100)]
        public static void ViewAbout()
        {
            EditorUtility.DisplayDialog("About Davigedit", Version, "Back");
        }
    }
}
