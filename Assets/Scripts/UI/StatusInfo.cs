using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class StatusInfo : MonoBehaviour
{
    public TextMeshProUGUI countDownTime;
    private float gametime;



    // Start is called before the first frame update
    void Start()
    {
        GameManager.Instance.GetEventManager().StartListening(Event.EVENT_GAME_START, OnGameStart);
        GameManager.Instance.GetEventManager().StartListening(Event.EVENT_GAME_OVER, OnGameOver);
        GameManager.Instance.GetEventManager().StartListening(Event.EVENT_PASSENGER_DELVERED, OnPassengerDeliver);
    }

    // Update is called once per frame
    void Update()
    {
        if (GameManager.Instance.GetGameState() == GameState.GS_Running)
        {
            int minutes = (int)gametime / 60;
            int seconds = (int)gametime % 60;

            countDownTime.text = string.Format("{0:D2}:{1:D2}", minutes, seconds);

            gametime -= Time.deltaTime;
            gametime = gametime < 0 ? 0 : gametime;
        }
    }

    private void OnGameStart(Dictionary<string, object> message)
    {
        gametime = (float)message["gametime"];
    }

    private void OnGameOver(Dictionary<string, object> message)
    {
        gametime = 0.0f;
        countDownTime.text = "00:00";
    }

    private void OnPassengerDeliver(Dictionary<string, object> message)
    {
        float reward = (float)message["reward"];
        gametime += reward;
    }

    private void ShowExtraTime()
    {

    }
}
