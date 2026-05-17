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
    [SerializeField] [Min(0)] private int m_maxSlings = 1;
    [SerializeField] private float m_coyotyTime = 0.55f;
    [SerializeField] private Vector2 m_gravity = Vector2.down * 9.8f;
    [SerializeField] private AnimationCurve m_gravity_mult_graph = AnimationCurve.Linear(0, 0, 1, 1);
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
    [HideInInspector] public UnityEvent<int> OnSlingValuesChanged; // <new remaining slings>

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
        AddToGameManager();
        TryGetComponent(out m_rigidbody);
        TryGetComponent(out m_collider);
    }


    private void Awake() {
        if (m_rigidbody == null) TryGetComponent(out m_rigidbody);
        if (m_collider == null) TryGetComponent(out m_collider);
    }

    private Vector2 startPos;

    private void Start() {
        startPos = transform.position;
        OnDamagedEvent.AddListener(OnDamaged);
        GameManager.instance.onInitialised.AddListener(AddToGameManager);
    }

    private void AddToGameManager() {
        if (GameManager.instance.player == null) {
            if (GameManager.is_instanced) {
                GameManager.instance.player = this;
            }
        }
    }

    private void OnDamaged(int new_health) {
        if (new_health == 0) { // dead

            health = max_health;
            Vector2 respawn_point = (RespawnZone.current_respawn_zone != null) ? RespawnZone.current_respawn_zone.respawn_point : startPos;
            transform.position = new Vector3(respawn_point.x, respawn_point.y, transform.position.z);
        }
    }

    private void OnDestroy() { GameManager.instance.player = null; }

    [HideInInspector] public System.Collections.Generic.List<Damager> touching_damagers;

    public bool is_touching_damager { get { return touching_damagers.Count > 0; } }

    public int current_damage_being_applied { get {

        if (!is_touching_damager) return 0; // damage is 0 if no damagers are touching the player

        int highest_value = int.MinValue;

        foreach (Damager damager in touching_damagers) { // applies the highest damage in the list of currently touching damagers
            highest_value = (damager.damage_delt > highest_value) ? damager.damage_delt : highest_value;
        }
        return highest_value;
    }}

    Vector2 ground_tangent = Vector2.right;

    private void FixedUpdate() {
        if (!IsGrounded()) { // gravity
            if (is_touching_celeing) {
                m_rigidbody.AddForce(m_gravity * Time.fixedDeltaTime, ForceMode.Impulse);
            }
            else {
                float gravity_mult = m_gravity_mult_graph.Evaluate(m_rigidbody.linearVelocity.y);
                m_rigidbody.AddForce(m_gravity * gravity_mult * Time.fixedDeltaTime, ForceMode.Impulse);
            }

        }

        if (!movement_enabled) return;

        m_rigidbody.AddForce(move_input.x * m_moveSpeed * Time.fixedDeltaTime * ground_tangent);

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

    int _jumps_remaining = 0;
    int jumps_remaining {
        get {
            return _jumps_remaining;
        }
        set {
            _jumps_remaining = value;
        }
    }
    float _jump_cooldown_remaining = jump_cooldown;

    public void Jump(InputAction.CallbackContext context) {

        if (context.started) _is_jump_button_down = true;
            else if (context.canceled) _is_jump_button_down = false;
        }

    void LateUpdate() {

        if (movement_enabled && _is_jump_button_down) {
            if (jumps_remaining > 0 && _jump_cooldown_remaining <= 0) {
                if (jumps_remaining == m_maxSlings + 1) { // grounded jump
                    m_rigidbody.linearVelocity = new Vector3(m_rigidbody.linearVelocity.x, 0, m_rigidbody.linearVelocity.z); // set y velocity to 0.
                    m_rigidbody.AddForce(transform.up * m_jumpPower, ForceMode.Impulse);
                    OnSlingValuesChanged.Invoke(m_maxSlings);

                }
                else {  // sling jump
                    Vector2 target_direction;
                    if (Mathf.Abs(move_input.x) + Mathf.Abs(move_input.y) < 0.1) { // if direction is near 0
                        target_direction = Vector2.up;
                    }
                    else {
                        // Thank you to this post (https://stackoverflow.com/a/1437934) for the equation to get a direction index from a direction.
                        int direction_index = (Mathf.RoundToInt(Mathf.Atan2(move_input.y, move_input.x) / (2 * Mathf.PI / 8)) + 8) % 8;

                        const float one_over_sqrt2 = 0.70710678118f;

                        target_direction = direction_index switch
                        {
                            0 => Vector2.right,                                 // E
                            1 => new Vector2(one_over_sqrt2, one_over_sqrt2),   // NE
                            2 => Vector2.up,                                    // N
                            3 => new Vector2(-one_over_sqrt2, one_over_sqrt2),  // NW
                            4 => Vector2.left,                                  // W
                            5 => Vector2.left,                                  // W
                            6 => Vector2.up,                                    // N
                            7 => Vector2.right,                                 // E
                            _ => throw new System.NotImplementedException(),
                        };
                    }

                    m_rigidbody.linearVelocity = Vector3.zero;

                    Vector2 force = target_direction * m_slingPower;
                    force += Vector2.up * 1f;
                    m_rigidbody.AddForce(force, ForceMode.Impulse);

                    WebLineRenderer.Render(transform.position, force.normalized);

                    OnSlingValuesChanged.Invoke(jumps_remaining - 1);
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
                jumps_remaining = m_maxSlings + 1; // the extra 1 is the jump from the ground.
                OnSlingValuesChanged.Invoke(m_maxSlings);

            }
            else {
                _seconds_since_last_grounded = 0;
            }
        }
    }

    private void OnCollisionEnter(Collision collision) {
        CheckGroundedAndCeiling(collision);
    }

    private void OnCollisionStay(Collision collision) {
        CheckGroundedAndCeiling(collision);
    }

    private void OnCollisionExit(Collision collision) {
        switch (collision.gameObject.tag)
        {
            case "Ground": { // player stopped being grounded
                    grounded = false;
                    ground_tangent = Vector2.right;
                    break;
            }
            default: break;
        }
    }

    bool is_touching_celeing = false;

    void CheckGroundedAndCeiling(Collision collision) {
        switch (collision.gameObject.tag)
        {
            case "Ground":
                {
                    ground_tangent = Vector2.right; // default value (value while in the air)
                    is_touching_celeing = false; // default value
                    foreach (ContactPoint contact in collision.contacts)
                    {
                        Vector2 normal = contact.normal;
                        grounded = normal.y > 0;
                        if (grounded)
                        {
                            ground_tangent = new Vector2(normal.y, -normal.x);
                            break; // no need to check all contacts when one confirming the player is grounded has already been found
                        }
                        else if (!is_touching_celeing)
                        {
                            is_touching_celeing = (normal.y < -0.25f);
                        }
                    }
                    break;
                }
            default: break;
        }
        if (is_touching_celeing) Debug.Log("Ceiling");
    }

    public void SetMaxSlings(int count) {

        m_maxSlings = Mathf.Max(count, 0);
    }

}
