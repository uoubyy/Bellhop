using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowWorldUICtrl : MonoBehaviour
{
    public void SetWorldPosition(Vector3 worldPos)
    {
        gameObject.transform.position = worldPos;
    }
}
