using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class StackAttackTilemap : MonoBehaviour
{
    [SerializeField] private Tilemap _tilemap;            // Ссылка на Tilemap
    [SerializeField] private Tilemap _tilemapSecond;
    [SerializeField] private SpriteRenderer _grid;

    [Header("Tailes on playboard")]
    [SerializeField] private TileBase _playerTile;        // Тайл игрока
    [SerializeField] private TileBase _playerTileAnimated;        // Тайл игрока
    [SerializeField] private TileBase[] _craneTile = new TileBase[2];         // Тайл крана
    [SerializeField] private TileBase _emptyTile;         // Тайл пустоты
    [SerializeField] private TileBase _emptyTilePlayer;
    [SerializeField] TileBase[] _boxesTiles = new TileBase[8];
    private HashSet<TileBase> _tilesBoxHash = new HashSet<TileBase>();
    [SerializeField] private AnimatedTile _animatedTile;
    [SerializeField] TileBase[] _goodsTiles = new TileBase[2];
    private HashSet<TileBase> _goodsTilesHash = new HashSet<TileBase>();

    [Header("Destroing Sprites")]
    [SerializeField] private Sprite[] _destroingSprites;

    [Header("Objects for smooth transition")]
    [SerializeField] private GameObject _crane;
    private Queue<GameObject> _cranePool;
    [SerializeField] private GameObject _player;
    [SerializeField] private GameObject _box;
    private Queue<GameObject> _boxPool;
    [SerializeField] private GameObject _boxAnimated;
    [SerializeField] private GameObject _goodsObject;
    private Queue<GameObject> _goodsPool;

    [Header("Control")]
    [SerializeField] private JoyStick _joyStick;
    [SerializeField] private TextJumps _secondJump;

    private Vector3Int _playerPosition = Vector3Int.zero;
    private bool _isMovingHorizontally;
    private bool _isFalling;
    public bool IsFalling
    {
        get { return _isFalling; }
        set { _isFalling = value; }
    }
    private bool _isGrounded;
    public bool IsGroundedPublic => _isGrounded;
    public bool IsPushing { get; private set; }
    private RectInt _bounds;
    private int _boundYForFall;
    private Vector3Int[] _cells;
    private Vector3Int _direction = Vector3Int.zero;
    public Vector3Int Direction => _direction;
    private bool _playerIsMoving = false;
    public bool PlayerIsMoving => _playerIsMoving;

    [HideInInspector]
    public bool PlayerIsHited = false;
    private float SPEEDPLAYER = 2f; // 2
    private float SPEEDBOX = 1.5f; // 1.5
    private float SPEEDCONVEER = 2.5f; // 2.5
    private float _chanseToSpawnBox = 3; // 3
    private float _spawningTime = 1f; // 1
    private float _SpawningTimeGoods;// 60
    private Vector3Int _boxSpawnPointLeft;
    private Vector3Int _boxSpawnPointRight;
    private float _timeToSpawnBox = 0;
    private Dictionary<Vector3Int, GameObject> _objectsMatrix;
    private HashSet<int> _dotsOfSpawn = new HashSet<int>();

    private bool _canDoubleJump = true;
    public bool CanDoubleJump => _canDoubleJump;

    private void Awake() ///////////////////// AWAKE
    {
        SetBounds();
        _boundYForFall = _bounds.yMax - 1;
        CreateBoxPool(4);
        CreateCranePool(3);
        CreateGoodsPool(1);
        _timeToSpawnBox = Time.time;
        _SpawningTimeGoods = Time.time;
        _objectsMatrix = new Dictionary<Vector3Int, GameObject>();
        SetBoxesToHash();
        SetGoodsToHash();
    }

    #region ////////////////// CranePoolManager

    private void CreateCranePool(int craneSize)
    {
        _cranePool = new Queue<GameObject>();
        for (int i = 0; i < craneSize; i++)
        {
            GameObject crane = Instantiate(_crane);
            crane.SetActive(false);
            _cranePool.Enqueue(crane);
        }
    }

    public GameObject GetCrane()
    {
        if (_cranePool.Count > 0)
        {
            GameObject crane = _cranePool.Dequeue();
            crane.SetActive(true);
            return crane;
        }
        else
        {
            // Если все объекты заняты, создаём новую коробку
            GameObject newCrane = Instantiate(_crane);
            newCrane.SetActive(false);
            _cranePool.Enqueue(newCrane);
            return GetCrane();
        }
    }

    public void ReturnCrane(GameObject crane)
    {
        crane.SetActive(false);  // Делаем неактивным
        _cranePool.Enqueue(crane);  // Возвращаем в очередь
    }

    #endregion

    #region ////////////////// BoxPoolManager

    private void CreateBoxPool(int poolSize)
    {
        _boxPool = new Queue<GameObject>();
        for (int i = 0; i < poolSize; i++)
        {
            GameObject box = Instantiate(_box);
            box.SetActive(false);
            _boxPool.Enqueue(box);
        }
    }

    public GameObject GetBox()
    {
        if (_boxPool.Count > 0)
        {
            GameObject box = _boxPool.Dequeue();
            box.SetActive(true);
            return box;
        }
        else
        {
            // Если все объекты заняты, создаём новую коробку
            GameObject newBox = Instantiate(_box);
            newBox.SetActive(false);
            _boxPool.Enqueue(newBox);
            return GetBox();
        }
    }

    public void ReturnBox(GameObject box)
    {
        box.SetActive(false);  // Делаем неактивным
        _boxPool.Enqueue(box);  // Возвращаем в очередь
    }

    #endregion

    #region ////////////////// GoodsPoolManager

    private void CreateGoodsPool(int goodsSize)
    {
        _goodsPool = new Queue<GameObject>();
        for (int i = 0; i < goodsSize; i++)
        {
            GameObject goods = Instantiate(_goodsObject);
            goods.SetActive(false);
            _goodsPool.Enqueue(goods);
        }
    }

    public GameObject GetGoods()
    {
        if (_goodsPool.Count > 0)
        {
            GameObject goods = _goodsPool.Dequeue();
            goods.SetActive(true);
            return goods;
        }
        else
        {
            // Если все объекты заняты, создаём новую коробку
            GameObject newGoods = Instantiate(_goodsObject);
            newGoods.SetActive(false);
            _goodsPool.Enqueue(newGoods);
            return GetGoods();
        }
    }

    public void ReturnGoods(GameObject goods)
    {
        goods.SetActive(false);  // Делаем неактивным
        _boxPool.Enqueue(goods);  // Возвращаем в очередь
    }

    #endregion

    #region ////////////////// SetBoardSize

    private void SetBounds()
    {
        Vector2Int position = Vector2Int.zero;
        Vector2Int boardSize = BoardSizeToGrid();
        position.x = -boardSize.x / 2;
        position.y = -boardSize.y / 2;
        _bounds = new RectInt(position, boardSize);
    }

    private Vector2Int BoardSizeToGrid()
    {
        Vector2Int boardSize = Vector2Int.zero;
        boardSize.x = (int)_grid.size.x;
        boardSize.y = (int)_grid.size.y;
        return boardSize;
    }

    #endregion

    #region ////////////////// PlayerCreator

    private void SpawnPlayer()
    {
        InitCells();
        _tilemap.SetTile(FindSpawnPosition(), _emptyTilePlayer);
        _player.transform.position = _tilemap.GetCellCenterWorld(_playerPosition);
        _player.SetActive(true);
    }

    private void InitCells()
    {
        Vector3Int[] cells = new Vector3Int[2] { Vector3Int.zero, Vector3Int.zero };
        for (int i = 0; i < cells.Length; i++)
        {
            cells[i].x = 0;
            cells[i].y = i;
        }
        // new Vector2Int[] { new Vector2Int(0, 0), new Vector2Int(0, 1)}
        _cells = cells;
    }

    private Vector3Int FindSpawnPosition()
    {
        Vector3Int spawnPosition = Vector3Int.zero;
        spawnPosition.x = Random.Range(_bounds.xMin, _bounds.xMax);
        for (int i = _bounds.yMin; ; i++)
        {
            spawnPosition.y = i;
            if (IsValidPositionBounds(spawnPosition) && !_tilemap.HasTile(spawnPosition))
                break;
        }
        //Debug.Log($"{spawnPosition.x} {spawnPosition.y}");
        _playerPosition = spawnPosition;
        return spawnPosition;
    }

    #endregion

    void Start() ///////////////////// START
    {
        Time.timeScale = 1f;
        SpawnBoxesOnStart();
        SpawnPlayer();
        SetSpawners();
    }

    private void FixedUpdate()
    {
        BoxSpawning();
    }

    void LateUpdate() ///////////////////// UPDATE
    {
        CheckForDotsOfSpawn();

        // Обновление падения коробок
        UpdateBoxes();

        // Управление игроком
        PlayerController();
    }

    #region ////////////////// Box Controller

    // Функция для проверки падения коробок
    private void UpdateBoxes()
    {
        Vector3Int currentPos = Vector3Int.zero;
        Vector3Int belowPos = Vector3Int.zero;

        bool rowIsFull = false;

        for (int y = _bounds.yMin; y < _boundYForFall; y++)
        {
            if (y == _bounds.yMin)
                rowIsFull = true;

            for (int x = _bounds.xMin; x < _bounds.xMax; x++)
            {
                currentPos.x = x;
                currentPos.y = y;

                bool isBox = IsBoxTile(currentPos, _tilemap);
                bool isGoods = IsGoodsTile(currentPos, _tilemap);

                if (isBox || isGoods)
                {
                    belowPos = currentPos + Vector3Int.down;

                    if (_tilemap.GetTile(belowPos) == null)
                    {
                        TileBase tile = _tilemap.GetTile(currentPos);
                        if (isBox)
                            StartCoroutine(CorutineBoxFall(currentPos, Vector3Int.down, GetBox(), tile));
                        else if (isGoods && _tilemap.GetColor(currentPos) == Color.white)
                            StartCoroutine(CorutineGoodsFall(currentPos, Vector3Int.down, GetGoods(), tile));
                    }
                    else if (belowPos == _playerPosition)
                    {
                        if (isBox)
                            BoxHitPlayer(currentPos);
                        else if (isGoods)
                            GoodsTouched(currentPos);
                    }
                }
                else rowIsFull = false;
            }
            if (rowIsFull)
                ClearLine(_bounds.yMin);
        }
    }

    private void GoodsTouched(Vector3Int position)
    {
        if (_tilemap.GetTile(position) == _goodsTiles[0])
            EventSystem.OnLivesChanged(1);
        else
            EventSystem.OnPlayerJumpedTwice(1);
        _tilemap.SetTile(position, null);
    }

    private void ClearLine(int line)
    {
        Vector3Int position = Vector3Int.zero;
        for (int x = _bounds.xMin; x < _bounds.xMax; x++)
        {
            position.x = x;
            position.y = line;
            StartCoroutine(AnimateBoxDestruction(position));
        }
        EventSystem.OnSalaryChanged(120);
    }

    private void SetBoxesToHash()
    {
        for (int i = 0; i < _boxesTiles.Length; i++)
            _tilesBoxHash.Add(_boxesTiles[i]);
    }

    private bool IsBoxTile(Vector3Int currentPos, Tilemap tilemap)
    {
        return _tilesBoxHash.Contains(tilemap.GetTile(currentPos));
    }

    private void SetGoodsToHash()
    {
        for (int i = 0; i < _goodsTiles.Length; i++)
            _goodsTilesHash.Add(_goodsTiles[i]);
    }

    private bool IsGoodsTile(Vector3Int position, Tilemap tilemap)
    {
        return _goodsTilesHash.Contains(tilemap.GetTile(position));
    }

    private TileBase ReturnTileFromHash (Sprite sprite)
    {
        foreach (var e in _tilesBoxHash)
            if ((e as Tile).sprite == sprite)
                return e;
        return _boxesTiles[0];
    }

    private void SpawnBoxesOnStart()
    {
        Vector3Int position = Vector3Int.zero;
        for (int y = _bounds.yMin; y < _bounds.yMin + 2; y++)
        {
            if (Random.Range(0, 1f) > 0.85f) continue;
            for (int x = _bounds.xMin; x < _bounds.xMax; x++)
            {
                if (Random.Range(0, 1f) > 0.4f) continue;
                position.x = x;
                position.y = y;
                if (_tilemap.HasTile(position + Vector3Int.down))
                    _tilemap.SetTile(position, _boxesTiles[Random.Range(0, _boxesTiles.Length)]);
            }
        }
    }

    private void BoxHitPlayer(Vector3Int boxPosition)
    {
        EventSystem.OnSalaryChanged(-10);
        EventSystem.OnLivesChanged(-1);
        CommonSettings.SetSetting();
        PlayerIsHited = true;
        _tilemap.SetTile(boxPosition, null);
        _boxAnimated.transform.position = _tilemap.GetCellCenterWorld(boxPosition);
        _boxAnimated.SetActive(true);
    }

    private void CheckForDotsOfSpawn()
    {
        Vector3Int boxPosition = Vector3Int.zero;
        boxPosition.y = 1;
        for (int x = _bounds.xMin; x < _bounds.xMax; x++)
        {
            boxPosition.x = x;
            if (!_tilemap.HasTile(boxPosition) || _tilemap.GetTile(boxPosition) == _emptyTilePlayer) //
                _dotsOfSpawn.Add(x);
            else
                _dotsOfSpawn.Remove(x);
        }
    }

    #endregion

    #region ////////////////// Player Controller

    private void PlayerController()
    {
        if (_playerIsMoving || PlayerIsHited) return;

        // Обработка ввода для перемещения игрока
        int dx = 0;
        int dy = 0;
        _isGrounded = IsGrounded(_playerPosition);
        if (_isGrounded)
        {
            _canDoubleJump = true;
            _isFalling = false;
        }

        if (Input.touchCount > 0) //Input.GetMouseButton(0) || Input.GetMouseButton(1))
        {
            dx = _joyStick.Horizontal();
            dy = _joyStick.Vertical();
        }
        else if (Input.anyKey)
        {
            if (Input.GetKey(KeyCode.A)) { dx = -1; }
            if (Input.GetKey(KeyCode.D)) { dx = 1; }
            if (Input.GetKeyDown(KeyCode.W)) { dy = 1; }
            if (Input.GetKeyDown(KeyCode.Q)) { dx = -1; dy = 1; }
            if (Input.GetKeyDown(KeyCode.E)) { dx = 1; dy = 1; }
        }

        _direction.x = dx;
        _direction.y = dy;

        if (_isFalling) Fall();
        else if (dx != 0 && dy != 0 && _isGrounded) { JumpDiagonal(_direction); _isMovingHorizontally = false; }
        else if (_isMovingHorizontally && !_isGrounded) { _isMovingHorizontally = false; _isFalling = true; MovePlayer(Vector3Int.down); }
        else if (dx != 0) { MovePlayer(_direction); _isMovingHorizontally = true; }
        else if (dy == 1 && _isGrounded) { MovePlayer(_direction); _isMovingHorizontally = false; }
        else if (!_isGrounded)
        {
            if (Input.GetKey(KeyCode.W) && _canDoubleJump && _secondJump.CountOfJumps > 0)
                DoubleJump();
            else
            _isFalling = true;
        }
    }

    public void DoubleJump()
    {
        if (_playerIsMoving || IsGrounded(_playerPosition)) return;
        EventSystem.OnPlayerJumpedTwice(-1);
        CommonSettings.SetSetting();
        MovePlayer(Vector3Int.up);
        _isMovingHorizontally = false;
        _direction.y = 1;
        _canDoubleJump = false;
    }

    public void Jump()
    {
        if (_playerIsMoving) return;
        if (IsGrounded(_playerPosition))
        {
            _direction.y = 1;
            _direction.x = 0;
            _isMovingHorizontally = false;
            MovePlayer(_direction);
        }
    }

    // Перемещение игрока
    private void MovePlayer(Vector3Int direction)
    {
        Vector3Int newPosition = _playerPosition + direction;

        // Проверка границ Tilemap
        if (!IsValidPositionBounds(newPosition)) return;

        if (IsGoodsTile(newPosition, _tilemap))
            GoodsTouched(newPosition);

        // Если клетка свободна, перемещаем игрока
        if (IsValidPositionBoxes(newPosition))
        {
            StartCoroutine(CorutinePlayer(_playerPosition, direction, false));
            _playerPosition = newPosition;
        }
        // Если игрок пытается сдвинуть коробку
        else
        {
            Vector3Int boxPosition = ReturnPositionOfBoxe(newPosition);
            if ( boxPosition == Vector3Int.one || _tilemap.GetTile(boxPosition) == _playerTile) return;

            if (IsGoodsTile(boxPosition, _tilemap)) {
                GoodsTouched(boxPosition);
                return;
            }

            Vector3Int boxNewPosition = boxPosition + direction;

            // Проверка, можно ли сдвинуть коробку (если за ней пусто)
            if (!_tilemap.HasTile(boxNewPosition) && _bounds.Contains((Vector2Int)boxNewPosition))
            {
                IsPushing = true;
                TileBase tile = _tilemap.GetTile(boxPosition);
                Vector3Int boxDirection = direction;

                StartCoroutine(CorutineBoxMove(boxPosition, boxDirection, GetBox(), tile));
                StartCoroutine(CorutinePlayer(_playerPosition, direction, false));
                _playerPosition = newPosition;
            }
        }
    }

    private void Fall()
    {
        Vector3Int newPosition = _playerPosition + Vector3Int.down;
        if (!_tilemap.HasTile(newPosition) || IsGoodsTile(newPosition,_tilemap))
            MovePlayer(Vector3Int.down);
    }

    private void JumpDiagonal(Vector3Int direction)
    {
        Vector3Int blockPosition = _playerPosition;
        blockPosition.x += direction.x;

        if (_tilemap.HasTile(blockPosition)) return;

        Vector3Int newPosition = _playerPosition + direction;

        if (!_tilemap.HasTile(newPosition) && IsValidPositionBounds(newPosition))
        {
            StartCoroutine(CorutinePlayer(_playerPosition, direction, true));
            _playerPosition = newPosition;
        }
    }

    #endregion

    // Передвижение объектов для плавшых переходов между тайлами


    #region /// Coruines

    private IEnumerator CorutinePlayer (Vector3Int position, Vector3Int direction, bool isDiogonal)
    {
        Vector3Int newPosition = position + direction;
        _playerIsMoving = true;
        _tilemap.SetTile(position, null);
        if (isDiogonal) _tilemap.SetTile(newPosition, _emptyTilePlayer);
        _player.transform.position = _tilemap.GetCellCenterWorld(position);
        Transform startPosition = _player.transform;
        Vector3 endPosition = _tilemap.GetCellCenterWorld(newPosition);
        
        while(true)
        {
            if (startPosition.position != endPosition)
                startPosition.position = Vector3.MoveTowards(startPosition.position, endPosition, SPEEDPLAYER * Time.deltaTime);
            else break;
            yield return new WaitForFixedUpdate();
        }
        _tilemap.SetTile(newPosition, _emptyTilePlayer);
        _playerIsMoving = false;
    }

    private IEnumerator CorutineBoxFall(Vector3Int position, Vector3Int direction, GameObject objectToMove, TileBase tile)
    {
        Vector3Int newPosition = position + direction;

        _tilemap.SetTile(position, null);
        _tilemap.SetTile(newPosition, _emptyTile);

        Transform startPosition = objectToMove.transform;
        startPosition.position = _tilemap.GetCellCenterWorld(position);
        Vector3 endPosition = _tilemap.GetCellCenterWorld(newPosition);

        SpriteRenderer spriteRenderer = objectToMove.GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = (tile as Tile).sprite;

        objectToMove.SetActive(true);

        while (true)
        {
            if (_tilemap.GetTile(newPosition) == null || _tilemap.GetTile(newPosition) == _playerTile)
            {
                _objectsMatrix.Add(newPosition, objectToMove);
                yield break;
            }

            if (startPosition.position != endPosition)
                startPosition.position = Vector3.MoveTowards(startPosition.position, endPosition, SPEEDBOX * Time.deltaTime);
            else break;
            yield return new WaitForEndOfFrame();
        }

        _tilemap.SetTile(newPosition, tile);
        objectToMove.SetActive(false);
        ReturnBox(objectToMove);
    }

    private IEnumerator CorutineGoodsFall(Vector3Int position, Vector3Int direction, GameObject objectToMove, TileBase tile)
    {
        Vector3Int newPosition = position + direction;
        _tilemap.SetTile(position, null);
        _tilemap.SetTile(newPosition, tile);
        _tilemap.SetTileFlags(newPosition, TileFlags.None);
        _tilemap.SetColor(newPosition, Color.clear);

        SpriteRenderer spriteRenderer = objectToMove.GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = (tile as Tile).sprite;
        objectToMove.transform.position = _tilemap.GetCellCenterWorld(position);
        Vector3 endPosition = _tilemap.GetCellCenterWorld(newPosition);
        objectToMove.SetActive(true);

        while (true)
        {
            if (_tilemap.GetTile(newPosition) == null || _tilemap.GetTile(newPosition) == _playerTile)
            {
                objectToMove.SetActive(false);
                yield break;
            }

            if (objectToMove.transform.position != endPosition)
                objectToMove.transform.position = Vector3.MoveTowards(objectToMove.transform.position, endPosition, SPEEDBOX * Time.deltaTime);
            else
                break;
            yield return new WaitForFixedUpdate();
        }

        _tilemap.SetColor(newPosition, Color.white);
        _tilemap.SetTileFlags(newPosition, TileFlags.None);
        objectToMove.SetActive(false);
        ReturnGoods(objectToMove);
    }

    private IEnumerator CorutineBoxMove(Vector3Int position, Vector3Int direction, GameObject objectToMove, TileBase tile)
    {
        Vector3Int newPosition = position + direction;

        _tilemap.SetTile(position, null);
        _tilemap.SetTile(newPosition, _emptyTile);

        float addSpeed = 1f;

        SpriteRenderer spriteRenderer = objectToMove.GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = (tile as Tile).sprite;
        objectToMove.transform.position = _tilemap.GetCellCenterWorld(position);
        Vector3 endPosition = _tilemap.GetCellCenterWorld(newPosition);
        objectToMove.SetActive(true);

        while (true)
        {
            if (_objectsMatrix.Count != 0)
            {
                addSpeed = 1.5f;
                objectToMove.SetActive(false);
                ReturnBox(objectToMove);
                foreach (var e in _objectsMatrix.Values)
                    objectToMove = e;
                _objectsMatrix.Clear();

                if (direction == Vector3Int.up)
                {
                    _boxAnimated.transform.position = objectToMove.transform.position;
                    ReturnBox(objectToMove);
                    _boxAnimated.SetActive(true);
                    _tilemap.SetTile(newPosition, null);
                    EventSystem.OnSalaryChanged(-15);
                    break;
                }

                spriteRenderer = objectToMove.GetComponent<SpriteRenderer>();
                tile = ReturnTileFromHash(spriteRenderer.sprite);
            }

            if (objectToMove.transform.position != endPosition)
                objectToMove.transform.position = Vector3.MoveTowards(objectToMove.transform.position, endPosition, SPEEDPLAYER * addSpeed * Time.deltaTime);
            else
            {
                _tilemap.SetTile(newPosition, tile);
                objectToMove.SetActive(false);
                ReturnBox(objectToMove);
                break;
            }
            yield return new WaitForFixedUpdate();
        }
        IsPushing = false;
    }

    private IEnumerator CorutineConveyer(Vector3Int position, Vector3Int direction, GameObject objectToMove, TileBase tile, Tilemap tilemap)
    {
        tilemap.SetTile(position, null);
        Vector3Int newPosition = position + direction;
        objectToMove.transform.position = tilemap.GetCellCenterWorld(position);
        Transform startPosition = objectToMove.transform;
        Vector3 endPosition = tilemap.GetCellCenterWorld(newPosition);

        objectToMove.SetActive(true);
        SpriteRenderer spriteRenderer = objectToMove.GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = (tile as Tile).sprite;

        while (true)
        {
            if (startPosition.position != endPosition)
                startPosition.position = Vector3.MoveTowards(startPosition.position, endPosition, SPEEDCONVEER * Time.deltaTime);
            else break;
            yield return new WaitForFixedUpdate();
        }
        tilemap.SetTile(newPosition, tile);
        objectToMove.SetActive(false);

        if (objectToMove.tag == "Box")
            ReturnBox(objectToMove);
        else if (objectToMove.tag == "Crane")
            ReturnCrane(objectToMove);
        else if (objectToMove.tag == "Goods")
            ReturnGoods(objectToMove);
    }

    private IEnumerator AnimateBoxDestruction(Vector3Int position)
    {
        float startTime = Time.time;
        _tilemap.SetTile(position, _animatedTile);
        
        while (Time.time - startTime < 0.45f)
            yield return new WaitForFixedUpdate();

        _tilemap.SetTile(position, null);
    }

    #endregion

    #region ////////////////// Checking Tile

    private bool IsGrounded(Vector3Int position)
    {
        bool isGrounded = _tilemap.HasTile(position + Vector3Int.down);
        if (IsGoodsTile(position + Vector3Int.down, _tilemap))
            isGrounded = false;
        return isGrounded;
    }

    private bool IsValidPositionBounds(Vector3Int position)
    {
        for (int i = 0; i < _cells.Length; i++)
        {
            Vector3Int tilePosition = _cells[i] + position;

            if (!_bounds.Contains((Vector2Int)tilePosition))// || tilePosition.y == _bounds.yMax - 1)
            {
                return false;
            }
        }
        return true;
    }

    private bool IsValidPositionBoxes(Vector3Int newPosition)
    {
        for (int i = 0; i < _cells.Length; i++)
        {
            Vector3Int tilePosition = _cells[i] + newPosition;

            if (_tilemap.HasTile(tilePosition))
            {
                return false;
            }
        }
        return true;
    }

    private Vector3Int ReturnPositionOfBoxe(Vector3Int newPosition)
    {
        Vector3Int result = Vector3Int.one;
        for (int i = 0; i < _cells.Length; i++)
        {
            Vector3Int tilePosition = _cells[i] + newPosition;

            if (_tilemap.HasTile(tilePosition))
            {
                if (result == Vector3Int.one)
                    result = tilePosition;
                else return Vector3Int.one;
            }
        }
        return result;
    }

    #endregion

    #region ////////////////// Box Spawner
    private void BoxSpawning()
    {
        int countOfBoxesOnLine = BoxConveyingToLeft() + BoxConveyingToRight();
        if (countOfBoxesOnLine < 1 && Time.time - _timeToSpawnBox > _spawningTime)
        {
            _timeToSpawnBox = Time.time;
            if (Random.Range(0, 6) < _chanseToSpawnBox)
                SpawnBoxOnConveer();
        }
    }

    private void SpawnBoxOnConveer()
    {
        
        TileBase tile = _boxesTiles[Random.Range(0, _boxesTiles.Length)];

        if (Time.time - _SpawningTimeGoods > 60f)
        {
            _SpawningTimeGoods = Time.time + 60f;
            tile = _goodsTiles[Random.Range(0, _goodsTiles.Length)];
        }

        if (Random.Range(0, 2) > 0)
            SetTileOnConveer(_boxSpawnPointLeft, tile, _tilemap);
        else
            SetTileOnConveer(_boxSpawnPointRight, tile, _tilemapSecond);
    }

    private void SetTileOnConveer(Vector3Int _boxSpawnPoint, TileBase tile, Tilemap tilemap)
    {
        tilemap.SetTile(_boxSpawnPoint, tile);
        tilemap.SetTile(_boxSpawnPoint + Vector3Int.up, _craneTile[0]);
    }

    private int BoxConveyingToLeft()
    {
        Vector3Int boxPosition = Vector3Int.zero;
        Vector3Int cranePosition = Vector3Int.zero;
        boxPosition.y = 2;
        cranePosition.y = 3;
        int boxesOnLine = 0;

        for (int x = _boxSpawnPointRight.x; x >= _bounds.xMin - 1; x--)
        {
            boxPosition.x = x;
            cranePosition.x = x;

            if (_tilemapSecond.GetTile(boxPosition) != null)
            {
                bool isBox = IsBoxTile(boxPosition, _tilemapSecond);
                bool isGoods = IsGoodsTile(boxPosition, _tilemapSecond);
                boxesOnLine++;
                TileBase tile = _tilemapSecond.GetTile(boxPosition);
                int startPosition = boxPosition.x == _boxSpawnPointRight.x ? boxPosition.x - 1 : boxPosition.x;
                int randomX = Random.Range(startPosition, _boxSpawnPointLeft.x);
                if (x == randomX && _dotsOfSpawn.Contains(x))
                {
                    if (isBox)
                    {
                        if (boxPosition + Vector3Int.down == _playerPosition)
                            BoxHitPlayer(boxPosition);
                        StartCoroutine(CorutineBoxFall(boxPosition, Vector3Int.down, GetBox(), tile));
                    }
                    else if (isGoods)
                        StartCoroutine(CorutineGoodsFall(boxPosition, Vector3Int.down, GetGoods(), tile));

                    _tilemapSecond.SetTile(boxPosition, null);
                    _tilemapSecond.SetTile(cranePosition, _craneTile[1]);
                }
                else
                {
                    //if (boxPosition == _playerPosition + Vector3Int.up)
                    //    BoxHitPlayer(boxPosition);
                    if (isBox)
                        StartCoroutine(CorutineConveyer(boxPosition, Vector3Int.left, GetBox(), tile, _tilemapSecond));
                    else if (isGoods)
                        StartCoroutine(CorutineConveyer(boxPosition, Vector3Int.left, GetGoods(), tile, _tilemapSecond));
                }
            }

            if (_tilemapSecond.HasTile(cranePosition))
            {
                TileBase tile = _tilemapSecond.GetTile(cranePosition);
                StartCoroutine(CorutineConveyer(cranePosition, Vector3Int.left, GetCrane(), tile, _tilemapSecond));
            }
        }
        return boxesOnLine;
    }

    private int BoxConveyingToRight()
    {
        Vector3Int boxPosition = Vector3Int.zero;
        Vector3Int cranePosition = Vector3Int.zero;
        boxPosition.y = 2;
        cranePosition.y = 3;
        int boxesOnLine = 0;

        for (int x = _boxSpawnPointLeft.x; x < _bounds.xMax + 1; x++)
        {
            boxPosition.x = x;
            cranePosition.x = x;

            bool isBox = IsBoxTile(boxPosition, _tilemap);
            bool isGoods = IsGoodsTile(boxPosition, _tilemap);

            if (isBox || isGoods)
            {
                boxesOnLine++;
                TileBase tile = _tilemap.GetTile(boxPosition);
                int startPosition = boxPosition.x == _boxSpawnPointLeft.x ? boxPosition.x + 1 : boxPosition.x;
                int randomX = Random.Range(startPosition, _boxSpawnPointRight.x);
                if (x == randomX && _dotsOfSpawn.Contains(x))
                {
                    if (isBox)
                    {
                        if (boxPosition + Vector3Int.down == _playerPosition)
                            BoxHitPlayer(boxPosition);
                        StartCoroutine(CorutineBoxFall(boxPosition, Vector3Int.down, GetBox(), tile));
                    } else if (isGoods)
                    {
                        StartCoroutine(CorutineGoodsFall(boxPosition, Vector3Int.down, GetGoods(), tile));
                    }
                    _tilemap.SetTile(cranePosition, _craneTile[1]);
                }
                else
                {
                    if (isBox)
                        StartCoroutine(CorutineConveyer(boxPosition, Vector3Int.right, GetBox(), tile, _tilemap));
                    else if (isGoods)
                        StartCoroutine(CorutineConveyer(boxPosition, Vector3Int.right, GetGoods(), tile, _tilemap));
                }
            }

            if (_tilemap.HasTile(cranePosition))
            {
                TileBase tile = _tilemap.GetTile(cranePosition);
                StartCoroutine(CorutineConveyer(cranePosition, Vector3Int.right, GetCrane(), tile, _tilemap));
            } 
        }
        return boxesOnLine;
    }

    private void SetSpawners()
    {
        _boxSpawnPointLeft = Vector3Int.zero;
        _boxSpawnPointLeft.y = 2;
        _boxSpawnPointRight = _boxSpawnPointLeft;
        _boxSpawnPointLeft.x = -7;
        _boxSpawnPointRight.x = 6;
    }
    #endregion

    #region // DifficultySystem

    private void OnEnable() => EventSystem.DifficultyChanged += DifficultyСhange;

    private void OnDisable() => EventSystem.DifficultyChanged -= DifficultyСhange;

    public void DifficultyСhange()
    {
        _spawningTime = _spawningTime <= 0.5f ? _spawningTime : _spawningTime - 0.005f;
        SPEEDPLAYER += 0.05f; // 2
        SPEEDBOX += 0.05f; // 1.5
        SPEEDCONVEER += 0.05f; // 2.5
        _chanseToSpawnBox = _chanseToSpawnBox < 6 ? _chanseToSpawnBox + 0.1f : _chanseToSpawnBox; // 3
    }

    #endregion
}