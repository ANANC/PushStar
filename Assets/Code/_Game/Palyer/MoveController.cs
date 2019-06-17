using System.Collections.Generic;
using UnityEngine;

public class MoveController
{
    private float m_Speed;

    private Obstructer m_Obstructer;
    private List<Obstructer.Rect> m_BoxRects;
    private Obstructer.Rect m_PlayerRect;

    private Obstructer.Rect m_OveRect;
    private Obstructer.line[] m_SideLines;
    private Obstructer.line m_DirLine;

    private bool m_Again;

    public MoveController(Obstructer target)
    {
        m_Obstructer = target;
        m_BoxRects = App.manager.obstructMgr.obstructRects;
        m_Speed = GameDefine.MoveSpeed;
        //摇杆
        ETCJoystick etcJoystick = ETCInput.GetControlJoystick("Joystick");
        etcJoystick.onMove.AddListener(JoystickMove);
    }

    private void JoystickMove(Vector2 vec)
    {
        if (vec != Vector2.zero)
        {
            Move(m_Speed * vec);
        }
    }

    private void Move(Vector2 dir)
    {
        if (dir == Vector2.zero)
        {
            return;
        }

        Vector2 absDir = new Vector2(Abs(dir.x), Abs(dir.y));
        //使用归一化的方向值
        Vector2 speed = dir.normalized;
        speed.x = absDir.x < 1 ? dir.x : speed.x;
        speed.y = absDir.y < 1 ? dir.y : speed.y;
        Vector2 absSpeed = new Vector2(Abs(speed.x), Abs(speed.y));
        Vector2 curMove = Vector2.zero;

        Obstructer.Rect curRect = m_Obstructer.rect;
        m_PlayerRect = curRect;
        m_Again = true;

        while (m_Again && curMove.x <= absDir.x && curMove.y <= absDir.y)
        {       
            //偏移值
            m_PlayerRect.position += speed;
            Go(speed);
            curMove += absSpeed;
        }

        Vector2 position = m_PlayerRect.position - curRect.position;
        m_Obstructer.transform.position += new Vector3(position.x, position.y);
    }

    private void Go(Vector2 dir)
    {
        Vector2 newDir = Vector2.zero;

        for (var index = 0; index < m_BoxRects.Count; index++)
        {
            if (CheckBoxLine(m_BoxRects[index], dir,out newDir))
            {
                //修正位置直到没有发生碰撞
                m_Again = false;
                m_PlayerRect.position += newDir;
                Go(dir + newDir);
                break;
            }
        }
    }

    private bool CheckBoxLine(Obstructer.Rect boxRect, Vector2 dir,out Vector2 newDir)
    {
         newDir = Vector2.zero;
        if (!boxRect.Overlaps(m_PlayerRect, out m_OveRect))
        {
            return false;
        }

        //场景碰撞盒的四条边
        m_SideLines = m_OveRect.boxLines;

#if DRAWDEBUG
        m_OveRect.DrawBox(Color.blue);
#endif

        float maxValue = m_OveRect.width > m_OveRect.height ? m_OveRect.width : m_OveRect.height;
        //检测线段
        m_DirLine = new Obstructer.line(m_OveRect.center, m_OveRect.center - dir * (maxValue * 2));

        for (int index = 0; index < m_SideLines.Length; index++)
        {
            if (IsRectCross(m_SideLines[index], m_DirLine))
            {
                //得到相交矩形的非相交轴的长度
                if (index < 2)
                {
                    newDir.y = (dir.y > 0 ? -1 : 1) * m_OveRect.height;
                }
                else
                {
                    newDir.x = (dir.x > 0 ? -1 : 1) * m_OveRect.width;
                }

                return true;
            }
        }

        return false;
    }

    //todo:向左移动有问题
    //计算线段是否相交
    private bool IsRectCross(Obstructer.line p1, Obstructer.line p2)
    {
        bool IsCross = !(Mathf.Max(p1.start.x, p1.end.x) < Mathf.Min(p2.start.x, p2.end.x) ||
                         Mathf.Max(p1.start.y, p1.end.y) < Mathf.Min(p2.start.y, p2.end.y) ||
                         Mathf.Max(p2.start.x, p2.end.x) < Mathf.Min(p1.start.x, p1.end.x) ||
                         Mathf.Max(p2.start.y, p2.end.y) < Mathf.Min(p1.start.y, p1.start.y));
        if (IsCross)
        {
            if ((((p1.start.x - p2.start.x) * (p2.end.y - p2.start.y) -
                  (p1.start.y - p2.start.y) * (p2.end.x - p2.start.x)) *
                 ((p1.end.x - p2.start.x) * (p2.end.y - p2.start.y) -
                  (p1.end.y - p2.start.y) * (p2.end.x - p2.start.x))) > 0 ||
                (((p2.start.x - p1.start.x) * (p1.end.y - p1.start.y) -
                  (p2.start.y - p1.start.y) * (p1.end.x - p1.start.x)) *
                 ((p2.end.x - p1.start.x) * (p1.end.y - p1.start.y) -
                  (p2.end.y - p1.start.y) * (p1.end.x - p1.start.x))) > 0)
            {
                IsCross = false;
            }
        }

#if DRAWDEBUG
        Debug.DrawLine(p1.start, p1.end, IsCross ? Color.red : Color.green);
        Debug.DrawLine(p2.start, p2.end, Color.yellow);
        if (IsCross)
        {
            // Debug.Break();
        }
#endif

        return IsCross;
    }

    public float Abs(float value)
    {
        if (value < 0)
        {
            return -value;
        }

        return value;
    }

    public int Abs(int value)
    {
        if (value < 0)
        {
            return -value;
        }

        return value;
    }

    public double Abs(double value)
    {
        if (value < 0)
        {
            return -value;
        }

        return value;
    }
}

