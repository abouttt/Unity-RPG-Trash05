using UnityEngine;

public class GameManager : SingletonBehaviour<GameManager>
{
    public bool IsDefaultSpawn { get; set; }
    public bool IsPortalSpawn { get; set; }
    public Vector3 PortalSpawnPosition { get; set; }
    public float PortalSpawnYaw { get; set; }
}
