using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class MapCell : MonoBehaviour
{
    public Vector2Int position;
    public Character character;
    public Map.CellType type;

    private void OnMouseEnter()
    {
        if (MainGame.instance == null 
            || MainGame.instance.Step == MainGame.GameStep.None
            || MainGame.instance.Step == MainGame.GameStep.ChoosingDestination && type == Map.CellType.CanMoveTo
            || MainGame.instance.Step == MainGame.GameStep.ChoosingTarget && type == Map.CellType.CanAttack)
        {
            Material mat = GetComponent<MeshRenderer>().material;
            EnableHighlight(mat);

            if (EditMode.instance)
                EditMode.instance.SelectCell(this);
        }
    }
    private void OnMouseOver()
    {
        if (MainGame.instance)
        {
            if (MainGame.instance.Step == MainGame.GameStep.ChoosingTarget && type == Map.CellType.CanAttack)
                UIManager.instance.DisplayAttackInfos(character.data);
            else
                UIManager.instance.HideAttackInfos();
        }

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
