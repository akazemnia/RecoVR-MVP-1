// ThrowManager.cs
using System.Collections;
using UnityEngine;

public class ThrowManager : MonoBehaviour
{
    [Header("References")]
    public Transform spawnOrigin;
    public Transform targetPoint;
    public ObjectPool ballPool;

    [Header("Defaults")]
    public GameObject ballPrefab; // used by pool but still handy
    public float defaultLifetime = 6f;

    void Awake()
    {
        if (ballPool == null)
        {
            Debug.LogWarning("ThrowManager: ballPool not assigned. Create a pool and assign.");
        }
    }

    public GameObject SpawnThrow(ThrowConfig cfg)
    {
        // get pooled ball
        GameObject go = ballPool.Get();
        go.tag = "Ball";

        // reset parent and rotation
        go.transform.position = spawnOrigin.position;
        go.transform.rotation = Quaternion.identity;

        Ball b = go.GetComponent<Ball>();
        b.lifetime = cfg.lifetime;
        b.appearInterval = cfg.appearInterval;
        b.initialSpeed = cfg.speed;

        // ensure ReturnToPool is assigned
        b.ReturnToPool = (g) => { ballPool.Return(g); };

        Vector3 targetPos = (targetPoint != null) ? targetPoint.position + cfg.targetOffset : spawnOrigin.position + (spawnOrigin.forward * 8f) + cfg.targetOffset;

        if (cfg.throwType == ThrowType.Basic)
        {
            Vector3 dir = (targetPos - spawnOrigin.position).normalized;
            b.InitAsBasic(dir * cfg.speed, cfg.lifetime);
        }
        else if (cfg.throwType == ThrowType.AppearingDisappearing)
        {
            Vector3 dir = (targetPos - spawnOrigin.position).normalized;
            b.InitAsAppearing(dir * cfg.speed, cfg.lifetime, cfg.appearInterval);
        }
        else if (cfg.throwType == ThrowType.Curve)
        {
            Vector3 start = spawnOrigin.position;
            Vector3 end = targetPos;
            Vector3 control = (start + end) / 2f + Vector3.up * cfg.curveHeight;
            b.InitAsCurve(start, control, end, cfg.curveDuration);
        }

        return go;
    }
}
