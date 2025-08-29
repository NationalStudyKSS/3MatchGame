using UnityEngine;

/// <summary>
/// 타일 타입 정의
/// 일반 색깔 타일과 특수 타일 구분
/// </summary>
public enum TileType
{
    Apple,
    Banana,
    Grape,
    Orange,
    Pear,
    Strawberry,
    Horizontal, // 가로 폭발
    Vertical,   // 세로 폭발
    Bomb        // 주변 폭발
}

public class TileModel
{
    int _row; // 행
    int _col; // 열
    TileType _type; // 타일 타입
    Sprite _sprite; // 타일 이미지

    bool _isSelected = false; // 선택 여부
    bool _isMatched = false; // 매치 여부

    public int Row => _row;
    public int Col => _col;
    public TileType Type => _type;
    public Sprite Sprite => _sprite;

    /// <summary>
    /// 타일이 선택되었는지 여부
    /// </summary>
    public bool IsSelected { get => _isSelected; set => _isSelected = value; }
    /// <summary>
    /// 타일이 매치되었는지 여부
    /// </summary>
    public bool IsMatched { get => _isMatched; set => _isMatched = value; }

    /// <summary>
    /// 타일 모델 생성자
    /// </summary>
    /// <param name="row"></param>
    /// <param name="col"></param>
    /// <param name="type"></param>
    /// <param name="sprite"></param>
    public TileModel(int row, int col, TileType type, Sprite sprite)
    {
        _row = row;
        _col = col;
        _type = type;
        _sprite = sprite;
    }

    /// <summary>
    /// 타일의 위치를 설정하는 함수
    /// </summary>
    /// <param name="row"></param>
    /// <param name="col"></param>
    public void SetPosition(int row, int col)
    {
        _row = row;
        _col = col;
    }
}
