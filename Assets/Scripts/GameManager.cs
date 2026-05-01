using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public static class GameManager
{
    public enum Layer : int {
        Default = 0,
        TransparentFX,
        Ignore_Raycast,
        Player,
        Water,
        UI,
        Ground,
        Damager,
        Respawn_Zone,
    }
    
    // --- globals --- //


    private static PlayerCharCon _player;

    public static Transform respawn_point;

    public static PlayerCharCon player
    {
        get
        {
            return _player;
        }
        set
        {
            if (respawn_point == null && value != null)
            { // set a start respawn point if none is set
                respawn_point = new GameObject("Start Respawn Point").transform;
                respawn_point.position = value.transform.position;
            }

            _player = value;
            return;
        }
    }

    private static System.Collections.Generic.
        List<RespawnZone> respawn_zones = new();

    public static void AddRespawnZone(RespawnZone item) {
        if (respawn_zones.Contains(item)) return;

        for (int i = 0; i < respawn_zones.Count; i++) { 
            if (respawn_zones[i].transform.position.x < item.transform.position.x) {
                respawn_zones.Insert(i, item);
                return;
            }
        }
        respawn_zones.Add(item); // Add to end
    }

    public static void ClearRespawnZoneList() {
        respawn_zones.Clear();
    }

    public static int GetRespawnZoneID(RespawnZone item) {
        return respawn_zones.IndexOf(item);
    }

    public static void SetRespawZoneIOf(RespawnZone item, int index) {
        respawn_zones.Remove(item);
        respawn_zones.Insert(index, item);
    }

    private static int highest_reached_respawn_zone = 0;
    public static void OnRespawnZoneHit(RespawnZone item) {
        int index_of_item = respawn_zones.IndexOf(item);
        if (index_of_item > highest_reached_respawn_zone) {
            highest_reached_respawn_zone = index_of_item;
            respawn_point = item.respawn_point;
        }
    }

    public static void CameraShake(float intencity, float duration) { 

    }

    private static IEnumerator CameraShakeCour () { 
    
    }

    public static void StartRiverEvent() { 
        
    }

    private static IEnumerator RiverEvent(float warning_duration, float duration) { 

    }

}