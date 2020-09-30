using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 员工类的临时管理器
/// </summary>
public class EmpManager : MonoBehaviour
{
    public static EmpManager Instance { get; private set; }
    private GameObject empPrefabs;
    private Material[] empMaterials;

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
        if (Input.GetKeyDown(KeyCode.D))
        {
            CreateEmp(BuildingManage.Instance.AimingPosition);
        }
    }

    public EmpEntity CreateEmp(Vector3 position)
    {
        EmpEntity emp = GameObject.Instantiate(empPrefabs, position, Quaternion.identity).GetComponentInChildren<EmpEntity>();
        emp.Init();
        emp.Renderer.material = empMaterials[Random.Range(0, empMaterials.Length)];
        return emp;
    }
}
