using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// 员工类的临时管理器
/// </summary>
public class EmpManager : MonoBehaviour
{
    public static EmpManager Instance { get; private set; }
    private GameObject empPrefabs;
    private Material[] empMaterials;
    private EmpEntity pointEmp;

    private void Awake()
    {
        Instance = this;
        empPrefabs = ResourcesLoader.LoadPrefab("Prefabs/Employee/Emp");
        empMaterials = ResourcesLoader.LoadAll<Material>("Image/Employee/Materials");
    }

    private void Start()
    {
    
    }

    private void Update()
    {
        pointEmp = null;

        if (CameraController.CharacterHit && !CameraController.IsPointingUI)
        {
            pointEmp = CameraController.CharacterRaycast.collider.transform.parent.parent.GetComponentInChildren<EmpEntity>();
            DynamicWindow.Instance.SetEmpName(pointEmp.EmpName, pointEmp.transform, Vector3.up * 10);
        }
        if (pointEmp && Input.GetMouseButtonDown(1))
            pointEmp.ShowDetailPanel();
    }

    public Employee FindBoss()
    {
        return null;
    }

    public List<Employee> FindColleague()
    {
        return null;
    }


    public EmpEntity CreateEmp(Vector3 position)
    {
        EmpEntity emp = GameObject.Instantiate(empPrefabs, position, Quaternion.identity).GetComponentInChildren<EmpEntity>();
        emp.Init();
        emp.Renderer.material = empMaterials[Random.Range(0, empMaterials.Length)];
        return emp;
    }

    public EmpEntity RandomEventTarget(Employee self)
    {
        List<Event> eventList = RandomEventList(out int index);


        return null;
    }

    private List<Event> RandomEventList(out int listIndex)
    {
        float value = Random.Range(0f, 1f);
        if (value < 0.3f) 
        {
            listIndex = 0;
            return EventData.InitialList;
        }
        if (value < 0.6f)
        {
            listIndex = 1;
            return EventData.CompanyList;
        }
        listIndex = 2;
        return EventData.RelationList;
    }
}
