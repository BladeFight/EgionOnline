﻿using UnityEngine;
using MySql.Data;
using MySql.Data.MySqlClient;
using System.Collections;
using System.Collections.Generic;

// Structure of a Atavism Quests
/*
/* Table structure for table `quests`
/*
 
CREATE TABLE `quests` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `category` int(11) NOT NULL,
  `name` varchar(64) NOT NULL,
  `faction` int(11) NOT NULL,
  `chain` varchar(64) DEFAULT NULL,
  `level` int(11) DEFAULT NULL,
  `zone` varchar(64) DEFAULT NULL,
  `numGrades` int(11) NOT NULL,
  `repeatable` tinyint(1) NOT NULL,
  `description` varchar(512) NOT NULL,
  `objectiveText` varchar(512) NOT NULL,
  `progressText` varchar(512) NOT NULL,
  `deliveryItem1` int(11) NOT NULL DEFAULT '-1',
  `deliveryItem2` int(11) NOT NULL DEFAULT '-1',
  `deliveryItem3` int(11) NOT NULL DEFAULT '-1',
  `questPrereq` int(11) NOT NULL DEFAULT '-1',
  `questStartedReq` int(11) NOT NULL DEFAULT '-1',
  `levelReq` int(11) DEFAULT NULL,
  `raceReq` varchar(32) DEFAULT NULL,
  `aspectReq` varchar(32) DEFAULT NULL,
  `skillReq` int(11) DEFAULT NULL,
  `skillLevelReq` int(11) DEFAULT NULL,
  `repReq` varchar(64) DEFAULT NULL,
  `repLevelReq` int(11) DEFAULT NULL,

 Quest templates are also quite confusing as the design request was made to have multiple grades to achieve. 
 There is the base grade (0) and then players can complete additional objectives to get better rewards. 
 To accommodate this, the quest table was split into 3: quests, questobjectives and questrewards.
  A quest requires an entry in all tables, but can have multiple entries in questobjectives and questrewards
   based on how many grades are on offer for the quest. 
   As a quest grade can have multiple objectives the layout (we can set the limit at 4) for the UI needs to be very flexible.

Quests:

id - Integer - Auto generated by the server, used a lot by other tables to reference a quest
category - Integer - leave this as 0
name - String - the name (title) of the quest
faction - Integer - what faction this quest belongs to, leave as 0 initially
chain - String - a name used to help link quests together in a chain/story
level - Integer - what level the quest is
zone - String - what zone (or type) the quest is
numGrades - Integer - how many grades the quest has (minimum 1, usually no more than 3)
repeatable - Boolean - can this quest be done multiple times (such as a daily quest)
description - String - the description of the quest which is read by the player
objectiveText - String - a written version of the objective for the player to read
progressText - String - the text shown to the player when talking to the quest concluder
deliveryItem1 - Integer - the id of the first item given to the player upon starting the quest (optional)
deliveryItem2 - Integer - the id of the second item given to the player upon starting the quest (optional)
deliveryItem3 - Integer - the id of the third item given to the player upon starting the quest (optional)
questPrereq - Integer - what quest the player must have completed before being offered this quest (optional)
questStartedReq - Integer - what quest the player must have started before being offered this quest (optional)
levelReq - Integer - what level the player must be before being offered the quest (optional)
raceReq - String - what race the player needs to be to be offered the quest (optional)
aspectReq - String - what aspect the player needs to be to be offered the quest (optional)
skillReq - Integer - what skill the player needs to have to be offered the quest (optional)
skillLevelReq - Integer - what level of the skill the player needs to have to be offered the quest (optional)
repReq - String - what rep the player needs to haveto be offered the quest (optional - leave this out atm)
repLevelReq - Integer - what level of rep the player needs to be to be offered the quest (optional - leave this out atm)

Quest Objectives: Quest objectives have 2 types, collect and deliver items or kill mobs. This is defined by the objective type.
id - Integer - Auto generated by the server
questID - Integer - the id of the quest this objective refers to
primaryObjective - Boolean - is this objective required to complete the first level of the quest?
objectiveType - String - can be either 'mob' or 'item'
target - Integer - the id of the mob or item required for this objective
targetCount - Integer - how many of the mob or item is required to complete the objective
targetText - String - a more readable way to describe the objective target

Quest Rewards:
id - Integer - Auto generated by the server
questID - Integer - the id of the quest this objective refers to
rewardLevel - Integer - what grade the reward is
completionText - String - the text shown to the player when they hand in the quest
experience - Integer - how much experience the player gets
item1 - Integer - the id of the first item given to the player as a reward (optional)
item1count - Integer - how many of the first item is given
item2 - Integer - the id of the second item given to the player as a reward (optional)
item2count - Integer - how many of the second item is given
item3 - Integer - the id of the third item given to the player as a reward (optional)
item3count - Integer - how many of the third item is given
item4 - Integer - the id of the fourth item given to the player as a reward (optional)
item4count - Integer - how many of the fourth item is given
itemToChoose1 - Integer - the id of the first item the player can choose to receive (optional)
itemToChoose1count - Integer - how many of the first item the player gets
itemToChoose2 - Integer - the id of the second item the player can choose to receive (optional)
itemToChoose2count - Integer - how many of the second item the player gets
itemToChoose3 - Integer - the id of the third item the player can choose to receive (optional)
itemToChoose3count - Integer - how many of the third item the player gets
itemToChoose4 - Integer - the id of the fourth item the player can choose to receive (optional)
itemToChoose4count - Integer - how many of the fourth item the player gets
currency - Integer - what currency the player is given (optional)
currencyCount - Integer - how much of that currency
reputation1 - Integer - what reputation the player is given (optional)
reputation1Count - Integer - how much rep
reputation2 - Integer - what reputation the player is given (optional)
reputation2Count - Integer - how much rep
*/

