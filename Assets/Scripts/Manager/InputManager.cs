using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

[System.Serializable]
public class KeyBinding
{
    public int ID;
    public string name;
    public string key1;
    public string key2;
    public bool canDisable;
}

[System.Serializable]
public class KeyBindings
{
    public KeyBinding[] KeyBindingsList;
}

public class InputManager : MonoBehaviour
{
    enum BtnState
    {
        Idle,
        Pressed,
        Released
    }

    private KeyBindings m_keyBinding;
    public TextAsset m_keyBindingCfg;

    private bool m_enableInput = true;
    public void EnableInput(bool value) { m_enableInput = value; }

    private List<string> m_inputs;
    public List<string> GetInputs() { return m_inputs; }

    private Dictionary<string, BtnState> m_btnState;

    // Start is called before the first frame update
    void Start()
    {
        m_inputs = new List<string>();
        m_btnState = new Dictionary<string, BtnState>();

        Assert.IsNotNull(m_keyBindingCfg);
        m_keyBinding = JsonUtility.FromJson<KeyBindings>("{\"KeyBindingsList\":" + m_keyBindingCfg.text + "}");
        // Debug.Log(string.Format("KeyBindings num{0}.", mKeyBinding.KeyBindingsList.Length));

        foreach (KeyBinding keyBinding in m_keyBinding.KeyBindingsList)
        {
            m_btnState[keyBinding.name] = BtnState.Idle;
        }

        m_enableInput = false;
    }

    void Update()
    {
        m_inputs.Clear();

        foreach (KeyBinding keyBinding in m_keyBinding.KeyBindingsList)
        {
            bool keyUp = false, keyDown = false;

            keyDown = Input.GetKeyDown(keyBinding.key1);
            keyUp = Input.GetKeyUp(keyBinding.key1);
/*#else
            keyDown = Input.GetKeyDown(keyBinding.key2);
            keyUp = Input.GetKeyUp(keyBinding.key2);
#endif*/

            if (!m_enableInput && keyBinding.canDisable)
                continue;

            if(keyDown)
            {
                m_inputs.Add(keyBinding.name);
                Debug.Log(string.Format("{0} button pressed.", keyBinding.name));

                if(m_btnState[keyBinding.name] != BtnState.Pressed)
                    GameManager.Instance.GetEventManager().InvokeEvent(keyBinding.name, new Dictionary<string, object> { { "pressed", true }, { "released", false } });

                m_btnState[keyBinding.name] = BtnState.Pressed;
            }
            else if(keyUp)
            {
                if (m_btnState[keyBinding.name] != BtnState.Released)
                    GameManager.Instance.GetEventManager().InvokeEvent(keyBinding.name, new Dictionary<string, object> { { "pressed", false }, { "released", true} });
                m_btnState[keyBinding.name] = BtnState.Released;
            }
            else
            {
                m_btnState[keyBinding.name] = BtnState.Idle;
            }
        }
    }
}
