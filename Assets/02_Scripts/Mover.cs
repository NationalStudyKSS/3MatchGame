using System;
using UnityEngine;

/// <summary>
/// 이동을 담당하는 추상 클래스
/// </summary>
public abstract class Mover : MonoBehaviour
{
    [SerializeField] protected float _moveSpeed;    // 이동 속력

    public abstract event Action<Vector3> OnMoved;  // 이동 시 발행할 이벤트

    /// <summary>
    /// 방향대로 이동시키는 함수
    /// </summary>
    /// <param name="direction"></param>
    public abstract void Move(Vector3 direction);

    /// <summary>
    /// 이동속력을 설정하는 함수
    /// </summary>
    /// <param name="moveSpeed"></param>
    public void SetMoveSpeed(float moveSpeed)
    {
        _moveSpeed = moveSpeed;
    }
}