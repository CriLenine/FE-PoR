using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable, CreateAssetMenu(fileName = "WeaponData", menuName = "ScriptableObjects/Weapon", order = 1)]
public class Weapon : ScriptableObject
{
    public enum WeaponRank
    {
        E,
        D,
        C,
        B,
        A,
        S
    }
    public enum WeaponType
    {
        Sword,
        Axe,
        Lance,
        Bow,
        Knife,
        Wind,
        Fire,
        Thunder,
        Light,
        Staff
    }
    [HideInInspector] public int currentDurability;

    [SerializeField] private string _name;
    [SerializeField] private WeaponType _type;
    [SerializeField] private WeaponRank _requiredRank;
    [SerializeField] private int _damage;
    [SerializeField] private int _speed;
    [SerializeField] private int _maxDurability;
    [SerializeField] private List<int> _attackRanges;

    public string Name => _name;
    public WeaponType Type => _type;
    public WeaponRank RequiredRank => _requiredRank;
    public int Damage => _damage;
    public int Speed => _speed;
    public int MaxDurability => _maxDurability;
    public List<int> AttackRanges => _attackRanges;
}
