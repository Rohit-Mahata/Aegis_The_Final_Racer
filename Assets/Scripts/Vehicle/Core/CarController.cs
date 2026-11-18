using UnityEngine;

public class CarController : MonoBehaviour
{
    [System.Serializable]
    public class Wheel
    {
        public WheelCollider collider;
        public Transform visual;
        public bool steer;
        public bool drive;
        public Vector3 visualRotationOffset;
    }

    [Header("References")]
    [SerializeField] private Rigidbody rb;
    [SerializeField] private Transform centerOfMass;
    [SerializeField] private Wheel[] wheels;

    [Header("Driving")]
    [SerializeField] private float motorTorque = 1800f;
    [SerializeField] private float maxSteerAngle = 32f;
    [SerializeField] private float highSpeedSteerAngle = 13f;
    [SerializeField] private float highSpeedSteerStart = 180f;
    [SerializeField] private float maxSpeedKmh = 220f;
    [SerializeField] private CarInput carInput;

    private void Awake()
    {
        if (rb == null)
            rb = GetComponent<Rigidbody>();

        if (carInput == null)
            carInput = GetComponent<CarInput>();

        if (centerOfMass != null)
        {
            rb.centerOfMass =
                transform.InverseTransformPoint(centerOfMass.position);

        }
        if (wheels != null && wheels.Length > 0 && wheels[0].collider != null)
        {
            wheels[0].collider.ConfigureVehicleSubsteps(
                30f, 5, 10
            );
        }
    }

    private void FixedUpdate()
    {
        float throttle = carInput.Throttle;
        float steering = carInput.Steering;

        float speedKmh =
            rb.linearVelocity.magnitude * 3.6f;

        float torque =
            speedKmh < maxSpeedKmh
                ? throttle * motorTorque
                : 0f;

        // Reduce steering authority progressively as speed increases.
        float speedFactor = Mathf.Clamp01(
            speedKmh / highSpeedSteerStart
        );

        float currentMaxSteer = Mathf.Lerp(
            maxSteerAngle,
            highSpeedSteerAngle,
            speedFactor
        );

        float steerAngle =
            steering * currentMaxSteer;

        foreach (Wheel wheel in wheels)
        {
            if (wheel.collider == null)
                continue;

            wheel.collider.steerAngle =
                wheel.steer ? steerAngle : 0f;

            wheel.collider.motorTorque =
                wheel.drive ? torque : 0f;
        }
    }

    private void LateUpdate()
    {
        foreach (Wheel wheel in wheels)
        {
            if (wheel.collider == null ||
                wheel.visual == null)
                continue;

            wheel.collider.GetWorldPose(
                out Vector3 position,
                out Quaternion rotation
            );

            wheel.visual.SetPositionAndRotation(
                position,
                rotation *
                Quaternion.Euler(
                    wheel.visualRotationOffset
                )
            );
        }
    }
}