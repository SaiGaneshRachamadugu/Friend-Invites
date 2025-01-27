using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class FriendsSearchHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private GameObject _highLightImage;
    [SerializeField] private GameObject _normalImage;

    public void OnPointerEnter(PointerEventData eventData)
    {
        _highLightImage.SetActive(true);
        _normalImage.SetActive(false);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _highLightImage.SetActive(false);
        _normalImage.SetActive(true);
    }
}
