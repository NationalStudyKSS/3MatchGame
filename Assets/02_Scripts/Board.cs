using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 보드 클래스
/// </summary>
public class Board : MonoBehaviour
{
    [Header("----- Component References -----")]
    [SerializeField] GameObject _tilePrefab;            // 타일 프리팹
    [SerializeField] Sprite[] _tileSprites;             // 타일 스프라이트 배열 

    [Header("----- Board Settings -----")]
    [SerializeField] int _boardSize = 8;                // 보드 크기 (8x8)
    [SerializeField] int _matchCount = 3;               // 매치 카운트 (3개 이상 매치)
    [SerializeField] int _tileTypeCount = 6;            // 타일 타입 개수 (6가지)
    [SerializeField] float _swapDuration = 0.15f;       // 타일 교환 애니메이션 시간
    [SerializeField] float _fallDuration = 0.2f;        // 타일 낙하 애니메이션 시간
    [SerializeField] float _matchDelay = 0.2f;          // 매치 후 대기 시간 

    Tile[,] _tiles;                                     // 타일 배열
    Tile _selectedTile = null;                          // 선택된 타일
    bool _isProcessing = false;                         // 현재 타일 움직임 처리 중인지 여부
    float _offsetX;                                     // 보드 오프셋 X
    float _offsetY;                                     // 보드 오프셋 Y

    public event Action<Tile> OnTileRemoved;            // 타일 제거 이벤트
    public event Action<int> OnScoreChanged;            // 점수 변경 이벤트
    public event Action OnBoardStable;                  // 보드 안정화 이벤트 (모든 처리 완료 후)

    public bool IsProcessing => _isProcessing;
    public int BoardSize => _boardSize;

    void Start()
    {
        InitializeBoard();
    }

    #region Board Initialization
    /// <summary>
    /// 보드를 초기화하는 함수
    /// </summary>
    void InitializeBoard()
    {
        SetupOffsets();
        _tiles = new Tile[_boardSize, _boardSize];
        CreateInitialBoard();
    }

    /// <summary>
    /// 보드 오프셋 설정
    /// 1080 * 1920 기준으로 가운대 아래쪽 정렬인데
    /// 화면 해상도마다 좀 달라져서 문제임
    /// </summary>
    void SetupOffsets()
    {
        _offsetX = (_boardSize - 1) / 2f;
        _offsetY = (_boardSize + 3) / 2f;
    }

    /// <summary>
    /// 보드판을 생성하는 함수
    /// </summary>
    void CreateInitialBoard()
    {
        for (int row = 0; row < _boardSize; row++)
        {
            for (int col = 0; col < _boardSize; col++)
            {
                CreateTileAt(row, col, GetSafeTileType(row, col));
            }
        }
    }

    /// <summary>
    /// 지정한 위치에 타일을 생성하고
    /// 위에서 생성된 경우에는 떨어뜨리는 함수도 포함하여 실행하는 함수
    /// </summary>
    /// <param name="row"></param>
    /// <param name="col"></param>
    /// <param name="type"></param>
    /// <param name="isFromTop">위에서 생성할지 여부</param>
    void CreateTileAt(int row, int col, TileType type, bool isFromTop = false)
    {
        // 시작위치는 지금 자신의 위치를 가져오는거지만
        Vector3 startPos = GetTileWorldPosition(row, col);

        // 만약에 위에서 떨어뜨려야 하는 상황이면(빈칸이 생겨서 위에서 생성해서 떨어뜨리는 경우)
        if (isFromTop)
        {
            // 보드 크기만큼 y좌표를 올려서 시작 위치로 설정
            startPos.y += _boardSize;
        }

        // 타일 생성
        GameObject tileObj = Instantiate(_tilePrefab, startPos, Quaternion.identity, transform);
        Tile tile = tileObj.GetComponent<Tile>();

        Sprite sprite = _tileSprites[(int)type];
        TileModel model = new TileModel(row, col, type, sprite);

        tile.Initialize(model);
        tile.OnTileClicked += OnTileClicked;
        _tiles[row, col] = tile;

        // 위에서 생성된 타일이면 떨어뜨리는 코루틴 실행
        if (isFromTop)
        {
            StartCoroutine(tile.MoveRoutine(GetTileWorldPosition(row, col), _fallDuration));
        }
    }

