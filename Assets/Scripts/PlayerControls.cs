using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerControls : MonoBehaviour
{
    [SerializeField] private float movementSpeed;
    [SerializeField] private InputActionReference movementInput;
    
    float xRotation;
    float yRotation;

    void Start()
    {
        xRotation = transform.eulerAngles.x;
    }
    
    void FixedUpdate()
    {
        moveCamera();
    }

    private void moveCamera()
    {
        Vector2 movement = movementInput.action.ReadValue<Vector2>();
        yRotation += -movement.x * movementSpeed;
        xRotation += movement.y * movementSpeed;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        transform.rotation = Quaternion.Euler(xRotation, yRotation, 0f);
    }
}
