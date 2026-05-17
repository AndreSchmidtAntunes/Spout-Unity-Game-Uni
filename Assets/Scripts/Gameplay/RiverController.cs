using System;
using System.Collections;
using UnityEngine;

public class RiverController : MonoBehaviour
{
    [SerializeField] public static RiverController instance = null;

    [SerializeField] float m_start_duration = 3;
    [SerializeField] float m_end_duration = 2.5f;
    [SerializeField] float m_loop_cooldown = 5;
    [SerializeField] float m_burst_duration = 2;
    [SerializeField] BoxCollider m_damage_box;

    [Serializable] public enum RiverAction { None, Enable, Disable, Toggle, FireOnce, Loop }

    bool is_river_flowing = false;

    private Coroutine current_river_event;

    private void Awake() {
        if (instance == null)
            instance = this;
    }

    private IEnumerator FireOnceCour() {
        EffectManager.instance?.RiverStartAnimation(m_start_duration);
        is_river_flowing = true;

        yield return new WaitForSeconds(m_start_duration);
        m_damage_box.enabled = true;

        yield return new WaitForSeconds(m_burst_duration);
        m_damage_box.enabled = false;

        EffectManager.instance?.RiverEndAnimation(m_end_duration);

        is_river_flowing = false;

        yield return new WaitForSeconds(m_end_duration);
        current_river_event = null;
    }

    private IEnumerator EnableCour () {
        EffectManager.instance?.RiverStartAnimation(m_start_duration, 0.35f);
        is_river_flowing = true;

        m_damage_box.enabled = true;
        yield return new WaitForSeconds(m_start_duration);

        current_river_event = null;
    }

    private IEnumerator DisableCour () {
        EffectManager.instance?.RiverEndAnimation(m_end_duration, 0.35f);
        is_river_flowing = false;

        m_damage_box.enabled = false;
        yield return new WaitForSeconds(m_end_duration);

        current_river_event = null;
    }

    private IEnumerator LoopingBurstCour()
    {
        while (true)
        {
            EffectManager.instance?.RiverStartAnimation(m_start_duration);

            is_river_flowing = true;

            yield return new WaitForSeconds(m_start_duration);
            m_damage_box.enabled = true;

            yield return new WaitForSeconds(m_burst_duration);
            m_damage_box.enabled = false;

            EffectManager.instance?.RiverEndAnimation(m_end_duration);
            is_river_flowing = false;
            yield return new WaitForSeconds(m_end_duration);

            yield return new WaitForSeconds(m_loop_cooldown);
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