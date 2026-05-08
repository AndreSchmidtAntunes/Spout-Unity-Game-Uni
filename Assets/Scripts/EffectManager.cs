using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

public class EffectManager : MonoBehaviour
{
    [SerializeField] private static EffectManager m_instance = null;
    public static EffectManager instance {
        private set { m_instance = value; }
        get { return m_instance; }
    }

    [Header("River Event")]

    [SerializeField] AnimationCurve warning_shake_intencity_curve = new(new Keyframe[] { new Keyframe(0, 0), new Keyframe(1, 1) });
    [SerializeField] AnimationCurve ending_shake_intencity_curve = new(new Keyframe[] { new Keyframe(1, 0), new Keyframe(1, 0) });

    [SerializeField] AnimationCurve river_start_shader_active_curve = new(new Keyframe[] { new Keyframe(0, 0), new Keyframe(1, 1)});
    [SerializeField] AnimationCurve river_end_shader_active_curve   = new(new Keyframe[] { new Keyframe(1, 0), new Keyframe(0, 1)});

    [SerializeField] Material river_flowing_material;

    [SerializeField] float river_shake_intensity = 1.0f;
    [SerializeField] CinemachineBasicMultiChannelPerlin CameraShakeNoise;

    private void Awake() {
        if (m_instance == null) {
            m_instance = this;
            GameObject.DontDestroyOnLoad(gameObject);
        }
    }

    static Camera current_camera = null;

    public void CameraShake(AnimationCurve curve, float intencity, float duration)
    {
        if (current_camera == null)
            current_camera = Camera.main;

        if (current_camera)
        StartCoroutine(CameraShakeCour(current_camera, curve, intencity, duration));
    }
    private IEnumerator CameraShakeCour(Camera camera, AnimationCurve curve, float intensity, float duration)
    {
        float elapsed_time = 0;
        while (elapsed_time < duration)
        {
            CameraShakeNoise.FrequencyGain = curve.Evaluate(elapsed_time/duration) * river_shake_intensity;
            CameraShakeNoise.AmplitudeGain = curve.Evaluate(elapsed_time/duration) * river_shake_intensity;
            yield return new WaitForFixedUpdate();
            elapsed_time += Time.fixedDeltaTime;
        }
    }

    private IEnumerator RiverShaderActiveAnimatorCour(Material river_flowing_mat, AnimationCurve curve, float duration) {

        float elapsed_time = 0;
        while (elapsed_time < duration) {

            river_flowing_mat.SetFloat("_Active", curve.Evaluate(elapsed_time / duration));

            Debug.Log(curve.Evaluate(elapsed_time / duration));


            yield return new WaitForFixedUpdate();
            elapsed_time += Time.fixedDeltaTime;
        }
    }

    bool can_animate_river_visuals = true; // animation coroutines use this to wait for other animation coroutines to complete

    private IEnumerator RiverEventVisualsAnimationCour(Camera camera, Material river_flowing_mat, AnimationCurve river_shader_active_curve, AnimationCurve camera_shake_curve, float shake_intensity, float duration) {
        can_animate_river_visuals = false;
            StartCoroutine(RiverShaderActiveAnimatorCour(river_flowing_mat, river_shader_active_curve, duration));
            yield return StartCoroutine(CameraShakeCour(camera, camera_shake_curve, shake_intensity, duration)); // we are yielding here to wait for the specific duration before allowing the river state to animate
        can_animate_river_visuals = true;
    }

    private bool river_active = false;

    public void StartRiverEvent(float starting_duration)
    {
        if (!river_active) {
            StartCoroutine(StartRiverEventCour(starting_duration));
            river_active = true;
        }
    }
    private bool can_run_river_start_cour = true; // used to force only one active coroutine for running the river starting animation.
    private IEnumerator StartRiverEventCour(float starting_duration)
    {
        if (can_run_river_start_cour) {
            can_run_river_start_cour = false;

            can_run_river_end_cour = true;

            yield return new WaitWhile(() => can_animate_river_visuals);
            yield return StartCoroutine(RiverEventVisualsAnimationCour(current_camera, river_flowing_material, river_start_shader_active_curve, warning_shake_intencity_curve, river_shake_intensity, starting_duration));
            can_run_river_start_cour = true;
        }
    }

    public void EndRiverEvent(float ending_duration) {
        if (river_active) { // allow the ending of the animation to be queued by ignoring if an animation is currently playing
            StartCoroutine(EndRiverEventCour(ending_duration));
            river_active = false;
        }
    }
    private bool can_run_river_end_cour = false; // used to force only one active coroutine for running the river ending animation.
    private IEnumerator EndRiverEventCour(float ending_duration)
    {
        if (can_run_river_end_cour) {
            can_run_river_end_cour = false;
            yield return new WaitWhile(() => can_animate_river_visuals);
            yield return StartCoroutine(RiverEventVisualsAnimationCour(current_camera, river_flowing_material, river_end_shader_active_curve, ending_shake_intencity_curve, river_shake_intensity, ending_duration));
            can_run_river_end_cour = true;
        }
    }
}