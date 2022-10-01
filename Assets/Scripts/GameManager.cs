using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class GameManager : MonoBehaviour
{
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
    [SerializeField] private float _characterSpeed = 5f;
    [SerializeField] private Animator _anim;

    private GameStep _step;
    private Character _currentCharacter;
    private MapCell _lastSelectedCell;
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
    private bool _characterIsMoving = false;

    public GameStep Step => _step;
    public bool DisplayActionMenu => _displayActionMenu;
    public Vector3 ActionMenuAnchor => _actionMenuAnchor;
    public bool HasATarget => _hasATarget;
    public CharacterData CurrentData => _currentCharacter.data;
    public Map Map => _map;

    #region START/UPDATE
    private void Start()
    {
        _astarMain = new AStar.Main();

        _map.Initialize();

        _allCharacters = _map.InitCharacters(_startCharacters);

        foreach (Character character in _allCharacters)
        {
            if (character.data.Team == _currentTeam)
                _charactersToPlay.Add(character);
        }

        UIManager.PlayingManager.phaseText.text = "Ally phase";
        UIManager.PlayingManager.phaseText.color = MainManager.Red;
    }


    private void Update()
    {
        if (_whereToMove.Count > 0) //when we command a character to move
        {
            if (!_characterIsMoving)
                _characterIsMoving = true;
            _currentCharacter.transform.LookAt(_whereToMove[0]);
            _currentCharacter.transform.position = Vector3.MoveTowards(_currentCharacter.transform.position, _whereToMove[0], _characterSpeed * Time.deltaTime);
            if (_currentCharacter.transform.position == _whereToMove[0])
                _whereToMove.RemoveAt(0);
            if (_whereToMove.Count == 0)
            {
                _characterIsMoving = false;
                CharacterHasMoved();
            }
        }
    }

    #endregion

    public void Validate()
    {
        if (_characterIsMoving)
            return;

        if (_step == GameStep.None)
        {
            if (MainManager.CurrentCell.Character != null)
            {
                if (_charactersToPlay.Contains(MainManager.CurrentCell.Character))
                    SelectCharacter(MainManager.CurrentCell);
            }
        }
        else if (_step == GameStep.ChoosingDestination && MainManager.CurrentCell.Type == Map.CellType.CanMoveTo)
            MoveCharacter(MainManager.CurrentCell);

        else if (_step == GameStep.ChoosingTarget && MainManager.CurrentCell.Type == Map.CellType.CanAttack)
        {
            _hasATarget = false;
            ResetCells();
            Fight(_lastSelectedCell, MainManager.CurrentCell);
            EndCharacterTurn();
        }
    }

    #region MOVE METHODS
    private void SelectCharacter(MapCell cell)
    {
        _currentCharacter = cell.Character;
        _lastSelectedCell = cell;

        _step = GameStep.ChoosingDestination;
        _visitedCoords.Clear();
        ShowReachableCells(cell.Position, cell.Character.data.Range);

        //add the obstacles
        for (int x = 0; x < _map.Width; x++)
        {
            for (int y = 0; y < _map.Height; y++)
            {
                Vector2Int vect = new Vector2Int(x, y);
                MapCell c = _map.GetCell(vect);
                if (c != null)
                {
                    if (!AreSameMaterial(c.GetComponent<MeshRenderer>().material, _map.CellMaterials[Map.CellType.CanMoveTo]))
                        pathfindingGrid.AddObstacle(vect);
                }
            }
        }
    }
    private void ShowReachableCells(Vector2Int currentPos, int currentRange)
    {
        if (!_visitedCoords.ContainsKey(currentRange))
            _visitedCoords.Add(currentRange, new List<Vector2Int>());

        if (!_astarMain.isValid(_map.Height, _map.Height, currentPos) || _visitedCoords[currentRange].Contains(currentPos) || currentRange < 1)
            return;

        MapCell cell = _map.GetCell(currentPos);
        if (cell == null)
            return;
        if (cell.Character != null)
        {
            if (cell.Character.data.Team != _currentTeam)
                return;
        }
        _visitedCoords[currentRange].Add(currentPos);
        cell.GetComponent<MeshRenderer>().material = _map.CellMaterials[Map.CellType.CanMoveTo];
        cell.Type = (cell.Character == null || cell.Character == _currentCharacter) ? Map.CellType.CanMoveTo : Map.CellType.None;
        ShowReachableCells(currentPos + Vector2Int.left, currentRange - 1);
        ShowReachableCells(currentPos + Vector2Int.up, currentRange - 1);
        ShowReachableCells(currentPos + Vector2Int.right, currentRange - 1);
        ShowReachableCells(currentPos + Vector2Int.down, currentRange - 1);
    }

    private void MoveCharacter(MapCell cell)
    {
        if (cell == _lastSelectedCell)
            CharacterHasMoved();
        else
        {
            _lastSelectedCell.Character = null;
            cell.Character = _currentCharacter;

            // use Astar pathfinding
            List<Vector2Int> path = _astarMain.aStarSearch(pathfindingGrid.array, _currentCharacter.data.position, MainManager.CurrentCell.Position);
            for (int i = 0; i < path.Count; i++)
            {
                _whereToMove.Add(_map.GetCell(path[i]).transform.position + Vector3.up * (_map.CharactersAltitude - _map.Altitude));
            }

            //character will move in the update method because whereToMove is not empty
        }
    }

    private void CharacterHasMoved()
    {
        _characterIsMoving = false;
        _currentCharacter.data.position = MainManager.CurrentCell.Position;

        ResetCells();

        _actionMenuAnchor = _map.GetWorldPosition(MainManager.CurrentCell.Position);
        _displayActionMenu = true;
        _step = GameStep.ChoosingAction;
        _targetableCells.Clear();
        GetTargetableCells();
    }
    #endregion

    #region ATTACK METHODS
    public void GetTargetableCells()
    {
        for (int x = 0; x < _map.Width; x++)
        {
            for (int y = 0; y < _map.Height; y++)
            {
                Vector2Int vect = new Vector2Int(x, y);
                if (_currentCharacter.data.CanAttackAtRange(_astarMain.CalculateHValue(_currentCharacter.data.position, vect)))
                {
                    MapCell cell = _map.GetCell(vect);
                    if (cell != null)
                    {
                        _targetableCells.Add(cell);
                        if (cell.Character != null)
                        {
                            if (cell.Character.data.Team != _currentTeam)
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
            cell.GetComponent<MeshRenderer>().material = _map.CellMaterials[Map.CellType.CanAttack];
            if (cell.Character != null)
            {
                if (cell.Character.data.Team != _currentTeam)
                    cell.Type = Map.CellType.CanAttack;
            }
        }
    }

    private void Fight(MapCell attackingCell, MapCell defendingCell)
    {
        attackingCell.Character.transform.LookAt(defendingCell.Character.transform.position);
        defendingCell.Character.transform.LookAt(attackingCell.Character.transform.position);

        int distance = _astarMain.CalculateHValue(attackingCell.Position, defendingCell.Position);

        attackingCell.Character.Attack(defendingCell.Character);

        if (defendingCell.Character.data.currentHealth <= 0)
        {
            _allCharacters.Remove(defendingCell.Character);
            CharacterData.CharacterTeam t = defendingCell.Character.data.Team;
            Destroy(defendingCell.Character.gameObject);
            if (!_allCharacters.Exists(x => x.data.Team == t))
                EndGame();
        }

        else if (defendingCell.Character.data.weapons[0].AttackRanges.Contains(distance))
        {
            defendingCell.Character.Attack(attackingCell.Character);

            if (attackingCell.Character.data.currentHealth <= 0)
            {
                CharacterData.CharacterTeam t = attackingCell.Character.data.Team;
                _allCharacters.Remove(attackingCell.Character);
                Destroy(attackingCell.Character.gameObject);
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
            UIManager.PlayingManager.phaseText.text = $"{nextTeam} phase";
            UIManager.PlayingManager.phaseText.color =
                nextTeam == CharacterData.CharacterTeam.Enemy ? MainManager.Blue : MainManager.Red;
            _anim.SetTrigger("Growth");
        }
    }

    private void EndGame()
    {
        UIManager.PlayingManager.phaseText.text = $"The {_currentTeam} team won !";
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
        for (int x = 0; x < _map.Width; x++)
        {
            for (int y = 0; y < _map.Height; y++)
            {
                Vector2Int vect = new Vector2Int(x, y);
                MapCell cell = _map.GetCell(vect);
                if (cell != null)
                {
                    cell.GetComponent<MeshRenderer>().material = _map.CellMaterials[Map.CellType.None];
                    cell.Type = Map.CellType.None;
                }
            }
        }
    }

    private bool AreSameMaterial(Material mat1, Material mat2)
    {
        string name1 = mat1.name;
        string name2 = mat2.name;
        return name1 == name2 || name1 == name2 + " (Instance)" || name1 + " (Instance)" == name2;
    }
    #endregion
}
