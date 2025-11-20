using UnityEngine;

[CreateAssetMenu(fileName = "CommanderData", menuName = "ScriptableObjects/CommanderDataSO")]
public class CommanderDataSO : CreatureDataSO
{
    [Header("Info")]
    public string commanderName;
    [TextArea] public string commanderDescription;
    public string commanderPrefabName;

    public string FullPrefabPath => Constants.COMMANDER_ROOT_PATH + commanderPrefabName;

    [Header("Ranged Attack")]
    [Tooltip("원거리 지휘관만 해당")]
    public string projectilePrefabName;
    public string FullProjectilePrefabPath => Constants.PROJECTILE_ROOT_PATH + projectilePrefabName;
}
