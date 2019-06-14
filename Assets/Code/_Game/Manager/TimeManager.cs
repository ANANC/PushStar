using System;
using System.Collections.Generic;


public class TimeManager : BaseManager
{
    private List<Timer> m_TimerList = new List<Timer>();
    private List<Timer> m_DeleteList = new List<Timer>();
    
    public override void Update()
    {
        Timer curTimer;

        for (int index = 0; index < m_TimerList.Count; index++)
        {
            curTimer = m_TimerList[index];

            if (curTimer.Playing())
            {
                curTimer.Update();
            }
            else
            {
                TimerEndCallback(curTimer);
            }
        }

        for (int index = 0; index < m_DeleteList.Count; index++)
        {
            curTimer = m_DeleteList[index];
            m_TimerList.Remove(curTimer);
        }
    }

    public void CreatTimer(float time, float interval, Action<float, object> update, Action<object> endCallback,
        object param)
    {
        Timer timer = new Timer(time, interval, update, endCallback, param);
        timer.Start();
        m_TimerList.Add(timer);
    }

    private void TimerEndCallback(Timer timer)
    {
        m_DeleteList.Add(timer);
    }
}
