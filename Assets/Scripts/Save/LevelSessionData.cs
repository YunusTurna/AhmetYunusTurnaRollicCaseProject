using System;

[Serializable]
public class LevelSessionData
{
    public int levelIndex;
    public string levelName;
    public float timeSpent;
    public int timeExtensionsUsed;
    public int blockDestroyersUsed;
    public bool completed;
    public string timestamp;
}
