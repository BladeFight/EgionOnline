using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CraftingPlugin : MonoBehaviour
{
    public static CraftingPlugin Instance;
    bool toggleCraftingWindow = false;
    void Awake()
    {
        Instance = this;
    }
    // Use this for initialization
    void Start()
    {
        NetworkAPI.RegisterExtensionMessageHandler("CraftingMsg", HandleExtensionMessages);
    }

    void HandleExtensionMessages(Dictionary<string, object> target)
    {
        string msgType = (string)target["PluginMessageType"];

        switch (msgType)
        {
            case "CraftingStarted":
                {
                    GameObject ui = GameObject.Find("UI");
                    ui.GetComponent<CraftingUI>().StartProgressBar();
                    break;
                }
            case "CraftingFailed":
                {
                    Dictionary<string, object> errors = new Dictionary<string,object>();
                    errors.Add("ErrorText", (string)target["ErrorMsg"]);
                    GameObject ui = GameObject.Find("UI");
                    ui.GetComponent<ErrorMessage>().HandleErrorMessage(errors);
                    break;
                }
        }

        Debug.Log("Got A Crafting Message!");
    }

    // Update is called once per frame
    void Update()
    {
        /*if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            LinkedList<object> componentIds = new LinkedList<object>();
            componentIds.AddLast(6);
            componentIds.AddLast(7);

            LinkedList<object> componentCount = new LinkedList<object>();
            componentCount.AddLast(2);
            componentCount.AddLast(1);

            CraftItem("Long Sword", 1, componentIds, componentCount);
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            LinkedList<object> componentIds = new LinkedList<object>();
            componentIds.AddLast(9);

            LinkedList<object> componentCount = new LinkedList<object>();
            componentCount.AddLast(1);

            CraftItem("Bronze", 1, componentIds, componentCount);
        }*/
    }
    public void CraftItem(string recipeName, int craftType, LinkedList<object> itemIds, LinkedList<object> stackSizes)
    {
        Dictionary<string, object> properties = new Dictionary<string, object>();
        int recipeId = 10;
        properties["id"] = Client.Instance.CharacterId;
        properties.Add("ItemName", recipeName);
        properties["CraftType"] = craftType;
        properties["ItemIds"] = itemIds;
        properties["ItemStacks"] = stackSizes;
        properties["RecipeId"] = recipeId;
        NetworkAPI.SendExtensionMessage(Client.Instance.CharacterId.ToLong(), false, "crafting.CRAFT_ITEM", properties);
    }
}

public class Recipe
{
    public string name;

    public Recipe(string _name)
    {
        name = _name;
    }
}
