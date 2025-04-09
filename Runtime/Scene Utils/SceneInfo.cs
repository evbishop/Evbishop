using UnityEngine;

namespace Evbishop.Runtime.SceneUtils
{
    [CreateAssetMenu(fileName = "SceneInfo", menuName = "Scriptable Objects/Scene Info")]
    public class SceneInfo : ScriptableObject
    {
        [field: SerializeField] public bool IsKeptInBuildSettingsWhenBuilding { get; private set; }
        [field: SerializeField] public SceneDesignation SceneDesignation { get; private set; }
        [field: SerializeField] public SceneReference SceneReference { get; private set; }
    }
}