public class QuestsObjectivesData: DataStructure
{
	public int id = 0;					// Database Index
	
	// General Parameters
	public int questID = -1; 				// The id of the quest this objective refers to
	public bool primaryObjective = true;		// Is this objective required to complete the first level of the quest?
	public string objectiveType = ""; 		// Can be either 'mob' or 'item'
	public int target = 0;					// The id of the mob or item required for this objective
	public int targetCount = 1;				// How many of the mob or item is required to complete the objective
	public string targetText = "";			// A more readable way to describe the objective target
	
	public QuestsObjectivesData ()
	{
		// Database fields
		fields = new Dictionary<string, string> () {
			{"id", "int"},
			{"questID", "int"},
			{"primaryObjective", "bool"},
			{"objectiveType", "string"},
			{"target", "int"},
			{"targetCount", "int"},
			{"targetText", "string"}
			
		};
	}
	
	
	public QuestsObjectivesData Clone()
	{
		return (QuestsObjectivesData) this.MemberwiseClone();
	}
	
	public override string GetValue (string fieldKey)
	{
		switch (fieldKey) {
		case "id":
			return id.ToString();
			break;
		case "questID":
			return questID.ToString();
			break;
		case "primaryObjective":
			return primaryObjective.ToString();
			break;
		case "objectiveType":
			return objectiveType;
			break;
		case "target":
			return target.ToString();
			break;
		case "targetCount":
			return targetCount.ToString();
			break;
		case "targetText":
			return targetText;
			break;
		}	
		return "";
	}
	
}

public class QuestsData: DataStructure
{
	public int id = 0;					// Database Index

