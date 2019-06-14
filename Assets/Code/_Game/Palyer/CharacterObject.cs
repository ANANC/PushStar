using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class CharacterObject : Obstructer
{
    private Dictionary<GameDefine.PropStar, int> m_CreateStarNumberDict = new Dictionary<GameDefine.PropStar, int>();
    private GameDefine.Camp m_Camp;
    private HpData m_HpData;
    private UIHpViewController m_UIHp;
    private MoveController m_MoveController;


    public CharacterObject(Transform transform, GameDefine.Camp camp):base(transform)
    {
        m_Camp = camp;
        m_HpData = new HpData(GameDefine.CharacterObjectMaxHp, GameDefine.CharacterObjectMaxHp, camp);
        m_UIHp = App.manager.uiMgr.CreateUI<UIHpViewController>(UIRegister.UIHp, transform, new Vector3(0,36,0));
        m_UIHp.SetData(m_HpData);
    }

    public void SetMoveController(MoveController controller)
    {
        m_MoveController = controller;
    }

    public void SmearColor(GameDefine.Camp camp, float value)
    {
        if (camp == m_Camp)
        {
            m_HpData.ChangeCurHp(value);
        }
        else
        {
            m_HpData.ChangeHurt(camp, value);
        }
        m_UIHp.SetData(m_HpData);
    }


    public void Attack(GameDefine.PropStar star)
    {
        int createCound;
        if (m_CreateStarNumberDict.TryGetValue(star, out createCound))
        {
            if (createCound >= GameDefine.PropStarLimit[star])
            {
                return;
            }
        }
        else
        {
            m_CreateStarNumberDict.Add(star, 0);
        }

        if (star == GameDefine.PropStar.SquareBomb)
        {
            PutSquareBomb();
        }

        m_CreateStarNumberDict[star] += 1;
    }

    private void Remove(GameDefine.PropStar star)
    {
        if(m_CreateStarNumberDict.ContainsKey(star))
        {
            m_CreateStarNumberDict[star] -= 1;
        }
    }

    private void PutSquareBomb()
    {
        App.manager.obstructMgr.AddSquarBoom(transform, m_Camp, Remove);
    }
    

}



