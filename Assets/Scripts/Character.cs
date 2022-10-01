using System;
using TMPro;
using UnityEngine;

public class Character : MonoBehaviour
{
    public CharacterData data;
    [SerializeField] private TextMeshProUGUI _dodgeText;
    [SerializeField] private float _destroySpeed = 1.5f;
    [SerializeField] private GameObject _onHitParticleSystemPrefab;

    public void Attack(Character enemy)
    {
        if (UnityEngine.Random.value > (float)enemy.data.dodgeChance / 100)
        {
            enemy.data.currentHealth -= data.damage + data.weapons[0].Damage;
            if (--data.weapons[0].currentDurability == 0)
            {
                for (int i = 0; i < data.weapons.Length - 1; i++)
                    data.weapons[i] = data.weapons[i + 1];
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
}
