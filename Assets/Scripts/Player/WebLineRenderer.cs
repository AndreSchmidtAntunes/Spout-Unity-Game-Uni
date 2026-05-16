using UnityEngine;

public class WebLineRenderer : MonoBehaviour
{
    [SerializeField][Range(1, 10)] private int m_meshResolution;
    [SerializeField] AnimationCurve m_positionOffsetOverDistance;
    [SerializeField] AnimationCurve m_positionWeightOverDistance;
    [SerializeField] AnimationCurve m_xOffsetOverLifetime;
    [SerializeField] AnimationCurve m_yOffsetOverLifetime;
    [SerializeField] AnimationCurve m_xOffsetOverDistance;
    [SerializeField] AnimationCurve m_yOffsetOverDistance;
    [SerializeField] AnimationCurve m_thicknessOverLifetime;
    [SerializeField][Min(0)] private float m_animationSpeed = 1;
    [SerializeField] private Vector3 m_gravity = new Vector3(9.5f, 0.3f, 0); // pipe is being viewed sideways so gravity should be to the camera's left.
    [SerializeField] private LayerMask m_layerMask; // pipe is being viewed sideways so gravity should be to the camera's left.
    [SerializeField][Range(0, 1)] float elapsed_time = 0;

    private LineRenderer _lineRenderer;

    [SerializeField] static private float _positions_step_size = -1; // distance between points in the line renderer.
    private float _line_length = -1; // total distance from the player to the end of the web.
    private Vector3 _ray_start_point;
    private Vector3 _ray_end_point;
    private Vector3 _ray_direction;

#if UNITY_EDITOR
    [SerializeField] private bool init = false;

    private void OnValidate() {
        _positions_step_size = 1 / (float)m_meshResolution;
        if (TryGetComponent(out _lineRenderer)) {
            _lineRenderer.useWorldSpace = true;
        }
        if (init) {
            Init(transform.position, transform.forward);
            init = false;
        }
        AnimationStep(elapsed_time);
    }
#endif

    bool CanAnimate() {
        return _line_length > 0;
    }

    void Init(Vector3 origin, Vector3 direction) {
        _ray_start_point = transform.position;
        Physics.Raycast(origin, direction, out RaycastHit hit, 1000, m_layerMask);

        _ray_start_point = origin;
        _ray_end_point = hit.point;
        _line_length = hit.distance;
        _ray_direction = direction;
        elapsed_time = 0;
        GeneratePositions(Mathf.FloorToInt(m_meshResolution * _line_length) + 1);
    }

    void GeneratePositions(int count) {
        _lineRenderer.positionCount = count;
    }

    void FixedUpdate() {
        if (CanAnimate())
        {
            AnimationStep(elapsed_time);
            elapsed_time += Time.fixedDeltaTime * m_animationSpeed;

            if (elapsed_time > 1) { // reset completely so it can be reused in the web renderer cache
                _positions_step_size = -1;
                _line_length = -1;
                gameObject.SetActive(false);
            }
        }
    }

    void AnimationStep(float time) { // time is between 0 and 1
        if (!CanAnimate()) return;

        for (int i = 0; i < _lineRenderer.positionCount; i ++)
        {
            Vector3 position;

            float normalised_distance = (float)i / (_lineRenderer.positionCount - 1);

            if (i == _lineRenderer.positionCount - 1) { // last point
                position = _ray_end_point;
            }
            else {
                position = _ray_start_point + _ray_direction * normalised_distance * _line_length;
                Vector2 offsetOverDistance = new Vector2(m_xOffsetOverDistance.Evaluate(normalised_distance), m_xOffsetOverDistance.Evaluate(normalised_distance));
                offsetOverDistance *= m_positionOffsetOverDistance.Evaluate(normalised_distance);
                Vector2 offsetOverLifetime = new Vector2(m_xOffsetOverLifetime.Evaluate(time), m_yOffsetOverLifetime.Evaluate(time));
                Vector2 offsetFromGravity = m_gravity * m_positionWeightOverDistance.Evaluate(normalised_distance) * time;

                position += (Vector3)(offsetOverDistance + offsetOverLifetime + offsetFromGravity);
            }

            _lineRenderer.SetPosition(i, position);
        }

        // animate thickness

        _lineRenderer.widthMultiplier = m_thicknessOverLifetime.Evaluate(time);
    }

    private static System.Collections.Generic.List<WebLineRenderer> renderer_cache = new();
    private static Transform cache_container;

    const int cache_size = 6;


    void Awake() {
        if (_lineRenderer == null) TryGetComponent(out _lineRenderer);

        if (prefab == null) Initialise();
    }

    static GameObject prefab = null;

    private void Initialise() {

        prefab = gameObject;

        cache_container = new GameObject("Web Renderer Cache").transform;

        for (int i = 0; i < cache_size; i++) {
            GameObject renderer_go = Instantiate(prefab, cache_container);
            if (renderer_go != null && renderer_go.TryGetComponent(out WebLineRenderer new_renderer)) {
                new_renderer.gameObject.SetActive(true);
                new_renderer.gameObject.name = "Web Renderer " + i.ToString();
                new_renderer._line_length = -1;
                new_renderer.gameObject.SetActive(false);
                renderer_cache.Add(new_renderer);
            }
            else Destroy(renderer_go);
        }
    }

    public static void Render(Vector3 origin, Vector3 direction) {
        foreach (WebLineRenderer renderer in renderer_cache) {
            if (!renderer.gameObject.activeSelf) {
                renderer.gameObject.SetActive(true);
                renderer.Init(origin, direction);
                return;
            }
        }
        throw new System.Exception("Attempted to render a web while the web renderer cache was full.");
    }
}
