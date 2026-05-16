using NUnit.Framework;
using System;
using System.Collections;
using UnityEngine;

public class WebRenderer : MonoBehaviour
{
    [SerializeField] private Material m_material;
    [SerializeField] private Joint m_jointPrefab;

    [SerializeField][UnityEngine.Range(1,16)] private int m_meshResolution = 8;
    [SerializeField][Min(0)] float m_maxLength = 1000;
    [SerializeField] LayerMask m_layerMask = 0;
    [SerializeField] bool m_flipDirection = true;

    private float _jointStepSize;
    private LineRenderer _lineRenderer;
    private Rigidbody _rigidRoot;
    private void OnValidate()
    {
        _jointStepSize = 1 / (float)m_meshResolution;
        if (TryGetComponent(out _lineRenderer)) {

            if (m_material != null)
            _lineRenderer.material = m_material;
        }
        else {
            _lineRenderer = gameObject.AddComponent<LineRenderer>();
        }

        if (_rigidRoot == null ) {
            _rigidRoot = new GameObject("web_root (Rigidbody)").AddComponent<Rigidbody>();
            _rigidRoot.isKinematic = true;
            _rigidRoot.useGravity = false;
        }
    }

    public void Render(Vector3 origin, Vector3 direction, float maxDistance, int layerMask, LineRenderer lineRenderer, Material material, Joint jointPrefab, float jointStepSize, bool flipDirection) {
        Physics.Raycast(origin, direction, out RaycastHit hit, maxDistance, layerMask);

        float distance = hit.distance;
        float distance_to_root = flipDirection ? (distance - jointStepSize) : jointStepSize;

        Vector3 point = hit.point;
        Vector3 start_position = flipDirection ? point : Vector3.zero;
        Quaternion root_rotation = Quaternion.LookRotation(Vector3.up, flipDirection ? (-direction) : direction);

        _rigidRoot.transform.position = start_position;
        _rigidRoot.transform.rotation = root_rotation;
        _rigidRoot.transform.localScale = new Vector3(jointStepSize, jointStepSize, jointStepSize);
        Rigidbody current_joint = _rigidRoot;
        int joint_count = Mathf.FloorToInt(distance / jointStepSize) + 1;

        for (int i = 0; i < joint_count - 1; i++) {
            GameObject new_joint_go = Instantiate(jointPrefab.gameObject, Vector3.up, Quaternion.identity, current_joint.transform);
            new_joint_go.transform.localScale = Vector3.one;

            new_joint_go.GetComponent<Joint>().connectedBody = current_joint;

            current_joint = new_joint_go.GetComponent<Rigidbody>();
            current_joint.includeLayers = layerMask;
        }

        float remaining_distance = (distance / jointStepSize) - joint_count - 1;
        GameObject leaf_joint_go = Instantiate(jointPrefab.gameObject, Vector3.up * remaining_distance, Quaternion.identity, current_joint.transform);
        leaf_joint_go.GetComponent<Joint>().connectedBody = current_joint;

        line_positions_update_loop_condion = true;
        StartCoroutine(_UpdateLineRendererLoopCour(joint_count, current_joint.transform, lineRenderer));
    }

    private bool line_positions_update_loop_condion = false;
    private IEnumerator _UpdateLineRendererLoopCour(int jointCount, Transform leafJoint, LineRenderer lineRenderer) {
        System.Collections.Generic.List<Vector3> joint_positions = new(jointCount);
        Transform current_joint;

        while (true) {

            if (!line_positions_update_loop_condion)
                yield return new WaitUntil(()=>line_positions_update_loop_condion);

            current_joint = leafJoint;
            for(int i = 0; i < jointCount; i++) {
                joint_positions[i] = current_joint.position;
                if (i < (jointCount - 1)) current_joint = current_joint.parent;
            }
            lineRenderer.SetPositions(joint_positions.ToArray());
            yield return new WaitForFixedUpdate();
        }
    }

    public void Render (Vector3 origin, Vector3 direction) {
        Render(origin, direction, m_maxLength, m_layerMask, _lineRenderer, m_material, m_jointPrefab, _jointStepSize, m_flipDirection);
    }

    public void Start() {
        Render(transform.position, transform.up);
    }
}