    /// <summary>
    /// 타일의 월드 좌표를 계산하여 반환하는 함수
    /// </summary>
    /// <param name="row"></param>
    /// <param name="col"></param>
    /// <returns></returns>
    Vector3 GetTileWorldPosition(int row, int col)
    {
        return new Vector3(col - _offsetX, row - _offsetY, 0);
    }

    /// <summary>
    /// 보드 생성 시 초기 매치를 방지하는 안전한 타일 타입을 반환하는 함수
    /// 내 왼쪽 두칸과 아래쪽 두칸을 검사해서 같은 타입이 있으면 그 타입을 제외한 나머지 타입 중에서 랜덤하게 선택
    /// </summary>
    /// <param name="row"></param>
    /// <param name="col"></param>
    /// <returns></returns>
    TileType GetSafeTileType(int row, int col)
    {
        List<TileType> availableTypes = new List<TileType>();

        // 일단 모든 타입을 후보로 추가
        for (int i = 0; i < _tileTypeCount; i++)
        {
            availableTypes.Add((TileType)i);
        }

        // 2부터 도는 이유는 최소 3번째 칸부터
        // 왼쪽으로 2칸을 확인할 수 있기 때문
        // 만약 내 왼쪽 2칸이 서로 같은 타입이다?
        if (col >= 2 &&
            _tiles[row, col - 1].Model.Type == _tiles[row, col - 2].Model.Type)
        {
            // 그럼 그 타입을 가능한 타입 후보에서 제거
            availableTypes.Remove(_tiles[row, col - 1].Model.Type);
        }

        // 2부터 도는 이유는 최소 3번째 칸부터
        // 아래로 2칸을 확인할 수 있기 때문
        // 만약 내 아래 2칸이 서로 같은 타입이다?
        if (row >= 2 &&
            _tiles[row - 1, col].Model.Type == _tiles[row - 2, col].Model.Type)
        {
            // 그럼 그 타입을 가능한 타입 후보에서 제거
            availableTypes.Remove(_tiles[row - 1, col].Model.Type);
        }

        // 남은 후보들 중에서 랜덤하게 선택하여 반환
        return availableTypes[UnityEngine.Random.Range(0, availableTypes.Count)];
    }
    #endregion

    #region Tile Interaction
    /// <summary>
    /// 타일 클릭 이벤트를 처리하는 함수
    /// </summary>
    /// <param name="clickedTile">선택한 타일</param>
    void OnTileClicked(Tile clickedTile)
    {
        // 현재 타일 움직임 처리 중이면 무시
        if (_isProcessing) return;

        // 선택된 타일이 없으면 클릭한 타일 선택
        if (_selectedTile == null)
        {
            SelectTile(clickedTile);
        }

        // 이미 선택된 타일이 있으면
        else
        {
            // 같은 타일을 클릭했으면
            if (_selectedTile == clickedTile)
            {
                // 선택 해제
                DeselectTile();
            }
            // 다른 타일을 클릭했고 그 타일이 인접해 있으면
            else if (IsNeighbor(_selectedTile, clickedTile))
            {
                // 타일 교환 코루틴 실행
                StartCoroutine(SwapTilesRoutine(_selectedTile, clickedTile));
            }
            // 다른 타일을 클릭했지만 인접하지 않으면
            else
            {
                // 선택된 타일을 클릭한 타일로 변경
                SelectTile(clickedTile);
            }
        }
    }

    /// <summary>
    /// 타일을 선택했을 때 실행되는 함수
    /// </summary>
    /// <param name="tile">선택한 타일</param>
    void SelectTile(Tile tile)
    {
        // 이전 선택 해제
        DeselectTile(); 
        // 지금 선택한 타일을 다시 선택된 타일로 설정
        _selectedTile = tile;
        // TODO: 선택 효과 추가
        _selectedTile.transform.localScale = Vector3.one * 1.2f;
    }

    /// <summary>
    /// 타일 선택을 해제하는 함수
    /// </summary>
    void DeselectTile()
    {
        if (_selectedTile != null)
        {
            // TODO: 선택 해제 효과 추가
            _selectedTile.transform.localScale = Vector3.one;
            // 선택된 타일 초기화
            _selectedTile = null;
        }
    }

