using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ObjectSortHelper : EditorWindow
{
    private static class config
    {
        public static string cakeMaterialName = "cake";
        public static string pillarEmptyMaterialName = "pillarEmpty";
        public static string playerMaterialName = "player";
        public static string toothpasteMaterialName = "toothpaste";
        public static string waterMaterialName = "water";
    }

    public enum PillarType
    {
        empty,
        water,
        toothpaste
    }

    private Material cakeMaterial;
    private Material pillarEmptyMaterial;
    private Material playerMaterial;
    private Material toothpasteMaterial;
    private Material waterMaterial;

    private float floor; //层
    private float floorHight; //层高
    private Vector2 cellSize; //格子大小
    private Vector2 sort; //排序
    private Vector2 space; //间隔
    private Vector2 side; //边距


    private PillarType[] pillarDatas;
    private Dictionary<int, List<MeshRenderer>> pillarMaterialList;
    private GameObject rootGameObject;

    private void Build()
    {
        if (rootGameObject != null)
        {
            GameObject.DestroyImmediate(rootGameObject);
            rootGameObject = null;
            pillarMaterialList.Clear();
        }

        int count = (int) (sort.x * sort.y);
        pillarDatas = new PillarType[count];
        pillarMaterialList = new Dictionary<int, List<MeshRenderer>>();

        Vector2 cakeSize = side * 2 + cellSize * sort + new Vector2(sort.x - 1 * space.x, sort.y - 1 * space.y);
        rootGameObject = new GameObject("coke");
        Transform rooTransform = rootGameObject.transform;

        GameObject gameObject = new GameObject("floor");
        gameObject.transform.SetParent(rooTransform);
        for (int index = 0; index < floor; index++)
        {
            CreateCube(gameObject.transform, cakeMaterial, new Vector3(0, floorHight * index),
                new Vector3(cakeSize.x, 1, cakeSize.y));
        }

        gameObject = new GameObject("pillar");
        Vector2 size = cellSize + space;
        for (int index = 0; index < floor - 1; index++)
        {
            Vector3 initPos = new Vector3((-(cakeSize.x - side.x) + cellSize.x) / 2, floorHight * index);
            int initIndex = (int) (sort.x * sort.y) * index;
            for (int h = 0; h < sort.y; h++)
            {
                for (int w = 0; w < sort.x; w++)
                {
                    List<MeshRenderer> meshRendererList = new List<MeshRenderer>();
                    if (index == 0)
                    {
                        CreateCube(gameObject.transform, pillarEmptyMaterial,
                            initPos + new Vector3(size.x * w, 0.1f, size.y * h), Vector3.one, meshRendererList);
                    }

                    CreateCube(gameObject.transform, pillarEmptyMaterial,
                        initPos + new Vector3(size.x * w, floorHight - 0.1f, size.y * h), Vector3.one,
                        meshRendererList);
                    pillarMaterialList.Add(initIndex++, meshRendererList);
                }
            }
        }
    }

    private void UpdateTargetPillar(int floor, int x, int y, PillarType pillarType)
    {
        int index = (int) (sort.x * sort.y) * floor + (int) (y * sort.x) + x;
        List<MeshRenderer> meshRenderers = pillarMaterialList[index];
        int count = meshRenderers.Count;

        if (pillarType == PillarType.empty && ((floor == 0 && count == 3) || (floor != 0 && count == 2)))
        {
            meshRenderers[count - 1].gameObject.SetActive(false);
        }

        if (pillarType != PillarType.empty)
        {
            if (((floor == 0 && count == 2) || (floor != 0 && count == 1)))
            {
                CreateCube(meshRenderers[0].transform.parent, pillarEmptyMaterial,
                    meshRenderers[count - 1].transform.localPosition + new Vector3(0, floorHight / 2, 0),
                    new Vector3(1, floorHight, 1));
            }
            else
            {
                meshRenderers[count - 1].gameObject.SetActive(true);
            }
        }

        meshRenderers.ForEach(material =>
        {
            material.material = pillarType == PillarType.empty ? pillarEmptyMaterial :
                pillarType == PillarType.toothpaste ? toothpasteMaterial : waterMaterial;
        });
    }

    private void CreateCube(Transform parent, Material material, Vector3 pos, Vector3 scale,
        List<MeshRenderer> meshRendererList = null)
    {
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.transform.SetParent(parent);
        MeshRenderer meshRenderer = cube.GetComponent<MeshRenderer>();
        meshRenderer.material = material;
        cube.transform.localPosition = pos;
        cube.transform.localScale = scale;
        if (meshRendererList != null)
        {
            meshRendererList.Add(meshRenderer);
        }
    }
}
