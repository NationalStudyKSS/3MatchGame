using System;
using UnityEngine;

public class TransformMover : Mover
{
    public override event Action<Vector3> OnMoved;

    public override void Move(Vector3 direction)
    {
        if (direction == Vector3.zero) return;
        // ���� ������ ���̸� 1�� ����ȭ
        direction.Normalize();
        // �̵��� �Ÿ� ���
        Vector3 moveDelta = direction * _moveSpeed * Time.deltaTime;
        // ���� �̵�
        transform.position += moveDelta;
        // �̵� �̺�Ʈ ����
        OnMoved?.Invoke(moveDelta);
    }
}