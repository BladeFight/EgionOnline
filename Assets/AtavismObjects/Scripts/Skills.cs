using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Skills : MonoBehaviour
{
	
	int currentSkillPoints;
	int totalSkillPoints;
	int skillPointCost;
	Dictionary<int, Skill> playerSkills = new Dictionary<int, Skill> ();
	Dictionary<int, Skill> skills;
	
	void Start ()
	{
		skills = new Dictionary<int, Skill>();
		Object[] skillPrefabs = Resources.LoadAll("Content/Skills");
		foreach (Object skillPrefab in skillPrefabs) {
			GameObject go = (GameObject) skillPrefab;
			Skill skillData = go.GetComponent<Skill>();
			if (skillData.id > 0)
				skills.Add(skillData.id, skillData);
		}

		// Register for skills message
		NetworkAPI.RegisterExtensionMessageHandler ("skills", HandleSkillUpdate);
	}
	
	public void IncreaseSkill (int skillID)
	{
		NetworkAPI.SendTargetedCommand (ClientAPI.GetPlayerOid (), "/skillIncrease " + skillID);
	}
	
	public void DecreaseSkill (int skillID)
	{
		NetworkAPI.SendTargetedCommand (ClientAPI.GetPlayerOid (), "/skillDecrease " + skillID);
	}
	
	public void PurchaseSkillPoint ()
	{
		Dictionary<string, object> props = new Dictionary<string, object> ();
		NetworkAPI.SendExtensionMessage (ClientAPI.GetPlayerOid (), false, "combat.PURCHASE_SKILL_POINT", props);
	}
	
	public void HandleSkillUpdate (Dictionary<string, object> props)
	{
		playerSkills.Clear ();
		currentSkillPoints = (int)props ["skillPoints"];
		totalSkillPoints = (int)props ["totalSkillPoints"];
		skillPointCost = (int)props ["skillPointCost"];
		int numSkills = (int)props ["numSkills"];
		UnityEngine.Debug.Log ("Got skill update with numSkills: " + numSkills);
		for (int i = 0; i < numSkills; i++) {
			//Skill skill = gameObject.AddComponent<Skill> ();
			int skillID = (int)props ["skill" + i + "ID"];

			if (!skills.ContainsKey(skillID)) {
				UnityEngine.Debug.LogWarning("Skill " + skillID + " does not exist");
				continue;
			}
			Skill skill = gameObject.AddComponent<Skill>();
			skill.id = skillID;
			skill.name = skills[skillID].name;
			skill.icon = skills[skillID].icon;
			skill.CurrentPoints = (int)props ["skill" + i + "Current"];
			skill.CurrentLevel = (int)props ["skill" + i + "Level"];
			skill.MaximumLevel = (int)props ["skill" + i + "Max"];
			playerSkills.Add (skillID, skill);
		}
		string[] args = new string[1];
		EventSystem.DispatchEvent ("SKILL_UPDATE", args);
	}

	public Dictionary<int, Skill> SkillsList {
		get {
			return skills;
		}
	}
	
	public Dictionary<int, Skill> PlayerSkills {
		get {
			return playerSkills;
		}
	}
	
	public int CurrentSkillPoints {
		get {
			return currentSkillPoints;
		}
	}
	
	public int TotalSkillPoints {
		get {
			return totalSkillPoints;
		}
	}
	
	public int SkillPointCost {
		get {
			return skillPointCost;
		}
	}
}
