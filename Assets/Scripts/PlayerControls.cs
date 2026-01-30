using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerControls : MonoBehaviour
{
    [SerializeField] private float movementSpeed;
    [SerializeField] private InputActionReference movementInput;
    [SerializeField] private InputActionReference selectInput;
    
    float xRotation;
    float yRotation;

    void Start()
    {
        xRotation = transform.eulerAngles.x;
    }

    void Update()
    {
        checkLeftClick();
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

    private void checkLeftClick()
    {
        bool didClick = selectInput.action.WasCompletedThisDynamicUpdate();
        if (didClick)
        {
            handleSelect();
        }
    }

    private void handleSelect()
    {
        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            Tile hitTile = GameTiles.getTileByTriangleNormal(hit.normal.normalized);
        }
    }
}
