using System;
using UnityEngine;
using System.Linq;

[Serializable, CreateAssetMenu(fileName = "CharacterData", menuName = "ScriptableObjects/Character", order = 1)]
public class CharacterData : ScriptableObject
{
    public enum CharacterClass
    {
        Fighter,
        Knight,
        Mage,
        Ranger
    }

    public enum CharacterTeam
    {
        Ally,
        Enemy
    }

    #region Stats

    [SerializeField] private int _moveRange;
    [SerializeField] private int _maxHealth;
    [SerializeField] private int _speed;
    [SerializeField] private int _skill;
    [SerializeField] private int _strength;
    [SerializeField] private int _defence;
    [SerializeField] private int _magic;
    [SerializeField] private int _resistance;
    [SerializeField] private int _luck;
    public int MoveRange => _moveRange;
    public int MaxHealth => _maxHealth;
    public int Speed => _speed;
    public int Skill => _skill;
    public int Strength => _strength;
    public int Defence => _defence;
    public int Magic => _magic;
    public int Resistance => _resistance;
    public int Luck => _luck;
    #endregion

    [SerializeField] private CharacterClass _class;
    [SerializeField] private CharacterTeam _team;
    [SerializeField] private string _name;


    [SerializeField] private Vector2Int _startPosition;

    public CharacterClass Class => _class;
    public CharacterTeam Team => _team;
    public string Name => _name;

    public Vector2Int StartPosition => _startPosition;

    private Weapon[] _weapons = new Weapon[4];

    public Weapon Weapon => _weapons[0];

    public int AttackSpeed => (int)(_speed - Mathf.Max(0f, _weapons[0].Weight - _strength));
    public int Avoid => AttackSpeed * 2 + Luck;
    private int _criticalRate => _weapons[0].Critical + Skill / 2;
    /// <summary>
    /// Compute the damage, accuracy and critical rate corresponding to
    /// a fight against <paramref name="enemy"/> with the current equipped weapon
    /// </summary>
    /// <param name="enemy"></param>
    /// <returns>(damage, accuracy, criticalRate)</returns>
    public (int, int, int) FightInfos(Character enemy)
    {
        int weaponTriangleBonus = Weapon.TriangleBonus(_weapons[0], enemy.Data.Weapon);
        bool physical = _weapons[0].DamageType == Weapon.DamageNature.Physical;
        int damage = (physical ? Strength : Magic)
                    + (_weapons[0].Might + weaponTriangleBonus)
                    * (_weapons[0].EffectiveAgainst.Contains(enemy.Data.Class) ? 2 : 1)
                    - (physical ? enemy.Data.Defence : enemy.Data.Resistance);

        int accuracy = _weapons[0].Hit + Skill * 2 + Luck + weaponTriangleBonus * 10;
        int battleAccuracy = accuracy - enemy.Data.Avoid;

        int battleCriticalRate = _criticalRate - enemy.Data.Luck;

        return (damage, Mathf.Clamp(battleAccuracy, 0, 100), Mathf.Clamp(battleCriticalRate, 0, 100));
    }
    /// <returns>Whether this character wears a weapon capable of striking at a distance of <paramref name="range"/></returns>
    public bool CanAttackAtRange(int range)
    {
        return _weapons.Any(weapon => weapon != null && weapon.AttackRanges.Contains(range));
    }
    public void DeleteWeapon()
    {
        for (int i = 0; i < _weapons.Length - 1; i++)
            _weapons[i] = _weapons[i + 1];
    }
}