using System.Collections.Generic;
using UnityEngine;

public class FenceSpline : MonoBehaviour
{
    public GameObject fencePrefab;
    public List<Transform> points = new List<Transform>();
    public float fenceLength = 6.6f;

    [ContextMenu("Build Fence")]
    public void BuildFence()
    {
        if (fencePrefab == null || points.Count < 2)
        {
            Debug.LogWarning("Assign a fence prefab and at least 2 points.");
            return;
        }

        Transform generated = transform.Find("GeneratedFences");

        if (generated == null)
        {
            GameObject holder = new GameObject("GeneratedFences");
            generated = holder.transform;
            generated.SetParent(transform);
        }

        // Delete only old generated fences
        for (int i = generated.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(generated.GetChild(i).gameObject);
        }

        // Build fence between every pair of points
        for (int p = 0; p < points.Count - 1; p++)
        {
            if (points[p] == null || points[p + 1] == null)
                continue;

            Vector3 start = points[p].position;
            Vector3 end = points[p + 1].position;

            Vector3 direction = end - start;
            float distance = direction.magnitude;

            if (distance < 0.01f)
                continue;

            direction.Normalize();

            int count = Mathf.Max(1, Mathf.RoundToInt(distance / fenceLength));

            float spacing = distance / count;

            for (int i = 0; i < count; i++)
            {
                Vector3 position =
                    start + direction * (spacing * i + spacing / 2f);

                GameObject fence = Instantiate(
                    fencePrefab,
                    position,
                    Quaternion.LookRotation(direction) * Quaternion.Euler(0, 90, 0),
                    generated
                );
            }
        }
    }
}