using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Buildinger : MonoBehaviour
{
    public List<Transform> BuildingList;

    public List<Obstructer> GetStaticBuildingsObstructer()
    {
        if (BuildingList == null)
        {
            return new List<Obstructer>();
        }

        List<Obstructer> buildings = new List<Obstructer>(BuildingList.Count);
        for (int index = 0; index < BuildingList.Count; index++)
        {
            buildings.Add(new Obstructer(BuildingList[index]));
        }
        return buildings;
    }

    public List<Obstructer.Rect> GetStaticBuildingsRect()
    {
        if (BuildingList == null)
        {
            return new List<Obstructer.Rect>();
        }

        List<Obstructer.Rect> buildings = new List<Obstructer.Rect>(BuildingList.Count);
        for (int index = 0; index < BuildingList.Count; index++)
        {
            buildings.Add(new Obstructer(BuildingList[index]).rect);
        }
        return buildings;
    }
}
