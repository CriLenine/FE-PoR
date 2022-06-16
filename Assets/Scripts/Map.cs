using System;
using System.Collections.Generic;
using UnityEngine;

public class Map : MonoBehaviour
{
    public int width = 25, height = 25;
    public enum CellType
    {
        None,
        CanMoveTo,
        CanAttack
    }
    public Dictionary<CellType, Material> cellMaterials = new Dictionary<CellType, Material>();

    [Serializable]
    private struct TaggedMaterial
    {
        public CellType type;
        public Material material;
    }
    [SerializeField] private TaggedMaterial[] _materials;
    [SerializeField] private float _altitude;
    [SerializeField] private List<Vector2Int> _condemnedCells = new List<Vector2Int>();
    [SerializeField] private float _cellSize = 1.5f;
    [SerializeField] private Transform _mapStart;
    [SerializeField] private GameObject _prefabCell;
    [SerializeField] private GameObject _prefabTree;
    private MapCell[,] _grid;

    public float Altitude => _altitude;

    private void Awake()
    {
        _grid = new MapCell[width, height];
        foreach (TaggedMaterial mat in _materials)
        {
            cellMaterials.Add(mat.type, mat.material);
        }
    }


    public void Initialize(List<Vector2Int> condemnedCells = null)
    {
        condemnedCells ??= _condemnedCells;
        for (int x = 0; x < height; x++)
        {
            for (int y = 0; y < width; y++)
            {
                Vector3 position = GetWorldPosition(x, y);
                position.y = _altitude;

                if (condemnedCells.Contains(new Vector2Int(x, y)))
                {
                    Instantiate(_prefabTree, position, Quaternion.Euler(0f, UnityEngine.Random.value * 360f, 0f));
                    continue;
                }

                GameObject cellGo = Instantiate(_prefabCell, position, Quaternion.identity);
                MapCell cell = cellGo.GetComponent<MapCell>();
                cell.position = new Vector2Int(x, y);
                cell.type = CellType.None;
                _grid[x, y] = cell;
                cellGo.transform.SetParent(transform);
            }
        }

        if(MainGame.instance)
            ResetPathfindingGrid();
    }

    public List<Character> InitCharacters(List<(CharacterData, Vector2Int)> _characters, float charactersAltitude)
    {
        List<Character> characters = new List<Character>();
        foreach ((CharacterData data, Vector2Int pos) in _characters)
        {
            GameObject characterGo = Instantiate(
                data.Prefab,
                GetWorldPosition(pos) + Vector3.up * charactersAltitude,
                Quaternion.identity
                );

            Character character = characterGo.GetComponent<Character>();
            character.data = data;

            characters.Add(character);

            MapCell cell = GetCell(data.position);
            cell.character = character;
        }
        return characters;
    }

    public void ResetPathfindingGrid()
    {
        MainGame.instance.pathfindingGrid = new AStar.Grid(width, height);
        MainGame.instance.pathfindingGrid.FreeAllSpace();
        foreach (Vector2Int pos in _condemnedCells)
        {
            MainGame.instance.pathfindingGrid.AddObstacle(pos);
        }
    }

    public MapCell GetCell(int xGrid, int yGrid)
    {
        return _grid[xGrid, yGrid];
    }
    public MapCell GetCell(Vector2Int posGrid)
    {
        return GetCell(posGrid.x, posGrid.y);
    }
    public MapCell GetCell(float x, float z)
    {
        int xGrid = Mathf.RoundToInt((x - _mapStart.position.x) / _cellSize);
        int yGrid = Mathf.RoundToInt((z - _mapStart.position.z) / _cellSize);
        return GetCell(xGrid, yGrid);
    }

    public Vector3 GetWorldPosition(int xGrid, int yGrid)
    {
        return new Vector3(xGrid * _cellSize + _mapStart.position.x,
                           0,
                           yGrid * _cellSize + _mapStart.position.z);
    }
    public Vector3 GetWorldPosition(Vector2Int posGrid)
    {
        return GetWorldPosition(posGrid.x, posGrid.y);
    }
}
