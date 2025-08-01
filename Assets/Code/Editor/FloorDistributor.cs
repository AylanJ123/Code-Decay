using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace com.AylanJ123.CodeDecay.Editor
{
    [ExecuteInEditMode]
    public class FloorDistributorWindow : EditorWindow
    {
        private Transform floorParent;
        private float spacing = 0f;
        private bool seamless = true;

        [MenuItem("Tools/Code Decay/Floor Distributor")]
        public static void ShowWindow()
        {
            GetWindow<FloorDistributorWindow>("Floor Distributor");
        }

        private void OnGUI()
        {
            GUILayout.Label("Floor Distribution Tool", EditorStyles.boldLabel);
            floorParent = (Transform) EditorGUILayout
                .ObjectField("Floor Parent", floorParent, typeof(Transform), true);
            if (floorParent == null)
            {
                EditorGUILayout.HelpBox(
                    "Assign a floor parent Transform with floor pieces as children.", MessageType.Info
                );
                return;
            }
            int childCount = floorParent.childCount;
            EditorGUILayout.LabelField("Child floor pieces count:", childCount.ToString());
            seamless = EditorGUILayout.Toggle("Seamless (no spacing)", seamless);
            if (!seamless)
            {
                spacing = EditorGUILayout.FloatField("Spacing", spacing);
                if (spacing < 0) spacing = 0;
            }
            GUILayout.Space(10);
            if (GUILayout.Button("Distribute Floor Pieces")) DistributeFloor();
        }

        private void DistributeFloor()
        {
            if (floorParent == null)
            {
                Debug.LogWarning("Floor parent not assigned.");
                return;
            }
            int childCount = floorParent.childCount;
            if (childCount == 0)
            {
                Debug.LogWarning("No child floor pieces found under floorParent.");
                return;
            }

            // Get size from first child Renderer
            if (!floorParent.GetChild(0).TryGetComponent(out Renderer rend))
            {
                Debug.LogError("Child does not have a Renderer component to get size from.");
                return;
            }

            Vector3 size = rend.bounds.size;
            float width = size.x;
            float length = size.z;
            if (!seamless)
            {
                width += spacing;
                length += spacing;
            }
            int columns = Mathf.CeilToInt(Mathf.Sqrt(childCount));
            int rows = Mathf.CeilToInt((float)childCount / columns);
            Vector3 startPos = floorParent.position;

            Undo.RegisterCompleteObjectUndo(floorParent, "Distribute Floor Pieces");

            for (int i = 0; i < childCount; i++)
            {
                int row = i / columns;
                int col = i % columns;

                Vector3 pos = startPos + new Vector3(col * width, 0, row * length);

                Transform child = floorParent.GetChild(i);
                Undo.RecordObject(child.transform, "Move Floor Piece");
                child.position = pos;
            }
            Debug.Log($"Distributed {childCount} floor pieces {(seamless ? "seamlessly" : "with spacing")} in {rows} rows and {columns} columns.");
        }
    }

}
