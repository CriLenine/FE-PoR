using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;

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
    public TextMeshProUGUI attackerDodgeChance;
    public Image defenderInfoPanel;
    public TextMeshProUGUI defenderName;
    public TextMeshProUGUI defenderClass;
    public TextMeshProUGUI defenderHP;
    public TextMeshProUGUI defenderDamage;
    public TextMeshProUGUI defenderDodgeChance;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }

    public void DisplayCharacterInfos(CharacterData data)
    {
        characterInfoPanel.CrossFadeAlpha(0.8f, 0.5f, false);
        characterInfoPanel.color =
            data.Team == CharacterData.CharacterTeam.Ally ? new Color(0, 96, 231) : new Color(171, 0, 0);
        characterName.text = data.Name;
        characterClass.text = data.Class.ToString();
        characterHP.text = $"HP : {data.currentHealth}/{data.MaxHealth}";
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
        defenderDodgeChance.text = "";

        attackerInfoPanel.CrossFadeAlpha(0f, 0.5f, false);
        attackerName.text = "";
        attackerClass.text = "";
        attackerHP.text = "";
        attackerDamage.text = "";
        attackerDodgeChance.text = "";
    }

    public void DisplayAttackInfos(CharacterData defenderData)
    {
        defenderInfoPanel.CrossFadeAlpha(0.8f, 0.5f, false);
        defenderInfoPanel.color =
            defenderData.Team == CharacterData.CharacterTeam.Ally ? new Color(0, 96, 231) : new Color(171, 0, 0);
        defenderName.text = defenderData.Name;
        defenderClass.text = defenderData.Class.ToString();
        defenderHP.text = $"HP : {defenderData.currentHealth}/{defenderData.MaxHealth}";
        defenderDamage.text = $"Damage : {defenderData.damage}";
        defenderDodgeChance.text = $"Dodge Chance : {defenderData.dodgeChance}";

        CharacterData attackerData = MainGame.instance.CurrentData;

        attackerInfoPanel.CrossFadeAlpha(0.8f, 0.5f, false);
        attackerInfoPanel.color =
            attackerData.Team == CharacterData.CharacterTeam.Ally ? new Color(0, 96, 231) : new Color(171, 0, 0);
        attackerName.text = attackerData.Name;
        attackerClass.text = attackerData.Class.ToString();
        attackerHP.text = $"HP : {attackerData.currentHealth}/{attackerData.MaxHealth}";
        attackerDamage.text = $"Damage : {attackerData.damage}";
        attackerDodgeChance.text = $"Dodge Chance : {attackerData.dodgeChance}";
    }

    void OnGUI()
    {
        if (MainGame.instance.DisplayActionMenu)
        {
            Vector2 position = Camera.main.WorldToScreenPoint(MainGame.instance.ActionMenuAnchor);
            position.y = Screen.height - position.y;
            GUILayout.BeginArea(new Rect(position.x, position.y, 160, 200), GUI.skin.box);

            GUILayout.Label("Commands");

            if (MainGame.instance.HasATarget)
            {
                if (GUILayout.Button("Attack"))
                {
                    MainGame.instance.ShowTargetableCells();
                }
            }

            if (GUILayout.Button("Wait"))
            {
                MainGame.instance.EndCharacterTurn();
            }

            GUILayout.EndArea();
        }
    }
}
