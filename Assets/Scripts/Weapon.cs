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
    public enum DamageNature
    {
        Physical,
        Magical
    }

    [HideInInspector] public int CurrentDurability;

    [SerializeField] private string _name;
    [SerializeField] private WeaponType _type;
    [SerializeField] private DamageNature _damageType;
    [SerializeField] private List<int> _attackRanges;
    [SerializeField] private WeaponRank _requiredRank;
    [SerializeField] private int _weight;
    [SerializeField] private int _might;
    [SerializeField] private int _hit;
    [SerializeField] private int _critical;
    [SerializeField] private List<CharacterData.CharacterClass> _effectiveAgainst;
    [SerializeField] private int _maxDurability;

    public string Name => _name;
    public WeaponType Type => _type;
    public DamageNature DamageType => _damageType;
    public List<int> AttackRanges => _attackRanges;
    public WeaponRank RequiredRank => _requiredRank;
    public int Weight => _weight;
    public int Might => _might;
    public int Hit => _hit;
    public int Critical => _critical;
    public List<CharacterData.CharacterClass> EffectiveAgainst => _effectiveAgainst;
    public int MaxDurability => _maxDurability;

    public static int TriangleBonus(Weapon allyWeapon, Weapon enemyWeapon)
    {
        if (allyWeapon < enemyWeapon)
            return -1;
        if (allyWeapon > enemyWeapon)
            return 1;
        return 0;
    }

    public static bool operator <(Weapon wa, Weapon wb)
    {
        WeaponType a = wa.Type;
        WeaponType b = wb.Type;

        if (a == WeaponType.Sword)
            return b == WeaponType.Lance;
        if (a == WeaponType.Axe)
            return b == WeaponType.Sword;
        if (a == WeaponType.Lance)
            return b == WeaponType.Axe;

        if (a == WeaponType.Wind)
            return b == WeaponType.Fire;
        if (a == WeaponType.Fire)
            return b == WeaponType.Thunder;
        if (a == WeaponType.Thunder)
            return b == WeaponType.Wind;

        return false;
    }
    public static bool operator >(Weapon wa, Weapon wb)
    {
        WeaponType a = wa.Type;
        WeaponType b = wb.Type;

        if (a == WeaponType.Sword)
            return b == WeaponType.Axe;
        if (a == WeaponType.Axe)
            return b == WeaponType.Lance;
        if (a == WeaponType.Lance)
            return b == WeaponType.Sword;

        if (a == WeaponType.Wind)
            return b == WeaponType.Thunder;
        if (a == WeaponType.Fire)
            return b == WeaponType.Wind;
        if (a == WeaponType.Thunder)
            return b == WeaponType.Fire;

        return false;
    }
}
