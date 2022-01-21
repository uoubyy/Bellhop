using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Assertions;

public class ElevatorCtrl : MonoBehaviour
{
    public Text m_speedIndictor;
    public Text m_heightIndictor;

    public float MaxSpeed = 100.0f;

    private float m_minSpeed = 0.0f;
    private float m_maxSpeed = 0.0f;

    public float MaxHeight = 200.0f;
    public float MaxForce = 5.0f;

    public float m_gravity = 9.8f;

    private float m_mass = 10.0f;
    private float m_speed = 0.0f;
    private float m_height = 0.0f;

    private float m_pullingForce;

    private bool m_running = false;

    // Start is called before the first frame update
    void Start()
    {
        GameManager.Instance.GetEventManager().StartListening(Consts.EVENT_ELEVATOR_UP, OnUpBtnStateChange);
        GameManager.Instance.GetEventManager().StartListening(Consts.EVENT_ELEVATOR_DOWN, OnDownBtnStateChange);
    }

    // Update is called once per frame
    void Update()
    {
        bool m_prevState = m_running;

        float acc = (m_pullingForce - m_mass * m_gravity) / m_mass;
        float deltaV = acc * Time.deltaTime;
        m_speed += deltaV;

        m_speed = Mathf.Clamp(m_speed, m_minSpeed, m_maxSpeed);
        m_speedIndictor.text = string.Format("Speed {0,7:f3}", m_speed);

        if (m_speed <= -0.05f || m_speed >= 0.05f)
            m_running = true;
        else
            m_running = false;

        if (m_running)
            m_height += 0.5f * acc * Time.deltaTime * Time.deltaTime;

        m_height = Mathf.Clamp(m_height, 0.0f, MaxHeight);
        m_heightIndictor.text = string.Format("Height {0,7:f3}", m_height);

        if(m_prevState && !m_running)
        {
            GameManager.Instance.GetEventManager().InvokeEvent(Consts.EVENT_ELEVATOR_STOP, new Dictionary<string, object> { { "height", m_height} });
        }
    }

    private void OnUpBtnStateChange(Dictionary<string, object> message)
    {
        if ((bool)message["pressed"])
        {
            m_pullingForce = MaxForce;
            m_minSpeed = 0.0f;
            m_maxSpeed = MaxSpeed;
        }
        else if ((bool)message["released"])
        {
            m_pullingForce = 0.0f;
            m_minSpeed = 0.0f;
            m_maxSpeed = 0.0f;
        }
    }

    private void OnDownBtnStateChange(Dictionary<string, object> message)
    {
        if ((bool)message["pressed"])
        {
            m_pullingForce = -MaxForce;
            m_minSpeed = -MaxSpeed;
            m_maxSpeed = 0.0f;
        }
        else if ((bool)message["released"])
        {
            m_pullingForce = 0.0f;
            m_minSpeed = 0.0f;
            m_maxSpeed = 0.0f;
        }
    }
}
