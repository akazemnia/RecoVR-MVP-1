// ThrowConfig.cs
using UnityEngine;

[System.Serializable]
public enum ThrowType { Basic, AppearingDisappearing, Curve }

[System.Serializable]
public class ThrowConfig
{
    public ThrowType throwType = ThrowType.Basic;
    public float speed = 6f;                  // initial speed for Basic / Appearing
    public float lifetime = 6f;               // max lifetime before considered missed
    public float appearInterval = 0.12f;      // for AppearingDisappearing
    public float curveHeight = 1.0f;          // for Curve: control point height (relative)
    public float curveDuration = 2.5f;        // how long the curve lasts
    public Vector3 targetOffset = Vector3.zero; // random offset from target
    public string trialLabel = "VisionThrow"; // friendly string for CSV
}
