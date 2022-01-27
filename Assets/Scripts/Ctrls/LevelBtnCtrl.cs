using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelBtnCtrl : MonoBehaviour
{
    public int m_levelID; // start from 0

    private Text m_levelLable;
    private Image m_btnBG;

    private bool m_levelPressed = false;

    private float m_minHeight;
    private float m_maxHeight;

    void Awake()
    {
        m_levelLable = GetComponentInChildren<Text>();
        m_btnBG = GetComponent<Image>();
    }

    void Start()
    {
        m_levelLable.text = m_levelID.ToString();
        GameManager.Instance.GetEventManager().StartListening("Level" + m_levelID.ToString(), OnPressed);
        GameManager.Instance.GetEventManager().StartListening(Consts.EVENT_ELEVATOR_STOP, OnElevatorStop);

        m_minHeight = m_levelID - 0.1f; // TODO config
        m_maxHeight = m_levelID + 0.6f;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnPressed(Dictionary<string, object> message)
    {
        if ((bool)message["pressed"])
        {
            m_levelPressed = true;
            m_btnBG.color = Color.red;
        }
    }

    void OnElevatorStop(Dictionary<string, object> message)
    {
        float elevatorLevel = (float)message["level"];

        Debug.Log(string.Format("{0} => {1}", m_minHeight, m_maxHeight));
        if(elevatorLevel >= m_minHeight && elevatorLevel <= m_maxHeight && m_levelPressed)
        {
            m_levelPressed = false;
            m_btnBG.color = Color.green;

            GameManager.Instance.OnLevelArrived(m_levelID);

            StartCoroutine(PassengerOff());
        }

    }

    IEnumerator PassengerOff()
    {
        yield return new WaitForSeconds(3.0f);
        if(!m_levelPressed)
            m_btnBG.color = Color.white;
    }
}
