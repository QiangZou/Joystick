using UnityEngine;
using System;


public class Joystick : MonoBehaviour
{

#if UNITY_EDITOR
    /// <summary>
    /// 在scene视图绘制线 方便查看可点击区域
    /// </summary>
    void OnDrawGizmos()
    {
        if (Application.isPlaying == false)
        {
            return;
        }
        Gizmos.color = Color.black;

        //本地坐标
        Vector3 max = new Vector3(transform.localPosition.x + showRangeMax.x + 119, transform.localPosition.y + 119 + showRangeMax.y, 0);
        Vector3 min = new Vector3(transform.localPosition.x + showRangeMin.x - 119, transform.localPosition.y - 119 + showRangeMin.y, 0);

        //世界坐标
        max = transform.parent.TransformPoint(max);
        min = transform.parent.TransformPoint(min);


        Gizmos.DrawLine(new Vector3(min.x, min.y, 0f), new Vector3(max.x, min.y, 0f));
        Gizmos.DrawLine(new Vector3(max.x, min.y, 0f), new Vector3(max.x, max.y, 0f));
        Gizmos.DrawLine(new Vector3(max.x, max.y, 0f), new Vector3(min.x, max.y, 0f));
        Gizmos.DrawLine(new Vector3(min.x, max.y, 0f), new Vector3(min.x, min.y, 0f));
    }
#endif


    public enum JoystickState
    {
        /// <summary>
        /// 闲置
        /// </summary>
        ldle,
        /// <summary>
        /// 抬起
        /// </summary>
        TouchUp,
        /// <summary>
        /// 按下
        /// </summary>
        TouchDown,
        /// <summary>
        /// 准备
        /// </summary>
        Ready,
        /// <summary>
        /// 拖动
        /// </summary>
        Drag,
    }




    Transform joystick;
    Transform background;
    Transform stick;
    Transform direction;

    BoxCollider triggereBox;


    Vector3 joystickDirection;
    public Vector3 JoystickDirection
    {
        get
        {
            return joystickDirection;
        }
    }


    /// <summary>
    /// 摇杆抬起位置
    /// </summary>
    public Vector3 joystickInitPosition = new Vector3(165f, 165f, 0f);
    /// <summary>
    /// 点击触发范围
    /// </summary>
    public Vector2 triggeredRange = new Vector2(500f, 400f);
    /// <summary>
    /// 摇杆显示最小坐标值(相对于左下角)
    /// </summary>
    public Vector2 showRangeMin = new Vector2(145f, 145f);
    /// <summary>
    /// 摇杆显示最大坐标值(相对于左下角)
    /// </summary>
    public Vector2 showRangeMax = new Vector2(350f, 200f);




    /// <summary>
    /// 手柄状态
    /// </summary>
    public JoystickState joystickState = JoystickState.ldle;












    /// <summary>
    /// 按下状态位置
    /// </summary>
    Vector3 touchPosition;

    /// <summary>
    /// 切换到拖动状态最小距离差
    /// </summary>
    public float switchMoveMin = 20f;

    /// <summary>
    /// 杆拖动最大值
    /// </summary>
    public float stickMoveMax = 73f;

    /// <summary>
    /// 显示指向 鼠标距离中心最小距离
    /// </summary>
    public float showDirection = 20f;




    // Use this for initialization
    void Start()
    {
        joystick = transform.Find("joystick");
        background = transform.Find("joystick/background");
        stick = transform.Find("joystick/stick");
        direction = transform.Find("joystick/direction");
        triggereBox = transform.GetComponentInChildren<BoxCollider>();
        UIEventListener uiEventListener = transform.GetComponentInChildren<UIEventListener>();

        if (triggereBox == null || joystick == null || background == null || stick == null || direction == null || uiEventListener == null)
        {
            return;
        }

        uiEventListener.onDrag = onDrag;
        uiEventListener.onPress = onPress;


        InitState();
    }

    /// <summary>
    /// 鼠标按下触发
    /// </summary>
    /// <param name="go"></param>
    /// <param name="varPress"></param>
    void onPress(GameObject go, bool varPress)
    {
        if (varPress == true)
        {
            SwitchJoyStickState(JoystickState.TouchDown);
        }
        else
        {
            SwitchJoyStickState(JoystickState.TouchUp);
        }
    }

    void onDrag(GameObject go, Vector2 delta)
    {
        Action();
    }

    /// <summary>
    /// 切换摇杆状态
    /// </summary>
    /// <param name="state"></param>
    public void SwitchJoyStickState(JoystickState state)
    {
        joystickState = state;

        Action();
    }

