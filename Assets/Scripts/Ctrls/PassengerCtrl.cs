using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum EmotionState
{
    ES_HAPPY = 0,
    ES_NEUTRAL,
    //ES_TENSION,
    ES_ANGER,
    //ES_SAD,
    ES_COUNT // never use, just as end indicator
}

[System.Serializable]
public class EmotionInfo
{
    public EmotionState emotionId;
    public float duration; // percentage in 100
    public int rewardId;
    public float rewardNum;
}

public class PassengerCtrl : MonoBehaviour
{
    private int m_passengerID; // unique id

    private EmotionState m_emotionState;
    public float m_bestDeliverTime;
    private float m_currentTime;

    public int m_targetLevel;

    public float m_mass;
    public bool m_delivered = false;

    private EmotionInfo curEmotionConfig;

    private Rigidbody m_rigidbody;

    public PassengerInfo m_HUD;

    public void Init(int id, float bestDeliverTime, int targetLevel, EmotionState initialState)
    {
        m_passengerID = id;
        m_bestDeliverTime = bestDeliverTime;
        m_currentTime = 0.0f;
        m_targetLevel = targetLevel;
        m_emotionState = initialState;
        curEmotionConfig = ConfigManager.Instance.GetEmoitionConfig(initialState);

        m_HUD.Init(initialState, targetLevel);

        StartCoroutine(BoardElevator());
    }

    public void Reset()
    {
        m_bestDeliverTime = 0.0f;
        m_currentTime = 0.0f;
        m_targetLevel = 0;
        m_delivered = false;
    }

    private void Awake()
    {
        GameManager.Instance.GetEventManager().StartListening(Event.EVENT_ELEVATOR_STOP, OnElevatorStop);
        GameManager.Instance.GetEventManager().StartListening(Event.EVENT_CATASTROPHE_FIRE, OnCatastropheFire);
        //m_rigidbody = GetComponent<Rigidbody>();
    }

    void Update()
    {
        if (m_delivered || m_emotionState >= EmotionState.ES_ANGER)
            return;

        m_currentTime += Time.deltaTime;

        UpdateEmotionState();
    }

    protected void UpdateEmotionState()
    {
        if (m_currentTime < curEmotionConfig.duration * m_bestDeliverTime)
            return;

        m_currentTime = 0.0f;
        int state = (int)m_emotionState + 1;
        state = Mathf.Min(state, (int)EmotionState.ES_COUNT - 1);
        m_emotionState = (EmotionState)state;

        m_HUD.UpdateEmotion(m_emotionState);
    }

    public float GetReward()
    {
        return ConfigManager.Instance.GetEmoitionConfig(m_emotionState).rewardNum;
    }

    void OnElevatorStop(Dictionary<string, object> message)
    {
        if (!gameObject.activeSelf)
            return;

        int level = (int)message["level"];

        if(level == m_targetLevel)
        {
            m_delivered = true;

            float reward = GetReward();
            GameManager.Instance.GetEventManager().InvokeEvent(Event.EVENT_PASSENGER_DELVERED, new Dictionary<string, object> { { "level", level }, { "reward", reward } });

            StartCoroutine(AfterDeliver());
        }
    }

    void OnCatastropheFire(Dictionary<string, object> message)
    {
        m_emotionState = EmotionState.ES_ANGER;
    }

    IEnumerator AfterDeliver()
    {
        float elapsedTime = 0.0f;

        while (elapsedTime <= 0.8f)
        {
            elapsedTime += Time.deltaTime;

            transform.position += new Vector3(0.0f, 0.0f, 30f) * Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        Reset();
        var poolable = gameObject.GetComponent<Poolable>();
        PoolManager.Instance.ReturnPoolable(poolable);
    }

    IEnumerator BoardElevator()
    {
        float elapsedTime = 0.0f;

        while (elapsedTime <= 0.8f)
        {
            elapsedTime += Time.deltaTime;

            transform.position += new Vector3(0.0f, 0.0f, -30f) * Time.deltaTime;
            m_HUD.UpdatePos(transform.position);
            yield return new WaitForEndOfFrame();
        }
    }
}
