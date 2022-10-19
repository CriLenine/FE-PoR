using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrefabManager : MonoBehaviour
{
    private static PrefabManager _instance;

    [System.Serializable]
    public struct PrefabInfo
    {
        public string Name;
        public GameObject Prefab;
    }

    public List<PrefabInfo> PrefabInfos;

    public static Dictionary<string, GameObject> Prefabs = new Dictionary<string, GameObject>();

    private void Awake()
    {
        _instance = this;

        foreach(PrefabInfo prefabInfo in PrefabInfos)
        {
            Prefabs[prefabInfo.Name] = prefabInfo.Prefab;
        }
    }
}
