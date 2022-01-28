using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloorManager : MonoBehaviour
{
    public GameObject floorPrefab;

    private List<GameObject> m_floors;

    public float floorHeight;

    public int initialCapacity = 10;

    // Start is called before the first frame update
    void Start()
    {
        m_floors = new List<GameObject>();
        for (int i = 0; i < initialCapacity; ++i)
        {
            GameObject newFloor = Object.Instantiate(floorPrefab);
            m_floors.Add(newFloor);

            if (i == 0)
            {
                var maxBounds = new Bounds(newFloor.transform.position, Vector3.zero);
                foreach (var child in newFloor.GetComponentsInChildren<Collider>())
                {
                    maxBounds.Encapsulate(child.bounds);
                }

                floorHeight = maxBounds.size.y;
            }

            newFloor.transform.SetParent(this.transform);
            newFloor.transform.position = new Vector3(0.0f, 60.0f, 0.0f) * i;
        }
    }
}
