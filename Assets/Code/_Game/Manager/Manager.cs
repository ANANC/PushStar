using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Manager
{
    private List<BaseManager> m_Managers;

    private TimeManager m_TimeMgr;
    public TimeManager timeMgr
    {
        get
        {
            if (m_TimeMgr == null)
            {
                m_TimeMgr = new TimeManager();
            }
            return m_TimeMgr;
        }
    }

    private BattleSceneManager m_BattleSceneMgr;
    public BattleSceneManager battleSceneMgr
    {
        get
        {
            if (m_BattleSceneMgr == null)
            {
                m_BattleSceneMgr = new BattleSceneManager();
            }
            return m_BattleSceneMgr;
        }
    }

    private ResourceManager m_ResourceMgr;
    public ResourceManager resourceMgr
    {
        get
        {
            if (m_ResourceMgr == null)
            {
                m_ResourceMgr = new ResourceManager();
            }
            return m_ResourceMgr;
        }
    }

    private UIManager m_UIMgr;
    public UIManager uiMgr
    {
        get
        {
            if (m_UIMgr == null)
            {
                m_UIMgr = new UIManager();
            }
            return m_UIMgr;
        }
    }

    private ObstructManager m_ObstructMgr;
    public ObstructManager obstructMgr
    {
        get
        {
            if (m_ObstructMgr == null)
            {
                m_ObstructMgr = new ObstructManager();
            }
            return m_ObstructMgr;
        }
    }

    public Manager()
    {
        m_Managers = new List<BaseManager>()
        {
            resourceMgr,
            timeMgr,
            uiMgr,
            obstructMgr,
            battleSceneMgr,
        };
    }

    public void Start()
    {
        for(int index = 0;index<m_Managers.Count;index++)
        {
            m_Managers[index].Start();
        }
    }

    public void Update()
    {
        for (int index = 0; index < m_Managers.Count; index++)
        {
            m_Managers[index].Update();
        }
    }
}
