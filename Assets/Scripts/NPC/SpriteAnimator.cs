using System.Collections;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class SpriteAnimator : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] Sprite[] sprites;
    [SerializeField] [Range(1,60)] int FPS = 24;

    [SerializeField]
    [Unity.Cinemachine.MinMaxRangeSlider(0, 2)]
    Vector2 random_speed_range = Vector2.one;

    [SerializeField] bool random_start_frame = true;
    private int start_frame = 0;

    private float speed_multiplier;
    private float frame_duration; // how long a frame lasts in seconds

    [SerializeField] private SpriteRenderer m_renderer;

    void OnValidate() {

        // Set these local variables when values are changed in the inspector
        // so they do not have to be set for all users of this script on start.
        speed_multiplier = Random.Range(random_speed_range.x, random_speed_range.y);
        frame_duration = 1 / (speed_multiplier * FPS);

        if (m_renderer == null) TryGetComponent(out m_renderer);
    }

    void Awake() {
        speed_multiplier = Random.Range(random_speed_range.x, random_speed_range.y);
        frame_duration = 1 / (speed_multiplier * FPS);

        if (m_renderer == null) TryGetComponent(out m_renderer);
    }

    void Play(int start_frame = 0) {
        StartCoroutine(AnimationLoopCour(start_frame));
    }

    bool animate = false;
    IEnumerator AnimationLoopCour(int start_frame) {

        int current_frame;

        if (start_frame >= sprites.Length) {
            current_frame = 0;
            Debug.LogError($"Start frame (${start_frame}) is larger than number of frames (${sprites.Length}).", this);
        }
        else {
            current_frame = start_frame;
        }

        while (animate) {
            m_renderer.sprite = sprites[current_frame];
            yield return new WaitForSeconds(frame_duration);

            current_frame = (current_frame + 1) % sprites.Length; // loop through index of the sprite array
        }
    }

    void OnBecameVisible() {
        animate = true;
        float time_offset = 0;

        if(random_start_frame) {
            Random.state = new Random.State();
            time_offset = Random.Range(0, sprites.Length * frame_duration);
        }

        int elapsed_frames = Mathf.FloorToInt(((Time.fixedTime + time_offset) * FPS) + start_frame);
        int number_of_loops = Mathf.FloorToInt(elapsed_frames / sprites.Length);

        int current_frame = elapsed_frames - (sprites.Length * number_of_loops);

        Play(current_frame);
    }

    void OnBecameInvisible() {
        animate = false;
    }
}