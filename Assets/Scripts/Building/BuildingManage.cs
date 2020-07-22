using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum BuildingType
{
    技术部门, 市场部门, 产品部门, 运营部门, 高管办公司
}

public class BuildingManage : MonoBehaviour
{
    public Building BuildingPrefab, SelectBuilding;
    public Button ConfirmButton;
    public BuildingEffect BEPrefab;
    public GameControl GC;
    public Transform BuildingContent;
    public GameObject ControlPanel;

    public List<Building> ConstructedBuildings = new List<Building>();


    public void CreateOffice(int num)
    {
        //如果之前的建筑没有成功建造就删除
        if(SelectBuilding != null)
        {
            Destroy(SelectBuilding.effect.gameObject);
            Destroy(SelectBuilding.gameObject);
        }

        ToggleEffectRange();
        BuildingType type = (BuildingType)num;
        SelectBuilding = Instantiate(BuildingPrefab, BuildingContent);
        SelectBuilding.BM = this;
        SelectBuilding.Type = type; //枚举第一位是0
        SelectBuilding.effect = Instantiate(BEPrefab, BuildingContent);
        SelectBuilding.effect.CurrentBuilding = SelectBuilding;

        ControlPanel.transform.position = SelectBuilding.transform.position;
        ControlPanel.SetActive(true);
        int DepNum = 1;
        for(int i = 0; i < ConstructedBuildings.Count; i++)
        {
            if (ConstructedBuildings[i].Type == type)
                DepNum += 1;
        }
        SelectBuilding.Text_DepName.text = SelectBuilding.Type.ToString() + DepNum;

        if (type == BuildingType.技术部门 || type == BuildingType.市场部门 || type == BuildingType.产品部门 || type == BuildingType.运营部门)
            SelectBuilding.SetSize(2, 3);
        else if(type == BuildingType.高管办公司)
            SelectBuilding.SetSize(2, 2);

    }

    public void BuildingConfirm()
    {
        if (GC.Stamina > 30)
        {
            GC.Stamina -= 30;
            ControlPanel.SetActive(false);

            BuildingType T = SelectBuilding.Type;
            if (T == BuildingType.技术部门 || T == BuildingType.市场部门 || T == BuildingType.产品部门 || T == BuildingType.运营部门)
            {
                SelectBuilding.Department = GC.CreateDep((int)T + 1);
                SelectBuilding.Department.building = SelectBuilding;
            }
            else if (T == BuildingType.高管办公司)
            {
                SelectBuilding.Office = GC.CreateOffice();
                SelectBuilding.Office.building = SelectBuilding;
            }

            SelectBuilding.BuildingSet = true;

            //周围建筑对自身造成影响 
            for (int i = 0; i < SelectBuilding.EffectBuildings.Count; i++)
            {
                SelectBuilding.EffectBuildings[i].Affect();
            }
            //对自身周围建筑造成影响
            SelectBuilding.effect.Affect();

            ConstructedBuildings.Add(SelectBuilding);
            ToggleEffectRange();
            SelectBuilding = null;
        }
    }
    public void BuildingCancel()
    {
        ControlPanel.SetActive(false);
        ToggleEffectRange();
        Destroy(SelectBuilding.effect.gameObject);
        Destroy(SelectBuilding.gameObject);
        SelectBuilding = null;
    }
    void ToggleEffectRange()
    {
        for(int i = 0; i < ConstructedBuildings.Count; i++)
        {
            if (ConstructedBuildings[i].effect.image.enabled == true)
                ConstructedBuildings[i].effect.image.enabled = false;
            else
                ConstructedBuildings[i].effect.image.enabled = true;
        }
    }

    public void RotateBuilding()
    {
        SelectBuilding.Rotate();
        ControlPanel.transform.position = SelectBuilding.transform.position;
    }


}
