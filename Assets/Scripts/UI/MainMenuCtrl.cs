using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuCtrl : MonoBehaviour
{
    public Button m_startGameBtn;
    public Button m_pauseGameBtn;
    public Button m_exitGameBtn;

    private void Start()
    {
        m_startGameBtn.onClick.AddListener(OnStartGame);
        m_pauseGameBtn.onClick.AddListener(OnPauseGame);
        m_exitGameBtn.onClick.AddListener(OnExitGame);

        GameManager.Instance.GetEventManager().StartListening(Event.EVENT_GAME_OVER, OnGameOver);
    }

    private void OnStartGame()
    {
        GameManager.Instance.OnGameStart();
        gameObject.SetActive(false);
    }

    private void OnPauseGame()
    {

    }

    private void OnExitGame()
    {
        Application.Quit();
    }

    private void OnGameOver(Dictionary<string, object> message)
    {
        gameObject.SetActive(true);
    }
}
