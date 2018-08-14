# Unity 使用有限状态机 完美还原 王者荣耀 虚拟摇杆 #

![](https://i.imgur.com/calhLza.gif)



> 效果如图所示

----------


## 摇杆的UI组成 ##
![](https://i.imgur.com/bm3Xc3b.png)
> 如图所示 简单的可以认为摇杆由1、2、3贴图组成
1. 为摇杆的**底座**
2. 为摇杆的**杆**
3. 为摇杆的**指向**



*可以理解这就是街机上的摇杆*



----------


## 详解---摇杆显示规则 ##

![图1](https://i.imgur.com/2aU8LOp.png)
> 如图所示
* 最外面绿色的矩形为可点击区域
* 黑色矩形为摇杆的显示区域


1.  摇杆在操作结束后会**回到抬起位置**（如图状态）

2.  摇杆的**可点击**区域有限制（如图绿色框）

3.  摇杆的**显示**区域有限制(如图黑色款		作用：防止摇杆一半在屏幕外 )

4.  摇杆的**中心位置**随点击位置改变（如果在显示区域外则**取临界值**）




*更据上面的规则定义public变量可以方便策划大佬运行状态随时修改*






----------


## 详解---操作摇杆的几种动作、状态  ##



首先我们把摇杆系统分解成**状态、动作**
- **闲置（状态）**
- **按下（动作）**
- **抬起（动作）**
- **准备（状态）**
- **拖动（状态）**

动作、状态**区别**（**重点**）
 - 动作：一旦执行完毕就结束了（**调用一次**）
 - 状态：如果没有外部条件的触发，一个状态会一直持续下去（**不停的调用**）


----------


![图1](https://i.imgur.com/2aU8LOp.png)
* **闲置---状态**
  * 不需要做任何处理

----------


![图2.0](https://i.imgur.com/q25FVRo.png)![图2.1](https://i.imgur.com/vD9mGqA.png)
* **按下---动作**（手指按下屏幕 触发）
  * 获取手指按下坐标
  * 设置摇杆的位置（如左图）
   * 如果坐标在显示区域外，则取临界值（如右图）
  * UI、特效的显示或隐藏

----------

![图1](https://i.imgur.com/2aU8LOp.png)
* **抬起---动作**（手指离开屏幕 触发）
  * 摇杆回到抬起位置
  * UI、特效的显示或隐藏

----------

![图3](https://i.imgur.com/zitp5Q1.png)
* **准备---状态**（手指按下屏幕动作完成 触发）
  * 获取手指的实时坐标
  * 如果**实时坐标**与**按下坐标**的距离大于设定值则**切换到拖动状态**
  * UI、特效的显示或隐藏

----------

![图4](https://i.imgur.com/VvhKwjU.png)
* **拖动---状态**（手指滑动 触发）
  * 获取手指的实时坐标
  * 获取实时坐标与摇杆的坐标的**距离P**
  * 设置杆的位置
    * 如果杆的位置超过可拖动的最大值则取最大值
 * 设置指向的位置
   * 如果距离P大于显示指向最小值则**显示**指向同时
   * 否则**隐藏**指向

*这些动作、状态是我边测试边写代码总结出来的*
*使用枚举定义摇杆的几种状态、动作*
*可源代码中找到对应的方法*


----------

## 详解---几种状态、动作之间切换  ##

![](https://i.imgur.com/abwk0QW.png)


- 手指按下切换到---按下动作

- 手指抬起切换到---抬起动作

- 按下动作执行完成切换到---准备状态

- 准备状态达成条件切换到---拖动状态
----------


----------

## 使用到的API和方法  ##

*如果你理解了摇杆系统，但遇到了一些技术上的问题,下面的方法可能帮助你*


- 计算两个坐标的距离

```C#
        float distance = Vector3.Distance(Vec0, Vec1);
```

- 获取手指按下位置

```C#
        Vector3 mousePosition = UICamera.currentTouch.pos;
```

- 计算手指按下相对于摇杆的位置

```C#
        //转换为世界坐标
        mousePosition = UICamera.currentCamera.ScreenToWorldPoint(mousePosition);
        //转换为本地坐标
        mousePosition = transform.InverseTransformPoint(mousePosition);
```

- 设置摇杆指向的角度

```C#
        //mouseLocalPosition手指按下相对于摇杆的坐标
        //background摇杆
        //direction指向    
        Double angle = Math.Atan2((mouseLocalPosition.y - background.localPosition.y), (mouseLocalPosition.x - background.localPosition.x)) * 180 / Math.PI;
        //设置摇杆指向的角度 
        direction.eulerAngles = new Vector3(0, 0, (float)angle);
```

*如果有更好的办法 求大佬赐教*


----------


老规矩工程链接：[https://github.com/QiangZou/Joystick](https://github.com/QiangZou/Joystick "链接")

*UI、适配由NGUI实现，需要导入NGUI到工程中（NGUI有点大，没上传）*
