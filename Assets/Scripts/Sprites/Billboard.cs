using UnityEngine;

public class Billboard : MonoBehaviour
{

    enum FacingMode { CameraPosition, CameraForward}
    [SerializeField] FacingMode m_mode = FacingMode.CameraPosition;
    [SerializeField] bool flip = false;
    [SerializeField] float z_offset = 0;
    Camera m_camera;
    [SerializeField] SpriteRenderer m_renderer;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
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
    }

    void UpdateBillboardState() {
        if (!m_camera || !m_renderer) return;

        if (m_mode == FacingMode.CameraPosition)
        {
            Vector3 look_target = m_camera.transform.position;
            m_renderer.transform.LookAt(look_target);

            m_renderer.transform.position = (m_renderer.transform.forward * z_offset) + transform.position;
            m_renderer.transform.localScale = (Vector3.Distance(look_target, transform.position) - Vector3.Distance(look_target, m_renderer.transform.position)) * Vector3.one;
        }
        else if (m_mode == FacingMode.CameraForward)
        {
            Vector3 look_target = m_camera.transform.position;
            look_target.y = m_renderer.transform.position.y;
            m_renderer.transform.LookAt(look_target);

            m_renderer.transform.position = (m_renderer.transform.forward * z_offset) + transform.position;
            m_renderer.transform.localScale = (Vector3.Distance(m_camera.transform.position, transform.position) - Vector3.Distance(m_camera.transform.position, m_renderer.transform.position)) * Vector3.one;
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        UpdateBillboardState();
    }
}