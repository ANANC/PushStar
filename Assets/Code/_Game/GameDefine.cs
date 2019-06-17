using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GameDefine
{
    //*--- 角色 ---*/
    public static float CharacterObjectMaxHp = 100;
    public static float MoveSpeed = 0.5f;

    public enum Camp //颜色类型
    {
        Red,
        Yello,
        Green,
        Blue,
        Purple
    }

    public static Dictionary<Camp, Color> CampColor = new Dictionary<Camp, Color>()
    {
        {Camp.Red, new Color(0.85f, 0.25f, 0.25f)},
        {Camp.Yello, new Color(0.89f, 0.83f, 0.1f)},
        {Camp.Green, new Color(0.17f, 0.63f, 0.23f)},
        {Camp.Blue, new Color(0.14f, 0.56f, 0.85f)},
        {Camp.Purple, new Color(0.63f, 0.33f, 0.91f)}
    };

    //*--- 星星 ---*/
    public enum PropStar //星星类型
    {
        SquareBomb, //方爆炸星
        CircleBomb, //圆爆炸星
        Elastic //波波星
    }

    public static Dictionary<PropStar, StarData> PropStarData = new Dictionary<PropStar, StarData> //星星数据
    {
        {PropStar.SquareBomb, new BombStarData(2, 3,5)},
    };

    public static Dictionary<PropStar, int> PropStarLimit = new Dictionary<PropStar, int>   //创建星星上限
    {
        {PropStar.SquareBomb, 3},
    };

    //角色挂载的父节点
    private static Transform m_CharacterRoot;
    public static Transform characterRoot
    {
        get
        {
            if(m_CharacterRoot == null)
            {
                m_CharacterRoot = GameObject.FindGameObjectWithTag("CharacterObjectRoot").transform;
            }
            return m_CharacterRoot;
        }
    }

    private static Transform m_StarRoot;
    public static Transform starRoot
    {
        get
        {
            if (m_StarRoot == null)
            {
                m_StarRoot = GameObject.FindGameObjectWithTag("StarRoot").transform;
            }
            return m_StarRoot;
        }
    }

    private static Transform m_UIRoot;
    public static Transform uiRoot
    {
        get
        {
            if (m_UIRoot == null)
            {
                m_UIRoot = GameObject.FindGameObjectWithTag("UIRoot").transform;
            }
            return m_UIRoot;
        }
    }

    private static Transform m_BuildRoot;
    public static Transform buildRoot
    {
        get
        {
            if (m_BuildRoot == null)
            {
                m_BuildRoot = GameObject.FindGameObjectWithTag("BuildingRoot").transform;
            }
            return m_BuildRoot;
        }
    }
}

public class EventCenter
{
    public delegate void StarBoom(Transform starTransform, GameDefine.Camp camp, float round, float value);
    public static StarBoom StarBoomEvent;
    public static void SendStarBoomEvent(Transform starTransform, GameDefine.Camp camp, float round, float value)
    {
        if (StarBoomEvent != null)
        {
            StarBoomEvent(starTransform, camp, round, value);
        }
    }
}

public class HpData
{
    public float m_MaxHp;
    public float m_CurHp;
    public GameDefine.Camp m_Camp;
    public HpData[] m_HurtDict;
    public GameDefine.Camp[] m_HurtCamps;
    public int m_CurHurtCount;

    public HpData(float maxHp, float curHp, GameDefine.Camp camp)
    {
        m_MaxHp = maxHp;
        m_CurHp = curHp;
        m_Camp = camp;
        int hurtCount = Enum.GetNames(typeof(GameDefine.Camp)).Length;
        m_HurtDict = new HpData[hurtCount];
        m_HurtCamps = new GameDefine.Camp[hurtCount];
        m_CurHurtCount = 0;
    }

    public void ChangeCurHp(float value)
    {
        m_CurHp += value;
        m_CurHp = m_CurHp > m_MaxHp ? m_MaxHp : m_CurHp;
        m_CurHp = m_CurHp < 0 ? 0 : m_CurHp;
    }

    public void ChangeHurt(GameDefine.Camp camp, float value)
    {
        int targetIndex = -1;
        for (int index = 0; index < m_CurHurtCount; index++)
        {
            if (m_HurtCamps[index] == camp)
            {
                targetIndex = index;
                break;
            }
        }

        if (targetIndex == -1)
        {
            m_CurHurtCount += 1;
            targetIndex += 1;
            m_HurtCamps[targetIndex] = camp;
            m_HurtDict[targetIndex] = new HpData(m_MaxHp,value,camp);
        }
        else
        {
            m_HurtDict[targetIndex].ChangeCurHp(value);
        }

        int minIndex = -1;
        float minValue = m_MaxHp;
        for (int index = 0; index < m_CurHurtCount; index++)
        {
            if (index != targetIndex)
            {
                if (minValue > m_HurtDict[index].m_CurHp)
                {
                    minValue = m_HurtDict[index].m_CurHp;
                    minIndex = index;
                }
            }
        }

        float changeCurHp = 0;
        if (minIndex != -1)
        {
            if (m_HurtDict[minIndex].m_CurHp < value)
            {
                changeCurHp = value - m_HurtDict[minIndex].m_CurHp;
            }
            m_HurtDict[minIndex].ChangeCurHp(-value);
        }

        if (m_CurHurtCount == 1)
        {
            changeCurHp = -value;
        }

        ChangeCurHp(changeCurHp);


    }
}

public class StarData
{
    public float m_Time; //等待多少秒后爆炸

    public StarData(float time)
    {
        m_Time = time;
    }
}

public class BombStarData : StarData
{
    public float m_Round;//辐射半径
    public float m_Hurt;//伤害值

    public BombStarData(float time, float round,float hurt) : base(time)
    {
        m_Round = round;
        m_Hurt = hurt;
    }
}

public class Timer
{
    private float m_CurTime;
    private float m_CurIntervalTime;
    private float m_Time;
    private float m_IntervalTime;
    private Action<float, object> m_UpdateCallback;
    private Action<object> m_EndCallback;
    private object m_Param;
    private bool m_Play;

    public Timer(float time, float interval, Action<float, object> update, Action<object> endCallback, object param)
    {
        m_Time = time;
        m_IntervalTime = interval;
        m_UpdateCallback = update;
        m_EndCallback = endCallback;
        m_Param = param;
        m_Play = false;
    }

    public void Start()
    {
        m_CurTime = 0;
        m_CurIntervalTime = 0;
        m_Play = true;
    }

    public void Update()
    {
        if (!m_Play)
        {
            return;
        }

        m_CurTime += Time.deltaTime;
        m_CurIntervalTime += Time.deltaTime;
        if (m_CurIntervalTime >= m_IntervalTime)
        {
            m_CurIntervalTime = 0;
            if (m_UpdateCallback != null)
            {
                m_UpdateCallback(m_CurTime, m_Param);
            }
        }

        if (m_CurTime >= m_Time)
        {
            Break();
        }
    }

    public void Break()
    {
        if (m_EndCallback != null)
        {
            m_EndCallback(m_Param);
        }

        m_Play = false;
    }

    public bool Playing()
    {
        return m_Play;
    }
}

public class UIRegister
{
    public const string UIHp = "UIHp";
}

public class BuildRegister
{
    public const string Building = "Building";
}