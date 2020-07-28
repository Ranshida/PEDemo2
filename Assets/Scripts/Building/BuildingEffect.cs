using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BuildingEffect : MonoBehaviour
{
    public RectTransform Rt;
    public Building CurrentBuilding;
    public Image image;

    List<Building> AffectedBuildings = new List<Building>();
    List<Building> TargetBuildings = new List<Building>();

    public void Affect()
    {
        print("Affect");
        for (int i = 0; i < TargetBuildings.Count; i++)
        {
            BuildingType T = CurrentBuilding.Type;
            BuildingType T2 = TargetBuildings[i].Type;
            if (T == BuildingType.技术部门 || T == BuildingType.市场部门 || T == BuildingType.产品部门 || T == BuildingType.运营部门)
            {
                if ((T2 == BuildingType.技术部门 || T2 == BuildingType.市场部门 || T2 == BuildingType.产品部门
                    || T2 == BuildingType.运营部门) && T != T2)
                {
                    TargetBuildings[i].Department.Efficiency += 0.1f;
                    AffectedBuildings.Add(TargetBuildings[i]);
                }
            }
            else if(T == BuildingType.高管办公司)
            {
                if (T2 == BuildingType.技术部门 || T2 == BuildingType.市场部门 || T2 == BuildingType.产品部门
                    || T2 == BuildingType.运营部门 || T2 == BuildingType.研发部门)
                {
                    TargetBuildings[i].Department.InRangeOffices.Add(CurrentBuilding.Office);
                }
            }

        }
        TargetBuildings.Clear();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.tag == "Building")
        {
            Building b = collision.GetComponent<Building>();
            if (b != CurrentBuilding)
                TargetBuildings.Add(b);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Building")
        {
            TargetBuildings.Remove(collision.GetComponent<Building>());
        }
    }
}
