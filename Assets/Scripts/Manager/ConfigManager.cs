using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

[System.Serializable]
public class DifficultyInfo
{
    public int levelID;
    public int maxPassenger;

    public Vector2 bestDeliverTime;
    // TODO
}

public class ConfigManager : MonoBehaviour
{
#if UNITY_EDITOR || DEBUG
    public List<DifficultyInfo> debugDifficultyConfig;
#endif

    public Dictionary<int, DifficultyInfo> difficultiesConfig;
    public Dictionary<EmotionState, EmotionInfo> emotionsConfig;

    public EmotionInfo GetEmoitionConfig(EmotionState emotion) { return emotionsConfig[emotion]; }
    public DifficultyInfo GetDifficultyConfig(int difficulty) {

#if UNITY_EDITOR || DEBUG
        return debugDifficultyConfig[difficulty - 1];
#else
        return difficultiesConfig[difficulty];
#endif
    }

    // Start is called before the first frame update
    public void Init()
    {
        TextAsset emotionConfigAsset = Resources.Load("Config/EmotionConfig") as TextAsset;
        TextAsset difficultyConfigAsset = Resources.Load("Config/DifficultyConfig") as TextAsset;

        difficultiesConfig = new Dictionary<int, DifficultyInfo>();
        emotionsConfig = new Dictionary<EmotionState, EmotionInfo>();

        Assert.IsNotNull(emotionConfigAsset);
        Assert.IsNotNull(difficultyConfigAsset);

        // TODO read config file

        DifficultyInfo level1 = new DifficultyInfo();
        level1.levelID = 1;
        level1.maxPassenger = 2;

        DifficultyInfo level2 = new DifficultyInfo();
        level2.levelID = 2;
        level2.maxPassenger = 3;

        DifficultyInfo level3 = new DifficultyInfo();
        level3.levelID = 3;
        level3.maxPassenger = 5;

        difficultiesConfig.Add(1, level1);
        difficultiesConfig.Add(2, level2);
        difficultiesConfig.Add(3, level3);
    }
}