    /// <summary>
    /// 타일 두개를 입력받아 두 타일이 인접해 있는지 검사하는 함수
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns></returns>
    bool IsNeighbor(Tile tileA, Tile tileB)
    {
        // 두 타일의 행 차이를 절대값으로 구함
        int rowDiff = Mathf.Abs(tileA.Model.Row - tileB.Model.Row);

        // 두 타일의 열 차이를 절대값으로 구함
        int colDiff = Mathf.Abs(tileA.Model.Col - tileB.Model.Col);

        // 서로 가로로 붙어있거나 세로로 붙어있으면 true 반환
        return (rowDiff == 1 && colDiff == 0) || (rowDiff == 0 && colDiff == 1);
    }
    #endregion

    #region Tile Swapping
    /// <summary>
    /// 타일 두개를 입력받아 두 타일을 교환하는 코루틴
    /// </summary>
    /// <param name="tileA"></param>
    /// <param name="tileB"></param>
    /// <returns></returns>
    IEnumerator SwapTilesRoutine(Tile tileA, Tile tileB)
    {
        // 타일 움직임 처리 중으로 설정
        _isProcessing = true;

        // 선택된 타일을 해제
        DeselectTile();

        // 타일 교환 코루틴 실행
        yield return StartCoroutine(SwapTilePositions(tileA, tileB));

        // 매치 여부를 검사하여 매치가 있다면
        if (HasMatches())
        {
            // 매치 처리 코루틴 실행
            yield return StartCoroutine(ProcessAllMatches());
        }
        // 매치가 없다면
        else
        {
            // 두 타일을 교환하는 코루틴을 다시 실행하여 롤백
            yield return StartCoroutine(SwapTilePositions(tileB, tileA));
        }

        // 타일 움직임 처리를 끝냈으므로 false로 설정
        _isProcessing = false;

        // 보드가 안정화 되었음을 알리는 이벤트 실행
        OnBoardStable?.Invoke();
    }

    /// <summary>
    /// 두 타일의 '데이터'를 교환 후 'transform.position'을 교환하는 코루틴
    /// </summary>
    /// <param name="tileA"></param>
    /// <param name="tileB"></param>
    /// <returns></returns>
    IEnumerator SwapTilePositions(Tile tileA, Tile tileB)
    {
        // 모델 데이터 교환
        // 여기서는 아직 겉으로 보이는 transform.position은 바뀌지 않음
        SwapTileData(tileA, tileB);

        // 서로의 transform.position 저장
        Vector3 posA = GetTileWorldPosition(tileA.Model.Row, tileA.Model.Col);
        Vector3 posB = GetTileWorldPosition(tileB.Model.Row, tileB.Model.Col);

        // 서로의 trnansform.position을 교환하는 코루틴을 준비해서(변수에 담아서)
        Coroutine moveA = StartCoroutine(tileA.MoveRoutine(posA, _swapDuration));
        Coroutine moveB = StartCoroutine(tileB.MoveRoutine(posB, _swapDuration));

        // 코루틴 실행
        yield return moveA;
        yield return moveB;
    }

    /// <summary>
    /// 두 타일의 'Model' 데이터를 교환하는 코루틴
    /// </summary>
    /// <param name="tileA"></param>
    /// <param name="tileB"></param>
    void SwapTileData(Tile tileA, Tile tileB)
    {
        // 배열에서의 위치 정보 저장
        int rowA = tileA.Model.Row;
        int colA = tileA.Model.Col;
        int rowB = tileB.Model.Row;
        int colB = tileB.Model.Col;

        // 모델 위치 정보 교환
        tileA.Model.SetPosition(rowB, colB);
        tileB.Model.SetPosition(rowA, colA);

        // 배열에서 타일 위치 교환
        _tiles[rowA, colA] = tileB;
        _tiles[rowB, colB] = tileA;
    }
    #endregion

    #region Match Detection and Processing
    /// <summary>
    /// 보드에서 매치가 있는지 여부를 검사하고 반환하는 함수
    /// </summary>
    bool HasMatches()
    {
        // 타일들의 IsMatched 플래그 초기화 함수 실행
        ClearMatchFlags();
        // 매치 발견 여부 플래그 초기화
        bool hasMatch = false;

        // 가로 매치 검사
        if(CheckHorizontalMatches())
        {
            hasMatch = true;
        }

        // 세로 매치 검사
        if(CheckVerticalMatches())
        {
            hasMatch = true;
        }

        return hasMatch;
    }

