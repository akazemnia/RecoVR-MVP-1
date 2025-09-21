// AudioCueManager.cs
using System.Collections;
using UnityEngine;

public class AudioCueManager : MonoBehaviour
{
    public Transform[] sourcePositions; // set to transforms around player
    public AudioClip cueClip;
    public Transform headTransform;
    public float allowedHeadRotationDegrees = 6f;

    public IEnumerator RunAudioTrial(System.Action<int, bool> callback)
    {
        int idx = Random.Range(0, sourcePositions.Length);
        var pos = sourcePositions[idx];

        GameObject go = new GameObject("AudioTemp");
        go.transform.position = pos.position;
        var audioSource = go.AddComponent<AudioSource>();
        audioSource.clip = cueClip;
        audioSource.spatialBlend = 1f;
        audioSource.Play();

        Quaternion initial = headTransform.rotation;
        float start = Time.time;
        float duration = cueClip != null ? cueClip.length + 0.2f : 1.0f;

        bool movedTooMuch = false;
        while (Time.time - start < duration)
        {
            float angle = Quaternion.Angle(initial, headTransform.rotation);
            if (angle > allowedHeadRotationDegrees) { movedTooMuch = true; break; }
            yield return null;
        }

        Destroy(go);

        callback?.Invoke(idx, !movedTooMuch); // callback(index, success)
    }
}
