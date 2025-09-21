// TrialResult.cs
using System;

[Serializable]
public class TrialResult
{
    public int trialIndex;
    public string trialType; // user-friendly
    public ThrowType throwType;
    public string stance = "";
    public bool audioTrial = false;
    public bool hit = false;
    public float reactionTime = -1f;
    public float spawnTime = 0f;
    public float endTime = 0f;
    public string notes = "";
}
