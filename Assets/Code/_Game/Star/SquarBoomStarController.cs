using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BaseStarController : Obstructer
{
    protected GameDefine.PropStar m_PropStar;
    protected StarData m_Data;
    protected GameDefine.Camp m_Camp;
    protected Action<GameDefine.PropStar> m_BombCallback;

    public BaseStarController(Transform transform) : base(transform) {}

    protected void Init<T>(GameDefine.PropStar starType, GameDefine.Camp camp, Action<GameDefine.PropStar> callback) where T : BombStarData
    {
        m_PropStar = starType;
        m_Data = GameDefine.PropStarData[starType] as T;
        m_Camp = camp;
        m_BombCallback = callback;
    }

    public virtual void Play(GameDefine.Camp camp, Action<GameDefine.PropStar> callback) {}

    protected void BoomCallback()
    {
        if (m_BombCallback != null)
        {
            m_BombCallback(m_PropStar);
        }
    }
}


public class SquarBoomStarController : BaseStarController
{
    private BombStarData m_Data;

    public SquarBoomStarController(Transform transform) : base(transform) {}

    public override void Play(GameDefine.Camp camp, Action<GameDefine.PropStar> callback)
    {
        Init<BombStarData>(GameDefine.PropStar.SquareBomb, camp, callback);

        UpdateColor();

        App.manager.timeMgr.CreatTimer(m_Data.m_Time, m_Data.m_Time, null, Boom, null);
    }

    private void Boom(object param)
    {
        transform.Find("Boom").gameObject.SetActive(true);

        App.manager.obstructMgr.RemoveSquarBoom(this);
        App.manager.battleSceneMgr.StarBomb(transform, m_Camp, m_Data.m_Round, m_Data.m_Hurt);
        BoomCallback();

        App.manager.timeMgr.CreatTimer(0.2f, 0.2f, null, Finish, null);
    }

    private void Finish(object param)
    {
        GameObject.Destroy(transform.gameObject);
    }

    private void UpdateColor()
    {
        Color color = GameDefine.CampColor[m_Camp];
        Image normalImage = transform.Find("Normal").GetComponent<Image>();
        normalImage.color = color;
        Image BoomImage = transform.Find("Boom").GetComponent<Image>();
        BoomImage.color = color;
        BoomImage.rectTransform.sizeDelta = normalImage.rectTransform.sizeDelta * m_Data.m_Round;
    }

}