using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PassengerInfo : MonoBehaviour
{
    private Image m_panel;
    private TextMeshProUGUI m_targetLevel;
    private Image m_emotionIcon;

    public float fadeTime = 3f;

    private float initialY;

    public float amplitude = 2.0f;
    public float speed = 1.5f;
    private float radian = 0.0f;

    [SerializeField]
    private List<Sprite> emotionSprite = new List<Sprite>();

    private void Awake()
    {
        m_panel = GetComponent<Image>();
        m_targetLevel = GetComponentInChildren<TextMeshProUGUI>();
        m_emotionIcon = GetComponentsInChildren<Image>()[1]; // danger
    }

    // Update is called once per frame
    void Update()
    {
        radian += speed * Time.deltaTime;

        Mathf.Clamp(radian, 0, 2 * Mathf.PI);
        float dy = Mathf.Cos(radian) * amplitude;
        transform.position = new Vector3(transform.position.x, initialY + dy, transform.position.z);
    }

    protected void FadeOut(Color color)
    {
        m_panel.CrossFadeColor(color, fadeTime, true, true);
    }

    public void Init(EmotionState emotion, int targetLevel)
    {
        m_targetLevel.text = targetLevel.ToString();
        m_emotionIcon.sprite = emotionSprite[(int)emotion];

        // FadeOut(new Color(1f, 1f, 1f, 1f));

        gameObject.SetActive(false);
    }

    public void UpdatePos(Vector3 targetPosition)
    {
        transform.position = Camera.main.WorldToScreenPoint(targetPosition) + new Vector3(0.0f, 100.0f, 0.0f);
        initialY = transform.position.y;
        gameObject.SetActive(true);
    }

    public void UpdateEmotion(EmotionState emotion)
    {
        m_emotionIcon.sprite = emotionSprite[(int)emotion];

    }
}
