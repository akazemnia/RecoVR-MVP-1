// StanceManager.cs
using System.Collections;
using UnityEngine;

public enum StanceType { None, FeetTogether, ShoulderWidth, Staggered }

public class StanceManager : MonoBehaviour
{
    [Header("References (set your controller/head transforms)")]
    public Transform head;
    public Transform leftController;
    public Transform rightController;

    // thresholds (tune)
    public float feetTogetherThreshold = 0.45f;
    public float shoulderMin = 0.45f;
    public float shoulderMax = 0.8f;
    public float staggerThreshold = 0.2f; // difference in Z between controllers to indicate stagger

    private StanceType requiredStance = StanceType.FeetTogether;
    private StanceType lastDetected = StanceType.None;

    public string GetRequiredStanceString()
    {
        return requiredStance.ToString();
    }

    public string GetCurrentStanceName()
    {
        return lastDetected.ToString();
    }

    public IEnumerator PromptAndWaitForStance(float timeout)
    {
        float start = Time.time;
        while (Time.time - start < timeout)
        {
            if (CheckStance(requiredStance))
            {
                lastDetected = requiredStance;
                yield break;
            }
            yield return null;
        }
        // timeout: keep lastDetected as is (maybe None)
    }

    public bool CheckStance(StanceType s)
    {
        if (leftController == null || rightController == null) return false;
        float handDist = Vector3.Distance(leftController.position, rightController.position);
        float zDiff = leftController.position.z - rightController.position.z;

        switch (s)
        {
            case StanceType.FeetTogether:
                return handDist < feetTogetherThreshold;
            case StanceType.ShoulderWidth:
                return handDist >= shoulderMin && handDist <= shoulderMax;
            case StanceType.Staggered:
                return Mathf.Abs(zDiff) > staggerThreshold;
            default:
                return false;
        }
    }

    // Use this to set next required stance externally
    public void SetRequiredStance(StanceType s)
    {
        requiredStance = s;
    }
}
