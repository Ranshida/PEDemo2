using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// 相机控制器
/// 更新时间2020.9.7
/// </summary>
public class CameraController : MonoBehaviour
{
    public static CameraController Instance { get; private set; }
    private GameObject focus;   //聚焦中心物体，直接控制其位移
    
    [Tooltip("最大视野")]public float maxHeight = 80;
    [Tooltip("最低视野")]public float minHeight = 140;
    [Tooltip("高度平滑度")] [SerializeField] private float m_HeightSmooth = 10;
    [Tooltip("移动平滑度")] [SerializeField] private float m_SpeedSmooth = 1;
    [HideInInspector]public float height;    //高度参数
    private Transform mainCamera;

    public static bool IsPointingUI { get { return EventSystem.current.IsPointerOverGameObject(); } }
    private Ray RayTerrain; 
    public static bool TerrainHit { get; private set; }
    public static bool BuildingHit { get; private set; }
    public static bool ItemHit { get; private set; }
    public static bool CharacterHit { get; private set; }
    public static RaycastHit TerrainRaycast;      //与地面的碰撞
    public static RaycastHit BuildingRaycast;      //与地面的碰撞
    public static RaycastHit CharacterRaycast;   //与人物的碰撞
    public static RaycastHit ItemRaycast;   //与人物的碰撞


    private void Awake()
    {
        mainCamera = transform.Find("Main Camera");
        focus = Instantiate(ResourcesLoader.LoadPrefab("Prefabs/Scene/FucosGo"));
        focus.transform.position = new Vector3(75, 0, 115);
        height = minHeight;
    }

    private void Update()
    {
        focus.transform.position += new Vector3(Input.GetAxis("Horizontal") * m_SpeedSmooth, 0, Input.GetAxis("Vertical") * m_SpeedSmooth);
        if (focus.transform.position.x < GridContainer.Instance.MinX)
            focus.transform.position = new Vector3(GridContainer.Instance.MinX, focus.transform.position.y, focus.transform.position.z);
        if (focus.transform.position.x > GridContainer.Instance.MaxX)
            focus.transform.position = new Vector3(GridContainer.Instance.MaxX, focus.transform.position.y, focus.transform.position.z);
        if (focus.transform.position.z < GridContainer.Instance.MinZ)
            focus.transform.position = new Vector3(focus.transform.position.x, focus.transform.position.y, GridContainer.Instance.MinZ);
        if (focus.transform.position.z > GridContainer.Instance.MaxZ)
            focus.transform.position = new Vector3(focus.transform.position.x, focus.transform.position.y, GridContainer.Instance.MaxZ);

        if (Input.GetAxis("Mouse ScrollWheel") < 0) 
            height += 20;
        if (Input.GetAxis("Mouse ScrollWheel") > 0)
            height -= 20;
        height = Mathf.Clamp(height, minHeight, maxHeight);

        //检查鼠标位置及所属网格
        RayTerrain = Camera.main.ScreenPointToRay(Input.mousePosition);
        TerrainHit = Physics.Raycast(RayTerrain, out TerrainRaycast, 1000, 1 << 8);

        ItemHit = false;
        BuildingHit = false;
        CharacterHit = false;
        ItemHit = Physics.Raycast(RayTerrain, out ItemRaycast, 1000, 1 << 10);
        if (!ItemHit)
        {
            CharacterHit = Physics.Raycast(RayTerrain, out CharacterRaycast, 1000, 1 << 9);
            if (!CharacterHit)
            {
                BuildingHit = Physics.Raycast(RayTerrain, out BuildingRaycast, 1000, 1 << 8);
            }
        }
    }

    private float m_TempHeight;   //临时高度变量，像height靠拢
    private void LateUpdate()
    {
        //相机处理在LateUpdate进行
        m_TempHeight = Mathf.Lerp(m_TempHeight, height, Time.deltaTime * m_HeightSmooth);
        transform.position = focus.transform.position + Vector3.up * m_TempHeight;
        mainCamera.localPosition = new Vector3(0, 0, -m_TempHeight);                                                      
    }
}
