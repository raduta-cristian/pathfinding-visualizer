using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonSelect : MonoBehaviour
{
    [SerializeField] List<GameObject> allButtons;

    [SerializeField] Color NOTSelectedColor;
    [SerializeField] Color SelectedColor;

    private void Start()
    {
        foreach (GameObject btn in allButtons)
        {
            btn.GetComponent<Image>().color = (btn == GameObject.Find("PaintNothing")) ? SelectedColor : NOTSelectedColor;

        }
    }

    public void ButtonClicked(GameObject bt)
    {
        foreach(GameObject btn in allButtons)
        {
            btn.GetComponent<Image>().color = (btn == bt) ? SelectedColor : NOTSelectedColor;
        }
    }

}
