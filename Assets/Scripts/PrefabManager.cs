using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrefabManager : MonoBehaviour
{
    private static PrefabManager _instance;

    [System.Serializable]
    public struct PrefabInfo
    {
        public string name;
        public GameObject prefab;
    }

    public List<PrefabInfo> PrefabInfos;

    public static Dictionary<string, GameObject> Prefabs = new Dictionary<string, GameObject>();

    private void Start()
    {
        foreach(PrefabInfo prefabInfo in PrefabInfos)
        {
            Prefabs[prefabInfo.name] = prefabInfo.prefab;
        }
    }
}
