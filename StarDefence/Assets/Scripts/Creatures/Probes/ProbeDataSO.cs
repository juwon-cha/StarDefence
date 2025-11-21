using UnityEngine;

[CreateAssetMenu(fileName = "NewProbeData", menuName = "StarDefence/Probe Data")]
public class ProbeDataSO : ScriptableObject
{
    [Header("Probe Info")]
    public string probeName = "Probe";
    public int purchaseCost = 50;

    [Header("Movement")]
    public float moveSpeed = 5f;
    
    [Header("Mining")]
    public float miningDuration = 2f;
    public int mineralsPerTrip = 1;

    [Header("Prefab")]
    [Tooltip("The path to the prefab from the 'Resources' folder, without the extension.")]
    [SerializeField] private string probePrefabPath;

    public string FullPrefabPath => string.IsNullOrEmpty(probePrefabPath) ? null : Constants.PROBE_ROOT_PATH + probePrefabPath;
}
