using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleSceneManager: BaseManager
{
    private CharacterObject m_CurCharacterObject;
    private List<CharacterObject> m_AllCharacterObject;
    
    public override void Start()
    {
    }

	public override void Update ()
	{
        if(Input.GetKeyDown(KeyCode.Space))
        {
            m_CurCharacterObject.Attack(GameDefine.PropStar.SquareBomb);
        }
    }

    public void InitScene()
    {
        App.manager.obstructMgr.LocdScene(BuildRegister.Building);

        m_AllCharacterObject = new List<CharacterObject>();
        m_CurCharacterObject = App.manager.obstructMgr.AddPlayer(GameDefine.Camp.Blue,Vector2.zero);
        m_AllCharacterObject.Add(App.manager.obstructMgr.AddCharacterObject(GameDefine.Camp.Red,new Vector2(-60,6)));
    }

    public void StarBomb(Transform starTransform, GameDefine.Camp camp, float round,float value)
    {
        CharacterObject curCharacterObject;
        for (int index = 0; index < m_AllCharacterObject.Count; index++)
        {
            curCharacterObject = m_AllCharacterObject[index];
            Vector3 distance = curCharacterObject.transform.position - starTransform.position;
            if (distance.x <= round || distance.z <= round)
            {
                curCharacterObject.SmearColor(camp,value);
            }
        }
    }
}
