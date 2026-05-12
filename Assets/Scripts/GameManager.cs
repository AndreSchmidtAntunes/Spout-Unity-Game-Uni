using System.Diagnostics;

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
    public static PlayerCharCon player
    {
        get {
            return _player;
        }
        set {
            _player = value;
            return;
        }
    }
}