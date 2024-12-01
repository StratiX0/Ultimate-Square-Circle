using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class menu_diplay : MonoBehaviour
{
    [SerializeField] private GameObject _menu;

    void Update()
    {
        if (_menu.activeSelf == false)
        {
            Show_menu();
        }
    }

    public void Show_menu()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            _menu.SetActive(true);
            Time.timeScale = 0f;
            Cursor.visible = true;
        }
    }
}
