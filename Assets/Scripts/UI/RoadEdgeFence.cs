using System.Collections.Generic;
using UnityEngine;

public class RoadEdgeFence : MonoBehaviour
{
    [Header("References")]
    public MeshFilter roadMeshFilter;
    public GameObject fencePrefab;

    [Header("Fence Settings")]
    public float fenceLength = 6.6f;
    public float outsideGap = 0.5f;
    public float heightOffset = 0f;

    [Header("Collider Settings")]
    public bool addBoxColliders = true;
    public float colliderLength = 6.6f;
    public float colliderHeight = 1f;
    public float colliderThickness = 0.3f;
    public float colliderCenterY = 0.5f;

    [Header("Build")]
    public bool buildLeftSide = true;
    public bool buildRightSide = true;

    [ContextMenu("Build Fences")]
    public void BuildFences()
    {
        if (roadMeshFilter == null || fencePrefab == null)
        {
            Debug.LogWarning(
                "Assign Road Mesh Filter and Fence Prefab."
            );
            return;
        }

        Mesh mesh = roadMeshFilter.sharedMesh;

        if (mesh == null)
        {
            Debug.LogWarning("The Road Mesh Filter has no mesh.");
            return;
        }

        ClearFences();

        Vector3[] vertices = mesh.vertices;

        if (vertices.Length < 4)
        {
            Debug.LogWarning(
                "Road mesh does not have enough vertices."
            );
            return;
        }

        List<Vector3> leftEdge = new List<Vector3>();
        List<Vector3> rightEdge = new List<Vector3>();

        // Read the road mesh vertices in pairs.
        for (int i = 0; i < vertices.Length - 1; i += 2)
        {
            Vector3 a =
                roadMeshFilter.transform.TransformPoint(vertices[i]);

            Vector3 b =
                roadMeshFilter.transform.TransformPoint(vertices[i + 1]);

            leftEdge.Add(a);
            rightEdge.Add(b);
        }

        if (buildLeftSide)
        {
            BuildSide(
                leftEdge,
                "LeftFences",
                -outsideGap
            );
        }

        if (buildRightSide)
        {
            BuildSide(
                rightEdge,
                "RightFences",
                outsideGap
            );
        }

        Debug.Log("Road fences and colliders built.");
    }

    private void BuildSide(
        List<Vector3> edge,
        string holderName,
        float gapDirection)
    {
        GameObject holder = new GameObject(holderName);
        holder.transform.SetParent(transform);

        float distanceSinceLastFence = 0f;

        for (int i = 0; i < edge.Count - 1; i++)
        {
            Vector3 start = edge[i];
            Vector3 end = edge[i + 1];

            Vector3 direction = end - start;
            float segmentLength = direction.magnitude;

            if (segmentLength < 0.01f)
            {
                continue;
            }

            direction.Normalize();

            Vector3 sideways =
                Vector3.Cross(
                    Vector3.up,
                    direction
                ).normalized;

            float travelled = 0f;

            while (
                distanceSinceLastFence +
                segmentLength -
                travelled >= fenceLength)
            {
                float needed =
                    fenceLength - distanceSinceLastFence;

                travelled += needed;

                Vector3 position =
                    start +
                    direction * travelled +
                    sideways * gapDirection;

                position.y += heightOffset;

                Quaternion rotation =
                    Quaternion.LookRotation(direction) *
                    Quaternion.Euler(0f, 90f, 0f);

                GameObject fence = Instantiate(
                    fencePrefab,
                    position,
                    rotation,
                    holder.transform
                );

                if (addBoxColliders)
                {
                    AddFenceCollider(fence);
                }

                distanceSinceLastFence = 0f;
            }

            distanceSinceLastFence +=
                segmentLength - travelled;
        }
    }

    private void AddFenceCollider(GameObject fence)
    {
        // Remove colliders already included in the prefab.
        Collider[] oldColliders =
            fence.GetComponentsInChildren<Collider>();

        foreach (Collider oldCollider in oldColliders)
        {
            DestroyImmediate(oldCollider);
        }

        // Create a separate invisible collider child.
        GameObject colliderObject =
            new GameObject("FenceCollider");

        colliderObject.transform.SetParent(fence.transform);

        colliderObject.transform.localPosition = Vector3.zero;
        colliderObject.transform.localRotation = Quaternion.identity;
        colliderObject.transform.localScale = Vector3.one;

        BoxCollider box =
            colliderObject.AddComponent<BoxCollider>();

        box.size = new Vector3(
            colliderLength,
            colliderHeight,
            colliderThickness
        );

        box.center = new Vector3(
            0f,
            colliderCenterY,
            0f
        );
    }

    [ContextMenu("Clear Fences")]
    public void ClearFences()
    {
        Transform left = transform.Find("LeftFences");
        Transform right = transform.Find("RightFences");

        if (left != null)
        {
            DestroyImmediate(left.gameObject);
        }

        if (right != null)
        {
            DestroyImmediate(right.gameObject);
        }
    }
}