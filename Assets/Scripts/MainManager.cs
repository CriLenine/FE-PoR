﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainManager : MonoBehaviour
{
    private static MainManager _instance;

    public static GameManager GameManager;
    public static EditManager EditManager;
    public static SaveData SaveData;

    private Vector2Int _cursorMoveDirection;
    private MapCell _currentCell;
    private float _cursorMoveDelay = .1f;
    private float _cursorMoveTimer = 0f;
    private bool _cursorCanMove = true;

    public static Color Blue => new Color(171, 0, 0);
    public static Color Red => new Color(0, 96, 231);

    public static Map Map
    {
        get
        {
            if (GameManager != null) return GameManager.Map;
            return EditManager.Map;
        }
        private set { }
    }

    private void Awake()
    {
        _instance ??= this;

        GameManager = GetComponent<GameManager>();
        EditManager = GetComponent<EditManager>();
    }

    public static void MapInitialized()
    {
        _instance._currentCell = Map.GetCell(Vector2Int.zero);
        Map.GetCell(Vector2Int.zero).Focus();
    }

    private void Update()
    {

        if (!_cursorCanMove)
        {
            if ((_cursorMoveTimer += Time.deltaTime) > _cursorMoveDelay)
            {
                _cursorCanMove = true;
                _cursorMoveTimer = 0f;
            }
        }

        _cursorMoveDirection = Vector2Int.zero;
        if (Input.GetKey(KeyCode.Z))
        {
            _cursorMoveDirection += Vector2Int.up;
        }
        if (Input.GetKey(KeyCode.Q))
        {
            _cursorMoveDirection += Vector2Int.left;
        }
        if (Input.GetKey(KeyCode.S))
        {
            _cursorMoveDirection += Vector2Int.down;
        }
        if (Input.GetKey(KeyCode.D))
        {
            _cursorMoveDirection += Vector2Int.right;
        }

        if (_cursorMoveDirection != Vector2Int.zero)
        {
            if (_cursorCanMove)
                MoveCursor(_cursorMoveDirection);
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            if (GameManager != null && _currentCell.Obstacle == null)
                GameManager.Validate(_currentCell);
            else if (EditManager != null)
                EditManager.Validate(_currentCell);
            else
                Debug.LogError("Manager missing !");
        }
    }

    private void MoveCursor(Vector2Int cursorMoveDirection)
    {
        if (Map.IsValidMove(_currentCell.Position, cursorMoveDirection, out Vector2Int newCoords))
        {
            _cursorCanMove = false;
            _currentCell.LeaveFocus();
            MoveCursorVisual(_currentCell.Position, newCoords);
            _currentCell = Map.GetCell(newCoords);
            _currentCell.Focus();
        }
    }
    private void MoveCursorVisual(Vector2Int startPosition, Vector2Int newPosition)
    {
        //visualCursor
    }

    public static void FocusCell(MapCell cell)
    {
        Character character = cell.Character;
        CameraController.Instance.CameraFocus(Map.GetCell(cell.Position).transform.position);
        if (EditManager)
        {
            EditManager.FocusCell(cell);
        }
        if (GameManager)
        {
            if (character != null)
            {
                UIManager.FocusCell(cell, cell.Type == Map.CellType.CanAttack && cell.Character.Data.Team != GameManager.CurrentData.Team);
            }
        }
    }
}