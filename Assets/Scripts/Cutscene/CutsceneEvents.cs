using UnityEngine;

public class CutsceneEvents : MonoBehaviour
{

    [SerializeField] private Animation m_animation = null;
    public void SetMaxPlayerSlingsCount(int count)
    {
        if (GameManager.instance.player == null) return;
        GameManager.instance.player.SetMaxSlings(count);
    }

    public void RunClip(string name) {
        if (m_animation == null) {
            throw new System.Exception("No animation has been set to run.");
        }
        else {
            m_animation.Play(name);
        }
    }

    public void EnablePlayerMovement()
    {
        if (GameManager.instance.player == null) return;
        GameManager.instance.player.movement_enabled = true;
    }

    public void DisablePlayerMovement()
    {
        if (GameManager.instance.player == null) return;
        GameManager.instance.player.movement_enabled = false;
    }
}
