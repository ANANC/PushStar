using System.Collections.Generic;
using UnityEngine;
public class MoveController 
{
    private float m_Speed;
    private Obstructer m_MyObstructer;
    private List<Obstructer.Rect> m_BoxRects;
    private Obstructer.Rect m_MyRect;
    private Obstructer.Rect m_OveRect;

    private Obstructer.line m_DetectLine;

    public MoveController(Obstructer target)
    {
        m_MyObstructer = target;
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

        Obstructer.Rect curRect=  m_MyObstructer.rect;
        m_MyRect = curRect;

        while (curMove.x <= absDir.x && curMove.y <= absDir.y)
        {
            Go(speed, normalizedDir);
            curMove += absSpeed;
        }

        m_MyObstructer.transform.position += new Vector3(m_MyRect.position.x - curRect.position.x, m_MyRect.position.y - curRect.position.y);
    }

    private void Go(Vector2 dir, Vector2 normalizedDir)
    {
        //偏移值
        m_MyRect.position += dir;
        //世界坐标
        Vector2 newDir = Vector2.zero;
        m_BoxRects = App.manager.obstructMgr.obstructRects;
        for (var index = 0; index < m_BoxRects.Count; index++)
        {
            newDir = CheckBoxLine(m_BoxRects[index], dir, normalizedDir);
            if (newDir != Vector2.zero)
            {
                break;
            }
        }

        m_MyRect.position += newDir;

        dir += newDir;

        //修正位置直到没有发生碰撞
        if (newDir != Vector2.zero)
        {
            Go(dir, dir.normalized);
        }
    }

    private Vector2 CheckBoxLine(Obstructer.Rect boxRect, Vector2 dir, Vector2 normalizedDir)
    {
        Vector2 newDir = Vector2.zero;
        if (!boxRect.Overlaps(m_MyRect))
        {
            return newDir;
        }

        GetOverRectLine(boxRect);
        
        //检测线段
        float maxValue =(m_OveRect.width > m_OveRect.height ? m_OveRect.width : m_OveRect.height);
        Obstructer.line dirLine = new Obstructer.line(m_OveRect.center - dir * maxValue *0.5f, dir.x * maxValue, dir.y * maxValue);
        Obstructer.line[] boxLines = boxRect.boxLines;

        for (int index = 0; index < boxLines.Length; index++)
        {
            if (IsRectCross(boxLines[index], dirLine))
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

    private void GetOverRectLine(Obstructer.Rect boxObstructer)
    {
        Vector2 minPos = m_MyRect.min;
        Vector2 maxPos = m_MyRect.max;

        Vector2 boxMin = boxObstructer.min;
        Vector2 boxMax = boxObstructer.max;

        if (minPos.x < boxMin.x)
        {
            minPos.x = boxMin.x;
        }

        if (maxPos.x > boxMax.x)
        {
            maxPos.x = boxMax.x;
        }

        if (minPos.y > boxMin.y)
        {
            minPos.y = boxMin.y;
        }

        if (maxPos.y < boxMax.y)
        {
            maxPos.y = boxMax.y;
        }
        
        float width = Mathf.Abs(maxPos.x - minPos.x) + 0.02f;
        float height = Mathf.Abs(minPos.y - maxPos.y) + 0.02f;

        Vector2 size = new Vector2(width, height);
        minPos += new Vector2(size.x, -size.y)/2;

        //得到相交矩形
        m_OveRect = new Obstructer.Rect(minPos, size);

#if DRAWDEBUG
        m_OveRect.DrawBox(Color.yellow);
       // Debug.Break();
#endif

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
