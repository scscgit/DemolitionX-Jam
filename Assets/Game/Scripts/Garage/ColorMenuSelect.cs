using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorMenuSelect : MonoBehaviour
{
    public GameObject WheelSmokeMenu;
    public GameObject BodyColorMenu;
    public GameObject RimColorMenu;


    public void OpenMenu(GameObject activeMenu)
    {
        WheelSmokeMenu.SetActive(false);
        BodyColorMenu.SetActive(false);
        RimColorMenu.SetActive(false);

        activeMenu.SetActive(true);
    }
}
