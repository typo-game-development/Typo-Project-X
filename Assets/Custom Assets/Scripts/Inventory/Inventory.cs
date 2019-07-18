using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Inventory : MonoBehaviour {

    //[HideInInspector]
    public List<InventorySlot> slotList = new List<InventorySlot>();

    public int currentJunks = 0;

    [HideInInspector]
    public string openInventoryButton = "PS4_TRIANGLE";

    [HideInInspector]
    public string openInventoryKey = "I";

    public bool isOpened = false;

    public int maxSlotNumber;


    public void Start()
    {
        maxSlotNumber = 20;

    }

    public void Update()
    {
        if (Input.GetButtonDown(openInventoryButton) || Input.GetButtonDown(openInventoryKey))
        {
            isOpened = !isOpened;
        }

        if (isOpened)
        {
            Time.timeScale = 0;
        }

        else
        {
            Time.timeScale = 1;
        }

    }

    public ActionResult AddItem(BaseCollectibleItem objectToAdd)
    {
        bool needNewItemSlot = true;
        
        foreach (InventorySlot slot in slotList)
        {
            if (slot.item.type == objectToAdd.type)
            {
                if(!slot.slotFull)
                {
                    needNewItemSlot = false;
                    
                    slot.currentSlotValue += 1;

                    slot.CheckMaxValue();

                    Debug.Log("PICKED UP OBJECT!");

                    return ActionResult.Success; 
                }
            }
        }
        
        if (needNewItemSlot)
        {
            int slotCounter = 0;

            if (slotList.Count == maxSlotNumber)
            {
                Debug.Log("INVENTORY FULL!");
                return ActionResult.InventoryFull;
            }

            foreach (InventorySlot slot in slotList)
            {
                if(slot.item.type == objectToAdd.type)
                {
                    slotCounter += 1;
                }
            }

            if (slotCounter < objectToAdd.maxStackNumber)
            {
                if (slotList.Count - 1 < maxSlotNumber)
                {
                    InventorySlot tempSlot = new InventorySlot();

                    tempSlot.currentSlotValue = 1;

                    tempSlot.item = objectToAdd;

                    slotList.Add(tempSlot);

                    Debug.Log("NEW SLOT CREATED!");

                    return ActionResult.Success;
                }
            }
            else
            {
                Debug.Log("MAX STACK NUMBER REACHED!");

                return ActionResult.MaxStack;
            }
        }

        return ActionResult.Fail;
    }

    public ActionResult AddItem(JunkCollectibleItem objectToAdd)
    {
        currentJunks += objectToAdd.junkValue;
        return ActionResult.Success;
    }

    public ActionResult RemoveItem(BaseCollectibleItem.Type itemToRemove, int quantity)
    {
        foreach (InventorySlot slot in slotList)
        {
            if (slot.item.type == itemToRemove)
            {
                if (slot.currentSlotValue > 0)
                {
                    if (quantity < slot.currentSlotValue)
                    {
                        slot.currentSlotValue -= quantity;

                        Debug.Log(slot.currentSlotValue);

                        if (slot.currentSlotValue == 0)
                        {
                            slotList.Remove(slot);
                        }

                        return ActionResult.Success;

                    }
                    else
                    {
                        Debug.Log("You don't have enought items.");
                        return ActionResult.NotEnoughtItems;
                    }

                }

            }

        }
        return ActionResult.NoItemFound;
    }

    [HideInInspector]
    public enum ActionResult
    {
        Fail = -1,
        Success = 0,
        NotEnoughtItems = 1,
        MaxStack = 2,
        InventoryFull = 3,
        NoItemFound = 4,
    }
}
