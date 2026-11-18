using UnityEngine;

public class CarModelSetup : MonoBehaviour
{
    [Header("Wheel Prefab")]
    public GameObject wheelPrefab;

    [Header("Wheel Mount Points")]
    public Transform FL;
    public Transform FR;
    public Transform RL;
    public Transform RR;

    [Header("Wheel Settings")]
    public float wheelScale = 1f;
    public Vector3 wheelRotationOffset = Vector3.zero;

    [ContextMenu("Create Wheels")]
    public void CreateWheels()
    {
        if (wheelPrefab == null)
        {
            Debug.LogWarning("Assign a wheel prefab.");
            return;
        }

        if (FL == null || FR == null || RL == null || RR == null)
        {
            Debug.LogWarning("Assign FL, FR, RL and RR.");
            return;
        }

        ClearWheels();

        CreateWheel("VisualWheel_FL", FL);
        CreateWheel("VisualWheel_FR", FR);
        CreateWheel("VisualWheel_RL", RL);
        CreateWheel("VisualWheel_RR", RR);

        Debug.Log("Sports car wheels created.");
    }

    private void CreateWheel(string wheelName, Transform mountPoint)
    {
        GameObject wheel = Instantiate(
            wheelPrefab,
            mountPoint.position,
            mountPoint.rotation,
            mountPoint
        );

        wheel.name = wheelName;

        wheel.transform.localPosition = Vector3.zero;
        wheel.transform.localRotation =
            Quaternion.Euler(wheelRotationOffset);

        wheel.transform.localScale =
            Vector3.one * wheelScale;
    }

    [ContextMenu("Clear Wheels")]
    public void ClearWheels()
    {
        ClearWheelFrom(FL);
        ClearWheelFrom(FR);
        ClearWheelFrom(RL);
        ClearWheelFrom(RR);
    }

    private void ClearWheelFrom(Transform mountPoint)
    {
        if (mountPoint == null)
            return;

        for (int i = mountPoint.childCount - 1; i >= 0; i--)
        {
            Transform child = mountPoint.GetChild(i);

            if (child.name.StartsWith("VisualWheel_"))
            {
                DestroyImmediate(child.gameObject);
            }
        }
    }
}