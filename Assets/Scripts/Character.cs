using System;
using TMPro;
using UnityEngine;

public class Character : MonoBehaviour
{
    public CharacterData Data;
    public Vector2Int Position;

    [SerializeField] private TextMeshProUGUI _dodgeText;
    [SerializeField] private float _destroySpeed = 1.5f;
    [SerializeField] private GameObject _onHitParticleSystemPrefab;

    public int CurrentHealth { get; private set; }

    private void Start()
    {
        CurrentHealth = Data.MaxHealth;
        Position = Data.StartPosition;
    }

    public void Attack(Character enemy)
    {
        (int damage, int battleAccuracy, int battleCriticalRate) = Data.FightInfos(enemy);
        if (UnityEngine.Random.value * 100 < battleAccuracy)
        {
            bool physical = Data.Weapon.DamageType == Weapon.DamageNature.Physical;

            bool critical = battleCriticalRate > UnityEngine.Random.value * 100;

            enemy.TakeDamage(damage, physical, critical);
            if (--Data.Weapon.CurrentDurability == 0)
            {
                Data.DeleteWeapon();
            }

            GameObject particleGo = Instantiate(_onHitParticleSystemPrefab, enemy.transform.position, Quaternion.identity);
            particleGo.GetComponent<ParticleSystem>().Play();
            Destroy(particleGo, 0.5f);
        }
        else
            ShowDodgeText(enemy.transform.position);

        void ShowDodgeText(Vector3 position)
        {
            TextMeshProUGUI textGo = Instantiate(_dodgeText, Camera.main.WorldToScreenPoint(position + Vector3.up * 2), Quaternion.identity);
            textGo.transform.SetParent(FindObjectOfType<Canvas>().transform);
            Destroy(textGo, _destroySpeed);
        }
    }

    public void TakeDamage(int damage, bool physical, bool critical)
    {
        CurrentHealth -= damage * (critical ? 3 : 1);
    }
}
