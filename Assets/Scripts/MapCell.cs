using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class MapCell : MonoBehaviour
{
    public Character Character;
    public Map.CellType Type;
    public GameObject Obstacle;

    private Vector2Int _position;

    public Vector2Int Position => _position;

    public void Initialize(Vector2Int position, Map.CellType type, GameObject obstacle)
    {
        _position = position;
        Type = type;
        Obstacle = obstacle;
    }

    public void Focus()
    {
        Material mat = GetComponent<MeshRenderer>().material;
        EnableHighlight(mat);

        MainManager.FocusCell(this);
    }

    public void LeaveFocus()
    {
        Material mat = GetComponent<MeshRenderer>().material;
        DisableHighlight(mat);

        UIManager.LeaveCellFocus(this);
    }

    private void EnableHighlight(Material mat)
    {
        mat?.EnableKeyword("_EMISSION");
        mat?.SetColor("_EmissionColor", Color.grey);
    }
    private void DisableHighlight(Material mat)
    {
        mat?.DisableKeyword("_EMISSION");
    }
}
