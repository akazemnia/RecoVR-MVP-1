using System;
using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;

public class EyeGazeLogger : MonoBehaviour
{
    private InputAction positionAction;
    private InputAction rotationAction;
    private StreamWriter writer;
    private string filePath;

    void OnEnable()
    {
        // Create actions bound to the eye gaze paths
        positionAction = new InputAction(type: InputActionType.Value, binding: "<EyeGaze>/pose/position");
        rotationAction = new InputAction(type: InputActionType.Value, binding: "<EyeGaze>/pose/rotation");

        positionAction.Enable();
        rotationAction.Enable();

        // Build a file path in the user's Downloads folder
        string downloadsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
        filePath = Path.Combine(downloadsPath, $"EyeGazeLog_{DateTime.Now:yyyyMMdd_HHmmss}.csv");

        // Open CSV file for writing
        writer = new StreamWriter(filePath, false);
        writer.WriteLine("Time,PosX,PosY,PosZ,RotX,RotY,RotZ,RotW");
        writer.Flush();

        Debug.Log($"Eye gaze logging started. Writing to: {filePath}");
    }

    void Update()
    {
        Vector3 position = positionAction.ReadValue<Vector3>();
        Quaternion rotation = rotationAction.ReadValue<Quaternion>();

        string line = string.Format("{0},{1},{2},{3},{4},{5},{6},{7}",
            Time.time,
            position.x, position.y, position.z,
            rotation.x, rotation.y, rotation.z, rotation.w);

        writer.WriteLine(line);
        writer.Flush();
    }

    void OnDisable()
    {
        if (writer != null)
        {
            writer.Flush();
            writer.Close();
            writer.Dispose();
        }

        positionAction?.Disable();
        rotationAction?.Disable();
    }
}
