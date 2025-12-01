using UnityEngine;
using UnityEditor;

namespace Davigo.Davigedit
{
    [CustomEditor(typeof(IdentifiableObject))]
    [CanEditMultipleObjects]
    public class IdentifiableObjectEditor : Editor
    {
        private SerializedProperty guidProperty;

        private void OnEnable()
        {
            guidProperty = serializedObject.FindProperty("guid");
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.PropertyField(guidProperty);

            if (GUILayout.Button("Generate new GUID"))
            {
                var random = new System.Random();
                guidProperty.intValue = random.Next();
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