	// General Parameters
	public string name = "name";		// The skill template name
	public int category = 1;				//leave this as 1
	public int faction = 0; 				//what faction this quest belongs to, leave as 0 initially
	public string chain = ""; 			//a name used to help link quests together in a chain/story
	public int level = 1; 					//what level the quest is
	public string zone = "";			//what zone (or type) the quest is
	public int numGrades = 1; 				//how many grades the quest has (minimum 1, usually no more than 3)
	public bool repeatable = false; 			//can this quest be done multiple times (such as a daily quest)
	public string description = ""; 		//the description of the quest which is read by the player
	public string objectiveText = ""; 		//a written version of the objective for the player to read
	public string progressText = ""; 		//the text shown to the player when talking to the quest concluder
	public int deliveryItem1 = 0; 			//the id of the first item given to the player upon starting the quest (optional)
	public int deliveryItem2 = 0; 			//the id of the second item given to the player upon starting the quest (optional)
	public int deliveryItem3 = 0; 			//the id of the third item given to the player upon starting the quest (optional)
	public int questPrereq = 0; 			//what quest the player must have completed before being offered this quest (optional)	 
	public int questStartedReq = 0; 		//what quest the player must have started before being offered this quest (optional)
	public int levelReq = 0; 				//what level the player must be before being offered the quest (optional)
	public string raceReq = "";			//what race the player needs to be to be offered the quest (optional)
	public string aspectReq = ""; 			//what aspect the player needs to be to be offered the quest (optional)
	public int skillReq = 0; 				//what skill the player needs to have to be offered the quest (optional)
	public int skillLevelReq = 0; 			//what level of the skill the player needs to have to be offered the quest (optional)
	public int repReq = -1;				//what rep the player needs to haveto be offered the quest (optional;)
	public int repLevelReq; 			//what level of rep the player needs to be to be offered the quest (optional)

	// Objectives
	public List<QuestsObjectivesData> questObjectives = new List<QuestsObjectivesData>();
	// Objectives to be deleted
	public List<int> objectivesToBeDeleted = new List<int>();

	//Rewards
	public string completionText = "";		// the text shown to the player when they hand in the quest
	public int experience;				// how much experience the player gets
	public int item1;					// the id of the first item given to the player as a reward (optional)
	public int item1count;				// how many of the first item is given
	public int item2;					// the id of the second item given to the player as a reward (optional)
	public int item2count;				// how many of the second item is given
	public int item3;					// the id of the third item given to the player as a reward (optional)
	public int item3count;				// how many of the third item is given
	public int item4;					// the id of the fourth item given to the player as a reward (optional)
	public int item4count;				// how many of the fourth item is given
	public int chooseItem1;			// the id of the first item the player can choose to receive (optional)
	public int chooseItem1count;		// how many of the first item the player gets
	public int chooseItem2;			// the id of the second item the player can choose to receive (optional)
	public int chooseItem2count;		// how many of the second item the player gets
	public int chooseItem3;			// the id of the third item the player can choose to receive (optional)
	public int chooseItem3count;		// how many of the third item the player gets
	public int chooseItem4;			// the id of the fourth item the player can choose to receive (optional)
	public int chooseItem4count;		// how many of the fourth item the player gets
	public int currency;				// what currency the player is given (optional)
	public int currencyCount;			// how much of that currency
	public int currency2;				// what currency the player is given (optional)
	public int currency2count;			// how much of that currency
	public int rep1;				// what reputation the player is given (optional)
	public int rep1gain;		// how much rep
	public int rep2;				// what reputation the player is given (optional)
	public int rep2gain;		// how much rep
	
	public QuestsData ()
	{
		// Database fields
	fields = new Dictionary<string, string> () {
		{"name", "string"},
		{"category", "int"},
		{"faction", "int"},
		{"chain", "string"},
		{"level", "int"},
		{"zone", "string"},
		{"numGrades", "int"},
		{"repeatable", "bool"},
		{"description", "string"},
		{"objectiveText", "string"},
		{"progressText", "string"},
		{"deliveryItem1", "int"},
		{"deliveryItem2", "int"},
		{"deliveryItem3", "int"},
		{"questPrereq", "int"},
		{"questStartedReq", "int"},
		{"levelReq", "int"},
		{"raceReq", "string"},
		{"aspectReq", "string"},
		{"skillReq", "int"},
		{"skillLevelReq", "int"},
		{"repReq", "int"},
		{"repLevelReq", "int"},
		{"completionText", "string"},
		{"experience", "int"},
		{"item1", "int"},
		{"item1count", "int"},
		{"item2", "int"},
		{"item2count", "int"},
		{"item3", "int"},
		{"item3count", "int"},
		{"item4", "int"},
		{"item4count", "int"},
		{"chooseItem1", "int"},
		{"chooseItem1count", "int"},
		{"chooseItem2", "int"},
		{"chooseItem2count", "int"},
		{"chooseItem3", "int"},
		{"chooseItem3count", "int"},
		{"chooseItem4", "int"},
		{"chooseItem4count", "int"},
		{"currency1", "int"},
		{"currency1count", "int"},
		{"currency2", "int"},
		{"currency2count", "int"},
		{"rep1", "int"},
		{"rep1gain", "int"},
		{"rep2", "int"},
		{"rep2gain", "int"},
	};
	}
	
