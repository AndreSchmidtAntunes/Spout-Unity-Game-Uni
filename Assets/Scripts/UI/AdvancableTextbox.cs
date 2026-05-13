using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AdvancableTextbox : MonoBehaviour
{
    public static AdvancableTextbox instance { get; private set; }

    [SerializeField] private TMP_Text m_tmpText;
    [SerializeField] private GameObject m_visuals;
    [SerializeField] private Button m_advance_text_button;

    private void Awake()
    {
        if (instance == null)
            instance = this;
    }

    void Start() {
        m_advance_text_button.onClick.AddListener(ButtonListener);
        HideTextboxInstant();
    }

    bool button_is_pressed = false;
    void ButtonListener() {
        button_is_pressed = true;
    }

    private IEnumerator WaitForButtonPress()
    {
        yield return new WaitUntil(() => button_is_pressed);
        button_is_pressed = false;
    }

    public IEnumerator DisplayTextArray(string[] text) {
        for (int i = 0; i < text.Length; i++) {
            // run this after so that the show animation runs with the first text already visible.
            if (i == 0) yield return StartCoroutine(ShowTextBox());

            m_tmpText.text = text[i];

            yield return StartCoroutine(WaitForButtonPress());
        }
        yield return StartCoroutine(HideTextBox());
    }

    /// These functions can be changed to add animations ///

    protected virtual IEnumerator ShowTextBox() {
        m_visuals.SetActive(true);
        yield return null;
    }

    protected virtual IEnumerator HideTextBox() {
        m_visuals.SetActive(false);
        yield return null;
    }

    protected virtual void HideTextboxInstant() {
        m_visuals.SetActive(false);
    }
}