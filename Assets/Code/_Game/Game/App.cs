using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class App : MonoBehaviour
{
    private static Manager m_Manager;
    public static Manager manager
    {
        get
        {
            return m_Manager;
        }
    }


    void Awake()
    {
        m_Manager = new Manager();
        m_Manager.Start();
    }

    private void Start()
    {
        Main main = new Main();
        main.Init();
    }

    void Update()
    {
        m_Manager.Update();
    }
    
}
