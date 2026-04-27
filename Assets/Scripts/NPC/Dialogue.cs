using System.Collections;
using UnityEngine;

public class Dialogue : MonoBehaviour
{
    enum DialogueType { Cycle, Random, Repeat_Last, }

    [SerializeField] private DialogueType m_dialogueType = DialogueType.Cycle;
    [SerializeField][TextArea] private string[] m_dialogue_lines = { "" };
    [SerializeField] private float m_distance_to_player = 3f;
    [SerializeField] private GameObject DialogueBox;
    [SerializeField] private TMPro.TMP_Text[] DialogueTMPs = { null };

    const float tick_length = 0.25f;

    bool showing_dialogue = false;

    int current_line;

    void Start()
    {
        current_line = m_dialogue_lines.Length - 1; // will set to 0 when first shown

        HideDialogue();
        StartCoroutine(DistanceChecker());
    }

    private IEnumerator DistanceChecker()
    {
        while (true)
        {
            if (GameManager.player && Vector3.SqrMagnitude(GameManager.player.transform.position - transform.position) <= m_distance_to_player * m_distance_to_player)
            {
                if (showing_dialogue == false)
                {
                    showing_dialogue = true;
                    ShowDialogue();
                    yield return new WaitForSeconds(tick_length);
                }
            }
            else {
                if (showing_dialogue == true)
                {
                    HideDialogue();
                    showing_dialogue = false;
                }
            }
            yield return new WaitForSeconds(tick_length);
        }
    }

    private void ShowDialogue() {
        DialogueBox.SetActive(true);
        CycleLines();
    }

    private void HideDialogue() {
        DialogueBox.SetActive(false);
    }

    private void CycleLines() {

        switch (m_dialogueType) {
            case DialogueType.Cycle:
                current_line = (current_line + 1) % m_dialogue_lines.Length;
                break;
            case DialogueType.Repeat_Last:
                current_line = Mathf.Min(current_line + 1, m_dialogue_lines.Length - 1);
                break;
            case DialogueType.Random:
                // get a random line that isn't the current line
                current_line = (current_line + Random.Range(1, m_dialogue_lines.Length)) % m_dialogue_lines.Length;
                break;
        }

        foreach (var tmp in DialogueTMPs) {
            if (tmp.isActiveAndEnabled == false) continue;

            tmp.text = m_dialogue_lines[current_line];
        }
    }
}
