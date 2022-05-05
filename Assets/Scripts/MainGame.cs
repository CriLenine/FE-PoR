using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainGame : MonoBehaviour
{
    public static MainGame instance;

    public AStar.Grid pathfindingGrid;

    public enum GameStep
    {
        None,
        ChoosingAction,
        ChoosingDestination,
        ChoosingTarget
    }

    [SerializeField] private Map _map;
    [SerializeField] private List<CharacterData> _startCharacters;
    [SerializeField] private CharacterPrefab[] _prefabCharacters;
    [SerializeField] private float _charactersAltitude = 1f;
    [SerializeField] private float _characterSpeed = 5f;
    [SerializeField] private Animator _anim;
    [SerializeField] private List<Weapon> _weapons = new List<Weapon>();
    private GameStep _step;
    private Character _currentCharacter;
    private MapCell _currentCell;
    private AStar.Main _astarMain;
    private bool _displayActionMenu;
    private Vector3 _actionMenuAnchor;
    private CharacterData.CharacterTeam _currentTeam = CharacterData.CharacterTeam.Ally;
    private Dictionary<int, List<Vector2Int>> _visitedCoords = new Dictionary<int, List<Vector2Int>>();
    private List<Vector3> _whereToMove = new List<Vector3>();
    private List<MapCell> _targetableCells = new List<MapCell>();
    private bool _hasATarget = false;
    private List<Character> _allCharacters = new List<Character>();
    private List<Character> _charactersToPlay = new List<Character>();
    private bool _isMoving = false;

    public GameStep Step => _step;
    public bool DisplayActionMenu => _displayActionMenu;
    public Vector3 ActionMenuAnchor => _actionMenuAnchor;
    public bool HasATarget => _hasATarget;
    public CharacterData CurrentData => _currentCharacter.data;

    #region AWAKE/START/UPDATE

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }
    private void Start()
    {
        _astarMain = new AStar.Main();

        _map.Initialize();
        _map.OnMapCellClicked += OnCellClicked;

        InitCharacters();

        UIManager.instance.phaseText.text = "Ally phase";
        UIManager.instance.phaseText.color = new Color(0, 96, 231);

        void InitCharacters()
        {
            foreach (var characterData in _startCharacters)
            {
                GameObject characterGo = Instantiate(
                    GetPrefab(characterData),
                    _map.GetWorldPosition(characterData.position) + Vector3.up * _charactersAltitude,
                    Quaternion.identity
                    );

                Character character = characterGo.GetComponent<Character>();
                character.data = characterData;
                character.OnClick += OnCharacterClicked;

                if (characterData.Team == _currentTeam)
                    _charactersToPlay.Add(character);
                _allCharacters.Add(character);

                MapCell cell = _map.GetCell(characterData.position);
                cell.character = character;
            }
        }
    }


    private void Update()
    {
        if (_whereToMove.Count > 0) //when we command a character to move
        {
            if (!_isMoving)
                _isMoving = true;
            _currentCharacter.transform.LookAt(_whereToMove[0]);
            _currentCharacter.transform.position = Vector3.MoveTowards(_currentCharacter.transform.position, _whereToMove[0], _characterSpeed * Time.deltaTime);
            if (_currentCharacter.transform.position == _whereToMove[0])
                _whereToMove.RemoveAt(0);
            if (_whereToMove.Count == 0)
            {
                _isMoving = false;
                CharacterHasMoved();
            }
        }
    }

    #endregion

    private void OnCharacterClicked(Character Character)
    {
        OnCellClicked(_map.GetCell(Character.data.position));
    }

    private void OnCellClicked(MapCell cell)
    {
        if (_step == GameStep.None)
        {
            if (cell.character != null)
            {
                if (_charactersToPlay.Contains(cell.character))
                    SelectCharacter(cell);
            }
        }
        else if (_step == GameStep.ChoosingDestination && cell.type == Map.CellType.CanMoveTo && !_isMoving)
            MoveCharacter(cell);

        else if (_step == GameStep.ChoosingTarget && cell.type == Map.CellType.CanAttack)
        {
            _hasATarget = false;
            ResetCells();
            Fight(_currentCell, cell);
            EndCharacterTurn();
        }
    }

    #region MOVE METHODS
    private void SelectCharacter(MapCell cell)
    {
        _currentCharacter = cell.character;
        _currentCell = cell;

        _step = GameStep.ChoosingDestination;
        _visitedCoords.Clear();
        ShowReachableCells(cell.position, cell.character.data.Range);

        //add the obstacles
        for (int x = 0; x < _map.width; x++)
        {
            for (int y = 0; y < _map.height; y++)
            {
                Vector2Int vect = new Vector2Int(x, y);
                MapCell c = _map.GetCell(vect);
                if (c != null)
                {
                    if (!AreSameMaterial(c.GetComponent<MeshRenderer>().material, _map.cellMaterials[Map.CellType.CanMoveTo]))
                        pathfindingGrid.AddObstacle(vect);
                }
            }
        }
    }
    private void ShowReachableCells(Vector2Int currentPos, int currentRange)
    {
        if (!_visitedCoords.ContainsKey(currentRange))
            _visitedCoords.Add(currentRange, new List<Vector2Int>());

        if (!_astarMain.isValid(_map.height, _map.height, currentPos) || _visitedCoords[currentRange].Contains(currentPos) || currentRange < 1)
            return;

        MapCell cell = _map.GetCell(currentPos);
        if (cell == null)
            return;
        if (cell.character != null)
        {
            if (cell.character.data.Team != _currentTeam)
                return;
        }
        _visitedCoords[currentRange].Add(currentPos);
        cell.GetComponent<MeshRenderer>().material = _map.cellMaterials[Map.CellType.CanMoveTo];
        cell.type = (cell.character == null || cell.character == _currentCharacter) ? Map.CellType.CanMoveTo : Map.CellType.None;
        ShowReachableCells(currentPos + Vector2Int.left, currentRange - 1);
        ShowReachableCells(currentPos + Vector2Int.up, currentRange - 1);
        ShowReachableCells(currentPos + Vector2Int.right, currentRange - 1);
        ShowReachableCells(currentPos + Vector2Int.down, currentRange - 1);
    }

    private void MoveCharacter(MapCell cell)
    {
        if (cell == _currentCell)
            CharacterHasMoved();
        else
        {
            _currentCell.character = null;
            _currentCell = cell;
            _currentCell.character = _currentCharacter;

            // use Astar pathfinding
            List<Vector2Int> path = _astarMain.aStarSearch(pathfindingGrid.array, _currentCharacter.data.position, _currentCell.position);
            for (int i = 0; i < path.Count; i++)
            {
                _whereToMove.Add(_map.GetCell(path[i]).transform.position + Vector3.up * (_charactersAltitude - _map.Altitude));
            }

            //character will move in the update method because whereToMove is not empty
        }
    }

    private void CharacterHasMoved()
    {
        _isMoving = false;
        _currentCharacter.data.position = _currentCell.position;

        ResetCells();

        _actionMenuAnchor = _map.GetWorldPosition(_currentCell.position);
        _displayActionMenu = true;
        _step = GameStep.ChoosingAction;
        _targetableCells.Clear();
        GetTargetableCells();
    }
    #endregion

    #region ATTACK METHODS
    public void GetTargetableCells()
    {
        for (int x = 0; x < _map.width; x++)
        {
            for (int y = 0; y < _map.height; y++)
            {
                Vector2Int vect = new Vector2Int(x, y);
                if (_currentCharacter.data.CanAttackAtRange(_astarMain.CalculateHValue(_currentCell.position, vect)))
                {
                    MapCell cell = _map.GetCell(vect);
                    if (cell != null)
                    {
                        _targetableCells.Add(cell);
                        if (cell.character != null)
                        {
                            if (cell.character.data.Team != _currentTeam)
                                _hasATarget = true;
                        }
                    }
                }
            }
        }
    }

    public void ShowTargetableCells()
    {
        _displayActionMenu = false;
        _step = GameStep.ChoosingTarget;
        foreach (MapCell cell in _targetableCells)
        {
            cell.GetComponent<MeshRenderer>().material = _map.cellMaterials[Map.CellType.CanAttack];
            if (cell.character != null)
            {
                if (cell.character.data.Team != _currentTeam)
                    cell.type = Map.CellType.CanAttack;
            }
        }
    }

    private void Fight(MapCell attackingCell, MapCell defendingCell)
    {
        attackingCell.character.transform.LookAt(defendingCell.character.transform.position);
        defendingCell.character.transform.LookAt(attackingCell.character.transform.position);

        int distance = _astarMain.CalculateHValue(attackingCell.position, defendingCell.position);

        attackingCell.character.Attack(defendingCell.character);

        if (defendingCell.character.data.currentHealth <= 0)
        {
            _allCharacters.Remove(defendingCell.character);
            CharacterData.CharacterTeam t = defendingCell.character.data.Team;
            Destroy(defendingCell.character.gameObject);
            if (!_allCharacters.Exists(x => x.data.Team == t))
                EndGame();
        }

        else if (defendingCell.character.data.CanAttackAtRange(distance))
        {
            defendingCell.character.Attack(attackingCell.character);

            if (attackingCell.character.data.currentHealth <= 0)
            {
                CharacterData.CharacterTeam t = attackingCell.character.data.Team;
                _allCharacters.Remove(attackingCell.character);
                Destroy(attackingCell.character.gameObject);
                if (!_allCharacters.Exists(x => x.data.Team == t))
                    EndGame();
            }
        }
    }
    #endregion

    #region ENDING METHODS
    public void EndCharacterTurn()
    {
        _displayActionMenu = false;
        _charactersToPlay.Remove(_currentCharacter);
        _map.ResetPathfindingGrid();
        if (_charactersToPlay.Count < 1)
            EndTurn();
        _step = GameStep.None;
    }

    private void EndTurn()
    {
        CharacterData.CharacterTeam nextTeam;

        if (_currentTeam == CharacterData.CharacterTeam.Ally)
            nextTeam = CharacterData.CharacterTeam.Enemy;

        else //if (_currentTeam == CharacterData.Team.Enemy)
            nextTeam = CharacterData.CharacterTeam.Ally;

        if (!_allCharacters.Exists(x => x.data.Team == nextTeam))
            EndGame();
        else
        {
            _currentTeam = nextTeam;
            foreach (Character character in _allCharacters)
            {
                if (character.data.Team == nextTeam)
                    _charactersToPlay.Add(character);
            }
            UIManager.instance.phaseText.text = $"{nextTeam} phase";
            UIManager.instance.phaseText.color =
                nextTeam == CharacterData.CharacterTeam.Enemy ? new Color(171, 0, 0) : new Color(0, 96, 231);
            _anim.SetTrigger("Growth");
        }
    }

    private void EndGame()
    {
        UIManager.instance.phaseText.text = $"The {_currentTeam} team won !";
        Invoke("ReturnToMenu", 3);
    }

    private void ReturnToMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
    #endregion

    #region UTILITY METHODS
    private void ResetCells()
    {
        for (int x = 0; x < _map.width; x++)
        {
            for (int y = 0; y < _map.height; y++)
            {
                Vector2Int vect = new Vector2Int(x, y);
                MapCell cell = _map.GetCell(vect);
                if (cell != null)
                {
                    cell.GetComponent<MeshRenderer>().material = _map.cellMaterials[Map.CellType.None];
                    cell.type = Map.CellType.None;
                }
            }
        }
    }

    private GameObject GetPrefab(CharacterData data)
    {
        foreach (var prefabCharacter in _prefabCharacters)
        {
            if (data.Name == prefabCharacter.name)
                return prefabCharacter.Prefab;
        }

        throw new System.Exception($"Can't find prefab of {data.Name}");
    }

    private bool AreSameMaterial(Material mat1, Material mat2)
    {
        string name1 = mat1.name;
        string name2 = mat2.name;
        return name1 == name2 || name1 == name2 + " (Instance)" || name1 + " (Instance)" == name2;
    }
    #endregion
}
