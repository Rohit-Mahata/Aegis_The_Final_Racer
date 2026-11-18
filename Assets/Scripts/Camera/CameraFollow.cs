using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform target;

    [Header("Follow")]
    [SerializeField] private float followSpeed = 5f;
    [SerializeField] private Vector3 offset = new Vector3(0f, 2f, -5f);

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

    public Transform GetTarget()
    {
        return target;
    }

    private void LateUpdate()
    {
        if (target == null)
            return;

        Vector3 desiredPosition =
            target.position +
            target.TransformDirection(offset);

        transform.position = Vector3.Lerp(
            transform.position,
            desiredPosition,
            followSpeed * Time.deltaTime
        );

        Quaternion desiredRotation =
            Quaternion.LookRotation(
                target.forward,
                Vector3.up
            );

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            desiredRotation,
            followSpeed * Time.deltaTime
        );
    }
}