    /// <summary>
    /// 모든 타일의 매치 여부 초기화
    /// </summary>
    void ClearMatchFlags()
    {
        for (int row = 0; row < _boardSize; row++)
        {
            for (int col = 0; col < _boardSize; col++)
            {
                if (_tiles[row, col] != null)
                {
                    _tiles[row, col].Model.IsMatched = false;
                }
            }
        }
    }

    /// <summary>
    /// 가로 매치 검사
    /// </summary>
    bool CheckHorizontalMatches()
    {
        // 매치를 찾았는지 여부 boolean 변수
        bool foundMatch = false;

        // 한 행씩 올라가면서
        for (int row = 0; row < _boardSize; row++)
        {
            // 현재 매치 길이(3이상 되면 매치로 간주)
            int matchLength = 1;

            // 현재 타일 타입 가져오기
            TileType currentType = _tiles[row, 0]?.Model.Type??TileType.Apple;

            // 한 열씩 오른쪽으로 이동하면서
            for (int col = 1; col < _boardSize; col++)
            {
                // 만약 이번칸의 타일 타입이 이전 칸과 같으면
                if (_tiles[row, col].Model.Type == currentType)
                {
                    // 매치 길이 증가
                    matchLength++;
                }
                // 이번칸의 타일 타입이 이전 칸과 다르면
                else
                {
                    // 지금까지 카운트한 매치 길이가 매치 카운트 이상이면
                    if (matchLength >= _matchCount)
                    {
                        // 매치된 타일들 표시 함수 실행
                        MarkMatchedTiles(row, col - matchLength, row, col - 1);
                        // 매치 발견 여부 true로 설정
                        foundMatch = true;
                    }
                    // 현재 타일을 검사할 타일 타입으로 설정하고
                    currentType = _tiles[row, col].Model.Type;
                    // 매치 길이 초기화
                    matchLength = 1;
                }
            }

            // 행 끝에서 매치 확인(마지막 칸까지 매치된 경우)
            if (matchLength >= _matchCount)
            {
                MarkMatchedTiles(row, _boardSize - matchLength, row, _boardSize - 1);
                foundMatch = true;
            }
        }

        // 매치 발견 여부 반환
        return foundMatch;
    }

    /// <summary>
    /// 세로 매치 검사
    /// </summary>
    bool CheckVerticalMatches()
    {
        bool foundMatch = false;

        for (int col = 0; col < _boardSize; col++)
        {
            int matchLength = 1;
            TileType currentType = _tiles[0, col].Model.Type;

            for (int row = 1; row < _boardSize; row++)
            {
                if (_tiles[row, col].Model.Type == currentType)
                {
                    matchLength++;
                }
                else
                {
                    if (matchLength >= _matchCount)
                    {
                        MarkMatchedTiles(row - matchLength, col, row - 1, col);
                        foundMatch = true;
                    }
                    currentType = _tiles[row, col].Model.Type;
                    matchLength = 1;
                }
            }

            // 열 끝에서 매치 확인
            if (matchLength >= _matchCount)
            {
                MarkMatchedTiles(_boardSize - matchLength, col, _boardSize - 1, col);
                foundMatch = true;
            }
        }

        return foundMatch;
    }

    /// <summary>
    /// 가로 혹은 세로로 연속으로 매치된 타일들을 IsMatched 플래그를 true로 설정하는 함수
    /// </summary>
    /// <param name="startRow"></param>
    /// <param name="startCol"></param>
    /// <param name="endRow"></param>
    /// <param name="endCol"></param>
    void MarkMatchedTiles(int startRow, int startCol, int endRow, int endCol)
    {
        for (int row = startRow; row <= endRow; row++)
        {
            for (int col = startCol; col <= endCol; col++)
            {
                if (_tiles[row, col] != null)
                {
                    _tiles[row, col].Model.IsMatched = true;
                }
            }
        }
    }

