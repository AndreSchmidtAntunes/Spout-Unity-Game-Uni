using UnityEngine;

#if UNITY_EDITOR
    using UnityEditor;
#endif

public class Billboard : MonoBehaviour
{
    enum FacingMode { CameraPosition, CameraForward }
    [SerializeField] FacingMode m_mode = FacingMode.CameraPosition;
    [SerializeField] Transform m_billboardVisual;
    [SerializeField] public float z_offset = 0.55f;
    [SerializeField] public float m_scale = 20;
    [SerializeField] public float m_rotation = 0;
    [SerializeField] public Vector2 m_offset = Vector2.zero;

    [SerializeField] public bool m_flip = false;

    Camera m_camera;
    float camera_focal_length;

    public static Camera GetCurrentCamera()
    {
    #if UNITY_EDITOR
        // Use Scene view camera while editing
        if (!Application.isPlaying)
        {
            SceneView sceneView = SceneView.lastActiveSceneView;

            if (sceneView != null && sceneView.camera != null)
            {
                return sceneView.camera;
            }
        }
    #endif
        return Camera.main;
    }

    void OnValidate() {
        m_camera = GetCurrentCamera();
        camera_focal_length = 1 / Mathf.Tan(m_camera.fieldOfView * 0.5f * Mathf.Deg2Rad);
    }

    void UpdateBillboardState()
    {
        if (!m_camera || !m_billboardVisual) return;

        Vector3 camera_pos = m_camera.transform.position;
        Vector3 look_target = camera_pos;

        Vector3 dir_to_cam = (m_camera.transform.position - transform.position).normalized;

        m_billboardVisual.position = dir_to_cam * z_offset + transform.position + (m_offset.x * m_camera.transform.right) + (m_offset.y * m_camera.transform.up);

        if (m_mode == FacingMode.CameraPosition)
        {
            m_billboardVisual.LookAt(look_target);
        }
        else if (m_mode == FacingMode.CameraForward)
        {
            look_target.y = m_billboardVisual.position.y;
            m_billboardVisual.LookAt(look_target);
        }
        m_billboardVisual.transform.Rotate(Vector3.forward, m_rotation);


        float distance_to_camera = (transform.position - m_camera.transform.position).magnitude;
        float visual_distance_to_camera = (m_billboardVisual.position - m_camera.transform.position).magnitude;

        m_billboardVisual.localScale = Vector3.one * m_scale * visual_distance_to_camera / distance_to_camera;

        if (m_flip)
            m_billboardVisual.Rotate(0, 180, 0);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        UpdateBillboardState();
    }

    void OnDrawGizmos() {
        UpdateBillboardState();
    }
}

