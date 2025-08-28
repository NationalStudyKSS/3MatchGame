using System;
using UnityEngine;
using UnityEngine.EventSystems;



/// <summary>
/// Match-3 게임에서 사용되는 타일 클래스
/// Model + View + Controller 역할 일부 수행
/// </summary>
[RequireComponent(typeof(SpriteRenderer), typeof(BoxCollider2D))]
public class Tile : MonoBehaviour, IPointerClickHandler
{

    public void OnPointerClick(PointerEventData eventData)
    {
        throw new NotImplementedException();
    }
}
