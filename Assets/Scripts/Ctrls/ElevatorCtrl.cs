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
        ES_PreStop,
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

    public GameObject m_leftDoor;
    public GameObject m_rightDoor;
    private Vector3 m_targetStopPos;

    public AudioClip m_levelArrivedClip;
    public AudioClip m_elevatorRunningClip;
    public AudioSource m_musicPlayer;

    public ParticleSystem m_fileEffect;
    public ParticleSystem m_waterEffect;

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
        m_maxHeight = m_floorHeight * 9; // some magic code

        //StartCoroutine(CloseTheDoor());
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

        if (m_elevatorState == ElevateState.ES_Catastrophe)
            return;

        if (m_elevatorState == ElevateState.ES_PreStop)
        {
            float dist = Mathf.Abs(transform.position.y - m_targetStopPos.y);
            if (Mathf.Abs(transform.position.y - m_targetStopPos.y) >= 0.005f)
            {
                transform.position = Vector3.MoveTowards(transform.position, m_targetStopPos, 10.0f * Time.deltaTime);
            }
            else
            {
                ChangeElevatorState(ElevateState.ES_Stoped);
            }
        }
    }

    void FixedUpdate()
    {
        if (m_elevatorState == ElevateState.ES_Catastrophe)
            return;

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

        if (m_elevatorState == ElevateState.ES_FreeFall) // TODO SPECIAL EVENT 
        {
            m_pullingForce = -m_gravity * m_gravityScale * m_rigidBody.mass;
            Debug.Log(string.Format("Free fall force: {0}", m_pullingForce));
        }

        if (m_elevatorState == ElevateState.ES_Idle || m_elevatorState == ElevateState.ES_Stoped)
            return;

        m_speed = m_rigidBody.velocity.y;
        m_height = transform.position.y - m_initialHeight;


         Vector3 moveForce = new Vector3(0, m_pullingForce, 0) - m_firctionFactor * m_rigidBody.mass * m_gravity * m_rigidBody.velocity.normalized;
         m_rigidBody.AddForce(moveForce, ForceMode.Impulse);

        if (m_rigidBody.velocity.y >= 0.5f && transform.position.y - m_initialHeight >= m_maxHeight - 1.0f) // facing up
        {
            ChangeElevatorState(ElevateState.ES_Stoped);
            transform.position = new Vector3(transform.position.x, m_initialHeight + m_maxHeight, transform.position.z);
        }
        else if (m_rigidBody.velocity.y <= -0.5f && transform.position.y - m_initialHeight <= 1.0f) // facing down
        {
            ChangeElevatorState(ElevateState.ES_Stoped);
            transform.position = new Vector3(transform.position.x, m_initialHeight, transform.position.z);
        }

        if (m_elevatorState == ElevateState.ES_Stopping)
        {
            if(m_rigidBody.velocity.magnitude <= 8.0f)
            {
                if (m_rigidBody.velocity.y >= 0.5f ) // face up
                    m_targetStopPos = new Vector3(0.0f, m_initialHeight + Mathf.Ceil(m_height / m_floorHeight) * m_floorHeight, 0.0f);
                else if(m_rigidBody.velocity.y <= -0.5f)
                    m_targetStopPos = new Vector3(0.0f, m_initialHeight + Mathf.Floor(m_height / m_floorHeight) * m_floorHeight, 0.0f);

                if (Mathf.Abs(m_targetStopPos.y - transform.position.y) <= m_floorHeight * 0.4f)
                    ChangeElevatorState(ElevateState.ES_PreStop);
                else if(m_rigidBody.velocity.magnitude <= 2.0f)
                    ChangeElevatorState(ElevateState.ES_Stoped);
            }
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
        if (m_elevatorState == ElevateState.ES_Catastrophe)
            return;

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
        if (m_elevatorState == ElevateState.ES_Catastrophe)
            return;

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
        Debug.Log(string.Format("Change state from {0} to {1}", m_elevatorState.ToString(), newState.ToString()));

        if (newState == m_elevatorState)
            return;

        switch (newState)
        {
            case ElevateState.ES_Idle:
                m_rigidBody.isKinematic = true;
                break;
            case ElevateState.ES_Up:
            case ElevateState.ES_Down:
            case ElevateState.ES_Stopping:
                m_rigidBody.isKinematic = false;
                break;
            case ElevateState.ES_PreStop:
                m_rigidBody.isKinematic = true;
                break;
            case ElevateState.ES_Stoped:
                float dist = Mathf.Abs(m_initialHeight + Mathf.RoundToInt(m_height / m_floorHeight) * m_floorHeight - transform.position.y);
                if (dist <= 1.0f)
                    OnLevelArrived();
                    
                m_bFreeFalling = false;
                m_rigidBody.isKinematic = true;
                break;
            case ElevateState.ES_Catastrophe:
                m_rigidBody.isKinematic = true;
                m_fileEffect.Play();
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
        if ((bool)message["pressed"])
        {
            m_bFreeFalling = !m_bFreeFalling;
            ChangeElevatorState(m_bFreeFalling ? ElevateState.ES_FreeFall : ElevateState.ES_Stopping);
        }
    }

    private void OnPressShift(Dictionary<string, object> message)
    {
        //if (m_elevatorState == ElevateState.ES_Catastrophe)
        {
            m_shiftAmount--;
            if (m_shiftAmount <= 0)
            {
                m_fileEffect.Stop();
                m_waterEffect.Play();
                GameManager.Instance.GetInputManager().EnableInput(true);
                ChangeElevatorState(ElevateState.ES_Stoped);

                StartCoroutine(StopWater());
            }
        }
    }

    IEnumerator StopWater()
    {
        yield return new WaitForSeconds(0.5f);
        m_waterEffect.Stop();
    }

    private void OnLevelArrived()
    {
        m_musicPlayer.PlayOneShot(m_levelArrivedClip);
        GameManager.Instance.GetEventManager().InvokeEvent(Event.EVENT_ELEVATOR_STOP, new Dictionary<string, object> { { "level", Mathf.RoundToInt(m_height / m_floorHeight) } });
        // StartCoroutine(OpenTheDoor());
    }

    IEnumerator CloseTheDoor() // true to close
    {
        float elapsedTime = 0.5f;
        Vector3 leftStart = new Vector3(0.43f, m_leftDoor.transform.position.y, m_leftDoor.transform.position.z);
        Vector3 leftEnd = new Vector3(0.3f, leftStart.y, leftStart.z);

        Vector3 rightStart = m_rightDoor.transform.position;
        Vector3 rightEnd = new Vector3(0f, rightStart.y, rightStart.z);

        while (elapsedTime >= 0.0f)
        {
            m_leftDoor.transform.position = Vector3.Lerp(leftStart, leftEnd, (elapsedTime / 0.5f));
            //m_rightDoor.transform.position = Vector3.Lerp(rightStart, rightEnd, (elapsedTime / 0.5f));
            elapsedTime -= Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
    }

    IEnumerator OpenTheDoor() // true to close
    {
        float elapsedTime = 0.5f;
        Vector3 leftStart = new Vector3(0.3f, m_leftDoor.transform.position.y, m_leftDoor.transform.position.z);
        Vector3 leftEnd = new Vector3(0.44f, leftStart.y, leftStart.z);

        Vector3 rightStart = m_rightDoor.transform.position;
        Vector3 rightEnd = new Vector3(-0.22f, rightStart.y, rightStart.z);

        while (elapsedTime >= 0.0f)
        {
            m_leftDoor.transform.position = Vector3.Lerp(leftStart, leftEnd, (elapsedTime / 0.5f));
            m_rightDoor.transform.position = Vector3.Lerp(rightStart, rightEnd, (elapsedTime / 0.5f));
            elapsedTime -= Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
    }
}
