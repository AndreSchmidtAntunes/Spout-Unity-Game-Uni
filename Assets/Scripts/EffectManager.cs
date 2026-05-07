using System.Collections;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Splines.Interpolators;

public class EffectManager : MonoBehaviour
{
    public static EffectManager instance = null;

    [Header("River Event")]

    [SerializeField] AnimationCurve warning_shake_intencity_curve = new(new Keyframe[] { new Keyframe(0, 0), new Keyframe(1, 1) });
    [SerializeField] AnimationCurve flowing_shake_intencity_curve = new(new Keyframe[] { new Keyframe(1, 0), new Keyframe(1, 0) });

    [SerializeField] AnimationCurve river_shader_active_curve = new(new Keyframe[] { new Keyframe(0, 0), new Keyframe(0.5f, 1), new Keyframe(1, 0) });

    [SerializeField] Material river_flowing_material;

    [SerializeField] float warning_duration = 1.0f;
    [SerializeField] float flow_duration = 1.0f;
    [SerializeField] float river_shake_intensity = 1.0f;
    [SerializeField] CinemachineBasicMultiChannelPerlin CameraShakeNoise;

    private void Awake() {
        if (instance == null) {
            instance = this;
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

    static bool river_event_can_activate = true;

    private IEnumerator RiverEventVisualsCour(float warning_duration, float flow_duration, float intensity_mult, Material river_flowing_mat) {

        StartCoroutine(RiverShaderActiveAnimatorCour(river_flowing_mat, river_shader_active_curve, warning_duration + flow_duration));
        StartCoroutine(CameraShakeCour(current_camera, warning_shake_intencity_curve, intensity_mult, warning_duration));

        yield return new WaitForSeconds(warning_duration); // wait for when the river should appear

        StartCoroutine(CameraShakeCour(current_camera, flowing_shake_intencity_curve, intensity_mult, flow_duration));


        yield return new WaitForSeconds(flow_duration); // wait for when the river should appear

        yield return new WaitForSeconds(0.5f); // amount of downtime for the visuals 
        river_event_can_activate = true;
    }

    public void StartRiverEventVisuals() {
        if (river_event_can_activate) {
            river_event_can_activate = false;
            StartCoroutine(RiverEventVisualsCour(warning_duration, flow_duration, river_shake_intensity, river_flowing_material));
        }
    }

    private IEnumerator RiverEventTimerCour() {
        while (true) {
            StartRiverEventVisuals();
            yield return new WaitForSeconds(10);
        }
    }
    private void Start()  {
        current_camera = Camera.main;
        StartCoroutine(RiverEventTimerCour());
    }
    private void FixedUpdate()
    {
        //Debug.Log(river_event_elapsed);
    }
}