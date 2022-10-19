using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EditManager : MonoBehaviour
{
    public static EditManager Instance;

    [SerializeField] private Map _map;

    private const string _TREENAME = "Tree";
    private GameObject _currentInstantiatedPrefab;

    public Map Map => _map;
    public (bool display, CharacterData.CharacterTeam team, Vector2 anchor) PrefabMenu { get; private set; }

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        MainManager.SaveData.LoadJson();
    }

    public void Validate(MapCell cell)
    {
        if (_currentInstantiatedPrefab)
        {
            if (cell.Character)
                return;

            if (_currentInstantiatedPrefab.transform.CompareTag(_TREENAME))
            {
                MainManager.Map.CondemnedCells.Add(cell.Position);
                cell.Obstacle = _currentInstantiatedPrefab;
            }
            _currentInstantiatedPrefab = null;
        }
        else if(cell.Character || cell.Obstacle)
        {
            //Open menu : delete or move
        }
    }

    public void DisplayMenu(int team)
    {
        //0 = Ally, 1 = Enemy
        PrefabMenu = (true, (CharacterData.CharacterTeam) team, Input.mousePosition);
    }

    public void FocusCell(MapCell cell)
    {
        if (_currentInstantiatedPrefab)
        {
            _currentInstantiatedPrefab.transform.position = cell.transform.position;
        }
    }

    public void CreatePrefab(string prefabName)
    {
        if(_currentInstantiatedPrefab)
            Destroy(_currentInstantiatedPrefab);
        Quaternion quaternion = prefabName == _TREENAME ? Quaternion.Euler(0f, Random.value * 360f, 0f) : Quaternion.identity;
        _currentInstantiatedPrefab = Instantiate(PrefabManager.Prefabs[prefabName], _map.GetCell(Vector2Int.zero).transform.position, quaternion);
    }
}
