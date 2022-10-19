using System;
using System.Collections.Generic;
using UnityEngine;

public class Map : MonoBehaviour
{
    public int Width = 25, Height = 25;
    public enum CellType
    {
        None,
        CanMoveTo,
        CanAttack
    }
    public Dictionary<CellType, Material> CellMaterials = new Dictionary<CellType, Material>();

    [Serializable]
    private struct TaggedMaterial
    {
        public CellType type;
        public Material material;
    }
    [SerializeField] private TaggedMaterial[] _materials;
    [SerializeField] private float _altitude;
    [SerializeField] private float _charactersAltitude = 1f;
    [SerializeField] private List<Vector2Int> _condemnedCells = new List<Vector2Int>();
    [SerializeField] private float _cellSize = 1.5f;
    [SerializeField] private Transform _mapStart;
    [SerializeField] private GameObject _prefabCell;
    [SerializeField] private GameObject _prefabTree;
    private MapCell[,] _grid;

    public float Altitude => _altitude;
    public float CharactersAltitude => _charactersAltitude;
    public List<Vector2Int> CondemnedCells => _condemnedCells;

    private void Awake()
    {
        _grid = new MapCell[Width, Height];
        foreach (TaggedMaterial mat in _materials)
        {
            CellMaterials.Add(mat.type, mat.material);
        }
    }

    public List<Character> Initialize(List<CharacterData> _characters, List<Vector2Int> condemnedCells = null)
    {
        condemnedCells ??= _condemnedCells;
        for (int x = 0; x < Height; x++)
        {
            for (int y = 0; y < Width; y++)
            {
                Vector3 position = GetWorldPosition(x, y);
                position.y = _altitude;

                if (condemnedCells.Contains(new Vector2Int(x, y)))
                {
                    GameObject obstacle = Instantiate(_prefabTree, position, Quaternion.Euler(0f, UnityEngine.Random.value * 360f, 0f));
                    AddCell(x, y, obstacle);
                    continue;
                }

                AddCell(x, y, null);
            }
        }

        if(MainManager.GameManager)
            ResetPathfindingGrid();
        return InitCharacters();

        List<Character> InitCharacters()
        {
            List<Character> characters = new List<Character>();
            foreach (CharacterData data in _characters)
            {
                GameObject characterGo = Instantiate(
                    PrefabManager.Prefabs[data.Name],
                    GetWorldPosition(data.StartPosition) + Vector3.up * _charactersAltitude,
                    Quaternion.identity
                    );

                Character character = characterGo.GetComponent<Character>();

                characters.Add(character);

                MapCell cell = GetCell(data.StartPosition);
                cell.Character = character;
            }
            MainManager.MapInitialized();
            return characters;
        }
    }

    private void AddCell(int x, int y, GameObject obstacle)
    {
        Vector3 position = GetWorldPosition(x, y);
        position.y = _altitude;

        GameObject cellGo = Instantiate(_prefabCell, position, Quaternion.identity);
        MapCell cell = cellGo.GetComponent<MapCell>();
        cell.Initialize(new Vector2Int(x, y), CellType.None, obstacle);
        _grid[x, y] = cell;
        cellGo.transform.SetParent(transform);
    }

    public void ResetPathfindingGrid()
    {
        MainManager.GameManager.pathfindingGrid = new AStar.Grid(Width, Height);
        MainManager.GameManager.pathfindingGrid.FreeAllSpace();
        foreach (Vector2Int pos in _condemnedCells)
        {
            MainManager.GameManager.pathfindingGrid.AddObstacle(pos);
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

    public bool IsValidMove(Vector2Int coords, Vector2Int shift, out Vector2Int newCoords)
    {
        newCoords = coords + shift;
        bool valid = !(newCoords.x < 0 || newCoords.y < 0 || newCoords.x > Width || newCoords.y > Height);
        return valid;
    }
}
