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
        ES_Stoped,
        ES_Catastrophe,
        ES_FreeFall
    }

    public Text m_speedIndictor;
    public Text m_heightIndictor;
    public Text m_levelIndictor;
    public Text m_timeIndictor;

    public float MaxSpeed = 100.0f;
    public float MaxForce = 100.0f;

    private float m_gravity = 9.8f;
    public float m_gravityScale = 1.0f;
    public float m_firctionFactor = 0.2f;

    private float m_floorHeight = 60.0f;
    private float m_maxHeight;

    private float m_mass = 10.0f;
    private float m_speed = 0.0f;
    private float m_height = 0.0f;

    private float m_runningTime = 0.0f;

    private float m_pullingForce;

    public ElevateState m_elevatorState = ElevateState.ES_Idle;

    private Rigidbody m_rigidBody;

    private float m_initialHeight;

    private bool m_bFreeFalling = false;

    private int m_shiftAmount = 0;

    // Start is called before the first frame update
    void Start()
    {
        GameManager.Instance.GetEventManager().StartListening(Event.EVENT_ELEVATOR_UP, OnUpBtnStateChange);
        GameManager.Instance.GetEventManager().StartListening(Event.EVENT_ELEVATOR_DOWN, OnDownBtnStateChange);

        GameManager.Instance.GetEventManager().StartListening(Event.EVENT_GAME_START, OnGameStart);
        GameManager.Instance.GetEventManager().StartListening(Event.EVENT_GAME_OVER, OnGameOver);

        GameManager.Instance.GetEventManager().StartListening(Event.EVENT_CATASTROPHE_FIRE, OnCatastropheFire);
        GameManager.Instance.GetEventManager().StartListening(Event.EVENT_ELEVATOR_FREE_FALL, OnElevatorFreeFall);

        GameManager.Instance.GetEventManager().StartListening(Event.EVENT_PRESS_SHIFT, OnPressShift);

        m_rigidBody = GetComponent<Rigidbody>();
        m_initialHeight = transform.position.y;

        Reset();
    }

    public void OnGameStart(Dictionary<string, object> message)
    {
        m_runningTime = 0.0f;
        m_maxHeight = m_floorHeight * 9 + 5.0f; // some magic code
    }

    public void OnGameOver(Dictionary<string, object> message)
    {
        ChangeElevatorState(ElevateState.ES_Stoped);
        Reset();
    }

    private void Reset()
    {
        ChangeElevatorState(ElevateState.ES_Idle);
        m_shiftAmount = 0;
        m_bFreeFalling = false;
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

        if (m_elevatorState == ElevateState.ES_Catastrophe) // TODO SPECIAL EVENT 
            m_pullingForce = -m_gravity * m_gravityScale * m_rigidBody.mass;

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
        else if (m_rigidBody.velocity.y <= -0.5f && transform.position.y - m_initialHeight <= 1.0f) // facing down
        {
            ChangeElevatorState(ElevateState.ES_Stoped);
            transform.position = new Vector3(transform.position.x, m_initialHeight, transform.position.z);
        }else if(m_rigidBody.velocity.y >= 0.5f && transform.position.y - m_initialHeight >= m_maxHeight - 1.0f) // facing up
        {
            ChangeElevatorState(ElevateState.ES_Stoped);
            transform.position = new Vector3(transform.position.x, m_initialHeight + m_maxHeight, transform.position.z);
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
        if (newState == m_elevatorState)
            return;

        switch (newState)
        {
            case ElevateState.ES_Idle:
                m_rigidBody.isKinematic = true;
                break;
            case ElevateState.ES_Up:
                if (transform.position.y - m_initialHeight >= m_maxHeight - 1.0f)
                    return;
                m_rigidBody.isKinematic = false;
                break;
            case ElevateState.ES_Down:
                m_rigidBody.isKinematic = false;
                break;
            case ElevateState.ES_Stopping:
                m_rigidBody.isKinematic = false;
                break;
            case ElevateState.ES_Stoped:
                GameManager.Instance.GetEventManager().InvokeEvent(Event.EVENT_ELEVATOR_STOP, new Dictionary<string, object> { { "level", m_height / m_floorHeight } });
                m_rigidBody.isKinematic = true;
                break;
            case ElevateState.ES_Catastrophe:
                m_rigidBody.isKinematic = true;
                break;
            case ElevateState.ES_FreeFall:
                m_rigidBody.isKinematic = false;
                break;
        }

        m_elevatorState = newState;
    }

    private void OnCatastropheFire(Dictionary<string, object> message)
    {
        float stateTime = (float)message["duration"];
        ChangeElevatorState(ElevateState.ES_Catastrophe);

        int deliveredAmount = (int)message["deliveredAmount"];
        if(deliveredAmount == 4)
        {
            m_shiftAmount = (int)Random.Range(1.0f, 7.0f);
        }else if(deliveredAmount == 10)
        {
            m_shiftAmount = (int)Random.Range(1.0f, 7.0f);
        }
    }

    private void OnElevatorFreeFall (Dictionary<string, object> message)
    {
        m_bFreeFalling = !m_bFreeFalling;
        ChangeElevatorState(m_bFreeFalling ? ElevateState.ES_FreeFall : ElevateState.ES_Stopping);
    }

    private void OnPressShift(Dictionary<string, object> message)
    {
        if (m_elevatorState == ElevateState.ES_Catastrophe)
        {
            m_shiftAmount--;
            if (m_shiftAmount <= 0)
                ChangeElevatorState(ElevateState.ES_Stoped);
        }
    }
}
