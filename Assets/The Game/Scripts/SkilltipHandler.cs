using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class SkilltipHandler : MonoBehaviour, IPointerExitHandler
{
    private GameObject shownTip;
    // Update is called once per frame
    void Update()
    {
        ShowApropriateSkillTip();
    }

    private void ShowApropriateSkillTip()
    {
        PointerEventData pointerEventData = new PointerEventData(EventSystem.current);
        pointerEventData.position = Input.mousePosition;

        List<RaycastResult> raycastResults = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerEventData, raycastResults);
        for (int i = 0; i < raycastResults.Count; i++)
        {
            if (raycastResults[i].gameObject.name.Equals("Skill"))
            {
                raycastResults[i].gameObject.transform.GetChild(0).gameObject.SetActive(true);
                shownTip = raycastResults[i].gameObject.transform.GetChild(0).gameObject;
            }
        }
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        if (shownTip != null)
        {
            shownTip.SetActive(false);
        }
    }
}
