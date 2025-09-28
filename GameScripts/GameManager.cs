// GameManager.cs
using System.Collections;
using System.Collections.Generic;
using System.Linq; // used for shuffle
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("References")]
    public ThrowManager throwManager;
    public TrialLogger trialLogger;
    public StanceManager stanceManager;
    public AudioCueManager audioCueManager;
    public GameUIController uiController;

    [Header("Gameplay")]
    public List<ThrowConfig> trialSequence = new List<ThrowConfig>(); // filled at runtime by GenerateRandomTrials()
    [Tooltip("Pitch prototypes to choose from (configure in Inspector).")]
    public List<ThrowConfig> pitchTypes = new List<ThrowConfig>(); // assign Basic, Curve, Appearing etc.
    [Tooltip("How many randomized pitches to run")]
    public int totalPitches = 20;
    public float interTrialDelay = 1.5f;
    public bool autoStart = false; // if true, StartRun() is called automatically

    private int currentIndex = 0;
    private bool isRunning = false;

    void Start()
    {
        if (trialLogger != null)
        {
            trialLogger.StartSession("DefaultPlayer");
        }

        GenerateRandomTrials();

        if (autoStart)
        {
            StartRun();
        }
    }

    /// <summary>
    /// Build a randomized sequence of pitches using pitchTypes as prototypes.
    /// Creates copies of prototypes so changing one doesn't affect others at runtime.
    /// </summary>
    private void GenerateRandomTrials()
    {
        trialSequence.Clear();

        if (pitchTypes == null || pitchTypes.Count == 0)
        {
            Debug.LogWarning("[GameManager] pitchTypes is empty — no pitches available. Populate pitchTypes in the Inspector.");
            return;
        }

        for (int i = 0; i < totalPitches; i++)
        {
            int randIndex = Random.Range(0, pitchTypes.Count);
            ThrowConfig proto = pitchTypes[randIndex];
            ThrowConfig copy = CloneThrowConfig(proto);
            trialSequence.Add(copy);
        }

        // Shuffle the list to avoid clustering
        trialSequence = trialSequence.OrderBy(x => Random.value).ToList();

        Debug.Log($"[GameManager] Generated randomized trial sequence with {trialSequence.Count} pitches.");
    }

    // Make a shallow copy of the ThrowConfig so we don't mutate the inspector asset
    private ThrowConfig CloneThrowConfig(ThrowConfig src)
    {
        if (src == null) return null;
        return new ThrowConfig
        {
            throwType = src.throwType,
            speed = src.speed,
            lifetime = src.lifetime,
            appearInterval = src.appearInterval,
            curveHeight = src.curveHeight,
            curveDuration = src.curveDuration,
            targetOffset = src.targetOffset,
            trialLabel = src.trialLabel
        };
    }

    public void StartRun()
    {
        if (!isRunning)
        {
            if (trialSequence == null || trialSequence.Count == 0)
            {
                Debug.LogWarning("[GameManager] trialSequence is empty. Generating trials now.");
                GenerateRandomTrials();
            }

            StartCoroutine(RunTrials());
        }
    }

    IEnumerator RunTrials()
    {
        if (throwManager == null || trialLogger == null || uiController == null)
        {
            Debug.LogError("[GameManager] One or more core references (throwManager / trialLogger / uiController) are not assigned in the Inspector.");
            yield break;
        }

        isRunning = true;
        uiController.ShowMessage("Get Ready...");
        yield return uiController.CountdownSeconds(3);

        for (currentIndex = 0; currentIndex < trialSequence.Count; currentIndex++)
        {
            var cfg = trialSequence[currentIndex];
            if (cfg == null)
            {
                Debug.LogWarning($"[GameManager] trialSequence[{currentIndex}] is null — skipping.");
                continue;
            }

            // Optional: check stance before some trials
            if (stanceManager != null)
            {
                uiController.ShowMessage($"Assume stance: {stanceManager.GetRequiredStanceString()}");
                yield return stanceManager.PromptAndWaitForStance(5f); // timeout 5s
            }

            // spawn throw (safety)
            uiController.ShowMessage($"Trial {currentIndex + 1}/{trialSequence.Count}");
            if (throwManager == null)
            {
                Debug.LogError("[GameManager] throwManager not assigned. Cannot spawn throws.");
                break;
            }

            GameObject ballGO = throwManager.SpawnThrow(cfg);
            if (ballGO == null)
            {
                Debug.LogWarning("[GameManager] throwManager returned null ball GameObject.");
                yield return new WaitForSeconds(interTrialDelay);
                continue;
            }

            Ball ball = ballGO.GetComponent<Ball>();
            if (ball == null)
            {
                Debug.LogWarning("[GameManager] Spawned object does not contain Ball component.");
                yield return new WaitForSeconds(interTrialDelay);
                continue;
            }

            float spawnTime = Time.time;
            bool hit = false;
            float reactionTime = -1f;

            // subscribe to OnBallHit
            System.Action<Ball> onHit = null;
            onHit = (b) =>
            {
                if (b == ball)
                {
                    hit = true;
                    reactionTime = Time.time - spawnTime;
                }
            };

            ball.OnBallHit += onHit;

            // Wait until ball expires (Ball will return to pool) or it's hit
            float maxWait = cfg.lifetime + 0.5f;
            float t0 = Time.time;
            while (Time.time - t0 < maxWait && !hit)
                yield return null;

            // ensure unsubscribed safely (ball might be destroyed/returned)
            try { ball.OnBallHit -= onHit; } catch { /* ignore */ }

            // Compose result
            TrialResult r = new TrialResult
            {
                trialIndex = currentIndex,
                trialType = cfg.trialLabel,
                throwType = cfg.throwType,
                stance = (stanceManager != null) ? stanceManager.GetCurrentStanceName() : "",
                audioTrial = false,
                hit = hit,
                reactionTime = hit ? reactionTime : -1f,
                spawnTime = spawnTime,
                endTime = Time.time,
                notes = hit ? "Hit" : "Miss/Timeout"
            };

            // Log
            trialLogger.LogTrial(r);

            // UI feedback
            uiController.ShowTrialResult(hit, reactionTime);

            // inter trial gap
            yield return new WaitForSeconds(interTrialDelay);
        }

        // Done with sequence
        trialLogger.SaveAndClose();
        uiController.ShowMessage("Session complete!");
        isRunning = false;
    }

    // Optional quick method to add trial configs at runtime
    public void AddTrial(ThrowConfig cfg)
    {
        if (cfg != null) trialSequence.Add(CloneThrowConfig(cfg));
    }
}
