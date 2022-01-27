using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    private InputManager m_InputManager;

    private EventManager m_eventManager;

    private PassengersManager m_passengersManager;

    private ConfigManager m_configManager;

    protected override void OnAwake()
    {
        base.OnAwake();
        m_InputManager = GetComponent<InputManager>();
        m_eventManager = GetComponent<EventManager>();
        m_passengersManager = GetComponent<PassengersManager>();
        m_configManager = GetComponent<ConfigManager>();

        m_eventManager.Init();
        m_configManager.Init();
    }

    void Start()
    {
        m_passengersManager.OnGameStart(1);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public InputManager GetInputManager()
    {
        return m_InputManager;
    }

    public EventManager GetEventManager()
    {
        return m_eventManager;
    }

    public ConfigManager GetConfigManager()
    {
        return m_configManager;
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
}
