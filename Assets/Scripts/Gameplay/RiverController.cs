using System;
using System.Collections;
using UnityEngine;

public class RiverController : MonoBehaviour
{
    public static RiverController instance;

    [SerializeField] float m_warning_duration = 3;
    [SerializeField] float m_start_duration = 1.75f;
    [SerializeField] float m_end_duration = 2.5f;
    [SerializeField] float m_loop_cooldown = 5;
    [SerializeField] float m_burst_duration = 2;
    private bool m_river_is_damaging = false;

    [Serializable] public enum RiverAction { None, Enable, Disable, Toggle, FireOnce, Loop }
    // the Run action starts then stops the river on it's own.

    bool is_river_flowing = false;

    private Coroutine current_river_event;

    private void OnValidate()
    {
        if (instance == null)
            instance = this;
    }

    private IEnumerator FireOnceCour() {
        EffectManager.instance?.RiverStartAnimation(m_warning_duration);
        is_river_flowing = true;

        yield return new WaitForSeconds(m_warning_duration);
        m_river_is_damaging = true;

        yield return new WaitForSeconds(m_burst_duration);

        EffectManager.instance?.RiverEndAnimation(m_end_duration);
        m_river_is_damaging = false;
        is_river_flowing = false;

        yield return new WaitForSeconds(m_end_duration);
        current_river_event = null;
    }

    private IEnumerator EnableCour () {
        EffectManager.instance?.RiverStartAnimation(m_warning_duration);
        is_river_flowing = true;
        yield return new WaitForSeconds(m_warning_duration);
        current_river_event = null;
    }

    private IEnumerator DisableCour () {
        EffectManager.instance?.RiverEndAnimation(m_end_duration);
        is_river_flowing = false;
        yield return new WaitForSeconds(m_end_duration);
        current_river_event = null;
    }

    private IEnumerator LoopingBurstCour()
    {
        while (true)
        {
            EffectManager.instance?.RiverStartAnimation(m_warning_duration);
            is_river_flowing = true;

            yield return new WaitForSeconds(m_warning_duration);
            m_river_is_damaging = true;

            yield return new WaitForSeconds(m_burst_duration);

            EffectManager.instance?.RiverEndAnimation(m_end_duration);
            m_river_is_damaging = false;
            is_river_flowing = false;
            yield return new WaitForSeconds(m_end_duration);
            yield return new WaitForSeconds(m_loop_cooldown);
        }
    }

    bool is_player_touching = false;

    private void OnCollisionEnter(Collision collision) {
        if (collision.gameObject.tag == "Player")
            is_player_touching = true;
    }

    private void OnCollisionExit(Collision collision) {
        if (collision.gameObject.tag == "Player")
            is_player_touching = false;
    }

    void FixedUpdate()
    {
        if (m_river_is_damaging && is_player_touching)
        {
            // send kill request to player
            GameManager.player.health = 0;
        }
    }

    private void Start() {
        FireRiverEvent(RiverAction.Loop);
    }

    public void FireRiverEvent(RiverAction action)
    {
        if (current_river_event != null)
            StopCoroutine(current_river_event);

        switch (action) {
            case RiverAction.Enable:
                {
                    current_river_event = StartCoroutine(EnableCour());
                    break;
                }
            case RiverAction.Disable:
                {
                    current_river_event = StartCoroutine(DisableCour());
                    break;
                }
            case RiverAction.Toggle:
                {
                    if (is_river_flowing) {
                        current_river_event = StartCoroutine(DisableCour());
                    }
                    else {
                        current_river_event = StartCoroutine(EnableCour());
                    }
                    break;
                }
            case RiverAction.FireOnce:
                {
                    current_river_event = StartCoroutine(FireOnceCour());
                    break;
                }
            case RiverAction.Loop:
                {
                    current_river_event = StartCoroutine(LoopingBurstCour());
                    break;
                }
            default: break;
        }
    }
}
