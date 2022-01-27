using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class PassengersManager : MonoBehaviour
{
    public int CurDifficulty; // TODO save game

    public GameObject passengerPrefab;

    private int deliveredAmount = 0;

    public int debug_deliver_amount = 3;

    public void OnGameStart(int difficulty)
    {
        CurDifficulty = difficulty;
        
        GameManager.Instance.GetEventManager().StartListening(Consts.EVENT_PASSENGER_DELVERED, OnPassengerDeliver);

        Assert.IsNotNull(passengerPrefab);

        SpawnPassengers();
    }

    void OnPassengerDeliver(Dictionary<string, object> message)
    {
        deliveredAmount++;
        SpawnPassengers();
    }

    private void SpawnPassengers()
    {
        if(deliveredAmount >= debug_deliver_amount)
        {
            CurDifficulty++;
            deliveredAmount = 0;
            CurDifficulty = Mathf.Max(CurDifficulty, 2);// test code
        }    
        DifficultyInfo difficultyInfo = GameManager.Instance.GetConfigManager().GetDifficultyConfig(CurDifficulty);

        int amount = difficultyInfo.maxPassenger;
        while (amount > 0)
        {
            var poolable = Poolable.TryGetPoolable<Poolable>(passengerPrefab);
            if (poolable == null)
                return;

            Vector2 offset = Random.insideUnitCircle * 10.0f;
            poolable.gameObject.transform.position = new Vector3(offset.x, 0.0f, offset.y);

            PassengerCtrl passenger = poolable.gameObject.GetComponent<PassengerCtrl>();

            float bestDeliverTime = Random.Range(difficultyInfo.bestDeliverTime.x, difficultyInfo.bestDeliverTime.y);
            int targetLevel = Random.Range(1, 11);
            passenger.Init(bestDeliverTime, targetLevel);
            amount--;
        }
    }
}
