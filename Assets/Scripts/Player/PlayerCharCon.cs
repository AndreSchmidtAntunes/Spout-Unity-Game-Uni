using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class PlayerCharCon : MonoBehaviour
{
    [SerializeField] private float m_moveSpeed = 5f;
    [SerializeField] private float m_jumpPower = 5f;
    [SerializeField] private float m_coyotyTime = 0.15f;
    [SerializeField] private LayerMask m_GroundLayer;
    [SerializeField][Min(0)] private float m_gravityMult = 5;

    private Rigidbody m_rigidbody;
    private Collider m_collider;

    Vector2 move_input = Vector2.zero;

    // The passed int is the new player health
    [HideInInspector] public UnityEvent<int> OnHealthChangedEvent;
    [HideInInspector] public UnityEvent<int> OnDamagedEvent;
    [HideInInspector] public UnityEvent<int> OnHealedEvent;

    public const int max_health = 100;
    [SerializeField] private int m_health = max_health;

    const float invincibility_time = 0.75f;
    float invincibility_time_remaining = invincibility_time;

    public int health {
        get { return m_health; }
        set {
            if (value < m_health && invincibility_time_remaining <= 0) { // if can take damage
                m_health = (value < 0) ? 0 : value;
                invincibility_time_remaining = invincibility_time;
                OnHealthChangedEvent.Invoke(m_health);
                OnDamagedEvent.Invoke(m_health);
            }
            else if (value > m_health) { // heal
                m_health = (value > max_health) ? max_health : value;
                OnHealthChangedEvent.Invoke(m_health);
                OnHealedEvent.Invoke(m_health);
            }
        }
    }
    private void Start() {
        GameManager.player = this;
        TryGetComponent(out m_rigidbody);
        TryGetComponent(out m_collider);
        Physics.gravity = -transform.up * m_gravityMult;
        OnDamagedEvent.AddListener(OnDamaged);
    }

    private void OnDamaged(int new_health) { 
        if (new_health == 0) { // dead
            if (GameManager.respawn_point) {
                transform.position = GameManager.respawn_point.position;
                health = max_health;
            }
        }
    }

    private void OnDestroy() { GameManager.player = null; }

    public System.Collections.Generic.List<Damager> touching_damagers;

    public bool is_touching_damager { get { return touching_damagers.Count > 0; } }

    public int current_damage_being_applied { get {

        if (!is_touching_damager) return 0; // damage is 0 if no damagers are touching the player

        int highest_value = int.MinValue;

        foreach (Damager damager in touching_damagers) { // applies the highest damage in the list of currently touching damagers
            highest_value = (damager.damage_delt > highest_value) ? damager.damage_delt : highest_value;
        }
        return highest_value;
    } }

    private void FixedUpdate() {
        m_rigidbody.AddForce((move_input.x * move_input.x * move_input.x) * m_moveSpeed * Time.fixedDeltaTime * Vector3.right);

        invincibility_time_remaining -= Time.fixedDeltaTime;

        health -= current_damage_being_applied;
    }

    public void Move(InputAction.CallbackContext context) {
        Vector2 input = context.ReadValue<Vector2>();
        move_input = input;
    }

    public void Jump(InputAction.CallbackContext context) {
        if (context.started && IsGrounded()) {
            m_rigidbody.AddForce(transform.up * m_jumpPower, ForceMode.Impulse);
        }
    }

    private bool IsGrounded() {
        if (!Physics.Raycast(transform.position, -transform.up, 1f, m_GroundLayer)) Debug.Log("Not grounded");
        return Physics.Raycast(transform.position, -transform.up, 1f, m_GroundLayer);
    }

    private void OnCollisionEnter(Collision collision) {
        switch ((GameManager.Layer)collision.gameObject.layer) 
        {
            case (GameManager.Layer.Ground): // player landed on ground
            {
                    transform.rotation = Quaternion.LookRotation(collision.contacts[0].normal);
                    transform.Rotate(90, 0, 0);

                    Physics.gravity = -transform.up * m_gravityMult;
                    m_rigidbody.useGravity = false;
                    break;
            }
            default: { break; }
        }
    }

    private void OnCollisionExit(Collision collision) {
        switch ((GameManager.Layer)collision.gameObject.layer) 
        {
            case (GameManager.Layer.Ground):  // player stopped being grounded
            {
                Physics.gravity = -transform.up * m_gravityMult;
                m_rigidbody.useGravity = true;
                break;
            }
            default: { break; }
        }
    }
}