    void Action()
    {
        if (joystickState == JoystickState.ldle)
        {
            return;
        }

        switch (joystickState)
        {
            case JoystickState.TouchUp:

                InitState();

                SwitchJoyStickState(JoystickState.ldle);

                break;
            case JoystickState.TouchDown:

                TouchState();

                SwitchJoyStickState(JoystickState.Ready);

                break;
            case JoystickState.Ready:

                ReadyState();

                break;
            case JoystickState.Drag:

                DragState();

                break;
        }
    }

    /// <summary>
    /// 获取鼠标相对于对象的本地坐标
    /// </summary>
    /// <returns></returns>
    Vector3 GetMouseLocalPosition(Transform transform)
    {
        //获取鼠标屏幕坐标
        Vector3 mousePosition = UICamera.currentTouch.pos;

        //转换为世界坐标
        mousePosition = UICamera.currentCamera.ScreenToWorldPoint(mousePosition);

        //转换为本地坐标
        mousePosition = transform.InverseTransformPoint(mousePosition);

        return mousePosition;
    }

    /// <summary>
    /// 抬起动作
    /// </summary>
    void InitState()
    {
        joystick.localPosition = joystickInitPosition;
        stick.localPosition = Vector3.zero;
        direction.gameObject.SetActive(false);

        //设置虚拟摇杆 抬起 触发区域
        triggereBox.transform.localPosition = Vector3.zero;
        triggereBox.size = new Vector3(triggeredRange.x, triggeredRange.y, 1);
        triggereBox.center = new Vector3(triggeredRange.x / 2, triggeredRange.y / 2, 0);
    }

    /// <summary>
    /// 按下动作
    /// </summary>
    void TouchState()
    {
        touchPosition = GetMouseLocalPosition(transform);

        Vector3 position = touchPosition;

        //如果超过显示区域则取临界值
        position.x = Math.Min(showRangeMax.x, Math.Max(position.x, showRangeMin.x));
        position.y = Math.Min(showRangeMax.y, Math.Max(position.y, showRangeMin.y));

        joystick.localPosition = position;

        //设置虚拟摇杆 按下 触发区域
        triggereBox.transform.position = stick.position;
        triggereBox.size = new Vector3(100f, 100f, 1);
        triggereBox.center = Vector3.zero;
    }

    /// <summary>
    /// 准备状态
    /// </summary>
    void ReadyState()
    {
        Vector3 position = GetMouseLocalPosition(transform);

        float distance = Vector3.Distance(position, touchPosition);

        //点击屏幕拖动大于切换拖动状态最小距离 则切换到拖动状态
        if (distance > switchMoveMin)
        {
            SwitchJoyStickState(JoystickState.Drag);
        }

        //设置虚拟摇杆 准备 触发区域
        triggereBox.transform.position = stick.position;
        triggereBox.size = new Vector3(100f, 100f, 1);
        triggereBox.center = Vector3.zero;
    }


    /// <summary>
    /// 拖动状态
    /// </summary>
    void DragState()
    {
        Vector3 mouseLocalPosition = GetMouseLocalPosition(joystick);

        //鼠标与摇杆的距离
        float distance = Vector3.Distance(mouseLocalPosition, background.localPosition);


        //设置杆的位置
        Vector3 stickLocalPosition = mouseLocalPosition;

        //鼠标位置大于杆拖动的最大值
        if (distance > stickMoveMax)
        {
            float proportion = stickMoveMax / distance;

            stickLocalPosition = (mouseLocalPosition - background.localPosition) * proportion;
        }

        stick.localPosition = stickLocalPosition;


        //设置指向位置

        //摇杆与鼠标的距离 大于 指向显示最小距离  则显示指向 
        if (distance > showDirection)
        {
            direction.gameObject.SetActive(true);

            //获取鼠标位置与摇杆的角度
            Double angle = Math.Atan2((mouseLocalPosition.y - background.localPosition.y), (mouseLocalPosition.x - background.localPosition.x)) * 180 / Math.PI;

            direction.eulerAngles = new Vector3(0, 0, (float)angle);

            //设置摇杆指向
            joystickDirection = mouseLocalPosition - background.localPosition;
            joystickDirection.z = 0;
        }
        else
        {
            direction.gameObject.SetActive(false);
        }



        //设置虚拟摇杆 拖动 触发区域
        triggereBox.transform.position = stick.position;
        triggereBox.size = new Vector3(100f, 100f, 1);
        triggereBox.center = Vector3.zero;
    }

}
