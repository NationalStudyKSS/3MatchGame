using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// 타일 클래스
/// </summary>
public class Tile : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] SpriteRenderer _renderer;

    TileModel _model;

    public TileModel Model => _model;

    public event Action<Tile> OnTileClicked;

    public void Initialize(TileModel model)
    {
        _model = model;
        _renderer.sprite = _model.Sprite;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        OnTileClicked?.Invoke(this);
        Debug.Log($"Tile Clicked: ({_model.Row}, {_model.Col}) Type: {_model.Type}");
    }

    /// <summary>
    /// 타일을 목표지점으로 일정 시간 동안 이동시키는 코루틴
    /// </summary>
    /// <param name="targetPosition">목표 지점</param>
    /// <param name="duration">이동에 걸리는 시간</param>
    /// <returns></returns>
    public IEnumerator MoveRoutine(Vector3 targetPosition, float duration)
    {
        Vector3 startPosition = transform.position;
        float timer = 0f;
        while (timer < duration)
        {
            transform.position = Vector3.Lerp(startPosition, targetPosition, timer / duration);
            timer += Time.deltaTime;
            yield return null;
        }
        transform.position = targetPosition;
    }
}
