using UnityEngine;
using UnityEngine.UIElements;

public class Billboard : MonoBehaviour
{

    enum FacingMode { CameraPosition, CameraForward}
    [SerializeField] FacingMode m_mode = FacingMode.CameraPosition;
    [SerializeField] bool flip = false;
    [SerializeField] float z_offset = 0;
    Camera m_camera;
    [SerializeField] SpriteRenderer m_renderer;
    [SerializeField] float m_spriteScale;

    float camera_focal_length;

    void Start()
    {
        switch (Camera.current)
        {
            case null:
                m_camera = Camera.main;
                break;
            default:
                m_camera = Camera.current;
                break;
        }

        camera_focal_length = 1 / Mathf.Tan(m_camera.fieldOfView * 0.5f * Mathf.Deg2Rad);
    }

    void UpdateBillboardState() {
        if (!m_camera || !m_renderer) return;

        if (m_mode == FacingMode.CameraPosition)
        {
            Vector3 look_target = m_camera.transform.position;
            m_renderer.transform.LookAt(look_target);

            Vector3 dir_to_cam = (m_camera.transform.position - transform.position).normalized;
            float distance_to_cam = (m_camera.transform.position - transform.position).magnitude;

            m_renderer.transform.position = (dir_to_cam * z_offset + transform.position);
            m_renderer.transform.localScale = (Vector3.one) * m_spriteScale * camera_focal_length / (m_renderer.transform.position - m_camera.transform.position).magnitude;
        }
        else if (m_mode == FacingMode.CameraForward)
        {
            Vector3 look_target = m_camera.transform.position;
            look_target.y = m_renderer.transform.position.y;
            m_renderer.transform.LookAt(look_target);

            Vector3 dir_to_cam = (look_target - transform.position).normalized;
            float distance_to_cam = (look_target - transform.position).magnitude;

            m_renderer.transform.position = ((m_camera.transform.position - transform.position).normalized * z_offset + transform.position);
            m_renderer.transform.localScale = (Vector3.one) * m_spriteScale * camera_focal_length / (m_renderer.transform.position - look_target).magnitude;
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        UpdateBillboardState();
    }
}