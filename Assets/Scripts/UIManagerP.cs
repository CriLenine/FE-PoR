using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManagerP : MonoBehaviour
{
    public TextMeshProUGUI phaseText;
    public Image characterInfoPanel;
    public TextMeshProUGUI characterName;
    public TextMeshProUGUI characterClass;
    public TextMeshProUGUI characterHP;
    public Image attackerInfoPanel;
    public TextMeshProUGUI attackerName;
    public TextMeshProUGUI attackerClass;
    public TextMeshProUGUI attackerHP;
    public TextMeshProUGUI attackerDamage;
    public TextMeshProUGUI attackerAccuracy;
    public TextMeshProUGUI attackerCrit;
    public Image defenderInfoPanel;
    public TextMeshProUGUI defenderName;
    public TextMeshProUGUI defenderClass;
    public TextMeshProUGUI defenderHP;
    public TextMeshProUGUI defenderDamage;
    public TextMeshProUGUI defenderAccuracy;
    public TextMeshProUGUI defenderCrit;

    private void Start()
    {
        phaseText.text = "Ally phase";
        phaseText.color = MainManager.Red;
    }

    public void DisplayCharacterInfos(Character character)
    {
        CharacterData data = character.Data;
        characterInfoPanel.CrossFadeAlpha(0.8f, 0.5f, false);
        characterInfoPanel.color =
            character.Data.Team == CharacterData.CharacterTeam.Ally ? MainManager.Red : MainManager.Blue;
        characterName.text = data.Name;
        characterClass.text = data.Class.ToString();
        characterHP.text = $"HP : {character.CurrentHealth}/{data.MaxHealth}";
    }

    public void HideInfos()
    {
        characterInfoPanel.CrossFadeAlpha(0, 0.5f, false);
        characterName.text = "";
        characterClass.text = "";
        characterHP.text = "";
    }
    public void HideAttackInfos()
    {
        defenderInfoPanel.CrossFadeAlpha(0f, 0.5f, false);
        defenderName.text = "";
        defenderClass.text = "";
        defenderHP.text = "";
        defenderDamage.text = "";
        defenderAccuracy.text = "";
        defenderCrit.text = "";

        attackerInfoPanel.CrossFadeAlpha(0f, 0.5f, false);
        attackerName.text = "";
        attackerClass.text = "";
        attackerHP.text = "";
        attackerDamage.text = "";
        attackerAccuracy.text = "";
        attackerCrit.text = "";
    }

    public void DisplayAttackInfos(Character attacker, Character defender)
    {
        (int damage, int battleAccuracy, int battleCriticalRate) = defender.Data.FightInfos(attacker);
        int attackSpeedDelta = attacker.Data.AttackSpeed - defender.Data.AttackSpeed;

        defenderInfoPanel.CrossFadeAlpha(0.8f, 0.5f, false);
        defenderInfoPanel.color =
            defender.Data.Team == CharacterData.CharacterTeam.Ally ? MainManager.Red : MainManager.Blue;
        defenderName.text = defender.Data.Name;
        defenderClass.text = defender.Data.Class.ToString();
        defenderHP.text = $"HP : {defender.CurrentHealth}/{defender.Data.MaxHealth}";
        defenderDamage.text = $"Damage : {damage} {(attackSpeedDelta < -3 ? "x 2" : "")}";
        defenderAccuracy.text = $"Accuracy : {battleAccuracy}";
        defenderCrit.text = $"Crit : {battleCriticalRate}";

        (damage, battleAccuracy, battleCriticalRate) = attacker.Data.FightInfos(defender);

        attackerInfoPanel.CrossFadeAlpha(0.8f, 0.5f, false);
        attackerInfoPanel.color =
            attacker.Data.Team == CharacterData.CharacterTeam.Ally ? MainManager.Red : MainManager.Blue;
        attackerName.text = attacker.Data.Name;
        attackerClass.text = attacker.Data.Class.ToString();
        attackerHP.text = $"HP : {attacker.CurrentHealth}/{attacker.Data.MaxHealth}";
        attackerDamage.text = $"Damage : {damage}";
        attackerAccuracy.text = $"Accuracy : {battleAccuracy}";
        attackerCrit.text = $"Crit : {battleCriticalRate}";
    }

    void OnGUI()
    {
        if (MainManager.GameManager.DisplayActionMenu)
        {
            Vector2 position = Camera.main.WorldToScreenPoint(MainManager.GameManager.ActionMenuAnchor);
            position.y = Screen.height - position.y;
            GUILayout.BeginArea(new Rect(position.x, position.y, 160, 200), GUI.skin.box);

            GUILayout.Label("Commands");

            if (MainManager.GameManager.HasATarget)
            {
                if (GUILayout.Button("Attack"))
                {
                    MainManager.GameManager.ShowTargetableCells();
                }
            }

            if (GUILayout.Button("Wait"))
            {
                MainManager.GameManager.EndCharacterTurn();
            }

            GUILayout.EndArea();
        }

        if (EditManager.Instance && EditManager.Instance.PrefabMenu.display)
        {
            Vector2 position = EditManager.Instance.PrefabMenu.anchor;
            position.y = Screen.height - position.y;
            GUILayout.BeginArea(new Rect(position.x, position.y, Screen.width * 0.2f, Screen.height * 0.4f), GUI.skin.box);

            GUILayout.Label("Choose a character");

            foreach(string prefabName in PrefabManager.Prefabs.Keys)
            {
                if (PrefabManager.Prefabs[prefabName].GetComponent<Character>()?.Data.Team == EditManager.Instance.PrefabMenu.team)
                    if (GUILayout.Button(prefabName))
                        EditManager.Instance.CreatePrefab(prefabName);
            }
            GUILayout.EndArea();
        }
    }
}
