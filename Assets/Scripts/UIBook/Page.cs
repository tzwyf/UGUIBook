using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Page : MonoBehaviour
{
    private int _id;
    private Image _image;
    private Func<int, Sprite> _getSprite;
    public Shadow Shadow { get; private set; }
    protected RectTransform _rect;

    public int ID
    {
        get { return _id; }
        set
        {
            _id = value;
            ChangeSprite(value);
        }
    }
    /// <summary>
    /// 初始化
    /// </summary>
    /// <param name="getSprite"></param>
    public virtual void Init(Func<int,Sprite> getSprite)
    {

        _rect = GetComponent<RectTransform>();
        _image = GetComponent<Image>();
        //通过一个委托从外部获取需要修改的图片
        _getSprite = getSprite;

        Shadow = transform.GetChild(0).gameObject.AddComponent<Shadow>();
    }
    /// <summary>
    /// 根据id改变该页图片的显示
    /// </summary>
    /// <param name="id"></param>
    private void ChangeSprite(int id)
    {
        _image.sprite = _getSprite(id);
    }
    /// <summary>
    /// 向外提供的一个接口：控制该页显示或隐藏
    /// </summary>
    /// <param name="isActive"></param>
    public void SetActiveState(bool isActive)
    {
        gameObject.SetActive(isActive);
    }

    public void SetParent(Transform parent, bool isWorldPosStay)
    {
        transform.SetParent(parent,isWorldPosStay);
    }
}
