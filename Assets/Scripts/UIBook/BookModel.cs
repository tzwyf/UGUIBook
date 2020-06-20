
using System.Numerics;
using Vector3 = UnityEngine.Vector3;

public class BookModel
{
    /// <summary>
    /// 图片自身一页的宽度
    /// </summary>
    public float PageWidth;
    /// <summary>
    /// 对角线
    /// </summary>
    public float PageDiagonal;
    /// <summary>
    /// 底边中心点
    /// </summary>
    public Vector3 BottomCenter;
    /// <summary>
    /// 定边中心点
    /// </summary>
    public Vector3 TopCenter;
    /// <summary>
    /// 当前被拖动的书页顶点位置
    /// </summary>
    public Vector3 CurrentPageCorner;
    /// <summary>
    /// 右边书页右下角顶点
    /// </summary>
    public Vector3 RightCorner;
    /// <summary>
    /// 右边书页左下角顶点
    /// </summary>
    public Vector3 LeftCorner;
    /// <summary>
    /// 当前点击点
    /// </summary>
    public Vector3 ClickPoint;
    /// <summary>
    /// 翻转切面的轴心点Y值
    /// </summary>
    public float ClippingPivotY;
}