    /// <summary>
    /// 매치가 확인되었을 때 실행되는 코루틴
    /// 매치된 타일을 제거하고 타일을 낙하시키고 빈칸을 채우는 일련의 과정을 처리
    /// 매치가 계속된다면 반복한다.
    /// </summary>
    IEnumerator ProcessAllMatches()
    {
        do
        {
            yield return new WaitForSeconds(_matchDelay);
            RemoveMatchedTiles();
            yield return new WaitForSeconds(_matchDelay);
            yield return StartCoroutine(DropTiles());
            FillEmptySpaces();
            yield return new WaitForSeconds(_fallDuration);
        }
        while (HasMatches());
    }

    /// <summary>
    /// 매치된 타일들 제거하는 함수
    /// </summary>
    void RemoveMatchedTiles()
    {
        // 보드를 전체 돌면서
        for (int row = 0; row < _boardSize; row++)
        {
            for (int col = 0; col < _boardSize; col++)
            {
                // 타일이 매치된 상태이면
                if (_tiles[row, col].Model.IsMatched == true)
                {
                    // 타일이 제거되었음을 알리는 이벤트 실행
                    OnTileRemoved?.Invoke(_tiles[row, col]);
                    // 타일 제거(비유하자면 이사짐 빼고 주소도 지운거임...)
                    Destroy(_tiles[row, col].gameObject);
                    // 배열 비워주기(비유하자면 아파트 관리소에 가서 우리집 이사간다고 말한거임...)
                    _tiles[row, col] = null;
                }
            }
        }
    }

    /// <summary>
    /// 타일이 떨어지는 것을 처리하는 코루틴
    /// </summary>
    IEnumerator DropTiles()
    {
        List<Coroutine> dropCoroutines = new List<Coroutine>();

        for (int col = 0; col < _boardSize; col++)
        {
            int writeIndex = 0;

            for (int row = 0; row < _boardSize; row++)
            {
                if (_tiles[row, col] != null)
                {
                    if (writeIndex != row)
                    {
                        // 타일을 아래로 이동
                        _tiles[writeIndex, col] = _tiles[row, col];
                        _tiles[row, col] = null;

                        // 모델 위치 업데이트
                        _tiles[writeIndex, col].Model.SetPosition(writeIndex, col);

                        // 애니메이션으로 이동
                        Vector3 newPos = GetTileWorldPosition(writeIndex, col);
                        Coroutine dropCoroutine = StartCoroutine(_tiles[writeIndex, col].MoveRoutine(newPos, _fallDuration));
                        dropCoroutines.Add(dropCoroutine);
                    }
                    writeIndex++;
                }
            }
        }

        // 모든 낙하 애니메이션 완료까지 대기
        foreach (var coroutine in dropCoroutines)
        {
            yield return coroutine;
        }
    }

    /// <summary>
    /// 빈 타일이 생겼을 때 그 타일에 대응하는 위치에(보드 위쪽) 랜덤한 타입을 갖는
    /// 새로운 타일을 생성하고 떨어뜨리는 함수(CreateTileAt)를 실행하는 함수
    /// </summary>
    void FillEmptySpaces()
    {
        for (int col = 0; col < _boardSize; col++)
        {
            for (int row = _boardSize - 1; row >= 0; row--)
            {
                if (_tiles[row, col] == null)
                {
                    TileType randomType = (TileType)UnityEngine.Random.Range(0, _tileTypeCount);
                    CreateTileAt(row, col, randomType, true);
                }
            }
        }
    }
    #endregion

    void HorizontalBomb(Tile tile)
    {
        int row = tile.Model.Row;
        int col = tile.Model.Col;
        for (int c = col - 1; c <= col + 1; c++)
        {
            if (IsValidPosition(row, c) && _tiles[row, c] != null)
            {
                _tiles[row, c].Model.IsMatched = true;
            }
        }
    }

    #region Utility Methods
    /// <summary>
    /// 유효한 보드 좌표인지 확인
    /// </summary>
    bool IsValidPosition(int row, int col)
    {
        return row >= 0 && row < _boardSize && col >= 0 && col < _boardSize;
    }

    /// <summary>
    /// 지정된 위치의 타일 반환
    /// </summary>
    public Tile GetTileAt(int row, int col)
    {
        if (!IsValidPosition(row, col)) return null;
        return _tiles[row, col];
    }
    #endregion
}