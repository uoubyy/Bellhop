using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuCtrl : MonoBehaviour
{


    public void OnStartGame()
    {
        GameManager.Instance.OnGameStart();
    }
}
