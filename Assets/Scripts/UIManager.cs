using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManagerE EditManager;
    public static UIManagerP PlayingManager;

    private void Awake()
    {
        EditManager = GetComponent<UIManagerE>();
        PlayingManager = GetComponent<UIManagerP>();
    }

    public static void FocusCell(MapCell cell, bool showInfos)
    {
        if (PlayingManager)
        {
            if (showInfos)
                PlayingManager.DisplayAttackInfos(cell.Character.data);
            else
                PlayingManager.HideAttackInfos();

            if (cell.Character != null)
                PlayingManager.DisplayCharacterInfos(cell.Character.data);
        }
    }

    public static void LeaveCellFocus(MapCell cell)
    {
        if (PlayingManager)
        {
            if (cell.Character != null)
            {
                PlayingManager.HideInfos();
                PlayingManager.HideAttackInfos();
            }
        }
    }
}