using UnityEngine;

public class CarBrakeController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Rigidbody rb;
    [SerializeField] private CarInput carInput;

    [Header("Brake")]
    [Tooltip("Per-wheel brake torque at bias = 0.5 (i.e. an even front/rear split). " +
             "This matches the original per-wheel torque used before braking was split into an axle bias.")]
    [SerializeField] private float maxBrakeTorque = 5000f;

    [Header("Brake Bias")]
    [Range(0f, 1f)]
    [SerializeField] private float frontBrakeBias = 0.65f;

    [Header("Brake Response")]
    [SerializeField] private float brakeResponse = 8f;

    [Header("ABS")]
    [SerializeField] private bool useABS = true;
    [SerializeField] private float absSlipThreshold = 0.45f;
    [SerializeField] private float absReleaseRate = 0.5f;

    private float currentBrakeInput;

    private WheelCollider[] frontWheels;
    private WheelCollider[] rearWheels;

    private void Awake()
    {
        if (rb == null)
            rb = GetComponent<Rigidbody>();

        if (carInput == null)
            carInput = GetComponent<CarInput>();

        WheelCollider[] allWheels = GetComponentsInChildren<WheelCollider>();

        foreach (WheelCollider wheel in allWheels)
        {
            if (wheel.name.Contains("FL") || wheel.name.Contains("FR"))
            {
                AddFrontWheel(wheel);
            }
            else if (wheel.name.Contains("RL") || wheel.name.Contains("RR"))
            {
                AddRearWheel(wheel);
            }
        }
    }

    private void AddFrontWheel(WheelCollider wheel)
    {
        int oldLength = frontWheels != null ? frontWheels.Length : 0;
        WheelCollider[] newArray = new WheelCollider[oldLength + 1];
        if (frontWheels != null)
            frontWheels.CopyTo(newArray, 0);
        newArray[oldLength] = wheel;
        frontWheels = newArray;
    }

    private void AddRearWheel(WheelCollider wheel)
    {
        int oldLength = rearWheels != null ? rearWheels.Length : 0;
        WheelCollider[] newArray = new WheelCollider[oldLength + 1];
        if (rearWheels != null)
            rearWheels.CopyTo(newArray, 0);
        newArray[oldLength] = wheel;
        rearWheels = newArray;
    }

    private void FixedUpdate()
    {
        if (carInput == null)
            return;

        float brakeInput = carInput.Brake ? 1f : 0f;

        currentBrakeInput = Mathf.MoveTowards(
            currentBrakeInput,
            brakeInput,
            brakeResponse * Time.fixedDeltaTime
        );

        ApplyBrakes();
    }

    private void ApplyBrakes()
    {
        // FIX: previously "maxBrakeTorque" was treated as a total torque
        // budget that got split across the two axles (front + rear summed
        // back to maxBrakeTorque). That silently halved total system brake
        // torque compared with the original design, where every wheel
        // independently received the full 5000.
        //
        // Bias now scales each axle's PER-WHEEL torque around maxBrakeTorque
        // as the value at an even 0.5/0.5 split, so:
        //   bias = 0.5  -> every wheel gets maxBrakeTorque (matches the
        //                  original 4-wheel x 5000 behavior)
        //   bias = 0.65 -> front wheels get 1.3x, rear wheels get 0.7x
        float frontTorque = maxBrakeTorque * (frontBrakeBias * 2f) * currentBrakeInput;
        float rearTorque = maxBrakeTorque * ((1f - frontBrakeBias) * 2f) * currentBrakeInput;

        ApplyBrakeTorque(frontWheels, frontTorque);
        ApplyBrakeTorque(rearWheels, rearTorque);
    }

    private void ApplyBrakeTorque(WheelCollider[] wheels, float torque)
    {
        if (wheels == null)
            return;

        foreach (WheelCollider wheel in wheels)
        {
            if (wheel == null)
                continue;

            float finalTorque = torque;

            if (useABS)
            {
                finalTorque *= GetABSFactor(wheel);
            }

            wheel.brakeTorque = finalTorque;
        }
    }

    private float GetABSFactor(WheelCollider wheel)
    {
        if (!wheel.GetGroundHit(out WheelHit hit))
            return 1f;

        float slip = Mathf.Abs(hit.forwardSlip);

        if (slip <= absSlipThreshold)
            return 1f;

        float excessSlip = slip - absSlipThreshold;
        float reduction = excessSlip * absReleaseRate;

        return Mathf.Clamp01(1f - reduction);
    }
}