using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    private InputManager m_InputManager;

    private EventManager m_eventManager;

    // Start is called before the first frame update

    void Awake()
    {
        m_InputManager = GetComponent<InputManager>();
        m_eventManager = GetComponent<EventManager>();
        m_eventManager.Init();
    }

    void Start()
    {

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
