using UnityEditor;
using UnityEngine;

namespace Davigo.Davigedit
{
    public static class ViewHelp
    {
        [MenuItem("Davigedit/Help...", priority = 100)]
        private static void Help()
        {
            Application.OpenURL("https://docs.google.com/document/d/1_72LjcDgs3-Ail3sgl3JjO22NzHa5r2ap89jMioNg9M/edit?usp=sharing");
        }
    }
}