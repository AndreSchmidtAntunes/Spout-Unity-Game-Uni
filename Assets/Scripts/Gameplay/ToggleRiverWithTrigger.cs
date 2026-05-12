using System;
using UnityEngine;

public class ToggleRiverWithTrigger : MonoBehaviour
{

    [SerializeField] private PipeRiverEventController.RiverAction onTriggerEnter = PipeRiverEventController.RiverAction.None;
    [SerializeField] private PipeRiverEventController.RiverAction onTriggerExit = PipeRiverEventController.RiverAction.None;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other) {
        RunRiverAction(onTriggerEnter);
    }
    private void OnTriggerExit(Collider other) {
        RunRiverAction(onTriggerExit);
    }

    const float river_start_time = 1;
    const float river_end_time = 1;

    void RunRiverAction(PipeRiverEventController.RiverAction type) {


        PipeRiverEventController.is_looping = false;
        switch (type)
        {
            case PipeRiverEventController.RiverAction.SetTrue:
                {
                    EffectManager.instance.StartRiverEvent(river_start_time);
                    break;
                }
            case PipeRiverEventController.RiverAction.SetFalse:
                {
                    EffectManager.instance.EndRiverEvent(river_end_time);
                    break;
                }
            case PipeRiverEventController.RiverAction.Toggle:
                {
                    if (EffectManager.instance.river_active)
                        EffectManager.instance.EndRiverEvent(river_end_time);
                    else
                        EffectManager.instance.StartRiverEvent(river_start_time);
                    break;
                }
            case PipeRiverEventController.RiverAction.Loop: 
                {
                    PipeRiverEventController.is_looping = true;
                    break;
                }

            default: break;
        }
    }
}
