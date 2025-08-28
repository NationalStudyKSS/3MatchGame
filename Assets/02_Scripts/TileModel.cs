using UnityEngine;

/// <summary>
/// 타일 타입 정의
/// 일반 색깔 타일과 특수 타일 구분
/// </summary>
public enum TileType
{
    Red,
    Orange,
    Yellow,
    Green,
    Blue,
    Purple,
    Horizontal, // 가로 폭발
    Vertical,   // 세로 폭발
    Bomb        // 주변 폭발
}

public class TileModel
{
    [SerializeField] int _row; // 행
    [SerializeField] int _col; // 열
    [SerializeField] TileType _type; // 타일 타입
    [SerializeField] Sprite _sprite; // 타일 이미지

    [SerializeField] bool _isSelected = false; // 선택 여부
    [SerializeField] bool _isMatched = false; // 매치 여부

    public int Row => _row;
    public int Col => _col;
    public TileType Type => _type;
    public Sprite Sprite => _sprite;

    public bool IsSelected { get => _isSelected; set => _isSelected = value; }
    public bool IsMatched { get => _isMatched; set => _isMatched = value; }

    public TileModel(int row, int col, TileType type, Sprite sprite)
    {
        _row = row;
        _col = col;
        _type = type;
        _sprite = sprite;
    }
}
