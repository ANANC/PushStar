using UnityEngine;

public class Obstructer
{
    public struct line
    {
        public Vector2 start;
        public Vector2 end;

        public line(Vector2 start, float width,float height)
        {
            this.start = start;
            this.end = start + new Vector2(width,height);
        }
    }

    public GameObject gameObject;
    public Transform transform;
    public RectTransform rectTransform;
    
    public Obstructer(Transform target)
    {
        transform = target;
        rectTransform = transform.rectTransform();
        gameObject = transform.gameObject;
    }
    

    private Vector2[] m_BoxPosition;
    public Vector2[] boxPosition
    {
        get
        {
            // 0:min ----  |
            //    |        |
            //    |  ---- max:1

            if (m_BoxPosition == null)
            {
                float width = rectTransform.rect.width;
                float height = rectTransform.rect.height;
                Vector2 min = new Vector2(transform.position.x - width / 2, transform.position.y - height / 2);
                Vector2 max = new Vector2(transform.position.x + width / 2, transform.position.y + height / 2);
                Vector2 minPos = transform.localToWorldMatrix.MultiplyPoint3x4(min);
                Vector2 maxPos = transform.localToWorldMatrix.MultiplyPoint3x4(max);
                m_BoxPosition = new[] {minPos, maxPos};
            }

            return m_BoxPosition;
        }
    }

    private Vector2 m_Size = Vector2.zero;
    public Vector2 size
    {
        get
        {
            if (m_Size == Vector2.zero)
            {
                Vector2[] curBoxPosition = boxPosition;
                float width = curBoxPosition[1].x - curBoxPosition[0].x;
                float height = curBoxPosition[0].y - curBoxPosition[1].y;

                m_Size = new Vector2(Mathf.Abs(width), Mathf.Abs(height));
            }

            return m_Size;
        }
    }

    private Vector2 m_Radius = Vector2.zero;
    public Vector2 radius
    {
        get
        {
            if (m_Radius == Vector2.zero)
            {
                 m_Radius = size / 2;
            }

            return m_Radius;
        }
    }

    public Rect rect
    {
        get
        {
            Vector2 position = transform.position + new Vector3(-radius.x, +radius.y);

            return new Rect(position, m_Size);
        }
    }

    public Vector2 min
    {
        get
        {
            Vector2 position = transform.position + new Vector3(-radius.x, +radius.y);

            return position;
        }
    }

    public Vector2 max
    {
        get
        {
            Vector2 position = transform.position + new Vector3(radius.x, -radius.y);

            return position;
        }
    }

    public Vector2 center
    {
        get
        {
            return transform.position;
        }
    }

    public line[] boxLines
    {
        get
        {
            Vector2 min = rect.position;
            Vector2 max = rect.position + new Vector2(size.x, -size.y);

            line[] lines = new line[]
            {
                new line(min, size.x, 0),
                new line(min, 0, -size.y),
                new line(max, -size.x, 0),
                new line(max, 0, size.y),

            };

            return lines;
        }
    }

    public bool Overlaps(Obstructer other)
    {
        Vector2 curMin = min;
        Vector2 curMax = max;
        Vector2 otherMin = other.min;
        Vector2 otherMax = other.max;
        
        if ((otherMin.x >= curMin.x && otherMin.x <= curMax.x) && (otherMin.y <= min.y && otherMin.y >= max.y) ||
            ((otherMax.x <= curMax.x && otherMax.x >= curMin.x) && (otherMax.y >= curMax.y && otherMax.y <= curMin.y)))
        {
            return true;
        }

        return false;
    }

    public void DrawBox()
    {
        for (int index = 0; index < boxLines.Length; index++)
        {
            line curLine = boxLines[index];
            Debug.DrawLine(curLine.start, curLine.end, Color.blue);
        }
    }
}
