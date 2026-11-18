using UnityEngine;

/// <summary>
/// Physics-based drift controller for WheelCollider vehicles.
///
/// Design:
/// - WheelCollider remains the primary tire physics system.
/// - Rear sideways friction is reduced progressively during Space + steering.
/// - Rear FORWARD friction is also reduced in proportion to the sideways
///   grip loss, approximating a combined-slip / friction-circle tire: a tire
///   that is sliding sideways cannot simultaneously deliver full forward
///   (braking or driving) force. WheelCollider does not couple these two
///   curves on its own, so this is done explicitly here.
/// - This coupling is what causes real speed loss during a braking drift
///   and during a severe rotation, without any invented force or velocity
///   reset - the earlier separate "momentum loss" AddForce hack has been
///   removed because this makes it unnecessary.
/// - Actual WheelHit.sidewaysSlip is used as feedback.
/// - No transform rotation.
/// - No artificial forward force.
/// - No artificial drift angle.
/// - No minimum speed requirement.
/// - CarInput is the only input source.
/// </summary>
public class CarDriftController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Rigidbody rb;
    [SerializeField] private CarInput carInput;

    [Header("Wheel Colliders")]
    [SerializeField] private WheelCollider frontLeftWheel;
    [SerializeField] private WheelCollider frontRightWheel;
    [SerializeField] private WheelCollider rearLeftWheel;
    [SerializeField] private WheelCollider rearRightWheel;

    [Header("Rear Drift Grip (sideways)")]
    [Tooltip("Normal rear sideways friction stiffness.")]
    [Range(0.5f, 1.5f)]
    [SerializeField] private float normalRearGrip = 1.15f;

    [Tooltip("Rear sideways grip when Space + steering is used.")]
    [Range(0.15f, 0.8f)]
    [SerializeField] private float brakingDriftGrip = 0.38f;

    [Tooltip("Additional sideways grip reduction caused by throttle during powered drifting.")]
    [Range(0f, 0.3f)]
    [SerializeField] private float throttleGripReduction = 0.10f;

    [Tooltip("How quickly rear tire grip changes.")]
    [Range(1f, 15f)]
    [SerializeField] private float gripChangeSpeed = 5f;

    [Header("Rear Drift Grip (forward / combined-slip coupling)")]
    [Tooltip("Rear forward-friction stiffness once the rear is at maximum sideways slip " +
             "(currentRearGrip == brakingDriftGrip). 1.0 = no coupling (old behavior). " +
             "Lower values mean a sliding rear tire also loses braking/driving grip, " +
             "which is what removes real speed during a drift or spin.")]
    [Range(0.2f, 1f)]
    [SerializeField] private float minRearForwardGrip = 0.55f;

    [Header("Slip Feedback")]
    [Tooltip("Slip level at which the rear is considered to be sliding strongly.")]
    [Range(0.05f, 1f)]
    [SerializeField] private float highSlip = 0.35f;

    [Tooltip("Maximum additional sideways grip reduction when rear slip becomes high.")]
    [Range(0f, 0.2f)]
    [SerializeField] private float highSlipGripReduction = 0.08f;

    [Tooltip("How strongly steering affects drift grip.")]
    [Range(0f, 1f)]
    [SerializeField] private float steeringInfluence = 1.0f;

    [Header("Front Grip")]
    [Tooltip("Keeps front tire sideways grip at the original level.")]
    [Range(0.5f, 1.5f)]
    [SerializeField] private float frontGrip = 1.0f;

    [Header("Debug")]
    [SerializeField] private bool debugMode = true;

    [SerializeField] private float currentRearGrip;
    [SerializeField] private float currentRearForwardGrip;
    [SerializeField] private float rearSidewaysSlip;
    [SerializeField] private float speedKmh;
    [SerializeField] private bool driftInputActive;

    private WheelFrictionCurve rearLeftBaseSideways;
    private WheelFrictionCurve rearRightBaseSideways;
    private WheelFrictionCurve rearLeftBaseForward;
    private WheelFrictionCurve rearRightBaseForward;

    private WheelFrictionCurve frontLeftBaseFriction;
    private WheelFrictionCurve frontRightBaseFriction;

    private bool initialized;

    private void Awake()
    {
        if (rb == null)
            rb = GetComponent<Rigidbody>();

        if (carInput == null)
            carInput = GetComponent<CarInput>();

        if (!ValidateReferences())
            return;

        rearLeftBaseSideways = rearLeftWheel.sidewaysFriction;
        rearRightBaseSideways = rearRightWheel.sidewaysFriction;
        rearLeftBaseForward = rearLeftWheel.forwardFriction;
        rearRightBaseForward = rearRightWheel.forwardFriction;

        frontLeftBaseFriction = frontLeftWheel.sidewaysFriction;
        frontRightBaseFriction = frontRightWheel.sidewaysFriction;

        currentRearGrip = normalRearGrip;
        currentRearForwardGrip = rearLeftBaseForward.stiffness;

        initialized = true;

        ApplyRearGrip(currentRearGrip, currentRearForwardGrip);
        ApplyFrontGrip();
    }

    private void FixedUpdate()
    {
        if (!initialized)
            return;

        float steering = Mathf.Clamp(carInput.Steering, -1f, 1f);
        float throttle = Mathf.Clamp01(carInput.Throttle);
        bool braking = carInput.Brake;

        speedKmh = rb.linearVelocity.magnitude * 3.6f;

        bool steeringInput = Mathf.Abs(steering) > 0.05f;

        // Space is REQUIRED for drift.
        driftInputActive = braking && steeringInput;

        float targetGrip = normalRearGrip;

        if (driftInputActive)
        {
            float steeringAmount = Mathf.Abs(steering) * steeringInfluence;

            targetGrip = Mathf.Lerp(normalRearGrip, brakingDriftGrip, steeringAmount);

            // W + Space creates a powered drift; more throttle lowers rear grip slightly.
            targetGrip -= throttle * throttleGripReduction;

            // React to actual rear tire slip.
            float slip = GetAverageRearSidewaysSlip();
            float slip01 = Mathf.InverseLerp(highSlip, highSlip + 0.35f, Mathf.Abs(slip));
            targetGrip -= slip01 * highSlipGripReduction;
        }

        targetGrip = Mathf.Clamp(targetGrip, 0.15f, normalRearGrip);

        currentRearGrip = Mathf.MoveTowards(
            currentRearGrip,
            targetGrip,
            gripChangeSpeed * Time.fixedDeltaTime
        );

        // Combined-slip coupling: how far the rear is currently pushed
        // toward its lowest sideways grip (0 = fully normal, 1 = fully drifting).
        float sidewaysReductionAmount = Mathf.InverseLerp(normalRearGrip, brakingDriftGrip, currentRearGrip);

        float targetForwardGrip = Mathf.Lerp(1f, minRearForwardGrip, sidewaysReductionAmount);

        currentRearForwardGrip = Mathf.MoveTowards(
            currentRearForwardGrip,
            targetForwardGrip,
            gripChangeSpeed * Time.fixedDeltaTime
        );

        ApplyRearGrip(currentRearGrip, currentRearForwardGrip);
        ApplyFrontGrip();

        if (debugMode)
        {
            rearSidewaysSlip = GetAverageRearSidewaysSlip();
        }
    }

    private float GetAverageRearSidewaysSlip()
    {
        float totalSlip = 0f;
        int groundedWheels = 0;

        if (rearLeftWheel.GetGroundHit(out WheelHit leftHit))
        {
            totalSlip += Mathf.Abs(leftHit.sidewaysSlip);
            groundedWheels++;
        }

        if (rearRightWheel.GetGroundHit(out WheelHit rightHit))
        {
            totalSlip += Mathf.Abs(rightHit.sidewaysSlip);
            groundedWheels++;
        }

        if (groundedWheels == 0)
            return 0f;

        return totalSlip / groundedWheels;
    }

    private void ApplyRearGrip(float sidewaysGrip, float forwardGrip)
    {
        WheelFrictionCurve leftSideways = rearLeftBaseSideways;
        WheelFrictionCurve rightSideways = rearRightBaseSideways;
        leftSideways.stiffness = sidewaysGrip;
        rightSideways.stiffness = sidewaysGrip;
        rearLeftWheel.sidewaysFriction = leftSideways;
        rearRightWheel.sidewaysFriction = rightSideways;

        WheelFrictionCurve leftForward = rearLeftBaseForward;
        WheelFrictionCurve rightForward = rearRightBaseForward;
        leftForward.stiffness = forwardGrip;
        rightForward.stiffness = forwardGrip;
        rearLeftWheel.forwardFriction = leftForward;
        rearRightWheel.forwardFriction = rightForward;
    }

    private void ApplyFrontGrip()
    {
        WheelFrictionCurve left = frontLeftBaseFriction;
        WheelFrictionCurve right = frontRightBaseFriction;

        left.stiffness = frontGrip;
        right.stiffness = frontGrip;

        frontLeftWheel.sidewaysFriction = left;
        frontRightWheel.sidewaysFriction = right;
    }

    private bool ValidateReferences()
    {
        bool valid = true;

        if (rb == null)
        {
            Debug.LogError("[CarDriftController] Rigidbody reference is missing.", this);
            valid = false;
        }

        if (carInput == null)
        {
            Debug.LogError("[CarDriftController] CarInput reference is missing.", this);
            valid = false;
        }

        if (frontLeftWheel == null || frontRightWheel == null ||
            rearLeftWheel == null || rearRightWheel == null)
        {
            Debug.LogError("[CarDriftController] One or more WheelCollider references are missing.", this);
            valid = false;
        }

        return valid;
    }

    private void OnDisable()
    {
        if (!initialized)
            return;

        rearLeftWheel.sidewaysFriction = rearLeftBaseSideways;
        rearRightWheel.sidewaysFriction = rearRightBaseSideways;
        rearLeftWheel.forwardFriction = rearLeftBaseForward;
        rearRightWheel.forwardFriction = rearRightBaseForward;

        frontLeftWheel.sidewaysFriction = frontLeftBaseFriction;
        frontRightWheel.sidewaysFriction = frontRightBaseFriction;
    }
}