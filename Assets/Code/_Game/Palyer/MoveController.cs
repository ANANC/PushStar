using System.Collections.Generic;
using UnityEngine;
public class MoveController 
{
    private float m_Speed;
    private Obstructer m_Obstructer;
    private List<Obstructer> m_BoxRects;
    private Rect m_PlayerRect;
    private Rect m_OveRect;
    private Vector2 m_OverCenter;

    private Obstructer.line m_DetectLine;
    private Obstructer.line[] m_SideLines;

    public MoveController(Transform target)
    {
        m_Obstructer = new Obstructer(target);
        m_Speed = GameDefine.MoveSpeed;

        //摇杆
        ETCJoystick etcJoystick = ETCInput.GetControlJoystick("Joystick");
        etcJoystick.onMove.AddListener(JoystickMove);
    }
    private void JoystickMove(Vector2 vec)
    {
        if (vec != Vector2.zero)
        {
            Move(m_Speed*vec);
        }
    }
    private void Move(Vector2 dir)
    {
        Vector2 absDir = new Vector2(Mathf.Abs(dir.x), Mathf.Abs(dir.y));
        Vector2 normalizedDir = dir.normalized;
        //使用归一化的方向值
        Vector2 speed = normalizedDir;
        speed.x = absDir.x < 1 ? dir.x : speed.x;
        speed.y = absDir.y < 1 ? dir.y : speed.y;
        Vector2 absSpeed = new Vector2(Mathf.Abs(speed.x), Mathf.Abs(speed.y));
        Vector2 curMove = Vector2.zero;
        while (curMove.x <= absDir.x && curMove.y <= absDir.y)
        {
            Go(speed, normalizedDir);
            curMove += absSpeed;
        }
    }

    private void Go(Vector2 dir, Vector2 normalizedDir)
    {
        m_PlayerRect = m_Obstructer.rect;
        //偏移值
        m_PlayerRect.position += dir;
        //世界坐标
        Vector2 newDir = Vector2.zero;
        m_BoxRects = App.manager.obstructMgr.allObstructs;
        for (var index = 0; index < m_BoxRects.Count; index++)
        {
            newDir = CheckBoxLine(m_BoxRects[index], dir, normalizedDir);
            if (newDir != Vector2.zero)
            {
                break;
            }
        }

        dir += newDir;

        //修正位置直到没有发生碰撞
        if (newDir != Vector2.zero)
        {
            Go(dir, dir.normalized);
        }

        //位置要临时改变而不是真实改变
        m_Obstructer.rectTransform.anchoredPosition += dir;

    }

    private Vector2 CheckBoxLine(Obstructer boxObstructer, Vector2 dir, Vector2 normalizedDir)
    {
       // Rect boxRect = boxObstructer.rect;

        Vector2 newDir = Vector2.zero;
        if (!boxObstructer.Overlaps(m_Obstructer))
        {
            return newDir;
        }

        GetOverRectLine(boxObstructer);
        
        //检测线段
        float maxValue =( m_OveRect.width > m_OveRect.height ? m_OveRect.width : m_OveRect.height);
        Obstructer.line dirLines = new Obstructer.line(m_OverCenter - normalizedDir * maxValue *0.5f, normalizedDir.x * maxValue, normalizedDir.y * maxValue);

        for (int index = 0; index < m_SideLines.Length; index++)
        {
            if (IsRectCross(m_SideLines[index], dirLines))
            {
                //得到相交矩形的非相交轴的长度
                if (index < 2)
                {
                    newDir.y = (dir.y > 0 ? -1 : 1) * (m_OveRect.height);
                }
                else
                {
                    newDir.x = (dir.x > 0 ? -1 : 1) * (m_OveRect.width);
                }

                break;
            }
        }

        return newDir;
    }

    private void GetOverRectLine(Obstructer boxObstructer)
    {
        Vector2 minPos = m_Obstructer.min;
        Vector2 maxPos = m_Obstructer.max;

        Vector2 boxMin = boxObstructer.min;
        Vector2 boxMax = boxObstructer.max;

        if (boxMin.x > minPos.x)
        {
            minPos.x = boxMin.x;
        }

        if (boxMax.x < maxPos.x)
        {
            maxPos.x = boxMax.x;
        }

        if (boxMin.y < minPos.y)
        {
            minPos.y = boxMin.y;
        }

        if (boxMax.y > maxPos.y)
        {
            maxPos.y = boxMax.y;
        }

        float width = Mathf.Abs(maxPos.x - minPos.x) + 0.02f;
        float height = Mathf.Abs(maxPos.y - minPos.y) + 0.02f;

        //得到相交矩形
        m_OveRect = new Rect(minPos, new Vector2(width, height));
        m_OverCenter = new Vector2(minPos.x + width / 2, minPos.y - height / 2);

#if DRAWDEBUG

        Vector2 min = m_OveRect.position;
        Vector2 max = m_OveRect.position + new Vector2(width, -height);

        Obstructer.line[] lines = new Obstructer.line[]
        {
            new Obstructer.line(min, width, 0),
            new Obstructer.line(min, 0, -height),
            new Obstructer.line(max, -width, 0),
            new Obstructer.line(max, 0, height),

        };

        for (int index = 0; index < lines.Length; index++)
        {
            Obstructer.line curLine = lines[index];
            Debug.DrawLine(curLine.start, curLine.end, Color.yellow);
        }
        //Debug.Break();
#endif

        //场景碰撞盒的四条边
        m_SideLines = boxObstructer.boxLines;
    }

    //计算线段是否相交
    private bool IsRectCross(Obstructer.line p1, Obstructer.line p2)
    {
        bool IsCross = !(Mathf.Max(p1.start.x, p1.end.x) < Mathf.Min(p2.start.x, p2.end.x) || Mathf.Max(p1.start.y, p1.end.y) < Mathf.Min(p2.start.y, p2.end.y) ||
                         Mathf.Max(p2.start.x, p2.end.x) < Mathf.Min(p1.start.x, p1.end.x) || Mathf.Max(p2.start.y, p2.end.y) < Mathf.Min(p1.start.y, p1.start.y));
        if (IsCross)
        {
            if ((((p1.start.x - p2.start.x) * (p2.end.y - p2.start.y) - (p1.start.y - p2.start.y) * (p2.end.x - p2.start.x)) *
                 ((p1.end.x - p2.start.x) * (p2.end.y - p2.start.y) - (p1.end.y - p2.start.y) * (p2.end.x - p2.start.x))) > 0 ||
                (((p2.start.x - p1.start.x) * (p1.end.y - p1.start.y) - (p2.start.y - p1.start.y) * (p1.end.x - p1.start.x)) *
                 ((p2.end.x - p1.start.x) * (p1.end.y - p1.start.y) - (p2.end.y - p1.start.y) * (p1.end.x - p1.start.x))) > 0)
            {
                IsCross = false;
            }
        }

#if DRAWDEBUG

        Debug.DrawLine(p1.start, p1.end, IsCross ? Color.green : Color.red);
        Debug.DrawLine(p2.start, p2.end, IsCross ? Color.black : Color.red);

        //Debug.Break();
#endif
        return IsCross;
    }
    
}
