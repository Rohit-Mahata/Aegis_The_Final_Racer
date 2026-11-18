using UnityEngine;

public class PlayerAnimationController : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private CharacterController characterController;

    [Header("Animation")]
    [SerializeField] private float smoothing = 8f;

    private float currentSpeed;

    private void Awake()
    {
        if (animator == null)
            animator = GetComponent<Animator>();

        if (characterController == null)
            characterController = GetComponent<CharacterController>();
    }

    private void Update()
    {
        Vector3 velocity = characterController.velocity;
        velocity.y = 0f;

        float movementSpeed = velocity.magnitude;

        currentSpeed = Mathf.Lerp(
            currentSpeed,
            movementSpeed,
            smoothing * Time.deltaTime
        );

        animator.SetFloat("Speed", currentSpeed);
    }
}