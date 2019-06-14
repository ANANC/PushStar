using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIHpViewController : BaseUIController
{
    private Transform m_Transform;
    private Image m_BgImage;
    private GameObject m_OtherImageGameObject;
    private List<Image> m_OtherImages;
    private float m_Width;

    private HpData m_HpData;

    public override void Create(Transform transform)
    {
        m_Transform = transform;
        m_BgImage = m_Transform.Find("Bg").GetComponent<Image>();
        m_OtherImageGameObject = m_Transform.Find("Other/OtherColor").gameObject;
        m_OtherImages = new List<Image>();
        m_Width = m_BgImage.rectTransform.rect.width;
    }

    public void SetData(HpData hpData)
    {
        m_HpData = hpData;
        m_BgImage.color = GameDefine.CampColor[m_HpData.m_Camp];
        UpdateOtherColor();
    }

    private void AddOtherColor()
    {
        GameObject go = GameObject.Instantiate(m_OtherImageGameObject);
        go.transform.SetParent(m_OtherImageGameObject.transform.parent);
        go.transform.localScale = Vector3.one;
        Image image = go.GetComponent<Image>();
        m_OtherImages.Add(image);
    }

    private void UpdateOtherColor()
    {
        int cellCount = m_OtherImages.Count;
        int dataCount = m_HpData.m_CurHurtCount;
        int updateCount = cellCount > dataCount ? cellCount : dataCount;

        for (int index = 0; index < updateCount; index++)
        {
            if (index < dataCount)
            {
                if (index >= cellCount)
                {
                    AddOtherColor();
                }

                Image image = m_OtherImages[index];
                image.color = GameDefine.CampColor[m_HpData.m_HurtCamps[index]];
                RectTransform rectTransform = image.transform as RectTransform;
                Vector2 size = rectTransform.rect.size;
                size.x = m_HpData.m_HurtDict[index].m_CurHp / m_HpData.m_HurtDict[index].m_MaxHp;
                size.x *= m_Width;
                rectTransform.sizeDelta = size;
                image.gameObject.SetActive(true);
            }
            else if (index <= cellCount)
            {
                m_OtherImages[index].gameObject.SetActive(false);
            }
        }
    }


}
