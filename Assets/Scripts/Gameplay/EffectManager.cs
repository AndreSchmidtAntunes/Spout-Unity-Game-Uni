using System.Collections;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Rendering;

public class EffectManager : MonoBehaviour
{
    public static EffectManager instance;

    [Header("River Event")]

    [SerializeField] AnimationCurve warning_shake_intencity_curve = new(new Keyframe[] { new Keyframe(0, 0), new Keyframe(1, 1) });
    [SerializeField] AnimationCurve ending_shake_intencity_curve = new(new Keyframe[] { new Keyframe(1, 0), new Keyframe(1, 0) });

    [SerializeField] AnimationCurve river_start_shader_active_curve = new(new Keyframe[] { new Keyframe(0, 0), new Keyframe(1, 1)});
    [SerializeField] AnimationCurve river_end_shader_active_curve   = new(new Keyframe[] { new Keyframe(1, 0), new Keyframe(0, 1)});

    [SerializeField] Material river_flowing_material;

    [SerializeField] float river_shake_intensity = 1.0f;
    [SerializeField] CinemachineBasicMultiChannelPerlin CameraShakeNoise;
    [SerializeField] Volume m_river_flowing_volume;

    private void Awake() {
        if (instance == null) {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    static Camera current_camera = null;

    public void AnimateCameraShake(AnimationCurve curve, float intencity, float duration)
    {
        if (current_camera == null)
            current_camera = Camera.main;

        if (current_camera)
        StartCoroutine(CameraShakeCour(current_camera, curve, intencity, duration));
    }
    private IEnumerator CameraShakeCour(Camera camera, AnimationCurve curve, float camera_shake_multiplier, float duration)
    {
        float elapsed_time = 0;
        while (elapsed_time < duration)
        {
            CameraShakeNoise.FrequencyGain = curve.Evaluate(elapsed_time/duration) * river_shake_intensity * camera_shake_multiplier;
            CameraShakeNoise.AmplitudeGain = curve.Evaluate(elapsed_time/duration) * river_shake_intensity * camera_shake_multiplier;
            yield return new WaitForFixedUpdate();
            elapsed_time += Time.fixedDeltaTime;
        }
    }

    private IEnumerator RiverShaderActiveAnimatorCour(Material river_flowing_mat, AnimationCurve curve, float duration) {

        float elapsed_time = 0;

        while (elapsed_time < duration) {
            float normalized_elapsed_time = elapsed_time / duration;
            river_flowing_mat.SetFloat("_Active", curve.Evaluate(normalized_elapsed_time));
            m_river_flowing_volume.weight = curve.Evaluate(normalized_elapsed_time);

            yield return new WaitForFixedUpdate();
            elapsed_time += Time.fixedDeltaTime;
        }
    }

    bool can_animate_river_visuals = true; // animation coroutines use this to wait for other animation coroutines to complete

    private IEnumerator RiverEventVisualsAnimationCour(Camera camera, Material river_flowing_mat, AnimationCurve river_shader_active_curve, AnimationCurve camera_shake_curve, float camera_shake_multiplier, float duration) {
        yield return new WaitUntil(() => can_animate_river_visuals);

        can_animate_river_visuals = false;
            StartCoroutine(RiverShaderActiveAnimatorCour(river_flowing_mat, river_shader_active_curve, duration));
            yield return StartCoroutine(CameraShakeCour(camera, camera_shake_curve, camera_shake_multiplier, duration)); // we are yielding here to wait for the specific duration before allowing the river state to animate
        can_animate_river_visuals = true;
    }

    public bool river_active { get; private set; } = false;

    public void RiverStartAnimation(float starting_duration, float camera_shake_multiplier = 1) {
        if (!river_active) {
            StartCoroutine(StartRiverEventCour(starting_duration, camera_shake_multiplier));
            river_active = true;
        }
    }

    private void OnDestroy() {
        river_flowing_material.SetFloat("_Active", 0);
        CameraShakeNoise.FrequencyGain = 0;
        CameraShakeNoise.AmplitudeGain = 0;
        m_river_flowing_volume.weight = 0;
    }

    private bool can_run_river_start_cour = true; // used to force only one active coroutine for running the river starting animation.
    private IEnumerator StartRiverEventCour(float starting_duration, float camera_shake_multiplier) {

        if (can_run_river_start_cour) {

            can_run_river_start_cour = false;
            yield return StartCoroutine(RiverEventVisualsAnimationCour(current_camera, river_flowing_material, river_start_shader_active_curve, warning_shake_intencity_curve, camera_shake_multiplier, starting_duration));
            can_run_river_end_cour = true;
        }
    }

    public void RiverEndAnimation(float ending_duration, float camera_shake_multiplier = 1) {

        if (river_active) { // allow the ending of the animation to be queued by ignoring if an animation is currently playing
            StartCoroutine(EndRiverEventCour(ending_duration, camera_shake_multiplier));
            river_active = false;
        }
    }

    private bool can_run_river_end_cour = false; // used to force only one active coroutine for running the river ending animation.
    private IEnumerator EndRiverEventCour(float ending_duration, float camera_shake_multiplier) {

        if (can_run_river_end_cour) {
            can_run_river_end_cour = false;
            yield return StartCoroutine(RiverEventVisualsAnimationCour(current_camera, river_flowing_material, river_end_shader_active_curve, ending_shake_intencity_curve, camera_shake_multiplier, ending_duration));
            can_run_river_start_cour = true;
        }
    }
}