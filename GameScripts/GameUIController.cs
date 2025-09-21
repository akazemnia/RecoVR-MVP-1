// GameUIController.cs
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameUIController : MonoBehaviour
{
public TextMeshProUGUI messageText;
public TextMeshProUGUI trialText;
public TextMeshProUGUI feedbackText;

    public IEnumerator CountdownSeconds(int seconds)
    {
        for (int i = seconds; i > 0; i--)
        {
            ShowMessage(i.ToString());
            yield return new WaitForSeconds(1f);
        }
        ShowMessage("Go!");
        yield return new WaitForSeconds(0.5f);
        ClearMessage();
    }

    public void ShowMessage(string s)
    {
        if (messageText) messageText.text = s;
    }

    public void ClearMessage()
    {
        if (messageText) messageText.text = "";
    }

    public void ShowTrialResult(bool hit, float reactionTime)
    {
        if (feedbackText == null) return;
        if (hit)
        {
            feedbackText.text = $"Hit! RT: {reactionTime:F3}s";
        }
        else
        {
            feedbackText.text = $"Miss";
        }
        StartCoroutine(ClearFeedbackAfter(2f));
    }

    IEnumerator ClearFeedbackAfter(float t)
    {
        yield return new WaitForSeconds(t);
        if (feedbackText) feedbackText.text = "";
    }
}
