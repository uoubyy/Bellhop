using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameState
{
    GS_Idle,
    GS_Running,
    GS_GameOver
}

public class GameManager : Singleton<GameManager>
{
    private InputManager m_InputManager;

    private EventManager m_eventManager;

    private PassengersManager m_passengersManager;

    private FloorManager m_floorManager;

    private float m_timeCountDown = 0.0f; // read from config

    private GameState m_gameState;

    protected override void OnAwake()
    {
        base.OnAwake();
        m_InputManager = GetComponent<InputManager>();
        m_eventManager = GetComponent<EventManager>();
        m_passengersManager = GetComponent<PassengersManager>();
        m_floorManager = GetComponent<FloorManager>();

        m_eventManager.Init();
    }

    void Start()
    {
        m_gameState = GameState.GS_Idle;
        m_eventManager.StartListening(Event.EVENT_PASSENGER_DELVERED, OnPassengerDelivered);
    }

    public void OnGameStart()
    {
        m_gameState = GameState.GS_Running;

        m_passengersManager.OnGameStart(1);

        DifficultyInfo levelInfo = ConfigManager.Instance.GetDifficultyConfig(1);
        m_timeCountDown = levelInfo.gametime;

        m_eventManager.InvokeEvent(Event.EVENT_GAME_START, new Dictionary<string, object> { { "gametime", levelInfo.gametime } });
        m_InputManager.EnableInput(true);
    }

    public void OnGameOver()
    {
        m_gameState = GameState.GS_GameOver;
        PoolManager.Instance.ReturnAll();
        m_eventManager.InvokeEvent(Event.EVENT_GAME_OVER, null);
    }

    // Update is called once per frame
    void Update()
    {
/*        if (m_gameState == GameState.GS_Running)
        {
            m_timeCountDown -= Time.deltaTime;

            if (m_timeCountDown <= 0.0)
                OnGameOver();
        }*/
    }

    public GameState GetGameState() { return m_gameState; }

    public InputManager GetInputManager()
    {
        return m_InputManager;
    }

    public EventManager GetEventManager()
    {
        return m_eventManager;
    }

    public void OnLevelArrived(int levelID)
    {
        m_InputManager.EnableInput(false);
        StartCoroutine(PassengerOff());
    }

    IEnumerator PassengerOff()
    {
        yield return new WaitForSeconds(3.0f);
        m_InputManager.EnableInput(true);
    }

    private void OnPassengerDelivered(Dictionary<string, object> message)
    {
        float reward = (float)message["reward"];
        m_timeCountDown += reward;
    }
}
