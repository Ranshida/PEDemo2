using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Building : MonoBehaviour
{
    public bool BuildingSet = false;
    public BuildingType Type;

    public DepControl Department;
    public OfficeControl Office;
    public RectTransform Rt;
    public BuildingEffect effect;
    public BuildingManage BM;
    public Text Text_DepName;

    public List<BuildingEffect> EffectBuildings = new List<BuildingEffect>();

    int XPos, YPos;
    bool Rotated = false;

    Vector3 Offset;


    private void Start()
    {
        Rt = this.gameObject.GetComponent<RectTransform>();
    }

    public void SetSize(int x, int y)
    {
        Rt.localScale = new Vector3(x, y);
        effect.Rt.localScale = new Vector3(x + 4, y + 4);
        effect.transform.parent = this.transform;
        effect.Rt.anchoredPosition = new Vector2(0, 0);
    }

    public void DragStart()
    {
        if (BuildingSet == false)
        {
            Offset = transform.position - Input.mousePosition;
            BM.ControlPanel.SetActive(false);
        }
    }

    public void Drag()
    {
        if (BuildingSet == false)
        {
            transform.position = Input.mousePosition + Offset;
            CheckPos();
        }
    }

    public void DragEnd()
    {
        if (BuildingSet == false)
        {
            BM.ControlPanel.transform.position = transform.position;
            BM.ControlPanel.SetActive(true);
        }
    }

    public void Rotate()
    {
        if (Rotated == false)
        {
            Rt.rotation = Quaternion.Euler(new Vector3(0, 0, -90));
            Rt.pivot = new Vector2(1, 0);
            this.gameObject.GetComponent<BoxCollider2D>().offset *= new Vector2(-1, 1);
            Text_DepName.gameObject.GetComponent<RectTransform>().rotation = Quaternion.Euler(new Vector3(0, 0, 0));
            Rotated = true;
        }
        else
        {
            Rt.pivot = new Vector2(0, 0);
            Rt.rotation = Quaternion.Euler(new Vector3(0, 0, 0));
            this.gameObject.GetComponent<BoxCollider2D>().offset *= new Vector2(-1, 1);
            Text_DepName.gameObject.GetComponent<RectTransform>().rotation = Quaternion.Euler(new Vector3(0, 0, 0));
            Rotated = false;
        }
        CheckPos();
    }
    void CheckPos()
    {
        if (Rotated == false)
        {
            if (Rt.anchoredPosition.x < 0)
                Rt.anchoredPosition = new Vector2(0, Rt.anchoredPosition.y);
            else if (Rt.anchoredPosition.x > 1120 - Rt.localScale.x * 80)
                Rt.anchoredPosition = new Vector2(1120 - Rt.localScale.x * 80, Rt.anchoredPosition.y);
            if (Rt.anchoredPosition.y < 0)
                Rt.anchoredPosition = new Vector2(Rt.anchoredPosition.x, 0);
            else if (Rt.anchoredPosition.y > 800 - Rt.localScale.y * 80)
                Rt.anchoredPosition = new Vector2(Rt.anchoredPosition.x, 800 - Rt.localScale.y * 80);
        }
        else
        {
            if (Rt.anchoredPosition.x < 0)
                Rt.anchoredPosition = new Vector2(0, Rt.anchoredPosition.y);
            else if (Rt.anchoredPosition.x > 1120 - Rt.localScale.y * 80)
                Rt.anchoredPosition = new Vector2(1120 - Rt.localScale.y * 80, Rt.anchoredPosition.y);
            if (Rt.anchoredPosition.y < 0)
                Rt.anchoredPosition = new Vector2(Rt.anchoredPosition.x, 0);
            else if (Rt.anchoredPosition.y > 800 - Rt.localScale.x * 80)
                Rt.anchoredPosition = new Vector2(Rt.anchoredPosition.x, 800 - Rt.localScale.x * 80);
        }

        XPos = (int)(Rt.anchoredPosition.x / 80);
        YPos = (int)(Rt.anchoredPosition.y / 80);

        Rt.anchoredPosition = new Vector2(XPos * 80, YPos * 80);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.tag == "BuildingEffect" && BuildingSet == false && collision.gameObject != effect.gameObject)
        {
            EffectBuildings.Add(collision.gameObject.GetComponent<BuildingEffect>());
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Building" || collision.gameObject.tag == "BuildingBorder")
        {
            BM.ConfirmButton.interactable = false;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Building" || collision.gameObject.tag == "BuildingBorder")
            BM.ConfirmButton.interactable = true;

        if(collision.gameObject.tag == "BuildingEffect" && BuildingSet == false && collision.gameObject != effect.gameObject)
        {
            EffectBuildings.Remove(collision.gameObject.GetComponent<BuildingEffect>());
        }
    }
}
