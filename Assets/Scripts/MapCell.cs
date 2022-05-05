using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class MapCell : MonoBehaviour, IPointerClickHandler
{
    public Vector2Int position;
    public Character character;
    public Action<MapCell> onClick;
    public Map.CellType type;

    public void OnPointerClick(PointerEventData eventData)
    {
        onClick?.Invoke(this);
    }

    private void OnMouseEnter()
    {
        if (MainGame.instance.Step == MainGame.GameStep.None
            || MainGame.instance.Step == MainGame.GameStep.ChoosingDestination && type == Map.CellType.CanMoveTo
            || MainGame.instance.Step == MainGame.GameStep.ChoosingTarget && type == Map.CellType.CanAttack)
        {
            Material mat = GetComponent<MeshRenderer>().material;
            EnableHighlight(mat);
        }
    }
    private void OnMouseOver()
    {
        if (MainGame.instance.Step == MainGame.GameStep.ChoosingTarget && type == Map.CellType.CanAttack)
            UIManager.instance.DisplayAttackInfos(character.data);
        else
            UIManager.instance.HideAttackInfos();

        if (character != null)
            UIManager.instance.DisplayCharacterInfos(character.data);
    }
    private void OnMouseExit()
    {
        Material mat = GetComponent<MeshRenderer>().material;
        DisableHighlight(mat);
        if (character != null)
        {
            UIManager.instance.HideInfos();
            UIManager.instance.HideAttackInfos();
        }
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
