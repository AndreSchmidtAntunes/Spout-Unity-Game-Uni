using UnityEngine;
using UnityEngine.Events;
public class GameManager : MonoBehaviour
{
    public UnityEvent onInitialised;

    // --- globals --- //
    [SerializeField]
    public static GameManager instance { get; private set; }

    private void Awake() {
        if (instance == null) {
            instance = this;
        }

        if (instance == this) {
            DontDestroyOnLoad(gameObject);
        }
    }

    // private void OnValidate() {
    //     if (instance == null)
    //         instance = this;
    // }

    [SerializeField] public PlayerCharCon player;
}