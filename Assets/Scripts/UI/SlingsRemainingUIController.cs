using UnityEngine;

public class SlingsRemainingUIController : MonoBehaviour
{
    [SerializeField] private GameObject m_slingVisualPrefab;
    void Start() {
        GameManager.instance.player.OnSlingValuesChanged.AddListener(SlingValuesChanged);
    }

    private void SlingValuesChanged(int slingsRemaining) {
        if (m_slingVisualPrefab == null) return;
        //slingsRemaining = Mathf.Max(slingsRemaining - 1, 0);
        Debug.Log(slingsRemaining);

        foreach (Transform child in transform) { // disable all visuals
            if (child && child.gameObject.activeSelf) {
                child.gameObject.SetActive(false);
            }
        }

        if (slingsRemaining > 0) // enable correct number of visuals
            for (int i = 0; i < slingsRemaining; i++) {
                if (i + 1 > transform.childCount) {
                    Transform new_visual_t = Instantiate(m_slingVisualPrefab, transform).transform;
                    new_visual_t.gameObject.SetActive(true);
                }

                GameObject current_visual_go = transform.GetChild(i).gameObject;
                current_visual_go.SetActive(true);
            }
    }
}
