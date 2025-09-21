// Ball.cs
using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Ball : MonoBehaviour
{
    public ThrowType throwType = ThrowType.Basic;
    public float lifetime = 6f;
    public float appearInterval = 0.12f;
    public float initialSpeed = 6f;

    // For curve movement
    private Vector3 curveStart, curveControl, curveEnd;
    private float curveDuration = 2.5f;
    private float curveTimer = 0f;

    Rigidbody rb;
    MeshRenderer meshRenderer;
    bool hasBeenHit = false;
    float spawnTime = 0f;

    // Pool callback (set by ThrowManager)
    public System.Action<GameObject> ReturnToPool;

    // Events
    public event Action<Ball> OnBallHit; // invoked when the ball is hit

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        meshRenderer = GetComponentInChildren<MeshRenderer>();
    }

    public void InitAsBasic(Vector3 velocity, float life)
    {
        throwType = ThrowType.Basic;
        rb.isKinematic = false;
        rb.linearVelocity = velocity;
        lifetime = life;
        appearInterval = 0f;
        SetupSpawn();
    }

    public void InitAsAppearing(Vector3 velocity, float life, float interval)
    {
        throwType = ThrowType.AppearingDisappearing;
        rb.isKinematic = false;
        rb.linearVelocity = velocity;
        lifetime = life;
        appearInterval = interval;
        StartCoroutine(BlinkRoutine());
        SetupSpawn();
    }

    public void InitAsCurve(Vector3 start, Vector3 control, Vector3 end, float duration)
    {
        throwType = ThrowType.Curve;
        rb.isKinematic = true; // we'll move manually
        curveStart = start;
        curveControl = control;
        curveEnd = end;
        curveDuration = duration;
        curveTimer = 0f;
        lifetime = duration;
        SetupSpawn();
    }

    void SetupSpawn()
    {
        hasBeenHit = false;
        spawnTime = Time.time;
        CancelInvoke(nameof(Expire));
        Invoke(nameof(Expire), lifetime);
    }

    IEnumerator BlinkRoutine()
    {
        while (true)
        {
            if (meshRenderer) meshRenderer.enabled = !meshRenderer.enabled;
            yield return new WaitForSeconds(appearInterval);
        }
    }

    void Update()
    {
        if (throwType == ThrowType.Curve && !hasBeenHit)
        {
            curveTimer += Time.deltaTime;
            float t = Mathf.Clamp01(curveTimer / curveDuration);
            // Quadratic Bezier
            Vector3 p0 = curveStart;
            Vector3 p1 = curveControl;
            Vector3 p2 = curveEnd;
            Vector3 pos = (1 - t) * (1 - t) * p0 + 2 * (1 - t) * t * p1 + t * t * p2;
            transform.position = pos;
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (hasBeenHit) return;

        if (collision.collider.CompareTag("Bat"))
        {
            hasBeenHit = true;
            // broadcast hit
            OnBallHit?.Invoke(this);
            // optional: add physics response if rb exists
            if (rb != null && rb.isKinematic == false)
            {
                // simple bounce away
                Vector3 reflect = Vector3.Reflect(rb.linearVelocity, collision.contacts[0].normal);
                rb.linearVelocity = reflect * 0.6f;
            }
            // schedule return to pool
            Invoke(nameof(ReturnSelf), 1.0f);
        }
        else
        {
            // hit world / ground => expire sooner
            if (!hasBeenHit) Invoke(nameof(ReturnSelf), 0.5f);
        }
    }

    void Expire()
    {
        // lifetime expired: notify GameManager indirectly by not firing OnBallHit
        ReturnSelf();
    }

    void ReturnSelf()
    {
        StopAllCoroutines();
        // reset appearance
        if (meshRenderer) meshRenderer.enabled = true;
        // make sure rb reset
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = false;
        }
        ReturnToPool?.Invoke(this.gameObject);
    }

    public float GetSpawnTime() => spawnTime;
}
