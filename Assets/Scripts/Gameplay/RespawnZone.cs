using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class RespawnZone : MonoBehaviour
{
    [SerializeField] private Transform m_respawn_point_transform;
    public Vector3 respawn_point { get { return m_respawn_point_transform.position; } }

    [SerializeField] private bool override_checkpoint_id = false;
    [SerializeField] private int checkpoint_id = -1; // Should be in order of when the player will touch each respawn zone;

    [SerializeField] private bool priority_load = false;

    private Collider m_collider;

    private void Start() {
        if (priority_load) // load on start as soon as possible if needed
            Init();
        else
            StartCoroutine(InitCoroutine());
    }

    // allow loading throughout multiple frames
    private IEnumerator InitCoroutine() { Init(); yield return null; }

    private void Init()
    {
        TryGetComponent(out m_collider);

        GameManager.AddRespawnZone(this);
    }

    private void OnValidate() {
        if (override_checkpoint_id) {
            GameManager.SetRespawZoneIOf(this, checkpoint_id);
        }
    }

    private void OnCollisionEnter(Collision collision) {
        GameManager.OnRespawnZoneHit(this);

    }

    private void OnCollisionExit(Collision collision)  { }
}