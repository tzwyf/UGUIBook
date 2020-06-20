using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIBook : MonoBehaviour
{
    public string _bookName;
    private RectTransform _rect;
    private TheDraggingPage _leftSide;
    private TheDraggingPage _rightSide;
    private Page _leftPage;
    private Page _rightPage;
    private Sprite[] _sprites;
    private int _currentLeftId;
    private BookModel _model;
    private RectTransform _clippingMask;
    private DragPageBase _dragPage;
    private float _aniDuration = 0.5f;
    private bool _isDragging;

    public int CurrentLeftId {
        get { return _currentLeftId; }
        set
        {
            _currentLeftId = value;
            //限制_currentLeftId
            if (_currentLeftId < -1)
            {
                _currentLeftId = -1;
            }
            else if(_currentLeftId > _sprites.Length - 1)
            {
                _currentLeftId = _sprites.Length - 1;
            }
        }
    }

    void Start()
    {
        Canvas canvas;
        InitComponent(out canvas);
        InitData(canvas);

        UpdateID();
    }

    /// <summary>
    /// 初始化组件
    /// </summary>
    private void InitComponent(out Canvas canvas)
    {
        _rect = GetComponent<RectTransform>();
        canvas = null;
        foreach (Canvas c in _rect.GetComponentsInParent<Canvas>())
        {
            if (c.isRootCanvas)
            {
                canvas = c;
                break;
            }
        }

        _clippingMask = _rect.Find("ClippingMask").gameObject.GetComponent<RectTransform>();
        _leftSide = _rect.Find("LeftSide").gameObject.AddComponent<TheDraggingPage>();
        _rightSide = _rect.Find("RightSide").gameObject.AddComponent<TheDraggingPage>();
        _leftPage = _rect.Find("LeftPage").gameObject.AddComponent<Page>();
        _rightPage = _rect.Find("RightPage").gameObject.AddComponent<Page>();

        _rect.Find("RightDragButton").gameObject.AddComponent<DragButton>().Init(OnBeginDragRight,OnDragging,OnEndDragRight);
        _rect.Find("LeftDragButton").gameObject.AddComponent<DragButton>().Init(OnBeginDragLeft, OnDragging, OnEndDragLeft);

        _leftSide.Init(GetSprite);
        _rightSide.Init(GetSprite);
        _leftPage.Init(GetSprite);
        _rightPage.Init(GetSprite);
    }


    private Sprite GetSprite(int index)
    {
        if(index >=0 && index < _sprites.Length)
            return _sprites[index];
        return null;
    }
    
    /// <summary>
    /// 初始化数据
    /// </summary>
    /// <param name="canvas"></param>
    private void InitData(Canvas canvas)
    {
        _model = new BookModel();
        _sprites = Resources.LoadAll<Sprite>(_bookName);
        if(_sprites.Length > 0)
        {
            _rect.sizeDelta = new Vector2(_sprites[0].rect.width * 2, _sprites[0].rect.height);
        }
        CurrentLeftId = -1;//左边第一页是空白页
        _isDragging = false;//默认不是拖动状态

        float scaleFactor = 1;
        if (canvas != null)
        {
            scaleFactor = canvas.scaleFactor;
        }
        //计算屏幕上书页的显示尺寸，受画布缩放比例的影响
        float pageWidth = _rect.rect.width * scaleFactor;
        float pageHeight = _rect.rect.height * scaleFactor;
        //获取底边中点、顶边中点、左页左下角顶点、右页右下角顶点
        Vector3 pos = _rect.position + Vector3.down * pageHeight / 2;
        _model.BottomCenter = World2LocalPos(pos);
        pos = _rect.position + Vector3.up * pageHeight / 2;
        _model.TopCenter = World2LocalPos(pos);
        pos = _rect.position + Vector3.down * pageHeight / 2 + Vector3.left * pageWidth / 2;
        _model.LeftCorner = World2LocalPos(pos);
        pos = _rect.position + Vector3.down * pageHeight / 2 + Vector3.right * pageWidth / 2;
        _model.RightCorner = World2LocalPos(pos);

        //获取一页的宽度和对角线长度
        float width = _rect.rect.width/2;
        float height = _rect.rect.height;
        _model.PageWidth = width;
        _model.PageDiagonal = Mathf.Sqrt(Mathf.Pow(width, 2) + Mathf.Pow(height, 2));

        //获取剪切遮罩宽高及轴心点Y值
        _clippingMask.sizeDelta = new Vector2(_model.PageDiagonal,_model.PageDiagonal+_model.PageWidth);
        _model.ClippingPivotY = _model.PageWidth / _clippingMask.sizeDelta.y;

        //设置拖拽页正面和背面的阴影大小
        _leftSide.InitShadow(new Vector2(_model.PageDiagonal, _model.PageDiagonal));
        _rightSide.InitShadow(new Vector2(_model.PageDiagonal, _model.PageDiagonal));
    }

    /// <summary>
    /// 定义鼠标拖动右侧页方法
    /// </summary>
    private void OnBeginDragRight()
    {
        if(_currentLeftId < _sprites.Length - 1)
        {
            _dragPage = new DragRightPage(this, _model, _leftSide, _rightSide, _rightPage.transform.position);
            _dragPage.BeginDragPage(World2LocalPos(Input.mousePosition));
            _isDragging = true;
            UpdateID();
        }

    }
    /// <summary>
    /// 定义鼠标拖动左侧页方法
    /// </summary>
    private void OnBeginDragLeft()
    {
        if(_currentLeftId > 0)
        {
            _dragPage = new DragLeftPage(this, _model, _rightSide, _leftSide, _leftPage.transform.position);
            _dragPage.BeginDragPage(World2LocalPos(Input.mousePosition));
            _isDragging = true;
            _currentLeftId -= 2;
            UpdateID();
        }

    }

    private void OnDragging()
    {
        _dragPage.DraggingPage();
    }

    private void OnEndDragRight()
    {
        if (_isDragging)
        {
            _isDragging = false;
            bool isLeft = IsClickPointLeft();
            _dragPage.EndDragPage(()=> {
                if (isLeft)
                    _currentLeftId += 2;
            });
        }
        
    }
    private void OnEndDragLeft()
    {
        if (_isDragging)
        {
            _isDragging = false;
            bool isLeft = IsClickPointLeft();
            _dragPage.EndDragPage(() => {
                if (isLeft)
                    _currentLeftId += 2;
            });
        }
    }

    private bool IsClickPointLeft()
    {
        return _model.ClickPoint.x < _model.BottomCenter.x;
    }

    private Vector3 World2LocalPos(Vector3 worldPos)
    {
       return _rect.InverseTransformPoint(worldPos);
    }

    public Vector3 Local2WorldPos(Vector3 localPos)
    {
        return _rect.TransformPoint(localPos);
    }

    public void UpdateID()
    {
        if (_isDragging)
        {
            _leftPage.ID = CurrentLeftId;
            _leftSide.ID = CurrentLeftId + 1;
            _rightSide.ID = CurrentLeftId + 2;
            _rightPage.ID = CurrentLeftId + 3;
        }
        else
        {
            _leftPage.ID = CurrentLeftId;
            _rightPage.ID = CurrentLeftId + 1;
        }
    }

    /// <summary>
    /// 获取剪切遮罩
    /// </summary>
    /// <returns></returns>
    public RectTransform GetClippingMask()
    {
        return _clippingMask;
    }
    /// <summary>
    /// 获取鼠标点击位置
    /// </summary>
    /// <returns></returns>
    public Vector3 GetClickPos()
    {
        if (_isDragging)
        {
            return World2LocalPos(Input.mousePosition);
        }
        else
        {
            return _model.ClickPoint;
        }
        
    }
    /// <summary>
    /// 计算当前拖拽顶点的实时位置
    /// </summary>
    /// <param name="clickPos"></param>
    /// <returns></returns>
    public Vector3 CalculateDraggingCorner(Vector3 clickPos)
    {
        Vector3 corner = Vector3.zero;
        corner = LimitBottomCenter(clickPos);
        corner =  LimitTopCenter(corner);
        return corner;
    }
    /// <summary>
    /// 以下是两种拖拽顶点的极限情况
    /// </summary>
    /// <param name="clickPos"></param>
    /// <returns></returns>
    private Vector3 LimitBottomCenter(Vector3 clickPos)
    {
        Vector3 offset = clickPos - _model.BottomCenter;
        float radians = Mathf.Atan2(offset.y, offset.x);
        Vector3 cornerLimit = new Vector3(Mathf.Cos(radians) * _model.PageWidth,
                                  Mathf.Sin(radians) * _model.PageWidth) + _model.BottomCenter;
        float distance = Vector2.Distance(clickPos, _model.BottomCenter);
        if (distance < _model.PageWidth)
        {
            return clickPos;
        }
        else
        {
            return cornerLimit;
        }
    }
    private Vector3 LimitTopCenter(Vector3 clickPos)
    {
        Vector3 offset = clickPos - _model.TopCenter;
        float radians = Mathf.Atan2(offset.y, offset.x);
        Vector3 cornerLimit = new Vector3(Mathf.Cos(radians) * _model.PageDiagonal,
                                  Mathf.Sin(radians) * _model.PageDiagonal) + _model.TopCenter;
        float distance = Vector2.Distance(clickPos, _model.BottomCenter);
        if (distance < _model.PageDiagonal)
        {
            return clickPos;
        }
        else
        {
            return cornerLimit;
        }
    }

    /// <summary>
    /// 计算折叠角
    /// </summary>
    /// <param name="corner"></param>
    /// <param name="bookCorner"></param>
    /// <returns></returns>
    public float CalculateFoldAngle(Vector3 corner,Vector3 bookCorner,out Vector3 bottomCrossPoint)
    {
        Vector3 twoCornerCenter = (corner + bookCorner) / 2;
        Vector3 offset = bookCorner - twoCornerCenter;
        float radians = Mathf.Atan2(offset.y, offset.x);

        float offsetX = twoCornerCenter.x - offset.y * Mathf.Tan(radians);
        offsetX = LimitOffsetX(offsetX, bookCorner, _model.BottomCenter);
        bottomCrossPoint = new Vector3(offsetX,_model.BottomCenter.y);

        offset = bottomCrossPoint - twoCornerCenter;
        return Mathf.Atan(offset.y / offset.x) * Mathf.Rad2Deg;//弧度转角度
    }
    private float LimitOffsetX(float offsetX, Vector3 bookCorner, Vector3 bottomCenter)
    {
        if (offsetX < bottomCenter.x && bookCorner.x > bottomCenter.x)
        {
            return bottomCenter.x;
        }
        else if (offsetX > bottomCenter.x && bookCorner.x < bottomCenter.x)
        {
            return bottomCenter.x;
        }

        return offsetX;
    }


    public void FlipAni(Vector3 target, Action onComplete)
    {
        StartCoroutine(PageAni(target, _aniDuration, () =>
        {
            if (onComplete != null)
                onComplete();

            ResetState();

        }));
    }
    private void ResetState()
    {
        UpdateID();
        _leftSide.SetActiveState(false);
        _rightSide.SetActiveState(false);
    }
    private IEnumerator PageAni(Vector3 target,float duration,Action onComplete)
    {
        Vector3 offset = (target - _model.ClickPoint) / duration;
        float symbol = (target - _model.ClickPoint).x;

        yield return new WaitUntil(() =>
        {

            _model.ClickPoint += offset * Time.deltaTime;
            _dragPage.DraggingPage();
            if (symbol > 0)
            {
                return _model.ClickPoint.x >= target.x;
            }
            else
            {
                return _model.ClickPoint.x <= target.x;
            }
        });

        if(onComplete != null)
        {
            onComplete();
        }
    }
}
