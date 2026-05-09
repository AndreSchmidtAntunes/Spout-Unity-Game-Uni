using System.Collections;
using UnityEngine;

public class PipeRiverEventController : MonoBehaviour
{
    [SerializeField] float m_flow_duration = 4; // amount of time the river is dangerous

    [SerializeField] float m_warning_duration = 2;
    [SerializeField] float m_end_duration = 2;
    [SerializeField] bool m_is_river_active = false;
    [SerializeField] private bool m_river_is_damaging = false;

    //private bool __is_river_active = false;

    public bool is_river_active {
        get { return (m_is_river_active); }
        private set
        {
            //if (__is_river_active == value) return;

            //__is_river_active = value;
            m_is_river_active = value;

            if (value)
            {
                StartCoroutine(FireRiver(m_warning_duration, m_flow_duration, m_end_duration));
            }
            else
            {
                m_river_is_damaging = false;
                EffectManager.instance?.EndRiverEvent(m_end_duration);
                Debug.Log(EffectManager.instance);
            }
        }
    }

    private void OnValidate() {
        is_river_active = m_is_river_active;
    }

    private IEnumerator FireRiver(float warning_duration, float flowing_duration, float ending_duration) {

        EffectManager.instance?.StartRiverEvent(warning_duration);
        Debug.Log(EffectManager.instance);
        StartCoroutine(ForceDisableRiverTimer(warning_duration + flowing_duration, ending_duration));

        yield return new WaitForSeconds(warning_duration);
        if (m_is_river_active) {
            m_river_is_damaging = true;
        }
    }

    private IEnumerator ForceDisableRiverTimer(float wait_duration, float ending_duration) {
        yield return new WaitForSeconds(wait_duration);
        m_is_river_active = false;
        m_river_is_damaging = false;
        EffectManager.instance?.EndRiverEvent(ending_duration);
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
            // send damage request to player
            GameManager.player.health = 0;
        }
    }
}
