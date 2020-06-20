using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDragPage
{
    void BeginDragPage(Vector3 point);
    void DraggingPage();
    void EndDragPage(Action complete);
}

//拖拽左右页面的基类
public abstract class DragPageBase
{
    private UIBook _book;
    protected BookModel _model;
    private TheDraggingPage _frontPage;
    private TheDraggingPage _backPage;
    private Vector3 _startPos;
    private RectTransform _clippingMask;
    /// <summary>
    /// 构造初始化字段
    /// </summary>
    /// <param name="book"></param>
    /// <param name="model"></param>
    /// <param name="frontPage"></param>
    /// <param name="backPage"></param>
    /// <param name="startPos"></param>
    public DragPageBase(UIBook book, BookModel model, TheDraggingPage frontPage, TheDraggingPage backPage,
        Vector3 startPos)
    {
        _book = book;
        _model = model;
        _frontPage = frontPage;
        _backPage = backPage;
        _startPos = startPos;
        _clippingMask = book.GetClippingMask();
    }
    /// <summary>
    /// 开始拖拽
    /// </summary>
    /// <param name="point"></param>
    public void BeginDragPage(Vector3 point)
    {
        _clippingMask.pivot = GetClippingMaskPivot();

        _model.ClickPoint = point;

        _frontPage.BeginDragPage(_startPos, GetPagePivot());
        _backPage.BeginDragPage(_startPos, GetPagePivot());
    }

    //拖拽左右页面第三个不同点：剪切遮罩的轴心点不同
    protected abstract Vector2 GetClippingMaskPivot();
    //拖拽左右页面第一个不同点：轴心点的位置
    protected abstract Vector2 GetPagePivot();

    /// <summary>
    /// 拖拽中
    /// </summary>
    public void DraggingPage()
    {

        _model.ClickPoint = _book.GetClickPos();

        _backPage.SetParent(_clippingMask, true);
        _frontPage.SetParent(_book.transform, true);

        _model.CurrentPageCorner = _book.CalculateDraggingCorner(_model.ClickPoint);

        Vector3 bottomCrossPoint = UpdateClippingMask();
        UpdateBackSide(bottomCrossPoint);

        //在其他东西都准备好后将拖拽页前面设置到_clippingMask下并将其设为第一项
        _frontPage.SetParent(_clippingMask, true);
        _frontPage.ResetShadowData();
        _frontPage.transform.SetAsFirstSibling();

        //使阴影跟随clippingMask
        _backPage.SetShadowFollow(_clippingMask);
    }

    /// <summary>
    /// 更新剪切遮罩的角度和位置,并返回与书下边交点位置
    /// </summary>
    private Vector3 UpdateClippingMask()
    {
        Vector3 bottomCrossPoint;
        float angle = _book.CalculateFoldAngle(_model.CurrentPageCorner,
            GetBookCorner(), out bottomCrossPoint);
        if (angle > 0)
        {
            angle = angle - 90;
        }
        else
        {
            angle = angle + 90;
        }

        _clippingMask.eulerAngles = angle * Vector3.forward;
        _clippingMask.localPosition = bottomCrossPoint;

        return bottomCrossPoint;
    }

    protected abstract Vector3 GetBookCorner();

    /// <summary>
    /// 更新拖拽页背面的角度和位置
    /// </summary>
    /// <param name="bottomCrossPoint"></param>
    private void UpdateBackSide(Vector3 bottomCrossPoint)
    {
        _backPage.transform.position = _book.Local2WorldPos(_model.CurrentPageCorner);
        Vector3 offset = bottomCrossPoint - _model.CurrentPageCorner;
        float angle = Mathf.Atan2(offset.y, offset.x) * Mathf.Rad2Deg;
        _backPage.transform.eulerAngles = GetValidAngle(angle);
    }

    //拖拽左右页面第二个不同点：背面的旋转角度不同
    protected abstract Vector3 GetValidAngle(float angle);

    public void EndDragPage(Action complete)
    {
        Vector3 corner;
        if(_model.ClickPoint.x > _model.BottomCenter.x)
        {
            corner = _model.RightCorner;
        }
        else
        {
            corner = _model.LeftCorner;
        }
        _book.FlipAni(corner, complete);
    }
}