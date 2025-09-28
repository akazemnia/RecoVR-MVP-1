using UnityEngine;

public class Ball : MonoBehaviour
{
    public System.Action<Ball> OnBallHit;

    private Rigidbody rb;

    // Shared flight variables
    private Vector3 startPos;
    private Vector3 targetPos;
    private float startTime;
    private float flightDuration;
    private float speed;
    private bool active;

    // Curveball vars
    private bool isCurve;
    private int oscillations;
    private float curveAmplitude = 0.5f;

    // Appearing vars
    private bool isAppearing;
    private Camera playerCamera;
    private float blinkInterval;
    private float nextBlinkTime;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    // ---- LAUNCH METHODS ----

    public void LaunchStraight(Vector3 target, float speed)
    {
        ResetBall(target, speed);
        isCurve = false;
        isAppearing = false;
    }

    public void LaunchCurve(Vector3 target, float speed, int oscillations = 2)
    {
        ResetBall(target, speed);
        isCurve = true;
        this.oscillations = oscillations;
    }

    public void LaunchAppearing(Vector3 target, float speed, Camera cam, float interval = 0.5f)
    {
        ResetBall(target, speed);
        isAppearing = true;
        playerCamera = cam;
        blinkInterval = interval;
        nextBlinkTime = Time.time + blinkInterval;
    }

    private void ResetBall(Vector3 target, float speed)
    {
        startPos = transform.position;
        targetPos = target;
        this.speed = speed;

        float distance = Vector3.Distance(startPos, targetPos);
        flightDuration = distance / speed;
        startTime = Time.time;

        rb.isKinematic = true; // manual motion
        active = true;

        isCurve = false;
        isAppearing = false;
    }

    // ---- UPDATE LOOP ----

    void Update()
    {
        if (!active) return;

        float t = (Time.time - startTime) / flightDuration;
        if (t >= 1f)
        {
            gameObject.SetActive(false);
            active = false;
            return;
        }

        // Default linear movement
        Vector3 pos = Vector3.Lerp(startPos, targetPos, t);

        // Curveball offset
        if (isCurve)
        {
            float sideOffset = Mathf.Sin(t * Mathf.PI * oscillations) * curveAmplitude;
            Vector3 right = Camera.main != null ? Camera.main.transform.right : Vector3.right;
            pos += right * sideOffset;
        }

        // Appearing behavior
        if (isAppearing && playerCamera != null)
        {
            if (Time.time >= nextBlinkTime)
            {
                float randX = Random.Range(0.2f, 0.8f);
                float randY = Random.Range(0.2f, 0.8f);
                Vector3 vp = new Vector3(randX, randY, 5f); // 5m forward
                pos = playerCamera.ViewportToWorldPoint(vp);

                nextBlinkTime = Time.time + blinkInterval;
            }
        }

        transform.position = pos;
    }

    // ---- HIT DETECTION ----

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Bat"))
        {
            OnBallHit?.Invoke(this);
            gameObject.SetActive(false);
            active = false;
        }
    }
}
