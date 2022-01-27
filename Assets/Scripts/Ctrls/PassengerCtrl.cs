using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum EmotionState
{
    ES_HAPPY,
    ES_NEUTRAL,
    ES_TENSION,
    ES_ANGER,
    ES_SAD
}

[System.Serializable]
public class EmotionInfo
{
    public EmotionState emotionId;
    public float duration;
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

    public void Init(float bestDeliverTime, int targetLevel)
    {
        m_bestDeliverTime = bestDeliverTime;
        m_currentTime = 0.0f;
        m_targetLevel = targetLevel;
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
        GameManager.Instance.GetEventManager().StartListening(Consts.EVENT_ELEVATOR_STOP, OnElevatorStop);
    }

    void Update()
    {
        if (m_delivered)
            return;

        m_currentTime += Time.deltaTime;

        UpdateEmotionState();
    }

    protected void UpdateEmotionState()
    {

    }

    public float GetReward()
    {
        switch(m_emotionState)
        {
            case EmotionState.ES_HAPPY:

                break;
            case EmotionState.ES_NEUTRAL:
                break;
            case EmotionState.ES_TENSION:
                break;
            case EmotionState.ES_ANGER:
                break;
            case EmotionState.ES_SAD:
                break;
        }

        return 0.0f;
    }

    void OnElevatorStop(Dictionary<string, object> message)
    {
        float data = (float)message["level"];
        int level = (int)data;

        if(level == m_targetLevel)
        {
            m_delivered = true;

            GameManager.Instance.GetEventManager().InvokeEvent(Consts.EVENT_PASSENGER_DELVERED, new Dictionary<string, object> { { "level", level } });

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
}
