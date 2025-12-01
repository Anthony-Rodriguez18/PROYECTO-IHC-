using UnityEngine;
using UnityEditor;

namespace Davigo.Davigedit
{
    public abstract class ToolPlaceBirds : EditorWindow
    {
        public abstract void PerchBird(GameObject clone, Rigidbody rb);
        public abstract GameObject BirdField();

        protected GameObject birdPrefab;

        private Transform parent;        

        private void OnGUI()
        {
            if (birdPrefab == null)
            {
                EditorGUILayout.LabelField("Set a bird prefab to start placing.");
            }

            birdPrefab = BirdField();

            EditorGUILayout.LabelField("Set the placed birds' parent.");
            parent = (Transform)EditorGUILayout.ObjectField(parent, typeof(Transform), true);
        }

        void OnFocus()
        {
            SceneView.duringSceneGui -= OnSceneGUI;
            SceneView.duringSceneGui += OnSceneGUI;
        }

        void OnDestroy()
        {
            SceneView.duringSceneGui -= OnSceneGUI;
        }

        void OnSceneGUI(SceneView sceneView)
        {
            if (birdPrefab == null)
                return;

            HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));

            Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, ~0, QueryTriggerInteraction.Ignore))
            {
                Rigidbody rb = hit.collider.GetComponentInParent<Rigidbody>();

                if (rb != null)
                {
                    Handles.color = Color.yellow;
                    Handles.Label(hit.point, rb.name);
                }
                else
                {
                    Handles.color = Color.white;
                }

                Handles.DrawSolidDisc(hit.point, Vector3.up, 0.25f);
                Handles.ArrowHandleCap(
                    0,
                    hit.point + Vector3.up * 2.25f,
                    Quaternion.LookRotation(Vector3.down),
                    2f,
                    EventType.Repaint
                );

                if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
                {
                    GameObject clone = (GameObject)PrefabUtility.InstantiatePrefab(birdPrefab);

                    if (parent != null)
                        clone.transform.parent = parent;

                    clone.transform.position = hit.point;

                    if (rb != null)
                        PerchBird(clone, rb);

                    Undo.RegisterCreatedObjectUndo(clone, "Placed bird");
                    Event.current.Use();
                }

                SceneView.RepaintAll();
            }
        }
    }
}
