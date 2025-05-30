using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

public class PrefabPainterEditor : EditorWindow
{
    private List<GameObject> prefabs = new List<GameObject>();

    private float brushSize = 2f;
    private float randomScaleMin = 1f;
    private float randomScaleMax = 1f;
    private bool randomRotation = true;

    [MenuItem("Tools/Prefab Painter")]
    public static void ShowWindow()
    {
        GetWindow<PrefabPainterEditor>("Prefab Painter");
    }

    private void OnEnable()
    {
        SceneView.duringSceneGui += OnSceneGUI;
    }

    private void OnDisable()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
    }

    private void OnGUI()
    {
        GUILayout.Label("Prefab Painter Settings", EditorStyles.boldLabel);

        // Hiển thị danh sách prefab
        EditorGUILayout.LabelField("Prefab List:");
        int removeIndex = -1;
        for (int i = 0; i < prefabs.Count; i++)
        {
            EditorGUILayout.BeginHorizontal();
            prefabs[i] = (GameObject)EditorGUILayout.ObjectField(prefabs[i], typeof(GameObject), false);
            if (GUILayout.Button("X", GUILayout.Width(20)))
            {
                removeIndex = i;
            }
            EditorGUILayout.EndHorizontal();
        }
        if (removeIndex >= 0)
        {
            prefabs.RemoveAt(removeIndex);
        }

        if (GUILayout.Button("Add Prefab Slot"))
        {
            prefabs.Add(null);
        }

        GUILayout.Space(10);
        brushSize = EditorGUILayout.Slider("Brush Size", brushSize, 0.5f, 10f);
        randomScaleMin = EditorGUILayout.FloatField("Random Scale Min", randomScaleMin);
        randomScaleMax = EditorGUILayout.FloatField("Random Scale Max", randomScaleMax);
        randomRotation = EditorGUILayout.Toggle("Random Y Rotation", randomRotation);
    }

    private void OnSceneGUI(SceneView sceneView)
    {
        if (prefabs == null || prefabs.Count == 0) return;

        Event e = Event.current;

        if (e.type == EventType.MouseDown && e.button == 0 && !e.alt)
        {
            Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                // Lọc prefab null
                List<GameObject> validPrefabs = prefabs.FindAll(p => p != null);
                if (validPrefabs.Count == 0) return;

                GameObject prefabToSpawn = validPrefabs[Random.Range(0, validPrefabs.Count)];
                GameObject obj = (GameObject)PrefabUtility.InstantiatePrefab(prefabToSpawn);
                obj.transform.position = hit.point;

                if (randomRotation)
                    obj.transform.Rotate(Vector3.up, Random.Range(0f, 360f));

                float randomScale = Random.Range(randomScaleMin, randomScaleMax);
                obj.transform.localScale = Vector3.one * randomScale;

                Undo.RegisterCreatedObjectUndo(obj, "Paint Random Prefab");
                e.Use(); // Consume event
            }
        }
    }
}
