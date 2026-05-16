using UnityEngine;

[RequireComponent(typeof(Collider))]
public class RespawnZone : MonoBehaviour
{
    [SerializeField] private Transform m_respawn_point;

    public Vector2 respawn_point { get; private set; }

    public int checkpoint_id = -1; // Should be in order of when the player will touch each respawn zone;

    internal bool prioritise_id_over_position;

    private void Awake() {
        Physics.Raycast(m_respawn_point.position, Vector3.down, out RaycastHit hit);
        respawn_point = hit.point;
        respawn_point += new Vector2(0, GameManager.player.collision_radius);
    }

    public static RespawnZone current_respawn_zone { get; private set; } = null;

    private static bool ShouldSwitchRespawnZone(RespawnZone new_zone, RespawnZone old_zone)
    {
        if (new_zone.prioritise_id_over_position) {
            return new_zone.checkpoint_id > old_zone.checkpoint_id;
        }
        else {
            return new_zone.respawn_point.x < old_zone.respawn_point.x;
        }
    }

    private void OnTriggerEnter(Collider collision) {
        if (current_respawn_zone == null || ShouldSwitchRespawnZone(this, current_respawn_zone)) {
            current_respawn_zone = this;
            Debug.Log("New respawn zone: " + current_respawn_zone.name);
        }
    }
}