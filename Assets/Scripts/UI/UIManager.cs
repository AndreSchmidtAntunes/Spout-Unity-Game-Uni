using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager instance {  get; private set; }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Awake() {
        if (instance == null)
            instance = this;
    }

    [SerializeField] private AdvancableTextbox m_playerTextbox;
    public AdvancableTextbox playerTextbox { get { return m_playerTextbox; } }
}
