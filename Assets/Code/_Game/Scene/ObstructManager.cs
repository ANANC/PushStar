
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ObstructManager : BaseManager
{
    private Obstructer m_Player;
    private List<Rect> m_BuildingRectList;
    private List<Obstructer> m_ObstructList = new List<Obstructer>();
    private List<Obstructer> m_AllObstructList = new List<Obstructer>();

    private List<Rect> m_SceneObstructRectList = new List<Rect>(1024);
    public List<Rect> obstructs
    {
        get
        {
            m_SceneObstructRectList.Clear();
            m_SceneObstructRectList.AddRange(m_BuildingRectList);
            for (int i = 0; i < m_ObstructList.Count; i++)
            {
                m_SceneObstructRectList.Add(m_ObstructList[i].rect);
            }

            return m_SceneObstructRectList;
        }
    }

    public List<Obstructer> allObstructs
    {
        get { return m_AllObstructList; }
    }

    public override void Update()
    {
#if DRAWDEBUG

        m_Player.DrawBox();

        for (int index = 0; index < m_AllObstructList.Count; index++)
        {
            m_AllObstructList[index].DrawBox();
        }
#endif
    }

    public void LocdScene(string sceneName)
    {
        m_ObstructList.Clear();

        GameObject building = App.manager.resourceMgr.LoadBuild(sceneName);
        Buildinger buildinger = building.GetComponent<Buildinger>();

        List<Obstructer> staticBuildings = buildinger.GetStaticBuildingsObstructer();
        m_BuildingRectList = new List<Rect>(staticBuildings.Count);
        for (int i = 0; i < staticBuildings.Count; i++)
        {
            AddObstructer(staticBuildings[i], true);
            m_BuildingRectList.Add(staticBuildings[i].rect);
        }
    }

    public CharacterObject AddCharacterObject(GameDefine.Camp camp, Vector2 position)
    {
        CharacterObject characterObject = CreateCharacterObject(camp, position);
        AddObstructer(characterObject);
        return characterObject;
    }

    public CharacterObject AddPlayer(GameDefine.Camp camp, Vector2 position)
    {
        CharacterObject characterObject = CreateCharacterObject(camp, position);
        MoveController moveController = new MoveController(characterObject.transform.rectTransform());
        characterObject.SetMoveController(moveController);
        m_Player = characterObject;

        return characterObject;
    }

    private CharacterObject CreateCharacterObject(GameDefine.Camp camp, Vector2 position)
    {
        GameObject player = App.manager.resourceMgr.Instance(ResourceManager.Type.CharacterObject, "Player");
        player.transform.SetParent(GameDefine.characterRoot);
        player.transform.localPosition = position;
        player.transform.localScale = Vector3.one;
        CharacterObject CharacterObject = new CharacterObject(player.transform, camp);
        return CharacterObject;
    }

    public SquarBoomStarController AddSquarBoom(Transform target, GameDefine.Camp camp, Action<GameDefine.PropStar> callback)
    {
        GameObject squarBoom = App.manager.resourceMgr.Instance(ResourceManager.Type.CharacterObject, "SuqareBoom");
        squarBoom.transform.SetParent(GameDefine.starRoot);
        squarBoom.transform.localScale = Vector3.one;
        squarBoom.transform.position = target.position;
        SquarBoomStarController controller = new SquarBoomStarController(squarBoom.transform);
        controller.Play(camp, callback);

        AddObstructer(controller);

        return controller;
    }

    public void RemoveSquarBoom(BaseStarController star)
    {
        RemoveObstructer(star);
    }

    private void AddObstructer(Obstructer item, bool staticState = false)
    {
        if (!staticState)
        {
            m_ObstructList.Add(item);
        }
        m_AllObstructList.Add(item);
    }

    private void RemoveObstructer(Obstructer item,bool staticState = false)
    {
        if (!staticState)
        {
            m_ObstructList.Remove(item);
        }
        m_AllObstructList.Remove(item);
    }
}
