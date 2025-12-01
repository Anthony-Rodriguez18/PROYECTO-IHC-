using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Davigo.Davigedit
{
    public static class OpenMapsDirectory
    {
        [MenuItem("Davigedit/Show Maps Directory in Explorer", priority = 2)]
        public static void Open()
        {
            string directoryPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Davigo", CustomMap.MapDirectory);

            Application.OpenURL(directoryPath);
        }
    }
}