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

    public struct Rect
    {
        private Vector2 m_Position;
        private float m_Width;
        private float m_Height;
        private Vector2 m_Size;
        private Vector2 m_Radius;

        public Rect(Vector2 position, Vector2 size)
        {
            m_Position = position;
            m_Size = size;
            m_Radius = size / 2;
            m_Width = size.x;
            m_Height = size.y;
        }

        public Vector2 position
        {
            get { return m_Position; }
            set { m_Position = value; }
        }

        public float width
        {
            get { return m_Width; }
        }

        public float height
        {
            get { return m_Height; }
        }

        public Vector2 size
        {
            get { return m_Size; }
            set
            {
                m_Size = size;
                m_Radius = m_Size / 2;
                m_Width = m_Size.x;
                m_Height = m_Size.y;
            }
        }

        public Vector2 radius
        {
            get { return m_Radius; }
            set { size = value * 2; }
        }

        public Vector2 min
        {
            get { return m_Position + new Vector2(-m_Radius.x, m_Radius.y); }
        }

        public Vector2 max
        {
            get { return m_Position + new Vector2(m_Radius.x, -m_Radius.y); }
        }

        public Vector2 center
        {
            get { return m_Position; }
        }

        public line[] boxLines
        {
            get
            {
                Vector2 curMin = min;
                Vector2 curMax = max;

                line[] lines = new line[]
                {
                    new line(curMin, m_Size.x, 0),
                    new line(curMax, -m_Size.x, 0),
                    new line(curMin, 0, -m_Size.y),
                    new line(curMax, 0, m_Size.y),

                };

                return lines;
            }
        }

        public bool Overlaps(Rect other)
        {
            Vector2 curMin = min;
            Vector2 curMax = max;
            Vector2 otherMin = other.min;
            Vector2 otherMax = other.max;


            if (((otherMin.x >= curMin.x && otherMin.x <= curMax.x) && (otherMin.y <= min.y && otherMin.y >= max.y)) ||
                ((otherMax.x <= curMax.x && otherMax.x >= curMin.x) &&
                 (otherMax.y >= curMax.y && otherMax.y <= curMin.y)))
            {
                return true;
            }

            return false;
        }

        public void DrawBox()
        {
            DrawBox(Color.blue);
        }

        public void DrawBox(Color color)
        {
            for (int index = 0; index < boxLines.Length; index++)
            {
                line curLine = boxLines[index];
                Debug.DrawLine(curLine.start, curLine.end, color);
            }
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

    public Rect rect
    {
        get
        {
            Vector2[] curBoxPosition = boxPosition;
            Rect curRect = new Rect(transform.position,
                new Vector2(Mathf.Abs(curBoxPosition[1].x - curBoxPosition[0].x), Mathf.Abs(curBoxPosition[0].y - curBoxPosition[1].y)));

            return curRect;
        }
    }
}
