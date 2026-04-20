using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class PlayerCharCon : MonoBehaviour
{
    [SerializeField] private float m_moveSpeed = 5f;
    [SerializeField] private float m_jumpPower = 5f;
    [SerializeField] private float m_coyotyTime = 0.15f;
    [SerializeField] private LayerMask m_GroundLayer;
    [SerializeField] [Min(0)] private float m_gravityMult = 5;

    private Rigidbody m_rigidbody;
    private Collider m_collider;
    Vector2 move_input = Vector2.zero;

    void Start() {

        TryGetComponent(out m_rigidbody);
        TryGetComponent(out m_collider);

        Physics.gravity = -transform.up * m_gravityMult;
    }

    private void FixedUpdate() {
        m_rigidbody.AddForce((move_input.x * move_input.x * move_input.x) * m_moveSpeed * Time.fixedDeltaTime * Vector3.right);
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

    private void OnCollisionEnter(Collision collision)
    {
        // on landed

        transform.rotation = Quaternion.LookRotation(collision.contacts[0].normal);
        transform.Rotate(90, 0, 0);


        Physics.gravity = -transform.up * m_gravityMult;
        m_rigidbody.useGravity = false;
    }

    private void OnCollisionExit(Collision collision)
    {
        // on landed

        Physics.gravity = -transform.up * m_gravityMult;
        m_rigidbody.useGravity = true;
    }
}
