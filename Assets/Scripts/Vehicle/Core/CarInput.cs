using UnityEngine;

public class CarInput : MonoBehaviour
{
    public float Throttle { get; private set; }
    public float Steering { get; private set; }
    public bool Brake { get; private set; }

    [SerializeField] private bool inputEnabled = true;

    private void Update()
    {
        if (!inputEnabled)
        {
            Throttle = 0f;
            Steering = 0f;
            Brake = false;
            return;
        }

        Throttle = Input.GetAxis("Vertical");
        Steering = Input.GetAxis("Horizontal");
        Brake = Input.GetKey(KeyCode.Space);
    }

    public void SetInputEnabled(bool enabled)
    {
        inputEnabled = enabled;

        if (!enabled)
        {
            Throttle = 0f;
            Steering = 0f;
            Brake = false;
        }
    }
}