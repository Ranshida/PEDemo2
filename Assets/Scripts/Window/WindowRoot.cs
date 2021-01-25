using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// UI面板基类
/// </summary>
public abstract class WindowRoot : MonoBehaviour
{
    protected Transform thisTrans;

    public Button[] buttons;      //全部子物体按钮
    public Slider[] sliders;      //全部子物体滑动条

    //快速设置组件颜色
    protected virtual Color selectColor_Img { get; set; } = new Color(0.8f, 0.55f, 0.3f);
    protected virtual Color selectColor_txt { get; set; } = new Color(0.2f, 0.2f, 0.2f);
    protected virtual Color unselectColor_Img { get; set; } = Color.white;
    protected virtual Color unselectColor_Txt { get; set; } = Color.white;
    protected virtual Color pressColor { get; set; } = new Color(1f, 0.75f, 0.4f, 1f);
    protected virtual Color liftupColor { get; set; } = new Color(0.40f, 0.4f, 0.4f, 0.5f);

    protected virtual void Update()
    {
        //每帧注册监听事件。如果不需要，需要重写Update
        SetUpAllButton(this.gameObject);
        SetUpAllSlider(this.gameObject);
    }


    /// <summary>
    /// 每帧为子物体所有Button组件添加监听
    /// </summary>
    /// <param name="go">自身</param>
    protected void SetUpAllButton(GameObject go)
    {
        foreach (Button btn in buttons)
        {
            SetUpButton(btn);
        }
    }

