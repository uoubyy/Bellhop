using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class StatusInfo : MonoBehaviour
{
    public TextMeshProUGUI countDownTime;
    private float gametime;

    public TextMeshProUGUI extraTime; 

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
        int txt = (int)reward;
        extraTime.text = reward > 0 ? "+" + txt : txt.ToString();
        if (reward > 0)
            extraTime.color = new Color(104 / 255.0f, 222 / 255.0f, 26 / 255.0f);
        else
            extraTime.color = new Color(237 / 255.0f, 34 / 255.0f, 35 / 255.0f);
        // extraTime.transform.position = new Vector3(160.0f, 0.0f, 0.0f);
        Vector3 start = new Vector3(230.0f, 1080 - 150.0f, 0.0f);
        Vector3 end = new Vector3(230.0f, 1080- 50f, 0.0f);

        StartCoroutine(MoveOverSeconds(extraTime.gameObject, start, end, 3.0f));
    }

    private IEnumerator MoveOverSeconds(GameObject objectToMove, Vector3 start, Vector3 end, float seconds)
    {
        float elapsedTime = 0;
        Vector3 startingPos = objectToMove.transform.position;
        while (elapsedTime < seconds)
        {
            objectToMove.transform.position = Vector3.Lerp(start, end, (elapsedTime / seconds));
            elapsedTime += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        objectToMove.transform.position = end;
    }
}
