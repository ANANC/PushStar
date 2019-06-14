//#define DEBUGLOG

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CoreFramework
{
    [RequireComponent(typeof(ScrollRect))]
    public class EasyScrollView : MonoBehaviour
    {
        [SerializeField, HideInInspector] private ESVData m_ESVData = new ESVData();
        private List<GameObject> m_CellList = new List<GameObject>();
        private List<int> m_RealDataList = new List<int>();

        [SerializeField, HideInInspector] private ScrollRect m_ScrollRect = null;
        private RectTransform m_Content = null;
        private Func<GameObject> m_CreateCallback = null;
        private Action<int, int, int> m_UpdateCallback = null;

        [SerializeField, HideInInspector] private int m_MoveTweenSpeed = 30; /* Tween的速度 */
        [SerializeField, HideInInspector] private int m_PaddingLeft = 0; /* 偏移值左上宽度 */
        [SerializeField, HideInInspector] private int m_PaddingTop = 0; /* 偏移值左上高度 */

        //赋值后不变
        private Vector2 m_ScrollRectRadius = Vector2.zero; /* ScrollRect的宽高/2f */
        private int[] m_CellSizeRadiuss = null; /* Cell的宽高/2f */
        private int[] m_ScrollRectGrops = null; /* 排满ScorllRect的排版数 */
        private Vector2 m_ScrollDelta = Vector2.zero; /* 在Top时，ScrollRect的中心位置 */
        private int m_PosHorizontalCount = 0; /* Horizontal情况下，Data在水平的数量*/

        //动态改变
        private Vector3 m_ContentLocPos = Vector3.zero;
        private Vector3 m_CellHandPos = Vector3.zero;
        private Vector3 m_CellEndPos = Vector3.zero;
        private Vector2 m_curScorllDelta = Vector2.zero;

        private string m_Error = null;

        #region 基础

        public void OnDestroy()
        {
            m_CreateCallback = null;
            m_UpdateCallback = null;
            m_ScrollRect = null;
            m_Content = null;
        }

        #endregion

        #region 外观

        public int dataCount
        {
            get { return m_ESVData.m_DataCount; }
        }

        public int gropHorizontal
        {
            set
            {
                if (value < 1)
                {
                    value = 1;
                    LogWarning("行数最小为1。");
                }

                m_ESVData.m_GropHorizontalValue = value;
            }
            get { return m_ESVData.m_GropHorizontalValue; }
        }

        public int gropVertical
        {
            set
            {
                if (value < 1)
                {
                    value = 1;
                    LogWarning("列数最小为1。");
                }

                m_ESVData.m_GropVerticalValue = value;
            }
            get { return m_ESVData.m_GropVerticalValue; }
        }

        public int cellWidth
        {
            set
            {
                if (value < 0)
                {
                    value = 0;
                    LogWarning("宽度最小为零。");
                }

                m_ESVData.m_CellWidth = value;
            }
            get { return m_ESVData.m_CellWidth; }
        }

        public int cellHeight
        {
            set
            {
                if (value < 0)
                {
                    value = 0;
                    LogWarning("高度最小为零。");
                }

                m_ESVData.m_CellHeight = value;
            }
            get { return m_ESVData.m_CellHeight; }
        }

        public int paddingLeft
        {
            set
            {
                if (value < 0)
                {
                    value = 0;
                    LogWarning("偏移宽度最小为零。");
                }

                m_PaddingLeft = value;
            }
            get { return m_PaddingLeft; }
        }

        public int paddingTop
        {
            set
            {
                if (value < 0)
                {
                    value = 0;
                    LogWarning("偏移高度最小为零。");
                }

                m_PaddingTop = value;
            }
            get { return m_PaddingTop; }
        }

        public int moveTweenSpeed
        {
            set
            {
                if (!Error())
                {
                    if (value < 0)
                    {
                        value = 0;
                        LogWarning("速度最小为零。");
                    }

                    m_MoveTweenSpeed = value;
                }
            }
            get { return m_MoveTweenSpeed; }
        }


        /// <summary>
        /// ScrollView的滑动方向。Horizontal为true时水平滑动，为false时垂直滑动。
        /// </summary>
        public bool horizontal
        {
            get
            {
                if (m_ScrollRect == null)
                {
                    m_ScrollRect = this.GetComponent<ScrollRect>();
                }

                return m_ScrollRect.horizontal;
            }
            set
            {
                if (m_ScrollRect == null)
                {
                    m_ScrollRect = this.GetComponent<ScrollRect>();
                }

                m_ScrollRect.horizontal = value;
                m_ScrollRect.vertical = !m_ScrollRect.horizontal;

                OnScrollViewInit();

            }
        }

        public void OnCreate(Func<GameObject> createGOFunc, Action<int, int, int> updateAction)
        {
            m_CreateCallback = createGOFunc;
            m_UpdateCallback = updateAction;
            OnScrollViewInit();

            if (m_CreateCallback == null)
            {
                SetError("【EasyMove】错误：无创建GameObject函数。请调用OnCreate函数进行赋值。");
            }
            else if (m_UpdateCallback == null)
            {
                SetError("【EasyMove】错误：无更新函数。请调用OnCreate函数进行赋值。");
            }
            else if (m_ScrollRect == null)
            {
                SetError("【EasyMove】错误：并没有对ScrollRect赋值。");
            }
            else if (m_Content == null)
            {
                SetError("【EasyMove】错误：并没有对ScrollRect.content赋值。");
            }
        }

        public void ResetData(int dataCount, int moveToTarget = 0, bool moveTween = false)
        {
            if (Error())
            {
                return;
            }

            if (dataCount < 0)
            {
                dataCount = 0;
                LogWarning("数据总量最小为零。");
            }

            if (moveToTarget < 0)
            {
                moveToTarget = 0;
            }

            m_ESVData.m_DataCount = dataCount;
            m_RealDataList = new List<int>();
            for (int index = 0; index < m_ESVData.m_DataCount; index++)
            {
                m_RealDataList.Add(index);
            }

            m_ESVData.m_CellCount = 0;
            InitScrollbar();
            OnDataCountChange();
            RefereshAll();
            if (moveToTarget != 0)
            {
                MoveToTarget(moveToTarget, moveTween);
            }
        }

        public void ResetPos()
        {
            if (m_CellList.Count == 0)
            {
                EditorResetPos();
            }
            else
            {
                UpdateShowCount();
                ResetData(dataCount);
            }
        }


        public void Refresh(RefreshState state, int dataValue = 0)
        {
            if (Error())
            {
                return;
            }

            if (m_ESVData.m_DataCount == 0)
            {
                return;
            }

            if (state == RefreshState.All)
            {
                RefereshAll();
            }

            if (state == RefreshState.Single)
            {
                RefreshTarget(dataValue);
            }
        }

        public void RemoveTarget(int dataValue)
        {
            if (Error())
            {
                return;
            }

            if (m_ESVData.m_DataCount == 0)
            {
                return;
            }

            RemoveData(dataValue);
        }

        public void MoveToTarget(int dataValue, bool tween = false)
        {
            if (Error())
            {
                return;
            }

            if (m_ESVData.m_DataCount == 0)
            {
                return;
            }

            MoveTo(dataValue, tween);
        }

        #endregion

        #region 数据

        private void OnScrollViewInit()
        {
            if (m_ScrollRect == null)
            {
                m_ScrollRect = this.GetComponent<ScrollRect>();
            }

            m_Content = m_ScrollRect.content;

            UpdateShowCount();

            InitScrollbar();

            m_ScrollRect.onValueChanged.AddListener(OnValueChange);

            RectTransform rectTransform = m_ScrollRect.transform as RectTransform;
            m_ScrollRectRadius = new Vector2(rectTransform.rect.width, rectTransform.rect.height) / 2;
        }

        private void InitScrollbar()
        {
            if (m_ScrollRect == null)
            {
                return;
            }

            if (m_ScrollRect.verticalScrollbar != null)
            {
                m_ScrollRect.verticalScrollbar.value = 1;
            }

            if (m_ScrollRect.horizontalScrollbar != null)
            {
                m_ScrollRect.horizontalScrollbar.value = 0;
            }
        }

        private void OnDataCountChange()
        {
            UpdateCellCount();
            RenewBorder();
            RenewCellPosition();
        }

        private void UpdateShowCount()
        {
            RectTransform scrollrectTra = m_ScrollRect.transform as RectTransform;

            int horizontalCount = (int) (scrollrectTra.rect.width / m_ESVData.m_CellWidth) + (scrollrectTra.rect.width % m_ESVData.m_CellWidth > 0 ? 1 : 0);

            int verticalCount = (int) (scrollrectTra.rect.height / m_ESVData.m_CellHeight) + (scrollrectTra.rect.height % m_ESVData.m_CellHeight > 0 ? 1 : 0);

            if (m_ScrollRect.horizontal)
            {
                verticalCount = m_ESVData.m_GropVerticalValue;
            }
            else
            {
                horizontalCount = m_ESVData.m_GropHorizontalValue;
            }

            horizontalCount = horizontalCount <= 0 ? 1 : horizontalCount;
            verticalCount = verticalCount <= 0 ? 1 : verticalCount;

            m_ScrollRectGrops = m_ScrollRect.horizontal ? new[] {horizontalCount + 1, verticalCount} : new[] {horizontalCount, verticalCount + 1};
            m_ESVData.m_ShowCount = m_ScrollRectGrops[0] * m_ScrollRectGrops[1];

        }

        private void UpdateCellCount()
        {
            m_ESVData.m_CellCount = m_ESVData.m_DataCount < m_ESVData.m_ShowCount ? m_ESVData.m_DataCount : m_ESVData.m_ShowCount;
            int updateCount = m_ESVData.m_CellCount - m_CellList.Count;

            if (updateCount == 0)
            {
                return;
            }

            while (updateCount > 0)
            {
                updateCount -= 1;
                GameObject go = m_CreateCallback();
                m_CellList.Add(go);
            }

            int hideIndex = m_CellList.Count;
            while (updateCount < 0)
            {
                hideIndex -= 1;
                m_CellList[hideIndex].SetActive(false);
                updateCount += 1;
            }
        }

        #endregion

        #region 边界

        private void RenewBorder()
        {
            int endIndex = 0;

            if (m_ScrollRect.horizontal)
            {
                m_ScrollDelta = new Vector2(m_ScrollRectRadius.x, 0);

                m_Content.anchorMin = Vector2.zero;
                m_Content.anchorMax = new Vector2(0, 1);
                m_PosHorizontalCount = m_ESVData.m_DataCount / m_ESVData.m_GropVerticalValue + (m_ESVData.m_DataCount % m_ESVData.m_GropVerticalValue > 0 ? 1 : 0);
                m_Content.sizeDelta = new Vector2(m_PosHorizontalCount * m_ESVData.m_CellWidth, 0);

                if (m_ESVData.m_DataCount > 1)
                {
                    endIndex = m_ESVData.m_DataCount < m_ScrollRectGrops[0] ? m_ESVData.m_DataCount - 1 : m_ScrollRectGrops[0] - 1;
                }
            }
            else
            {
                m_ScrollDelta = new Vector2(0, -m_ScrollRectRadius.y);

                m_Content.anchorMin = new Vector2(0, 1);
                m_Content.anchorMax = Vector2.one;
                int posVerticalCount = m_ESVData.m_DataCount / m_ESVData.m_GropHorizontalValue + (m_ESVData.m_DataCount % m_ESVData.m_GropHorizontalValue > 0 ? 1 : 0);
                m_Content.sizeDelta = new Vector2(0, posVerticalCount * m_ESVData.m_CellHeight);


                if (m_ESVData.m_DataCount >= m_ESVData.m_GropHorizontalValue)
                {
                    endIndex = m_ESVData.m_GropHorizontalValue * ((posVerticalCount < m_ScrollRectGrops[1] ? posVerticalCount : m_ScrollRectGrops[1]) - 1);
                }
            }

            m_ESVData.m_CellBorderHand = 0;
            m_ESVData.m_CellBorderEnd = endIndex;
            m_ESVData.m_PosBorderHand = 0;
            m_ESVData.m_PosBorderEnd = endIndex;
        }


        #endregion

        #region 排版

        private void RenewCellPosition()
        {
            Vector2 position = Vector2.zero;
            position.x = m_ESVData.m_CellWidth / 2f + m_PaddingLeft;
            position.y = -m_ESVData.m_CellHeight / 2f - m_PaddingTop;

            int[] lineValue = {0, 0};
            int horizontalCount = m_ScrollRect.horizontal ? m_PosHorizontalCount < m_ScrollRectGrops[0] ? m_PosHorizontalCount : m_ScrollRectGrops[0] : m_ScrollRectGrops[0];

            for (int index = 0; index < m_ESVData.m_CellCount; index++)
            {
                m_CellList[index].transform.localPosition = position + new Vector2(m_ESVData.m_CellWidth * lineValue[0], -m_ESVData.m_CellHeight * lineValue[1]);

                if (lineValue[0] == horizontalCount - 1)
                {
                    lineValue[0] = 0;
                    lineValue[1] += 1;
                }
                else
                {
                    lineValue[0] += 1;
                }
            }
        }

        #endregion

        #region 显示

        private void RefreshCell(int cellIndex, int posIndex)
        {
#if DEBUGLOG
        Debug.Log(string.Format("RefreshCell:cellIndex:{0},posIndex:{1}", cellIndex, posIndex));
#endif
            if (posIndex < m_ESVData.m_DataCount)
            {
                m_CellList[cellIndex].SetActive(true);
                m_UpdateCallback(cellIndex, posIndex, m_RealDataList[posIndex]);
            }
            else
            {
                m_CellList[cellIndex].SetActive(false);
            }
        }

        private void RefereshAll()
        {
            RefreshRound(m_ESVData.m_PosBorderHand);
        }

        private void RefreshRound(int posHand)
        {
            int posIndex = m_ESVData.m_PosBorderHand;
            int cellIndex = m_ESVData.m_CellBorderHand;

            int updateCount = m_ESVData.m_DataCount < m_ESVData.m_ShowCount ? m_ESVData.m_DataCount : m_ESVData.m_ShowCount;
            int horizontalCount = m_ScrollRect.horizontal ? m_PosHorizontalCount < m_ScrollRectGrops[0] ? m_PosHorizontalCount : m_ScrollRectGrops[0] : m_ScrollRectGrops[0];
            int cellhorizontalEnd = horizontalCount;

            int lineValue = 0;
            int lineEnd = horizontalCount - 1;

            for (int updateIndex = 0; updateIndex < updateCount; updateIndex++)
            {
                if (posIndex >= posHand)
                {
                    RefreshCell(cellIndex, posIndex);
                }
#if DEBUGLOG
            Debug.Log(string.Format("cell:{0},pos:{1}",cellIndex,posIndex));
#endif
                posIndex += 1;
                cellIndex += 1;

                if (m_ScrollRect.horizontal)
                {
                    if (lineValue == lineEnd)
                    {
                        lineValue = 0;


                        posIndex = m_ScrollRect.vertical
                            ? posIndex
                            : (posIndex + m_PosHorizontalCount - horizontalCount);
                        cellIndex = cellhorizontalEnd + m_ESVData.m_CellBorderHand;
                        cellhorizontalEnd += horizontalCount;

                    }
                    else
                    {
                        lineValue += 1;

                        if (cellIndex == cellhorizontalEnd)
                        {
                            cellIndex -= horizontalCount;
                        }
                    }
                }
                else
                {
                    cellIndex = cellIndex == m_ESVData.m_CellCount ? 0 : cellIndex;
                }
            }
        }

        private void RefreshTarget(int dataValue)
        {
            int posIndex = GetPosIndex(dataValue);
            if (posIndex == -1)
            {
                return;
            }

            int cellIndex = GetCellIndex(posIndex);
            if (cellIndex != -1)
            {
                RefreshCell(cellIndex, posIndex);
            }
        }

        #endregion

        #region 删除

        private void RemoveData(int dataValue)
        {
            int posIndex = GetPosIndex(dataValue);

            if (posIndex == -1)
            {
                return;
            }

            int oldPosHorizontalCount = m_PosHorizontalCount;

            m_RealDataList.RemoveAt(posIndex);
            m_ESVData.m_DataCount -= 1;
            
            Action<int, int, int> tmp = m_UpdateCallback;
            m_UpdateCallback = (a, b, c) => {};
            Vector2 contentPos = m_Content.localPosition;

            OnDataCountChange();

            m_ContentLocPos = new Vector3(1, -1);
            m_Content.localPosition = contentPos;
            
            OnValueChange(Vector2.zero);

            m_UpdateCallback = tmp;
            
            if (oldPosHorizontalCount != m_PosHorizontalCount)
            {
                RefereshAll();
            }
            else
            {
                RefreshRound(posIndex);
            }
        }

        #endregion

        #region 滑动

        private void OnValueChange(Vector2 value)
        {
            if (m_ContentLocPos == m_Content.localPosition || m_ESVData.m_CellCount == 0)
            {
                return;
            }

            m_ContentLocPos = m_Content.localPosition;
            m_CellHandPos = m_CellList[m_ESVData.m_CellBorderHand].transform.localPosition;
            m_CellEndPos = m_CellList[m_ESVData.m_CellBorderEnd].transform.localPosition;

            m_curScorllDelta = m_ScrollDelta;
            m_curScorllDelta.x -= m_ContentLocPos.x;
            m_curScorllDelta.y -= m_ContentLocPos.y;

            if (m_CellSizeRadiuss == null || m_CellSizeRadiuss.Length == 0)
            {
                m_CellSizeRadiuss = new[] {m_ESVData.m_CellWidth / 2, m_ESVData.m_CellHeight / 2};
            }

            if (m_ScrollRect.horizontal)
            {
                if (IsMoveToRight())
                {
                    MoveCellToRight();
                }
                else if (IsMoveCellToLeft())
                {
                    MoveCellToLeft();
                }
            }
            else
            {
                if (IsMoveCellToTop())
                {
                    MoveCellToTop();
                }
                else if (IsMoveCellToBottom())
                {
                    MoveCellToBotton();
                }
            }
        }

        private bool IsMoveToRight()
        {
            return m_ESVData.m_PosBorderEnd < m_PosHorizontalCount - 1 && m_CellEndPos.x + m_CellSizeRadiuss[0] < m_curScorllDelta.x + m_ScrollRectRadius.x;
        }

        private bool IsMoveCellToLeft()
        {
            return m_ESVData.m_PosBorderHand > 0 && m_CellHandPos.x - m_CellSizeRadiuss[0] > m_curScorllDelta.x - m_ScrollRectRadius.x;
        }

        private bool IsMoveCellToTop()
        {
            return m_ESVData.m_PosBorderHand > 0 && m_CellHandPos.y + m_CellSizeRadiuss[1] < m_curScorllDelta.y + m_ScrollRectRadius.y;
        }

        private bool IsMoveCellToBottom()
        {
            return m_ESVData.m_PosBorderEnd + m_ScrollRectGrops[0] < m_ESVData.m_DataCount && m_CellEndPos.y - m_CellSizeRadiuss[1] > m_curScorllDelta.y - m_ScrollRectRadius.y;
        }

        private void MoveCellToRight()
        {
            int updateLength = 0;
            int[] updateCellIndexs = new int[m_ESVData.m_CellCount];
            int[] updatePosIndexs = new int[m_ESVData.m_CellCount];
            Vector3[] updateCellPoss = new Vector3[m_ESVData.m_CellCount];

            do
            {
                int cellIndex = m_ESVData.m_CellBorderHand;
                int posIndex = m_ESVData.m_PosBorderEnd + 1;

                int curCellHandIndex = cellIndex;
                int curDataHandIndex = posIndex;

                Vector3 initPos = updateLength == 0 ? m_CellList[m_ESVData.m_CellBorderEnd].transform.localPosition : updateCellPoss[m_ESVData.m_CellBorderEnd];
                initPos.x += m_ESVData.m_CellWidth;

                for (int updateIndex = 0; updateIndex < m_ScrollRectGrops[1]; updateIndex++)
                {
                    if (updateLength != m_ESVData.m_CellCount)
                    {
                        updateCellIndexs[updateLength++] = cellIndex;
                    }

                    updatePosIndexs[cellIndex] = posIndex;
                    updateCellPoss[cellIndex] = initPos + new Vector3(0, -m_ESVData.m_CellHeight * updateIndex);
#if DEBUGLOG
                Debug.Log(string.Format("add:cellIndex:{0},posIndex:{1}", cellIndex, posIndex));
#endif

                    cellIndex += m_ScrollRectGrops[0];
                    posIndex += m_PosHorizontalCount;
                }

                m_ESVData.m_CellBorderEnd = curCellHandIndex;
                m_ESVData.m_CellBorderHand = m_ESVData.m_CellBorderHand == m_ScrollRectGrops[0] - 1 ? 0 : m_ESVData.m_CellBorderHand + 1;

                m_ESVData.m_PosBorderEnd = curDataHandIndex;
                m_ESVData.m_PosBorderHand += 1;

                m_CellEndPos = updateCellPoss[m_ESVData.m_CellBorderEnd];
                if (updateLength == m_ESVData.m_CellCount)
                {
                    m_CellHandPos = updateCellPoss[m_ESVData.m_CellBorderHand];
                }
                else
                {
                    m_CellHandPos = m_CellList[m_ESVData.m_CellBorderHand].transform.localPosition;
                }

            } while (IsMoveToRight());

            for (int index = 0; index < updateLength; index++)
            {
                int cellIndex = updateCellIndexs[index];

                Vector3 cellPos = updateCellPoss[cellIndex];
                m_CellList[cellIndex].transform.localPosition = cellPos;
                int posIndex = updatePosIndexs[cellIndex];
                RefreshCell(cellIndex, posIndex);

#if DEBUGLOG
            Debug.Log(string.Format("set:cellIndex:{0},posIndex:{1}", cellIndex, posIndex));
#endif
            }
        }

        private void MoveCellToLeft()
        {
            int updateLength = 0;
            int[] updateCellIndexs = new int[m_ESVData.m_CellCount];
            int[] updatePosIndexs = new int[m_ESVData.m_CellCount];
            Vector3[] updateCellPoss = new Vector3[m_ESVData.m_CellCount];

            do
            {
                int cellIndex = m_ESVData.m_CellBorderEnd;
                int posIndex = m_ESVData.m_PosBorderHand - 1;

                int curCellHandIndex = cellIndex;
                int curDataHandIndex = posIndex;

                Vector3 initPos = updateLength == 0 ? m_CellList[m_ESVData.m_CellBorderHand].transform.localPosition : updateCellPoss[m_ESVData.m_CellBorderHand];
                initPos.x -= m_ESVData.m_CellWidth;

                for (int updateIndex = 0; updateIndex < m_ScrollRectGrops[1]; updateIndex++)
                {
                    if (updateLength != m_ESVData.m_CellCount)
                    {
                        updateCellIndexs[updateLength++] = cellIndex;
                    }

                    updatePosIndexs[cellIndex] = posIndex;
                    updateCellPoss[cellIndex] = initPos + new Vector3(0, -m_ESVData.m_CellHeight * updateIndex);

                    cellIndex += m_ScrollRectGrops[0];
                    posIndex += m_PosHorizontalCount;
                }

                m_ESVData.m_CellBorderHand = curCellHandIndex;
                m_ESVData.m_CellBorderEnd = m_ESVData.m_CellBorderHand == 0
                    ? m_ScrollRectGrops[0] - 1
                    : m_ESVData.m_CellBorderHand - 1;

                m_ESVData.m_PosBorderHand = curDataHandIndex;
                m_ESVData.m_PosBorderEnd -= 1;

                m_CellHandPos = updateCellPoss[m_ESVData.m_CellBorderHand];
                if (updateLength == m_ESVData.m_CellCount)
                {
                    m_CellEndPos = updateCellPoss[m_ESVData.m_CellBorderEnd];
                }
                else
                {
                    m_CellEndPos = m_CellList[m_ESVData.m_CellBorderEnd].transform.localPosition;
                }

            } while (IsMoveCellToLeft());

            for (int index = 0; index < updateLength; index++)
            {
                int cellIndex = updateCellIndexs[index];

                Vector3 cellPos = updateCellPoss[cellIndex];
                m_CellList[cellIndex].transform.localPosition = cellPos;
                int posIndex = updatePosIndexs[cellIndex];
                RefreshCell(cellIndex, posIndex);
            }
        }

        private void MoveCellToTop()
        {
            int updateLength = 0;
            int[] updateCellIndexs = new int[m_ESVData.m_CellCount];
            int[] updatePosIndexs = new int[m_ESVData.m_CellCount];
            Vector3[] updateCellPoss = new Vector3[m_ESVData.m_CellCount];

            do
            {
                int cellIndex = m_ESVData.m_CellBorderEnd;
                int posIndex = m_ESVData.m_PosBorderHand - m_ScrollRectGrops[0];

                int curCellHandIndex = cellIndex;
                int curDataHandIndex = posIndex;

                Vector3 initPos = updateLength == 0 ? m_CellList[m_ESVData.m_CellBorderHand].transform.localPosition : updateCellPoss[m_ESVData.m_CellBorderHand];
                initPos.y += m_ESVData.m_CellHeight;

                for (int updateIndex = 0; updateIndex < m_ScrollRectGrops[0]; updateIndex++, cellIndex++, posIndex++)
                {
                    if (updateLength != m_ESVData.m_CellCount)
                    {
                        updateCellIndexs[updateLength++] = cellIndex;
                    }

                    updatePosIndexs[cellIndex] = posIndex;
                    updateCellPoss[cellIndex] = initPos + new Vector3(m_ESVData.m_CellWidth * updateIndex, 0);
                }

                m_ESVData.m_CellBorderHand = curCellHandIndex;
                m_ESVData.m_CellBorderEnd = curCellHandIndex - m_ScrollRectGrops[0];
                m_ESVData.m_CellBorderEnd = m_ESVData.m_CellBorderEnd < 0 ? m_ESVData.m_CellCount - m_ScrollRectGrops[0] : m_ESVData.m_CellBorderEnd;

                m_ESVData.m_PosBorderHand = curDataHandIndex;
                m_ESVData.m_PosBorderEnd -= m_ScrollRectGrops[0];

                m_CellHandPos = updateCellPoss[m_ESVData.m_CellBorderHand];
                if (updateLength == m_ESVData.m_CellCount)
                {
                    m_CellEndPos = updateCellPoss[m_ESVData.m_CellBorderEnd];
                }
                else
                {
                    m_CellEndPos = m_CellList[m_ESVData.m_CellBorderEnd].transform.localPosition;
                }

            } while (IsMoveCellToTop());

            for (int index = 0; index < updateLength; index++)
            {
                int cellIndex = updateCellIndexs[index];

                Vector3 cellPos = updateCellPoss[cellIndex];
                m_CellList[cellIndex].transform.localPosition = cellPos;
                int posIndex = updatePosIndexs[cellIndex];
                RefreshCell(cellIndex, posIndex);
            }

        }

        private void MoveCellToBotton()
        {
            int updateLength = 0;
            int[] updateCellIndexs = new int[m_ESVData.m_CellCount];
            int[] updatePosIndexs = new int[m_ESVData.m_CellCount];
            Vector3[] updateCellPoss = new Vector3[m_ESVData.m_CellCount];

            do
            {
                int cellIndex = m_ESVData.m_CellBorderHand;
                int posIndex = m_ESVData.m_PosBorderEnd + m_ScrollRectGrops[0];

                int curCellHandIndex = cellIndex;
                int curDataHandIndex = posIndex;

                Vector3 initPos = updateLength == 0 ? m_CellList[m_ESVData.m_CellBorderEnd].transform.localPosition : updateCellPoss[m_ESVData.m_CellBorderEnd];
                initPos.y -= m_ESVData.m_CellHeight;

                for (int updateIndex = 0; updateIndex < m_ScrollRectGrops[0]; updateIndex++, cellIndex++, posIndex++)
                {
                    if (updateLength != m_ESVData.m_CellCount)
                    {
                        updateCellIndexs[updateLength++] = cellIndex;
                    }

                    updatePosIndexs[cellIndex] = posIndex;
                    updateCellPoss[cellIndex] = initPos + new Vector3(m_ESVData.m_CellWidth * updateIndex, 0);
                }

                m_ESVData.m_CellBorderEnd = curCellHandIndex;
                m_ESVData.m_CellBorderHand = cellIndex >=  m_ESVData.m_CellCount ? 0 : cellIndex;

                m_ESVData.m_PosBorderEnd = curDataHandIndex;
                m_ESVData.m_PosBorderHand += m_ScrollRectGrops[0];

                m_CellEndPos = updateCellPoss[m_ESVData.m_CellBorderEnd];
                if (updateLength == m_ESVData.m_CellCount)
                {
                    m_CellHandPos = updateCellPoss[m_ESVData.m_CellBorderHand];
                }
                else
                {
                    m_CellHandPos = m_CellList[m_ESVData.m_CellBorderHand].transform.localPosition;
                }

            } while (IsMoveCellToBottom());

            for (int index = 0; index < updateLength; index++)
            {
                int cellIndex = updateCellIndexs[index];

                Vector3 cellPos = updateCellPoss[cellIndex];
                m_CellList[cellIndex].transform.localPosition = cellPos;
                int posIndex = updatePosIndexs[cellIndex];
                RefreshCell(cellIndex, posIndex);
            }
        }

        #endregion

        #region 移动

        private void MoveTo(int dataValue, bool tween)
        {
            int posIndex = GetPosIndex(dataValue);
            if (posIndex == -1)
            {
                return;
            }

            Vector3 newPos = new Vector3(-m_PaddingLeft, m_PaddingTop);
            if (m_ScrollRect.horizontal)
            {
                int horizontalValue = posIndex % m_PosHorizontalCount;

                newPos.x += -horizontalValue * m_ESVData.m_CellWidth;
            }
            else
            {
                int verticalValue = posIndex / m_ScrollRectGrops[0];

                newPos.y += verticalValue * m_ESVData.m_CellHeight;
            }

            RectTransform scorllRectTransform = m_ScrollRect.transform as RectTransform;
            if (m_ScrollRect.horizontal)
            {
                float contentWidth = m_Content.rect.width;
                float MaxWidth = contentWidth - scorllRectTransform.rect.width;
                newPos.x = newPos.x < -MaxWidth ? -MaxWidth : newPos.x;
            }
            else
            {
                float contentHiehgt = m_Content.rect.height;
                float MaxHeight = contentHiehgt - scorllRectTransform.rect.height;
                newPos.y = newPos.y > MaxHeight ? MaxHeight : newPos.y;
            }

            if (!tween || m_MoveTweenSpeed == 0)
            {
                m_Content.localPosition = newPos;
            }
            else
            {
                StartCoroutine(MoveTween(newPos));
            }
        }


        IEnumerator MoveTween(Vector3 targetPos)
        {
            Vector3 dir = targetPos - m_Content.localPosition;
            dir /= m_MoveTweenSpeed;
            float round = m_ScrollRect.horizontal ? Mathf.Abs(dir.x) : Mathf.Abs(dir.y);
            round = round < 2 ? 2 : round;

            while ((m_ScrollRect.horizontal && (m_Content.localPosition.x - targetPos.x > round || m_Content.localPosition.x - targetPos.x < -round)) ||
                   (m_ScrollRect.vertical && (m_Content.localPosition.y - targetPos.y > round || m_Content.localPosition.y - targetPos.y < -round)))
            {
                m_Content.localPosition += dir;
                yield return new WaitForSeconds(0.01f);
            }

            m_Content.localPosition = targetPos;
        }

        #endregion

        #region 工具

        private int GetPosIndex(int dataValue)
        {
            int endIndex = -1;

            if (dataValue < 0)
            {
                return endIndex;
            }

            if (dataValue >= m_ESVData.m_DataCount)
            {
                int endValue = m_RealDataList[m_ESVData.m_DataCount - 1];
                if (endValue < dataValue)
                {
                    return endIndex;
                }
                else if (endValue == dataValue)
                {
                    return m_ESVData.m_DataCount - 1;
                }

                endIndex = m_ESVData.m_DataCount - 2;
            }
            else
            {
                int curValue = m_RealDataList[dataValue];
                if (curValue == dataValue)
                {
                    return dataValue;
                }

                endIndex = dataValue - 1;
            }

            for (int index = endIndex; index > 0; index--)
            {
                if (m_RealDataList[index] == dataValue)
                {
                    return index;
                }
            }

            endIndex = -1;

            return endIndex;
        }


        private int GetCellIndex(int posIndex)
        {
            int cellIndex = -1;

            if (posIndex < 0 || posIndex >= m_ESVData.m_DataCount)
            {
                return cellIndex;
            }

            if (m_ScrollRect.horizontal)
            {
                int verticalValue = posIndex / m_PosHorizontalCount;
                int horizontalValue = posIndex % m_PosHorizontalCount;
                if (horizontalValue >= m_ESVData.m_PosBorderHand && horizontalValue <= m_ESVData.m_PosBorderEnd)
                {
                    cellIndex = m_ESVData.m_CellBorderHand;
                    int updateCount = horizontalValue - m_ESVData.m_PosBorderHand;
                    int horizontalCount = m_ScrollRectGrops[0] < m_PosHorizontalCount ? m_ScrollRectGrops[0] : m_PosHorizontalCount;
                    for (int index = 0; index < updateCount; index++)
                    {
                        cellIndex = cellIndex == horizontalCount - 1 ? 0 : cellIndex + 1;
                    }

                    cellIndex += verticalValue * horizontalCount;
                }
            }
            else
            {
                if (posIndex >= m_ESVData.m_PosBorderHand && posIndex - m_ScrollRectGrops[0] <= m_ESVData.m_PosBorderEnd)
                {
                    int verticalValue = posIndex / m_ScrollRectGrops[0];
                    int horizontalValue = posIndex % m_ScrollRectGrops[0];

                    cellIndex = m_ESVData.m_CellBorderHand;
                    int updateCount = verticalValue * m_ScrollRectGrops[0] - m_ESVData.m_PosBorderHand;
                    updateCount = updateCount / m_ScrollRectGrops[0];

                    for (int index = 0; index < updateCount; index++)
                    {
                        cellIndex += m_ScrollRectGrops[0];
                        cellIndex = cellIndex >= m_ESVData.m_CellCount ? 0 : cellIndex;
                    }

                    cellIndex += horizontalValue;
                }
            }

            return cellIndex;
        }

        #endregion

        #region Error

        private void SetError(string err, bool Log = true)
        {
            m_Error = err;

            if (Log)
            {
                LogError();
            }

        }

        public bool Error()
        {
            return !string.IsNullOrEmpty(m_Error);
        }

        public void LogError()
        {
            if (Error())
            {
                Debug.LogError(m_Error);
            }
        }

        public void LogWarning(string warning)
        {
            Debug.LogWarning("【EasyScrollView】提示：" + warning);
        }

        #endregion

        #region 编辑器

        private void EditorResetPos()
        {
            List<GameObject> tmpGameObjectList = new List<GameObject>();
            ScrollRect scrollRect = this.GetComponent<ScrollRect>();
            
            int cellCount = 0;
            if (scrollRect.content != null)
            {
                Transform child;
                for (int index = 0; index < scrollRect.content.childCount; index++)
                {
                    child = scrollRect.content.GetChild(index);
                    if (!child.gameObject.activeSelf)
                    {
                        continue;
                    }

                    tmpGameObjectList.Add(child.gameObject);
                }

                cellCount = tmpGameObjectList.Count;
            }

            if (cellCount == 0)
            {
                return;
            }

            RectTransform scrollrectTra = scrollRect.transform as RectTransform;
            RectTransform content = scrollRect.content as RectTransform;

            int[] rectGrop;
            int dataHorizontalCount = 0;
            if (scrollRect.horizontal)
            {
                int horizontalCount = (int) (scrollrectTra.rect.width / m_ESVData.m_CellWidth) + (scrollrectTra.rect.width % m_ESVData.m_CellWidth > 0 ? 1 : 0);
                dataHorizontalCount = cellCount / m_ESVData.m_GropVerticalValue + (cellCount % m_ESVData.m_GropVerticalValue > 0 ? 1 : 0);
                rectGrop = new int[] {horizontalCount + 1, m_ESVData.m_GropVerticalValue};

                content.anchorMin = Vector2.zero;
                content.anchorMax = new Vector2(0, 1);
                m_PosHorizontalCount = cellCount / m_ESVData.m_GropVerticalValue + (cellCount % m_ESVData.m_GropVerticalValue > 0 ? 1 : 0);
                content.sizeDelta = new Vector2(m_PosHorizontalCount * m_ESVData.m_CellWidth, 0);
            }
            else
            {
                int verticalCount = (int) (scrollrectTra.rect.height / m_ESVData.m_CellHeight) + (scrollrectTra.rect.height % m_ESVData.m_CellHeight > 0 ? 1 : 0);
                rectGrop = new int[] {m_ESVData.m_GropHorizontalValue, verticalCount + 1};

                content.anchorMin = new Vector2(0, 1);
                content.anchorMax = Vector2.one;
                int posVerticalCount = cellCount / m_ESVData.m_GropHorizontalValue + (cellCount % m_ESVData.m_GropHorizontalValue > 0 ? 1 : 0);
                content.sizeDelta = new Vector2(0, posVerticalCount * m_ESVData.m_CellHeight);
            }

            scrollRect.content.localPosition = Vector3.zero;

            Vector2 position = Vector2.zero;
            position.x = m_ESVData.m_CellWidth / 2f + m_PaddingLeft;
            position.y = -m_ESVData.m_CellHeight / 2f - m_PaddingTop;

            int[] lineValue = {0, 0};
            int horizontalLineValue = scrollRect.horizontal ? dataHorizontalCount < rectGrop[0] ? dataHorizontalCount : rectGrop[0] : rectGrop[0];

            for (int index = 0; index < cellCount; index++)
            {
                tmpGameObjectList[index].transform.localPosition = position + new Vector2(m_ESVData.m_CellWidth * lineValue[0], -m_ESVData.m_CellHeight * lineValue[1]);

                if (lineValue[0] == horizontalLineValue - 1)
                {
                    lineValue[0] = 0;
                    lineValue[1] += 1;
                }
                else
                {
                    lineValue[0] += 1;
                }

            }

        }

        #endregion

        #region 结构

        [Serializable]
        private class ESVData
        {
            public int m_DataCount = 0;
            public int m_CellCount = 0;
            public int m_ShowCount = 0;

            /* 格子大小 */
            public int m_CellWidth = 0;
            public int m_CellHeight = 0;

            /* 排版数量 */
            public int m_GropHorizontalValue = 1;
            public int m_GropVerticalValue = 1;

            public int m_PosBorderHand = 0;
            public int m_PosBorderEnd = 0;

            public int m_CellBorderHand = 0;
            public int m_CellBorderEnd = 0;

        }

        public enum RefreshState
        {
            Single, /* 单个刷新 */
            All /* 全部刷新 */
        }

        #endregion

    }
}
