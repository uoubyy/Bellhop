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
        ES_Down,
        ES_Stopping,
        ES_Stoped
    }

    public Text m_speedIndictor;
    public Text m_heightIndictor;
    public Text m_levelIndictor;
    public Text m_timeIndictor;

    public float MaxSpeed = 100.0f;
    public float MaxForce = 100.0f;

    public float m_gravity = 9.8f;
    public float m_firctionFactor = 0.2f;

    public float m_levelHeight = 1.0f;

    private float m_mass = 10.0f;
    private float m_speed = 0.0f;
    private float m_height = 0.0f;

    private float m_runningTime = 0.0f;

    private float m_pullingForce;

    public ElevateState m_elevatorState = ElevateState.ES_Idle;

    private Rigidbody m_rigidBody;

    private float m_initialHeight;

    // Start is called before the first frame update
    void Start()
    {
        GameManager.Instance.GetEventManager().StartListening(Event.EVENT_ELEVATOR_UP, OnUpBtnStateChange);
        GameManager.Instance.GetEventManager().StartListening(Event.EVENT_ELEVATOR_DOWN, OnDownBtnStateChange);

        GameManager.Instance.GetEventManager().StartListening(Event.EVENT_GAME_START, OnGameStart);
        GameManager.Instance.GetEventManager().StartListening(Event.EVENT_GAME_OVER, OnGameOver);

        m_rigidBody = GetComponent<Rigidbody>();
        m_initialHeight = transform.position.y;

        Reset();
    }

    public void OnGameStart(Dictionary<string, object> message)
    {
        m_runningTime = 0.0f;
    }

    public void OnGameOver(Dictionary<string, object> message)
    {
        ChangeElevatorState(ElevateState.ES_Stoped);
        Reset();
    }

    private void Reset()
    {
        ChangeElevatorState(ElevateState.ES_Idle);
    }

    // Update is called once per frame
    void Update()
    {
        m_speedIndictor.text = string.Format("Speed {0,7:f3}", m_speed);
        m_heightIndictor.text = string.Format("Height {0,7:f3}", m_height);

        if (m_elevatorState != ElevateState.ES_Idle && m_elevatorState != ElevateState.ES_Stoped)
            m_runningTime += Time.deltaTime;

        m_timeIndictor.text = string.Format("Time {0,7:f3}", m_runningTime);
    }

    void FixedUpdate()
    {
#if UNITY_EDITOR || DEBUG
        if (Input.GetJoystickNames().Length > 0)
#endif
        {
            m_pullingForce = Input.GetAxis("Vertical") * MaxForce;
            if (!IsRunning())
            {
                if (m_pullingForce <= -0.5f)
                    ChangeElevatorState(ElevateState.ES_Down);
                else if (m_pullingForce >= 0.5f)
                    ChangeElevatorState(ElevateState.ES_Up);
            }
            else if(m_pullingForce >= -0.5f && m_pullingForce <= 0.5f)
            {
                ChangeElevatorState(ElevateState.ES_Stopping);
            }
        }

        if (m_elevatorState == ElevateState.ES_Idle || m_elevatorState == ElevateState.ES_Stoped)
            return;

        m_speed = m_rigidBody.velocity.y;
        m_height = transform.position.y - m_initialHeight;


         Vector3 moveForce = new Vector3(0, m_pullingForce, 0) - m_firctionFactor * m_rigidBody.mass * m_gravity * m_rigidBody.velocity.normalized;
         m_rigidBody.AddForce(moveForce, ForceMode.Impulse);

        if (m_elevatorState == ElevateState.ES_Stopping && m_rigidBody.velocity.magnitude <= 2.0f)
        {
            ChangeElevatorState(ElevateState.ES_Stoped);
        }
        else if (m_rigidBody.velocity.y <= -0.5f && transform.position.y - m_initialHeight <= 1.0f) // faceing down
        {
            ChangeElevatorState(ElevateState.ES_Stoped);
            transform.position = new Vector3(transform.position.x, m_initialHeight, transform.position.z);
        }
        else if (m_rigidBody.velocity.magnitude > MaxSpeed)
        {
            m_rigidBody.velocity = m_rigidBody.velocity.normalized * MaxSpeed;
        }
    }

    public bool IsRunning()
    {
        return m_elevatorState != ElevateState.ES_Idle && m_elevatorState != ElevateState.ES_Stoped;
    }

    private void OnUpBtnStateChange(Dictionary<string, object> message)
    {
        if ((bool)message["pressed"])
        {
            m_pullingForce = MaxForce;
            ChangeElevatorState(ElevateState.ES_Up);
        }
        else if ((bool)message["released"])
        {
            m_pullingForce = 0.0f;
            ChangeElevatorState(ElevateState.ES_Stopping);
        }
    }

    private void OnDownBtnStateChange(Dictionary<string, object> message)
    {
        if ((bool)message["pressed"])
        {
            m_pullingForce = -MaxForce;
            ChangeElevatorState(ElevateState.ES_Down);
        }
        else if ((bool)message["released"])
        {
            m_pullingForce = 0.0f;
            ChangeElevatorState(ElevateState.ES_Stopping);
        }
    }

    private void ChangeElevatorState(ElevateState newState)
    {
        switch (newState)
        {
            case ElevateState.ES_Idle:
                m_rigidBody.isKinematic = true;
                break;
            case ElevateState.ES_Up:
                m_rigidBody.isKinematic = false;
                break;
            case ElevateState.ES_Down:
                m_rigidBody.isKinematic = false;
                break;
            case ElevateState.ES_Stopping:
                m_rigidBody.isKinematic = false;
                break;
            case ElevateState.ES_Stoped:
                m_rigidBody.isKinematic = true;
                break;
        }

        m_elevatorState = newState;
    }
}
