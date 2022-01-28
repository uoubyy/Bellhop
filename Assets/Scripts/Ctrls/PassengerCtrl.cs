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
    private EmotionState m_emotionState;
    public float m_bestDeliverTime;
    private float m_currentTime;

    public int m_targetLevel;

    public float m_mass;
    public bool m_delivered = false;

    private EmotionInfo curEmotionConfig;

    private Rigidbody m_rigidbody;

    public void Init(float bestDeliverTime, int targetLevel, EmotionState initialState)
    {
        m_bestDeliverTime = bestDeliverTime;
        m_currentTime = 0.0f;
        m_targetLevel = targetLevel;
        m_emotionState = initialState;
        curEmotionConfig = ConfigManager.Instance.GetEmoitionConfig(initialState);
    }

    public void Reset()
    {
        m_bestDeliverTime = 0.0f;
        m_currentTime = 0.0f;
        m_targetLevel = 0;
        m_delivered = false;
    }

    void Start()
    {
        GameManager.Instance.GetEventManager().StartListening(Event.EVENT_ELEVATOR_STOP, OnElevatorStop);
        m_rigidbody = GetComponent<Rigidbody>();
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
    }

    public float GetReward()
    {
        return ConfigManager.Instance.GetEmoitionConfig(m_emotionState).rewardNum;
    }

    void OnElevatorStop(Dictionary<string, object> message)
    {
        if (!gameObject.activeSelf)
            return;

        float data = (float)message["level"];
        int level = (int)data;

        if(level == m_targetLevel)
        {
            m_delivered = true;

            float reward = GetReward();
            GameManager.Instance.GetEventManager().InvokeEvent(Event.EVENT_PASSENGER_DELVERED, new Dictionary<string, object> { { "level", level }, { "reward", reward } });

            // StartCoroutine(AfterDeliver());
            Reset();
            var poolable = gameObject.GetComponent<Poolable>();
            PoolManager.Instance.ReturnPoolable(poolable);
        }
    }

    IEnumerator AfterDeliver()
    {
        yield return new WaitForSeconds(3.0f);
        var poolable = gameObject.GetComponent<Poolable>();
        PoolManager.Instance.ReturnPoolable(poolable);
    }

    private void OnTriggerEnter(Collider other)
    {
        transform.SetParent(other.transform);
        m_rigidbody.isKinematic = true;
    }
}
