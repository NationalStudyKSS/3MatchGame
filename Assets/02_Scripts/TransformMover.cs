using System;
using UnityEngine;

public class TransformMover : Mover
{
    public override event Action<Vector3> OnMoved;

    public override void Move(Vector3 direction)
    {
        if (direction == Vector3.zero) return;
        // 방향 벡터의 길이를 1로 정규화
        direction.Normalize();
        // 이동할 거리 계산
        Vector3 moveDelta = direction * _moveSpeed * Time.deltaTime;
        // 실제 이동
        transform.position += moveDelta;
        // 이동 이벤트 발행
        OnMoved?.Invoke(moveDelta);
    }
}