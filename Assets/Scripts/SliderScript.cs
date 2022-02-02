using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SliderScript : MonoBehaviour
{
    [SerializeField] public GameObject Elevator;
    public float m_maxHeight = 540;
    private Slider slider;
    private float m_initHeight;

    // Start is called before the first frame update
    void Start()
    {
        slider = GetComponent<Slider>();
        m_initHeight = Elevator.transform.position.y;
    }

    // Update is called once per frame
    void Update()
    {
        slider.value = (Elevator.transform.position.y - m_initHeight) / m_maxHeight;
    }
}
