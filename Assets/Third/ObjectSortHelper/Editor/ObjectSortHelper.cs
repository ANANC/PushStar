using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ObjectSortHelper : EditorWindow
{
    private static class config
    {
        public static string cakeMaterialName = "Assets/Material/cake.mat";
        public static string pillarEmptyMaterialName = "Assets/Material/pillarEmpty.mat";
        public static string playerMaterialName = "Assets/Material/player.mat";
        public static string toothpasteMaterialName = "Material/toothpaste.mat";
        public static string waterMaterialName = "Assets/Material/water.mat";
    }

    public enum PillarType
    {
        empty,
        water,
        toothpaste
    }

    private bool init = false;

    private Material cakeMaterial;
    private Material pillarEmptyMaterial;
    private Material playerMaterial;
    private Material toothpasteMaterial;
    private Material waterMaterial;

    private int floor; //层
    private float floorHight; //层高
    private Vector2 cellSize; //格子大小
    private Vector2 sort; //排序
    private Vector2 space; //间隔
    private Vector2 side; //边距
    
    private PillarType[] pillarDatas;
    private Dictionary<int, List<MeshRenderer>> pillarMaterialList;
    private GameObject rootGameObject;

    [MenuItem("Tools/饼干地图编辑器")]
    private static void Open()
    {
        EditorWindow.GetWindow<ObjectSortHelper>();
    }

    private void Init()
    {
        if (init)
        {
            return;
        }
        pillarMaterialList = new Dictionary<int, List<MeshRenderer>>();

        cakeMaterial = AssetDatabase.LoadAssetAtPath<Material>(config.cakeMaterialName);
        pillarEmptyMaterial = AssetDatabase.LoadAssetAtPath<Material>(config.pillarEmptyMaterialName);
        playerMaterial = AssetDatabase.LoadAssetAtPath<Material>(config.playerMaterialName);
        toothpasteMaterial = AssetDatabase.LoadAssetAtPath<Material>(config.toothpasteMaterialName);
        waterMaterial = AssetDatabase.LoadAssetAtPath<Material>(config.waterMaterialName);

        init = true;
    }

    private void OnDestroy()
    {
        init = false;
    }

    private void OnEnable()
    {
        Init();
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("层");
        EditorGUILayout.BeginHorizontal();
        floor = EditorGUILayout.IntField("层数：", floor);
        floorHight = EditorGUILayout.FloatField("层高：", floorHight);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        EditorGUILayout.LabelField("排序");
        EditorGUILayout.BeginHorizontal();
        cellSize = EditorGUILayout.Vector2Field("宽高：", cellSize);
        sort = EditorGUILayout.Vector2Field("行列：", sort);
        space = EditorGUILayout.Vector2Field("间距：", space);
        side = EditorGUILayout.Vector2Field("边距：", side);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        if (GUILayout.Button("build"))
        {
            ReBuild();
        }

        if (rootGameObject == null)
        {
            return;
        }

        EditorGUILayout.LabelField("地形");
        for (int index = 0,id = 0; index < floor - 1; index++)
        {
            for (int h = 0; h < sort.y; h++)
            {
                EditorGUILayout.BeginHorizontal();

                for (int w = 0; w < sort.x; w++, id++)
                {
                    PillarType curPillar = (PillarType)EditorGUILayout.EnumFlagsField(pillarDatas[id]);
                    if (curPillar != pillarDatas[id])
                    {
                        UpdateTargetPillar(id, curPillar);
                    }
                }

                EditorGUILayout.EndHorizontal();

            }

            EditorGUILayout.Space();
        }
    }


    private void ReBuild()
    {
        if (pillarMaterialList != null)
        {
            pillarMaterialList.Clear();
        }
        pillarMaterialList = new Dictionary<int, List<MeshRenderer>>();

        int count = (int) (sort.x * sort.y) * floor;
        pillarDatas = new PillarType[count];
        for (int index = 0; index < count; index++)
        {
            pillarDatas[index] = PillarType.empty;
        }
        Build();
    }

    private void Build()
    {
        if (rootGameObject != null)
        {
            GameObject.DestroyImmediate(rootGameObject);
            rootGameObject = null;
        }

        Vector2 cakeSize = side * 2 + cellSize * sort + new Vector2((sort.x - 1) * space.x, (sort.y - 1) * space.y);
        rootGameObject = new GameObject("coke");
        Transform rooTransform = rootGameObject.transform;

        GameObject gameObject = new GameObject("floor");
        gameObject.transform.SetParent(rooTransform);
        for (int index = 0; index < floor; index++)
        {
            CreateCube(gameObject.transform, cakeMaterial, new Vector3(0, -floorHight * index, 0),
                new Vector3(cakeSize.x, 1, cakeSize.y));
        }

        gameObject = new GameObject("pillar");
        gameObject.transform.SetParent(rooTransform);

        Vector2 size = cellSize + space;
        for (int index = 0, id = 0; index < floor - 1; index++)
        {
            Vector3 initPos = cakeSize / 2 - side;
            for (int h = 0; h < sort.y; h++)
            {
                for (int w = 0; w < sort.x; w++, id++)
                {
                    List<MeshRenderer> meshRendererList = new List<MeshRenderer>();
                    if (index == 0)
                    {
                        CreateCube(gameObject.transform, pillarEmptyMaterial,
                            new Vector3(initPos.x - (side.x + size.x) * w, -floorHight * index + 0.1f, initPos.y - (side.y + size.y) * h),
                            new Vector3(size.x, 1, size.y),
                            meshRendererList);
                    }

                    CreateCube(gameObject.transform, pillarEmptyMaterial,
                        new Vector3(initPos.x - (side.x+ size.x) * w, -floorHight * index + 0.1f, initPos.y - (side.y +size.y)* h),
                        new Vector3(size.x, 1, size.y),
                        meshRendererList);

                    UpdateMeshRenderers(pillarDatas[id], meshRendererList);
                    pillarMaterialList.Add(id, meshRendererList);
                }
            }
        }
    }



    private void UpdateTargetPillar(int id, PillarType pillarType)
    {
        pillarDatas[id] = pillarType;
        List<MeshRenderer> meshRenderers = pillarMaterialList[id];
        UpdateMeshRenderers(pillarType, meshRenderers);
    }

    private void UpdateMeshRenderers(PillarType pillarType,List<MeshRenderer> meshRenderers)
    {
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
