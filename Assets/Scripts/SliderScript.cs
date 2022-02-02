using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SliderScript : MonoBehaviour
{
    [SerializeField] public GameObject Elevator;
    public float m_maxHeight = 540;
    private Slider slider;

    // Start is called before the first frame update
    void Start()
    {
        slider = GetComponent<Slider>();    
    }

    // Update is called once per frame
    void Update()
    {
        slider.value = Elevator.transform.position.y / m_maxHeight;
    }
}
