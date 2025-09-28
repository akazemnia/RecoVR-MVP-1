using UnityEngine;

public class ThrowManager : MonoBehaviour
{
    public Transform throwOrigin;
    public Transform targetPoint;
    public ObjectPool ballPool;
    public Camera playerCamera; // assign XR Rig camera (Main Camera)

    public GameObject SpawnThrow(ThrowConfig cfg)
    {
        if (ballPool == null)
        {
            Debug.LogError("[ThrowManager] ObjectPool not assigned.");
            return null;
        }

        GameObject ballGO = ballPool.Get();
        if (ballGO == null)
        {
            Debug.LogError("[ThrowManager] ObjectPool returned null.");
            return null;
        }

        Ball ball = ballGO.GetComponent<Ball>();
        if (ball == null)
        {
            Debug.LogError("[ThrowManager] Ball prefab missing Ball.cs script.");
            return null;
        }

        // Default: spawn at throwOrigin
        Vector3 spawnPos = throwOrigin.position;
        ballGO.transform.position = spawnPos;
        ballGO.transform.rotation = Quaternion.LookRotation(targetPoint.position - spawnPos);

        // Decide throw type
        switch (cfg.throwType)
        {
            case ThrowType.Curve:
                ball.LaunchCurve(targetPoint.position, cfg.speed, 2); // 2 = oscillations (left-right-left)
                break;

            case ThrowType.AppearingDisappearing:
                ball.LaunchAppearing(targetPoint.position, cfg.speed, playerCamera, 0.5f); 
                // 0.5s interval between teleports
                break;

            default:
                ball.LaunchStraight(targetPoint.position, cfg.speed);
                break;
        }

        ballGO.SetActive(true);
        return ballGO;
    }
}
