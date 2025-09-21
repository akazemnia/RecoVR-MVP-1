// GameManager.cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq; // for OrderBy

public class GameManager : MonoBehaviour
{
    [Header("References")]
    public ThrowManager throwManager;
    public TrialLogger trialLogger;
    public StanceManager stanceManager;
    public AudioCueManager audioCueManager;
    public GameUIController uiController;

    [Header("Gameplay")]
    public List<ThrowConfig> pitchTypes = new List<ThrowConfig>(); // assign Basic, Curve, Appearing etc. in Inspector
    public int totalPitches = 20; // number of trials per run
    public float interTrialDelay = 1.5f;

    private List<ThrowConfig> trialSequence = new List<ThrowConfig>();
    private int currentIndex = 0;
    private bool isRunning = false;

    void Start()
    {
        if (trialLogger != null)
        {
            trialLogger.StartSession("DefaultPlayer");
        }

        // build a randomized sequence of pitches
        GenerateRandomTrials();
    }

    /// <summary>
    /// Builds a randomized sequence of pitches for this run.
    /// </summary>
    private void GenerateRandomTrials()
    {
        trialSequence.Clear();

        // Pick random pitches from the available pitchTypes list
        for (int i = 0; i < totalPitches; i++)
        {
            int randIndex = Random.Range(0, pitchTypes.Count);
            trialSequence.Add(pitchTypes[randIndex]);
        }

        // Shuffle order
        trialSequence = trialSequence.OrderBy(x => Random.value).ToList();

        Debug.Log("Generated randomized trial sequence with " + trialSequence.Count + " pitches.");
    }

    public void StartRun()
    {
        if (!isRunning) StartCoroutine(RunTrials());
    }

    IEnumerator RunTrials()
    {
        isRunning = true;
        uiController.ShowMessage("Get Ready...");
        yield return uiController.CountdownSeconds(3);

        for (currentIndex = 0; currentIndex < trialSequence.Count; currentIndex++)
        {
            var cfg = trialSequence[currentIndex];

            // Optional stance requirement
            if (stanceManager != null)
            {
                uiController.ShowMessage($"Assume stance: {stanceManager.GetRequiredStanceString()}");
                yield return stanceManager.PromptAndWaitForStance(5f);
            }

            // spawn throw
            uiController.ShowMessage($"Trial {currentIndex + 1}/{trialSequence.Count}");
            GameObject ballGO = throwManager.SpawnThrow(cfg);

            Ball ball = ballGO.GetComponent<Ball>();
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

            // wait until ball expires or hit
            float maxWait = cfg.lifetime + 0.5f;
            float t0 = Time.time;
            while (Time.time - t0 < maxWait && !hit)
                yield return null;

            // unsubscribe
            ball.OnBallHit -= onHit;

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

            // delay
            yield return new WaitForSeconds(interTrialDelay);
        }

        // Done
        trialLogger.SaveAndClose();
        uiController.ShowMessage("Session complete!");
        isRunning = false;
    }

    // Optional: add pitch type at runtime
    public void AddTrial(ThrowConfig cfg)
    {
        trialSequence.Add(cfg);
    }
}
