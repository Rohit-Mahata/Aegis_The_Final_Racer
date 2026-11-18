using StarterAssets;
using UnityEngine;

public class VehicleInteraction : MonoBehaviour
{
    [Header("Vehicle Detection")]
    [SerializeField] private float interactionDistance = 4f;

    [Header("Player")]
    [SerializeField] private GameObject playerCharacter;
    [SerializeField] private MonoBehaviour playerController;
    [SerializeField] private Transform playerCameraTarget;

    [Header("Camera")]
    [SerializeField] private CameraFollow cameraFollow;

    private GameObject currentVehicle;
    private CarInput currentCarInput;

    private bool isInsideVehicle;

    private void Awake()
    {
        // Find Aegis automatically
        if (playerCharacter == null)
        {
            Transform aegis = transform.Find("Aegis");

            if (aegis != null)
                playerCharacter = aegis.gameObject;
        }

        // Find ThirdPersonController automatically
        if (playerController == null)
        {
            ThirdPersonController controller =
                GetComponent<ThirdPersonController>();

            if (controller != null)
                playerController = controller;
        }

        // Find PlayerCameraRoot automatically
        if (playerCameraTarget == null)
        {
            Transform cameraRoot =
                transform.Find("PlayerCameraRoot");

            if (cameraRoot != null)
                playerCameraTarget = cameraRoot;
        }

        // Find CameraFollow automatically
        if (cameraFollow == null)
        {
            Camera mainCamera = Camera.main;

            if (mainCamera != null)
                cameraFollow =
                    mainCamera.GetComponent<CameraFollow>();
        }
    }

    private void Update()
    {
        if (isInsideVehicle)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                ExitVehicle();
            }

            return;
        }

        FindNearbyVehicle();

        if (currentVehicle != null &&
            Input.GetKeyDown(KeyCode.E))
        {
            EnterVehicle();
        }
    }

    private void FindNearbyVehicle()
    {
        currentVehicle = null;

        Collider[] colliders = Physics.OverlapSphere(
            transform.position,
            interactionDistance
        );

        float closestDistance = Mathf.Infinity;

        foreach (Collider col in colliders)
        {
            CarController car =
                col.GetComponentInParent<CarController>();

            if (car == null)
                continue;

            float distance = Vector3.Distance(
                transform.position,
                car.transform.position
            );

            if (distance < closestDistance)
            {
                closestDistance = distance;
                currentVehicle = car.gameObject;
            }
        }
    }

    private void EnterVehicle()
    {
        if (currentVehicle == null)
            return;

        currentCarInput =
            currentVehicle.GetComponent<CarInput>();

        if (currentCarInput == null)
        {
            Debug.LogError(
                "CarInput not found on vehicle: " +
                currentVehicle.name
            );

            return;
        }

        // Disable player movement
        if (playerController != null)
            playerController.enabled = false;

        // Hide Aegis
        if (playerCharacter != null)
            playerCharacter.SetActive(false);

        // Enable car controls
        currentCarInput.SetInputEnabled(true);

        // Switch camera to car
        if (cameraFollow != null)
        {
            cameraFollow.SetTarget(
                currentVehicle.transform
            );
        }

        isInsideVehicle = true;

        Debug.Log(
            "ENTERED VEHICLE: " +
            currentVehicle.name
        );
    }

    private void ExitVehicle()
    {
        if (currentVehicle == null)
            return;

        // Disable car controls
        currentCarInput.SetInputEnabled(false);

        // Put player beside the car
        transform.position =
            currentVehicle.transform.position +
            currentVehicle.transform.right * 2.5f;

        // Show Aegis
        if (playerCharacter != null)
            playerCharacter.SetActive(true);

        // Enable player movement
        if (playerController != null)
            playerController.enabled = true;

        // Switch camera back to player
        if (cameraFollow != null &&
            playerCameraTarget != null)
        {
            cameraFollow.SetTarget(
                playerCameraTarget
            );
        }

        Debug.Log(
            "EXITED VEHICLE: " +
            currentVehicle.name
        );

        currentVehicle = null;
        currentCarInput = null;

        isInsideVehicle = false;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(
            transform.position,
            interactionDistance
        );
    }
}