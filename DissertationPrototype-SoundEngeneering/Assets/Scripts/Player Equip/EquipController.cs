using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class EquipController : MonoBehaviour
{
    private EquipItem curEquipItem;
    private GameObject curEquipObject;

    private bool useInput;

    [Header("Components")]
    [SerializeField] private Transform equipObjectOrigin;
    [SerializeField] private MouseUtilities mouseUtilities;

    void Update ()
    {
        Vector2 mouseDir = mouseUtilities.GetMouseDirection(transform.position);

        transform.up = mouseDir;

        if(HasItemEquipped())
        {
            if(useInput && EventSystem.current.IsPointerOverGameObject() == false)
            {
                curEquipItem.OnUse();
            }
        }
    }

    public void Equip (ItemData item)
    {
        if(HasItemEquipped())
            UnEquip();

        curEquipObject = Instantiate(item.EquipPrefab, equipObjectOrigin);
        curEquipItem = curEquipObject.GetComponent<EquipItem>();
    }

    public void UnEquip ()
    {
        if(curEquipObject != null)
            Destroy(curEquipObject);

        curEquipItem = null;
    }

    public bool HasItemEquipped ()
    {
        return curEquipItem != null;
    }

    public void OnUseInput (InputAction.CallbackContext context)
    {
        if(context.phase == InputActionPhase.Performed)
            useInput = true;
        if(context.phase == InputActionPhase.Canceled)
            useInput = false;
    }
}