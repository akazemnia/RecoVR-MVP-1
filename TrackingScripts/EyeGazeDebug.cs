using UnityEngine;
using UnityEngine.InputSystem;

public class EyeGazeDebug : MonoBehaviour
{
    private InputAction positionAction;
    private InputAction rotationAction;

    void OnEnable()
    {
        // Bind to the EyeGaze device
        positionAction = new InputAction(type: InputActionType.Value, binding: "<EyeGaze>/pose/position");
        rotationAction = new InputAction(type: InputActionType.Value, binding: "<EyeGaze>/pose/rotation");

        positionAction.Enable();
        rotationAction.Enable();
    }

    void Update()
    {
        Vector3 pos = positionAction.ReadValue<Vector3>();
        Quaternion rot = rotationAction.ReadValue<Quaternion>();

        if (pos != Vector3.zero || rot != Quaternion.identity)
        {
            Debug.Log($"EyeGaze position: {pos}, rotation: {rot}");
        }
        else
        {
            Debug.Log("EyeGaze data not available yet.");
        }
    }

    void OnDisable()
    {
        positionAction?.Disable();
        rotationAction?.Disable();
    }
}
