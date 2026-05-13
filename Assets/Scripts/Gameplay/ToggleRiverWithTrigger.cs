using UnityEngine;

public class ToggleRiverWithTrigger : MonoBehaviour
{
    [SerializeField] private RiverController.RiverAction
        onTriggerEnter, onTriggerExit;

    private void OnTriggerEnter(Collider other) {
        RiverController.instance.FireRiverEvent(onTriggerEnter);
    }
    private void OnTriggerExit(Collider other)
    {
        RiverController.instance.FireRiverEvent(onTriggerExit);
    }
}
