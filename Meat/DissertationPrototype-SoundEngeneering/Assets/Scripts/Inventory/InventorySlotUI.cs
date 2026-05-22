using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class InventorySlotUI : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI quantityText;

    private ItemSlot itemSlot;

    // Called when we CLICK on the slot UI.
    public void OnPointerClick (PointerEventData eventData)
    {
        if(itemSlot.Item != null)
            Inventory.Instance.UseItem(itemSlot);
    }

    // Called when our mouse ENTERS the slot UI.
    public void OnPointerEnter (PointerEventData eventData)
    {
        if(itemSlot.Item != null)
        {
            Inventory.Instance.UI.TooltipUI.SetTooltip(itemSlot.Item);
        }
    }

    // Called when our mouse EXITS the slot UI.
    public void OnPointerExit (PointerEventData eventData)
    {
        Inventory.Instance.UI.TooltipUI.DisableTooltip();
    }

    public void SetItemSlot (ItemSlot slot)
    {
        itemSlot = slot;

        if(slot.Item == null)
        {
            icon.enabled = false;
            quantityText.text = string.Empty;
        }
        else
        {
            icon.enabled = true;
            icon.sprite = slot.Item.Icon;
            quantityText.text = slot.Quantity > 1 ? slot.Quantity.ToString() : string.Empty;
        }
    }
}