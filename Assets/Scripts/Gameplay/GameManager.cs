using UnityEngine;
using UnityEngine.Events;
public class GameManager : MonoBehaviour
{
    public UnityEvent onInitialised;
    public UnityEvent onPlayerLoaded;

    // --- globals --- //
    [SerializeField]
    public static GameManager instance { get; private set; }

    public static bool is_instanced {get { return instance != null; }}

    private void Awake() {
        if (instance == null)
            instance = this;
    }

    private void OnValidate() {
        if (instance == null)
            instance = this;
    }

    private static PlayerCharCon _player;
    public PlayerCharCon player
    {
        get {
            return _player;
        }
        set {
            _player = value;
            instance.onPlayerLoaded.Invoke();
            return;
        }
    }
}