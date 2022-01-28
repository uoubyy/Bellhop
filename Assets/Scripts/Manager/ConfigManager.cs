using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

[System.Serializable]
public class DifficultyInfo
{
    public int levelID;
    public int maxPassenger;
    public float minDeliverTime;
    public float maxDeliverTime;
    public float gametime;
    public int passNum;
    public float[] posibility;
}

[System.Serializable]
public class EmotionsInfoList
{
    public EmotionInfo[] EmotionList;
}

[System.Serializable]
public class DifficultyInfoList
{
    public DifficultyInfo[] DifficultyList;
}

public class ConfigManager : Singleton<ConfigManager>
{
#if UNITY_EDITOR || DEBUG
    public List<DifficultyInfo> debugDifficultyConfig;
#endif

    public Dictionary<int, DifficultyInfo> difficultiesConfig;
    public Dictionary<EmotionState, EmotionInfo> emotionsConfig;

    public EmotionInfo GetEmoitionConfig(EmotionState emotion) { return emotionsConfig[emotion]; }
    
    // difficulty start from 1
    public DifficultyInfo GetDifficultyConfig(int difficulty) {
        return difficultiesConfig[difficulty];
    }

    protected override void OnAwake()
    {
        base.OnAwake();

        TextAsset emotionConfigAsset = Resources.Load("Config/EmotionConfig") as TextAsset;
        TextAsset difficultyConfigAsset = Resources.Load("Config/DifficultyConfig") as TextAsset;

        difficultiesConfig = new Dictionary<int, DifficultyInfo>();
        emotionsConfig = new Dictionary<EmotionState, EmotionInfo>();

        Assert.IsNotNull(emotionConfigAsset);
        Assert.IsNotNull(difficultyConfigAsset);

        EmotionsInfoList emotionsList = JsonUtility.FromJson<EmotionsInfoList>("{\"EmotionList\":" + emotionConfigAsset.text + "}");
        foreach(var info in emotionsList.EmotionList)
        {
            emotionsConfig.Add(info.emotionId, info);
        }

        DifficultyInfoList difficultyList = JsonUtility.FromJson<DifficultyInfoList>("{\"DifficultyList\":" + difficultyConfigAsset.text + "}");
        foreach (var info in difficultyList.DifficultyList)
        {
            difficultiesConfig.Add(info.levelID, info);
        }
    }

    public int MaxDifficulty() { return difficultiesConfig.Count; }
}