    protected virtual void SetUpButton(Button button)
    {
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => OnButton(button.gameObject));
    }

    protected void OnButton(GameObject btnGo)
    {
        Debug.Log(btnGo.name);
        OnButton(btnGo.name);
    }

    protected virtual void OnButton(string btnName) { }

    /// <summary>
    /// 为子物体所有Slider组件添加监听
    /// </summary>
    /// <param name="go">自身</param>
    protected void SetUpAllSlider(GameObject go)
    {
        foreach (Slider slider in sliders)
        {
            SetUpSlider(slider);
        }
    }

    protected void SetUpSlider(Slider slider)
    {
        slider.onValueChanged.RemoveAllListeners();
        slider.onValueChanged.AddListener((float value) => OnSlider(slider.gameObject, value));
    }

    protected void OnSlider(GameObject sliderGo, float value)
    {
        OnSlider(sliderGo.name, value);
    }

    protected virtual void OnSlider(string sliderName, float value)
    {
       
    }
    
    /// <summary>
    /// 开关面板
    /// </summary>
    /// <param name="isActive"></param>
    public virtual void SetWndState(bool isActive = true)
    {
        if (gameObject.activeSelf != isActive)
        {
            SetActive(gameObject, isActive);
        }
        if (isActive)
        {
            InitWnd();
        }
        else
        {
            ClearWnd();
        }
    }

    public bool GetWndState()
    {
        return gameObject.activeSelf;
    }

    /// <summary>
    /// 每次激活面板进行初始化
    /// </summary>
    protected void InitWnd()
    {
        thisTrans = this.transform;
        buttons = gameObject.GetComponentsInChildren<Button>(true);
        sliders = gameObject.GetComponentsInChildren<Slider>(true);
        InitSpecific();
    }
    protected virtual void InitSpecific() { }

    /// <summary>
    /// 每次关闭面板进行清理
    /// </summary>
    private void ClearWnd()
    {
        ClearWindowSpecific();
    }
    protected virtual void ClearWindowSpecific() { }
    
    #region Tool Functions

    protected void SetActive(GameObject go, bool isActive = true)
    {
        go.SetActive(isActive);
    }
    protected void SetActive(Transform trans, bool state = true)
    {
        trans.gameObject.SetActive(state);
    }
    protected void SetActive(RectTransform rectTrans, bool state = true)
    {
        rectTrans.gameObject.SetActive(state);
    }
    protected void SetActive(Image img, bool state = true)
    {
        img.transform.gameObject.SetActive(state);
    }
    protected void SetActive(Text txt, bool state = true)
    {
        txt.transform.gameObject.SetActive(state);
    }

    protected void SetText(Text txt, string context = "")
    {
        txt.text = context;
    }
    protected void SetText(Transform trans, int num = 0)
    {
        SetText(trans.GetComponent<Text>(), num);
    }
    protected void SetText(Transform trans, string context = "")
    {
        SetText(trans.GetComponent<Text>(), context);
    }
    protected void SetText(Text txt, int num = 0)
    {
        SetText(txt, num.ToString());
    }

    protected void SetColor(Image img,Color col)
    {
        img.color = col;
    }
    protected void SetColor(Text txt, Color col)
    {
        txt.color = col;
    }

    protected T GetOrAddComponent<T>(GameObject go) where T : Component
    {
        T t = go.GetComponent<T>();
        if (t == null)
        {
            t = go.AddComponent<T>();
        }
        return t;
    }

    #endregion

    #region ClickEvts

    public void OnClickDown(GameObject go, Action<PointerEventData> cb)
    {
        Listener listener = GetOrAddComponent<Listener>(go);
        listener.onClickDown = cb;
    }

    public void OnClickUp(GameObject go, Action<PointerEventData> cb)
    {
        Listener listener = GetOrAddComponent<Listener>(go);
        listener.onClickUp = cb;
    }
    public void OnDrag(GameObject go, Action<PointerEventData> cb)
    {
        Listener listener = GetOrAddComponent<Listener>(go);
        listener.onDrag = cb;
    }
    public void OnPointerEnter(GameObject go, Action cb)
    {
        Listener listener = GetOrAddComponent<Listener>(go);
        listener.onPointerEnter = cb;
    }
    public void OnpointerExit(GameObject go, Action cb)
    {
        Listener listener = GetOrAddComponent<Listener>(go);
        listener.onPointerExit = cb;
    }
    public void OnPointing(GameObject go, Action cb)
    {
        Listener listener = GetOrAddComponent<Listener>(go);
        listener.onPointing = cb;
    }

    #endregion

    //当ESC键按下
    public virtual void OnESCKeyUp()
    {

    }

    //反选全部按钮（改变颜色UI）
    protected virtual void UnselectAll(List<Transform> transList)
    {
        foreach (Transform btnTrans in transList)
        {
            btnTrans.GetComponent<Button>().enabled = true;
            btnTrans.GetComponent<Image>().color = unselectColor_Img;
            btnTrans.GetComponentInChildren<Text>().color = unselectColor_Txt;
            btnTrans.GetComponentInChildren<Text>().GetComponent<Outline>().enabled = true;
        }
    }
    //正选一个按钮（改变颜色UI）
    protected virtual void SelectOne(Transform btnTrans)
    {
        btnTrans.GetComponent<Button>().enabled = false;   //这个按钮暂时不需要了
        btnTrans.GetComponent<Image>().color = selectColor_Img;
        btnTrans.GetComponentInChildren<Text>().color = selectColor_txt;
        btnTrans.GetComponentInChildren<Text>().GetComponent<Outline>().enabled = false;
    }
    //反选一个按钮（改变颜色UI）
    protected virtual void UnselectOne(Transform btnTrans)
    {
        btnTrans.GetComponent<Button>().enabled = true;
        btnTrans.GetComponent<Image>().color = unselectColor_Img;
        btnTrans.GetComponentInChildren<Text>().color = unselectColor_Txt;
        btnTrans.GetComponentInChildren<Text>().GetComponent<Outline>().enabled = true;
    }

    //使按键处于按下状态
    protected virtual void PressBtn(Transform btnTrans)
    {
        btnTrans.GetComponent<Image>().color = pressColor;
        btnTrans.GetComponentInChildren<Text>().color = selectColor_txt;
        btnTrans.GetComponentInChildren<Text>().GetComponent<Outline>().enabled = false;
        btnTrans.GetComponent<Outline>().enabled = true;
    }
    //使按键处于松开状态
    protected virtual void LiftupBtn(Transform btnTrans)
    {
        btnTrans.GetComponent<Image>().color = liftupColor;
        btnTrans.GetComponentInChildren<Text>().color = unselectColor_Txt;
        btnTrans.GetComponentInChildren<Text>().GetComponent<Outline>().enabled = true;
        btnTrans.GetComponent<Outline>().enabled = false;
    }
}