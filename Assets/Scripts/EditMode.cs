using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EditMode : MonoBehaviour
{
    public static EditMode instance;

    [SerializeField] private Map _map;
    private List<(CharacterData, Vector2Int)> _characters = new List<(CharacterData, Vector2Int)>();
    private List<Vector2Int> _treesPositions = new List<Vector2Int>();

    private const string _TREENAME = "Tree";
    private GameObject _currentInstantiatedPrefab;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        Load();
        _map.Initialize(_treesPositions);
        _map.InitCharacters(_characters, 1f);
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hitInfo))
            {
                Character character = hitInfo.transform.GetComponent<Character>();
                if (character)
                    Debug.Log("fdp");
                else if (hitInfo.transform.CompareTag(_TREENAME))
                {
                    Debug.Log("fdp");
                }
            }
        }
    }

    public void SelectCell(MapCell cell)
    {
        if (_currentInstantiatedPrefab)
        {
            _currentInstantiatedPrefab.transform.position = cell.transform.position;
        }
    }

    public void CreatePrefab(string prefabName)
    {
        Quaternion quaternion = prefabName == _TREENAME ? Quaternion.Euler(0f, Random.value * 360f, 0f) : Quaternion.identity;
        _currentInstantiatedPrefab = Instantiate(PrefabManager.Prefabs[prefabName], _map.GetCell(Vector2Int.zero).transform.position, quaternion);
    }

    public void Load()
    {

    }

    public void Save(List<(CharacterData, Vector2Int)> characters, List<Vector2Int> treesPositions)
    {

    }
}
