using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[Serializable]
public class CharacterData
{
    public enum CharacterClass
    {
        Fighter,
        Knight,
        Mage
    }

    public enum CharacterTeam
    {
        Ally,
        Enemy
    }

    [SerializeField] private GameObject _prefab;
    [SerializeField] private CharacterClass _class;
    [SerializeField] private CharacterTeam _team;
    [SerializeField] private string _name;
    [SerializeField] private int _range;
    [SerializeField] private int _maxHealth;
    [SerializeField] private Vector2Int _startPosition;

    public GameObject Prefab => _prefab;
    public CharacterClass Class => _class;
    public CharacterTeam Team => _team;
    public string Name => _name;
    public int Range => _range;
    public int MaxHealth => _maxHealth;
    public Vector2Int StartPosition => _startPosition;



    public int damage;
    public int dodgeChance;
    public Weapon[] weapons = new Weapon[4];

    [HideInInspector] public int currentHealth;
    [HideInInspector] public Vector2Int position;

    public bool CanAttackAtRange(int range)
    {
        return weapons.Any(weapon => weapon != null && weapon.AttackRanges.Contains(range));
    }
}