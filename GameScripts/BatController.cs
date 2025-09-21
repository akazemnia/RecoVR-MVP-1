// BatController.cs
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class BatController : MonoBehaviour
{
    public float minHitSpeed = 1.2f; // tune to ignore light touches
    Vector3 lastPos;
    Vector3 velocity;

    [Header("Optional debug")]
    public bool drawVelocityGizmo = false;

    void Start()
    {
        lastPos = transform.position;
    }

    void Update()
    {
        Vector3 delta = transform.position - lastPos;
        velocity = delta / Time.deltaTime;
        lastPos = transform.position;
    }

    public float GetSpeed() => velocity.magnitude;

    void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("Ball"))
        {
            float speed = GetSpeed();
            if (speed >= minHitSpeed)
            {
                // let Ball handle its own OnCollision — GameManager will get OnBallHit event
                // Optionally apply additional impulse to ball:
                Rigidbody brb = collision.collider.attachedRigidbody;
                if (brb != null)
                {
                    brb.linearVelocity = velocity; // simple transfer
                }
            }
        }
    }

    void OnDrawGizmos()
    {
        if (!drawVelocityGizmo) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, transform.position + transform.forward * 0.2f);
    }
}
