using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class PassengersManager : MonoBehaviour
{
    public int CurDifficulty; // TODO save game

    public GameObject passengerPrefab;

    private int deliveredAmount = 0;

    private DifficultyInfo curDifficultyInfo;

    //private Vector3 initialPos = new Vector3(34.1230469f, -559f, 121.300003f);

    private static int passengerId = 0;

    public GameObject elevatorFloor;

    public void OnGameStart(int difficulty)
    {
        CurDifficulty = difficulty;
        curDifficultyInfo = ConfigManager.Instance.GetDifficultyConfig(CurDifficulty);

        GameManager.Instance.GetEventManager().StartListening(Event.EVENT_PASSENGER_DELVERED, OnPassengerDeliver);

        Assert.IsNotNull(passengerPrefab);

        SpawnPassengers();
    }

    void OnPassengerDeliver(Dictionary<string, object> message)
    {
        deliveredAmount++;
        SpawnPassengers();
    }

    private void SpawnPassengers() // TODO
    {
        if(deliveredAmount >= curDifficultyInfo.passNum)
        {
            CurDifficulty++;
            deliveredAmount = 0;
            CurDifficulty = Mathf.Max(CurDifficulty, ConfigManager.Instance.MaxDifficulty());
            curDifficultyInfo = ConfigManager.Instance.GetDifficultyConfig(CurDifficulty);
        }

        int amount = 0;
        while (amount < curDifficultyInfo.maxPassenger)
        {
            float rate = Random.Range(1.0f, 100.0f);
            if (rate > curDifficultyInfo.posibility[amount])
                return;

            var poolable = Poolable.TryGetPoolable<Poolable>(passengerPrefab);
            if (poolable == null)
                return;

            Vector2 offset = Random.insideUnitCircle * 3.0f;
            Vector3 floorPos = elevatorFloor.transform.position + new Vector3(offset.x, 1.0f, offset.y);
            poolable.gameObject.transform.position = floorPos;

            PassengerCtrl passenger = poolable.gameObject.GetComponent<PassengerCtrl>();

            float bestDeliverTime = Random.Range(curDifficultyInfo.minDeliverTime, curDifficultyInfo.maxDeliverTime);
            int targetLevel = Random.Range(1, 10);
            passenger.Init(passengerId, bestDeliverTime, targetLevel, EmotionState.ES_HAPPY);

            passengerId++;
            amount++;
        }
    }
}
