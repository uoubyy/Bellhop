using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    private InputManager m_InputManager;

    private EventManager m_eventManager;

    private PassengersManager m_passengersManager;

    private float m_timeCountDown;

    protected override void OnAwake()
    {
        base.OnAwake();
        m_InputManager = GetComponent<InputManager>();
        m_eventManager = GetComponent<EventManager>();
        m_passengersManager = GetComponent<PassengersManager>();

        m_eventManager.Init();
    }

    void Start()
    {
        OnGameStart();
    }

    public void OnGameStart()
    {
        m_passengersManager.OnGameStart(1);
        m_eventManager.InvokeEvent(Consts.EVENT_GAME_START, null);
    }

    public void OnGameOver()
    {
        // m_eventManager.InvokeEvent(Consts.EVENT_GAME_OVER, null);
    }

    // Update is called once per frame
    void Update()
    {
        m_timeCountDown -= Time.deltaTime;

        if (m_timeCountDown <= 0.0)
            OnGameOver();

    }

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

    }
}
