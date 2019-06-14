using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : BaseManager
{
    private List<BaseUIController> m_UI = new List<BaseUIController>();
    
    public override void Update()
    {
        for (int index = 0; index < m_UI.Count; index++)
        {
            m_UI[index].Update();
        }
    }

    public T CreateUI<T>(string uiName,Transform parent,Vector3 position)where T:BaseUIController,new()
    {
        GameObject ui = App.manager.resourceMgr.Instance(ResourceManager.Type.UI,uiName);
        parent = parent == null ? GameDefine.uiRoot : parent;
        ui.transform.SetParent(parent);
        ui.transform.localPosition = position;
        ui.transform.localScale = Vector3.one;
        T controller = new T();
        controller.Create(ui.transform);
        m_UI.Add(controller);
        controller.Start();
        return controller;
    }

    public void DestroyUI<T>(T controller) where T : BaseUIController
    {
        if (m_UI.Contains(controller))
        {
            m_UI.Remove(controller);
        }
    }
}
