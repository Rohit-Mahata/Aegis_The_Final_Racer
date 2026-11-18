using UnityEngine;
using UnityEditor;

[InitializeOnLoad]
public static class PointPlacerEditor
{
    static PointPlacerEditor()
    {
        SceneView.duringSceneGui += OnSceneGUI;
    }

    static void OnSceneGUI(SceneView sceneView)
    {
        GameObject selected = Selection.activeGameObject;

        if (selected == null)
            return;

        if (!selected.name.StartsWith("Point"))
            return;

        Event e = Event.current;

        if (e.shift && e.type == EventType.MouseDown && e.button == 0)
        {
            Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit, 10000f))
            {
                Undo.RecordObject(selected.transform, "Place Fence Point");

                selected.transform.position = hit.point;

                EditorUtility.SetDirty(selected.transform);

                e.Use();
            }
        }
    }
}