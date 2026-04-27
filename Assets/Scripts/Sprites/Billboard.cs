using UnityEngine;
using UnityEngine.UIElements;

public class Billboard : MonoBehaviour
{
    enum FacingMode { CameraPosition, CameraForward }
    [SerializeField] FacingMode m_mode = FacingMode.CameraPosition;
    [SerializeField] Transform m_billboardVisual;
    [SerializeField] float z_offset = 0.55f;
    [SerializeField] float m_spriteScale = 20;
    [SerializeField] bool m_flip = false;

    Camera m_camera;
    float camera_focal_length;

    void Start()
    {
        m_camera = Camera.current != null ? Camera.current : Camera.main;

        camera_focal_length = 1 / Mathf.Tan(m_camera.fieldOfView * 0.5f * Mathf.Deg2Rad);
    }
    void UpdateBillboardState()
    {
        if (!m_camera || !m_billboardVisual) return;

        if (m_mode == FacingMode.CameraPosition)
        {
            Vector3 look_target = m_camera.transform.position;
            m_billboardVisual.LookAt(look_target);

            Vector3 dir_to_cam = (m_camera.transform.position - transform.position).normalized;
            float distance_to_cam = (m_camera.transform.position - transform.position).magnitude;

            m_billboardVisual.position = (dir_to_cam * z_offset + transform.position);
            m_billboardVisual.localScale = (Vector3.one) * m_spriteScale * camera_focal_length / (m_billboardVisual.position - m_camera.transform.position).magnitude;
        }
        else if (m_mode == FacingMode.CameraForward)
        {
            Vector3 look_target = m_camera.transform.position;
            look_target.y = m_billboardVisual.position.y;
            m_billboardVisual.LookAt(look_target);

            Vector3 dir_to_cam = (look_target - transform.position).normalized;
            float distance_to_cam = (look_target - transform.position).magnitude;

            m_billboardVisual.position = ((m_camera.transform.position - transform.position).normalized * z_offset + transform.position);
            m_billboardVisual.localScale = (Vector3.one) * m_spriteScale * camera_focal_length / (m_billboardVisual.position - look_target).magnitude;
        }

        if (m_flip)
            m_billboardVisual.Rotate(0, 180, 0);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        UpdateBillboardState();
    }
}