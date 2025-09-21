// TrialLogger.cs
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Text;

public class TrialLogger : MonoBehaviour
{
    private List<TrialResult> trials = new List<TrialResult>();
    private string sessionFolder;
    private string summaryFilePath;

    public void StartSession(string playerName = "DefaultPlayer")
    {
        sessionFolder = Path.Combine(Application.persistentDataPath, "Sessions");
        if (!Directory.Exists(sessionFolder)) Directory.CreateDirectory(sessionFolder);

        string timeStamp = System.DateTime.Now.ToString("yyyyMMdd_HHmmss");
        summaryFilePath = Path.Combine(sessionFolder, $"session_{playerName}_{timeStamp}.csv");

        // header
        File.WriteAllText(summaryFilePath,
            "trialIndex,trialType,throwType,stance,audioTrial,hit,reactionTime,spawnTime,endTime,notes\n",
            Encoding.UTF8);

        trials.Clear();
    }

    public void LogTrial(TrialResult r)
    {
        trials.Add(r);

        string line = string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9}",
            r.trialIndex,
            Sanitize(r.trialType),
            r.throwType.ToString(),
            Sanitize(r.stance),
            r.audioTrial ? 1 : 0,
            r.hit ? 1 : 0,
            r.reactionTime.ToString("F4"),
            r.spawnTime.ToString("F4"),
            r.endTime.ToString("F4"),
            Sanitize(r.notes)
        );

        File.AppendAllText(summaryFilePath, line + "\n", Encoding.UTF8);
    }

    public void SaveAndClose()
    {
        Debug.Log("TrialLogger saved session summary to: " + summaryFilePath);
    }

    string Sanitize(string s)
    {
        if (string.IsNullOrEmpty(s)) return "";
        return s.Replace(",", ";").Replace("\n", " ");
    }
}
