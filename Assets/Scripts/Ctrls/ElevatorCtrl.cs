using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Assertions;

public class ElevatorCtrl : MonoBehaviour
{
    public enum ElevateState
    {
        ES_Idle,
        ES_Up,
        ES_Down
    }

    public Text m_speedIndictor;
    public Text m_heightIndictor;
    public Text m_levelIndictor;
    public Text m_timeIndictor;

    public float MaxSpeed = 100.0f;

    private float m_minSpeed = 0.0f;
    private float m_maxSpeed = 0.0f;

    public float MaxHeight = 200.0f;
    public float MaxForce = 5.0f;

    public float m_gravity = 9.8f;

    public float m_levelHeight = 1.0f;

    private float m_mass = 10.0f;
    private float m_speed = 0.0f;
    private float m_height = 0.0f;

    private float m_runningTime = 0.0f;

    private float m_pullingForce;

    private ElevateState m_elevatorState = ElevateState.ES_Idle;

    private Rigidbody rb;

    // Start is called before the first frame update
    void Start()
    {
        GameManager.Instance.GetEventManager().StartListening(Event.EVENT_ELEVATOR_UP, OnUpBtnStateChange);
        GameManager.Instance.GetEventManager().StartListening(Event.EVENT_ELEVATOR_DOWN, OnDownBtnStateChange);

        GameManager.Instance.GetEventManager().StartListening(Event.EVENT_GAME_START, OnGameStart);
        GameManager.Instance.GetEventManager().StartListening(Event.EVENT_GAME_OVER, OnGameOver);

        rb = GetComponent<Rigidbody>();

        Reset();
    }

    public void OnGameStart(Dictionary<string, object> message)
    {

    }

    public void OnGameOver(Dictionary<string, object> message)
    {
        Reset();
    }

    private void Reset()
    {
        m_runningTime = 0.0f;
    }

    // Update is called once per frame
    void Update()
    {
         m_speedIndictor.text = string.Format("Speed {0,7:f3}", rb.velocity.y);
        return;
#if UNITY_EDITOR || DEBUG
        if (Input.GetJoystickNames().Length > 0)
#endif
        {
            m_pullingForce = Input.GetAxis("Vertical") * MaxForce;
            UpdateSpeedRange();
        }

        ElevateState m_prevState = m_elevatorState;

        float acc = (m_pullingForce - m_mass * m_gravity) / m_mass;
        float deltaV = acc * Time.deltaTime;
        m_speed += deltaV;

        m_speed = Mathf.Clamp(m_speed, m_minSpeed, m_maxSpeed);
        m_speedIndictor.text = string.Format("Speed {0,7:f3}", m_speed);

        if (m_speed <= -0.05f)
            m_elevatorState = ElevateState.ES_Down;
        else if (m_speed >= 0.05)
            m_elevatorState = ElevateState.ES_Up;
        else
            m_elevatorState = ElevateState.ES_Idle;

        if (m_elevatorState != ElevateState.ES_Idle)
            m_height += 0.5f * acc * Time.deltaTime * Time.deltaTime;

        m_height = Mathf.Clamp(m_height, 0.0f, MaxHeight);
        m_heightIndictor.text = string.Format("Height {0,7:f3}", m_height);
        m_levelIndictor.text = string.Format("Level {0,7:f3}", (int)(m_height / m_levelHeight));

        // elevator stopped
        if(m_prevState != ElevateState.ES_Idle && m_elevatorState == ElevateState.ES_Idle)
        {
            float level = (int)(m_height / m_levelHeight);
            GameManager.Instance.GetEventManager().InvokeEvent(Event.EVENT_ELEVATOR_STOP, new Dictionary<string, object> { { "height", m_height}, { "level", level} });
        }

        if (m_elevatorState != ElevateState.ES_Idle)
            m_runningTime += Time.deltaTime;

        m_timeIndictor.text = string.Format("Time {0,7:f3}", m_runningTime);
    }

    void FixedUpdate()
    {
        // rb.AddForce( new Vector3(0, 100, 0), ForceMode.Impulse);
        //   if (rb.velocity.magnitude > MaxSpeed) {
        //     rb.velocity = rb.velocity.normalized * MaxSpeed;
        // }
        // return;
#if UNITY_EDITOR || DEBUG
        if (Input.GetJoystickNames().Length > 0)
#endif
        {
            m_pullingForce = Input.GetAxis("Vertical") * 100;

        }

        Vector3 moveForce = new Vector3(0, m_pullingForce, 0);
        rb.AddForce(moveForce, ForceMode.Impulse);
        UpdateSpeedRange();

        Debug.Log(string.Format("for {0}", m_pullingForce));

        if (rb.velocity.magnitude > MaxSpeed) {
            rb.velocity = rb.velocity.normalized * MaxSpeed;
        }
    }

    private void OnUpBtnStateChange(Dictionary<string, object> message)
    {
        if ((bool)message["pressed"])
            m_pullingForce = 100;
        else if ((bool)message["released"])
            m_pullingForce = 0.0f;

        UpdateSpeedRange();
    }

    private void OnDownBtnStateChange(Dictionary<string, object> message)
    {
        if ((bool)message["pressed"])
            m_pullingForce = -MaxForce;
        else if ((bool)message["released"])
            m_pullingForce = 0.0f;

        UpdateSpeedRange();
    }

    private void UpdateSpeedRange()
    {
        if(m_pullingForce >= 0.05)
        {
            m_minSpeed = 0.0f;
            m_maxSpeed = MaxSpeed;
        }
        else if(m_pullingForce <= -0.05)
        {
            m_minSpeed = -MaxSpeed;
            m_maxSpeed = 0.0f;
        }
        else
        {
            m_minSpeed = 0.0f;
            m_maxSpeed = 0.0f;
        }
    }
}
