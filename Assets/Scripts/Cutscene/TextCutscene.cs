using System;
using UnityEngine;
using Unity.Cinemachine;
using System.Collections;

public class TextCutscene : MonoBehaviour
{
    [Serializable] private enum RunType { None, OnceOnTriggerEnter, OnceOnTriggerExit }
    [SerializeField] bool m_disablePlayerMovement = true;
    [SerializeField] RunType m_runEvent = RunType.None;
    [SerializeField] CinemachineCamera m_cutsceneCamera;
    [SerializeField] string[] m_text;


    int trigger_enter_count = 0;
    private void OnTriggerEnter(Collider other) {
        if (m_runEvent == RunType.OnceOnTriggerEnter && trigger_enter_count == 0) {
            StartCoroutine(CutsceneCour());
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
        if (m_disablePlayerMovement) 
            GameManager.player.movement_enabled = false;

        yield return StartCoroutine(UIManager.instance.playerTextbox.DisplayTextArray(m_text));

        m_cutsceneCamera.Priority -= 100;
        if (m_disablePlayerMovement) 
            GameManager.player.movement_enabled = true;
    }
}