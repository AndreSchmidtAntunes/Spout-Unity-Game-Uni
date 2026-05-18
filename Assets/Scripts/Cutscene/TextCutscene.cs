using System;
using UnityEngine;
using UnityEngine.Events;
using Unity.Cinemachine;
using System.Collections;

public class TextCutscene : MonoBehaviour
{
    [Serializable] private enum RunType { None, OnceOnTriggerEnter, OnceOnTriggerExit }
    [SerializeField] bool m_disablePlayerMovement = true;
    [SerializeField] RunType m_runEvent = RunType.None;
    [SerializeField] CinemachineCamera m_cutsceneCamera;
    [SerializeField] string[] m_text;
    [SerializeField] private bool m_retainCameraAfterCutscene = false;

    public UnityEvent onCutsceneStart;
    public UnityEvent onCutsceneEnd;

    int trigger_enter_count = 0;
    private void OnTriggerEnter(Collider other) {
        if (m_runEvent == RunType.OnceOnTriggerEnter && trigger_enter_count == 0) {
            Debug.Log("Entered trigger" + gameObject.name);
            StartCoroutine(CutsceneCour());
            onCutsceneStart.Invoke();
            trigger_enter_count++;
        }
    }

    int trigger_exit_count = 0;
    private void OnTriggerExit(Collider other) {
        if (m_runEvent == RunType.OnceOnTriggerExit && trigger_exit_count == 0) {
            StartCoroutine(CutsceneCour());
            trigger_exit_count++;
        }
    }

    private IEnumerator CutsceneCour() {
        m_cutsceneCamera.Priority += 100;
        if (m_disablePlayerMovement) {
            GameManager.instance.player.movement_enabled = false;
        }

        yield return StartCoroutine(UIManager.instance.playerTextbox.DisplayTextArray(m_text));
        onCutsceneEnd.Invoke();
        if (!m_retainCameraAfterCutscene)
            m_cutsceneCamera.Priority -= 100;
        if (m_disablePlayerMovement) {
            GameManager.instance.player.movement_enabled = true;
        }
    }
}