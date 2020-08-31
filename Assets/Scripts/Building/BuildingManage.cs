using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum BuildingType
{
    技术部门, 市场部门, 产品部门, 运营部门, 高管办公室, 研发部门, 人力资源部A, 
    心理咨询室, 按摩房, 健身房, 目标修正小组, 档案管理室, 效能研究室,
    财务部, 战略咨询部B, 精确标准委员会, 高级财务部A, 高级财务部B, 人力资源部B
}

public class BuildingManage : MonoBehaviour
{
    public Building BuildingPrefab, SelectBuilding;
    public Button ConfirmButton;
    public BuildingEffect BEPrefab;
    public GameControl GC;
    public Transform BuildingContent, EntityContent, ExitPos, MaxPos, MinPos;
    public GameObject ControlPanel;

    public List<Building> ConstructedBuildings = new List<Building>();


    public void CreateOffice(int num)
    {
        //如果之前的建筑没有成功建造就删除
        if (SelectBuilding != null)
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
        for (int i = 0; i < ConstructedBuildings.Count; i++)
        {
            if (ConstructedBuildings[i].Type == type)
                DepNum += 1;
        }
        SelectBuilding.Text_DepName.text = SelectBuilding.Type.ToString() + DepNum;

        if (type == BuildingType.技术部门 || type == BuildingType.市场部门 || type == BuildingType.产品部门 || type == BuildingType.运营部门)
            SelectBuilding.SetSize(2, 3);
        else if (type == BuildingType.高管办公室 || type == BuildingType.人力资源部A || type == BuildingType.人力资源部B  || type == BuildingType.按摩房)
            SelectBuilding.SetSize(2, 2);
        else if (type == BuildingType.研发部门)
            SelectBuilding.SetSize(2, 4);
        else if (type == BuildingType.健身房)
            SelectBuilding.SetSize(3, 3);
        else if (type == BuildingType.目标修正小组 || type == BuildingType.档案管理室 || type == BuildingType.效能研究室
            || type == BuildingType.财务部 || type == BuildingType.战略咨询部B || type == BuildingType.精确标准委员会
            || type == BuildingType.高级财务部A || type == BuildingType.高级财务部B)
            SelectBuilding.SetSize(2, 3);

    }

    public void BuildingConfirm()
    {
        if (GC.Money >= 100)
        {
            GC.Money -= 100;
            ControlPanel.SetActive(false);

            BuildingType T = SelectBuilding.Type;
            if (T == BuildingType.技术部门 || T == BuildingType.市场部门 || T == BuildingType.产品部门 || T == BuildingType.运营部门)
            {
                SelectBuilding.Department = GC.CreateDep((int)T + 1);
                SelectBuilding.Department.building = SelectBuilding;
            }
            else if (T == BuildingType.高管办公室)
            {
                SelectBuilding.Office = GC.CreateOffice(SelectBuilding);
                SelectBuilding.effectValue = 8;
            }
            else if (T == BuildingType.研发部门)
            {
                SelectBuilding.Department = GC.CreateDep(4);
                SelectBuilding.Department.building = SelectBuilding;
            }
            else if(T == BuildingType.人力资源部A)
            {
                SelectBuilding.Office = GC.CreateOffice(SelectBuilding);
                SelectBuilding.effectValue = 1;
                SelectBuilding.StaminaRequest = 10;
            }
            else if (T == BuildingType.按摩房 || T == BuildingType.健身房)
            {
                SelectBuilding.Office = GC.CreateOffice(SelectBuilding);
                SelectBuilding.effectValue = 3;
                if(T == BuildingType.健身房)
                    SelectBuilding.StaminaRequest = 10;
            }
            else if (T == BuildingType.战略咨询部B || T == BuildingType.精确标准委员会)
            {
                SelectBuilding.Office = GC.CreateOffice(SelectBuilding);
                SelectBuilding.effectValue = 5;
                SelectBuilding.StaminaRequest = 20;
            }
            else if (T == BuildingType.目标修正小组)
            {
                SelectBuilding.Office = GC.CreateOffice(SelectBuilding);
                SelectBuilding.effectValue = 6;
                SelectBuilding.StaminaRequest = 20;
            }
            else if (T == BuildingType.财务部 || T == BuildingType.高级财务部A || T == BuildingType.高级财务部B)
            {
                SelectBuilding.Office = GC.CreateOffice(SelectBuilding);
                SelectBuilding.effectValue = 7;
                SelectBuilding.StaminaRequest = 10;
            }
            else if (T == BuildingType.档案管理室 || T == BuildingType.效能研究室)
            {
                SelectBuilding.Office = GC.CreateOffice(SelectBuilding);
                SelectBuilding.effectValue = 8;
                SelectBuilding.StaminaRequest = 10;
                if(T == BuildingType.效能研究室)
                    SelectBuilding.StaminaRequest = 20;
            }
            else if (T == BuildingType.人力资源部B)
            {
                SelectBuilding.Department = GC.CreateDep(5);
                SelectBuilding.Department.building = SelectBuilding;
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
