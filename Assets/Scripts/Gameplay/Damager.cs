using System.Collections;
using UnityEngine;

[RequireComponent (typeof(Collider))]
public class Damager : MonoBehaviour
{
    [Range(-PlayerCharCon.max_health, PlayerCharCon.max_health)]
    [SerializeField] int m_damage_delt; // negative values are for healing the player
    public int damage_delt { get { return m_damage_delt; } }

    [SerializeField] bool m_override_invincibility = false;
    public bool override_invincibility { get { return m_override_invincibility; } }

    [SerializeField] bool m_override_i_frames = false;
    public bool override_i_frames { get { return m_override_i_frames; } }

    [SerializeField] private bool priority_load = false;

    private Collider m_collider;

    private void Start() {
        if (priority_load) // load on start as soon as possible if needed
            Init();
        else
            StartCoroutine(InitCoroutine());
    }

    private void Init() {
        TryGetComponent(out m_collider);
    }
    // allow loading throughout multiple frames
    private IEnumerator InitCoroutine() { Init(); yield return null; }

    // Only player can collide with a damager, so a check doesn't need to occur.
    private void OnCollisionEnter(Collision collision)
    { GameManager.instance.player.touching_damagers.Add(this);    }

    private void OnCollisionExit(Collision collision)
    { GameManager.instance.player.touching_damagers.Remove(this); }
}