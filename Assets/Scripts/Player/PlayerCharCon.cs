using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(SphereCollider))]
public class PlayerCharCon : MonoBehaviour
{
    [SerializeField] private float m_moveSpeed = 5f;
    [SerializeField] private float m_jumpPower = .5f;
    [SerializeField] private float m_slingPower = .8f;
    [SerializeField] private float m_coyotyTime = 0.55f;
    [SerializeField][Min(0)] private float m_gravityMult = 5;
    [SerializeField] private Transform m_bottomPosition;
    [SerializeField] private Billboard m_billboard;
    public bool movement_enabled = false;

    private Rigidbody m_rigidbody;
    private SphereCollider m_collider;

    public float collision_radius { 
        get { 
            if (m_collider == null) TryGetComponent(out m_collider);
            return m_collider.radius; 
        } 
    }

    Vector2 move_input = Vector2.zero;

    // The passed int is the new player health
    [HideInInspector] public UnityEvent<int> OnHealthChangedEvent;
    [HideInInspector] public UnityEvent<int> OnDamagedEvent;
    [HideInInspector] public UnityEvent<int> OnHealedEvent;

    public const int max_health = 100;
    [SerializeField][Range(0,100)] private int m_health = max_health;

    const float invincibility_time = 0.75f;
    float _invincibility_time_remaining = invincibility_time;

    public int health {
        get { return m_health; }
        set {
            if (value < m_health && _invincibility_time_remaining <= 0) { // if can take damage
                m_health = (value < 0) ? 0 : value;
                _invincibility_time_remaining = invincibility_time;
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

    void OnValidate() {
        GameManager.player = this;
        TryGetComponent(out m_rigidbody);
        TryGetComponent(out m_collider);
    }

    private Vector2 startPos;

    private void Start() {
        startPos = transform.position;
        Physics.gravity = -transform.up * m_gravityMult;
        OnDamagedEvent.AddListener(OnDamaged);
    }


    private void OnDamaged(int new_health) {
        if (new_health == 0) { // dead

            health = max_health;
            Vector2 respawn_point = (RespawnZone.current_respawn_zone != null) ? RespawnZone.current_respawn_zone.respawn_point : startPos;
            transform.position = new Vector3(respawn_point.x, respawn_point.y, transform.position.z);
        }
    }

    private void OnDestroy() { GameManager.player = null; }

    [HideInInspector] public System.Collections.Generic.List<Damager> touching_damagers;

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

        if (!movement_enabled) return;

        // cubing the user's input for a more snappy response.
        m_rigidbody.AddForce((move_input.x * move_input.x * move_input.x) * m_moveSpeed * Time.fixedDeltaTime * Vector3.right);

        health -= current_damage_being_applied; // invincibility is handled by <health>'s setter.

        // -- clocks -- //

        _invincibility_time_remaining -= Time.fixedDeltaTime;

        if (!grounded)
            _seconds_since_last_grounded += Time.fixedDeltaTime;

        if (_jump_cooldown_remaining > 0)
            _jump_cooldown_remaining -= Time.fixedDeltaTime;
    }

    public void Move(InputAction.CallbackContext context) {

        Vector2 input = context.ReadValue<Vector2>();
        move_input = input;

        if (!movement_enabled) return;

        if (input.x > 0) {
            m_billboard.m_flip = false;
        }
        else if (input.x < 0)
        {
            m_billboard.m_flip = true;
        }
    }

    bool _is_jump_button_down = false;

    const float jump_cooldown = 0.25f;
    const int max_jumps = 2;

    int jumps_remaining = max_jumps;
    float _jump_cooldown_remaining = jump_cooldown;

    public void Jump(InputAction.CallbackContext context) {

        if (context.started) _is_jump_button_down = true;
        else if (context.canceled) _is_jump_button_down = false;
    }

    void LateUpdate() {
        
        if (_is_jump_button_down && movement_enabled) {
            if (jumps_remaining > 0 && _jump_cooldown_remaining <= 0) {
                if (jumps_remaining == max_jumps) { // grounded jump
                    m_rigidbody.linearVelocity = new Vector3(m_rigidbody.linearVelocity.x, 0, m_rigidbody.linearVelocity.z); // set y velocity to 0.
                    m_rigidbody.AddForce(transform.up * m_jumpPower, ForceMode.Impulse);
                }
                else {  // sling jump
                    // Thank you to this post (https://stackoverflow.com/a/1437934) for the equation to get a direction index from a direction.
                    int direction_index = (Mathf.RoundToInt(Mathf.Atan2(move_input.y, move_input.x) / (2 * Mathf.PI / 8)) + 8) % 8;

                    const float one_over_sqrt2 = 0.70710678118f;

                    Vector2 target_direction = direction_index switch {
                        0 => Vector2.right,                                 // E
                        1 => new Vector2(one_over_sqrt2, one_over_sqrt2),   // NE
                        2 => Vector2.up,                                    // N
                        3 => new Vector2(-one_over_sqrt2, one_over_sqrt2),  // NW
                        4 => Vector2.left,                                  // W
                        5 => Vector2.left,                                  // W
                        6 => Vector2.up,                                    // N
                        7 => Vector2.right,                                 // E
                    };
                    /// TODO: 
                    m_rigidbody.linearVelocity = Vector3.zero;
                    m_rigidbody.AddForce(target_direction * m_slingPower, ForceMode.Impulse);
                }
                _jump_cooldown_remaining = jump_cooldown;
                jumps_remaining--;
            }
        }
    }

    private bool IsGrounded() {
        return grounded || _seconds_since_last_grounded <= m_coyotyTime;
    }

float _seconds_since_last_grounded;

bool _grounded = false;
    bool grounded {
        get { return _grounded; }
        set {
            if (value == _grounded) return;

            _grounded = value;

            if (_grounded) {
                Physics.gravity = -transform.up * m_gravityMult;
                m_rigidbody.useGravity = false;
                jumps_remaining = max_jumps;
            }
            else {
                Physics.gravity = -transform.up * m_gravityMult;
                m_rigidbody.useGravity = true;
                _seconds_since_last_grounded = 0;
            }
        }
    }

    private void OnCollisionEnter(Collision collision) {
        switch (collision.gameObject.tag)
        {
            case "Ground": { // player landed on ground
                    grounded = true;
                    break;
            }
            default: { break; }
        }
    }

    private void OnCollisionExit(Collision collision) {
        switch (collision.gameObject.tag)
        {
            case "Ground": { // player stopped being grounded
                    grounded = false;
                    break;
            }
            default: break;
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        switch (collision.gameObject.tag)
        {
            case "Ground":
                {
                    foreach (ContactPoint contact in collision.contacts) {
                        grounded = contact.normal.y > 0;
                        if (grounded) break; // no need to check all contacts when one confirming the player is grounded has already been found
                    }

                    break;
                }
            default: break;
        }
    }

}