	public QuestsData Clone()
	{
		return (QuestsData) this.MemberwiseClone();
	}
		
	public override string GetValue (string fieldKey)
	{
		switch (fieldKey) {
		case "id":
			return id.ToString();
			break;
		case "name":
			return name;
			break;
		case "category":
			return category.ToString();
			break;
		case "faction":
			return faction.ToString();
			break;
		case "chain":
			return chain;
			break;
		case "level":
			return level.ToString();
			break;
		case "zone":
			return zone;
			break;
		case "numGrades":
			return numGrades.ToString();
			break;
		case "repeatable":
			return repeatable.ToString();
			break;
		case "description":
			return description;
			break;
		case "objectiveText":
			return objectiveText;
			break;
		case "progressText":
			return progressText;
			break;
		case "deliveryItem1":
			return deliveryItem1.ToString();
			break;
		case "deliveryItem2":
			return deliveryItem2.ToString();
			break;
		case "deliveryItem3":
			return deliveryItem3.ToString();
			break;
		case "questPrereq":
			return questPrereq.ToString();
			break;
		case "questStartedReq":
			return questStartedReq.ToString();
			break;
		case "levelReq":
			return levelReq.ToString();
			break;
		case "raceReq":
			return raceReq.ToString();
			break;
		case "aspectReq":
			return aspectReq;
			break;
		case "skillReq":
			return skillReq.ToString();
			break;
		case "skillLevelReq":
			return skillLevelReq.ToString();
			break;
		case "repReq":
			return repReq.ToString();
			break;
		case "repLevelReq":
			return repLevelReq.ToString();
			break;
		case "completionText":
			return completionText;
			break;
			
		case "experience":
			return experience.ToString();
			break;
			
		case "item1":
			return item1.ToString();
			break;
			
		case "item1count":
			return item1count.ToString();
			break;
			
		case "item2":
			return item2.ToString();
			break;
			
		case "item2count":
			return item2count.ToString();
			break;
			
		case "item3":
			return item3.ToString();
			break;
			
		case "item3count":
			return item3count.ToString();
			break;
			
		case "item4":
			return item4.ToString();
			break;
			
		case "item4count":
			return item4count.ToString();
			break;
			
		case "chooseItem1":
			return chooseItem1.ToString();
			break;
			
		case "chooseItem1count":
			return chooseItem1count.ToString();
			break;
			
		case "chooseItem2":
			return chooseItem2.ToString();
			break;
			
		case "chooseItem2count":
			return chooseItem2count.ToString();
			break;
			
		case "chooseItem3":
			return chooseItem3.ToString();
			break;
			
		case "chooseItem3count":
			return chooseItem3count.ToString();
			break;
			
		case "chooseItem4":
			return chooseItem4.ToString();
			break;
			
		case "chooseItem4count":
			return chooseItem4count.ToString();
			break;
			
		case "currency1":
			return currency.ToString();
			break;
			
		case "currency1count":
			return currencyCount.ToString();
			break;

		case "currency2":
			return currency2.ToString();
			break;

		case "currency2count":
			return currency2count.ToString();
			break;

		case "rep1":
			return rep1.ToString();
			break;
			
		case "rep1gain":
			return rep1gain.ToString();
			break;
			
		case "rep2":
			return rep2.ToString();
			break;
			
		case "rep2gain":
			return rep2gain.ToString();
			break;
		}	
		return "";
	}
		
}