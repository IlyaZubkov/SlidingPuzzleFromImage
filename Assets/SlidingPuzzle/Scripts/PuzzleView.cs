using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PuzzleView : MonoBehaviour
{
    [SerializeField] private Texture2D _texture2D;
    [SerializeField] private int _tilesPerLine = 4;
    [SerializeField] private float _tileMoveDuration = .2f;
    [SerializeField] private int shuffleLength = 20;
    [SerializeField] private Material _material;
    [SerializeField] private float shuffleMoveDuration = .1f;

    private enum PuzzleState
    {
        Solved,
        Randomizing,
        InPlay
    };

    private PuzzleState state;

    private TilesView _emptyTile;
    private TilesView[,] _tiles;
    private Queue<TilesView> _inputs;
    private bool _tileIsMoving;
    private int shuffleMovesRemaining;
    private Vector2Int prevShuffleOffset;

    void Start()
    {
        CreatePuzzle();
        StartShuffle();
    }

    void CreatePuzzle()
    {
        _tiles = new TilesView[_tilesPerLine, _tilesPerLine];
        Texture2D[,] imageSlices = ImageSlicerView.GetSlices(_texture2D, _tilesPerLine);

        for (int y = 0; y < _tilesPerLine; y++)
        {
            for (int x = 0; x < _tilesPerLine; x++)
            {
                GameObject tileGameObject = GameObject.CreatePrimitive(PrimitiveType.Quad);
                tileGameObject.transform.position = -Vector2.one * (_tilesPerLine - 1) * 0.5f + new Vector2(x, y);
                tileGameObject.transform.parent = transform;

                TilesView tile = tileGameObject.AddComponent<TilesView>();
                tile.OnBlockPressed += PlayerMoveBlockInput;
                tile.OnFinishedMoving += OnBlockFinishedMoving;
                tile.Init(new Vector2Int(x, y), imageSlices[x, y], _material);
                _tiles[x, y] = tile;

                if (y == 0 && x == _tilesPerLine - 1)
                {
                    _emptyTile = tile;
                    _emptyTile.gameObject.SetActive(false);
                }
            }
        }

        var scale = 4f / _tilesPerLine;
        gameObject.transform.localScale = new Vector3(scale, scale, 1);
        _inputs = new Queue<TilesView>();
    }

    void PlayerMoveBlockInput(TilesView tilesToMove)
    {
        if (state == PuzzleState.InPlay)
        {
            _inputs.Enqueue(tilesToMove);
            MakeNextPlayerMove();
        }
    }

    void MakeNextPlayerMove()
    {
        while (_inputs.Count > 0 && !_tileIsMoving)
        {
            MoveBlock(_inputs.Dequeue(), _tileMoveDuration);
        }
    }

    void MoveBlock(TilesView tileToMove, float duration)
    {
        if ((tileToMove.coord - _emptyTile.coord).sqrMagnitude == 1)
        {
            _tiles[tileToMove.coord.x, tileToMove.coord.y] = _emptyTile;
            _tiles[_emptyTile.coord.x, _emptyTile.coord.y] = tileToMove;

            (_emptyTile.coord, tileToMove.coord) = (tileToMove.coord, _emptyTile.coord);

            var transformEmptyBlock = _emptyTile.transform;

            Vector2 targetPosition = transformEmptyBlock.position;
            transformEmptyBlock.position = tileToMove.transform.position;
            tileToMove.MoveToPosition(targetPosition, duration);
            _tileIsMoving = true;
        }
    }

    void OnBlockFinishedMoving()
    {
        _tileIsMoving = false;
        CheckIfSolved();

        if (state == PuzzleState.InPlay)
        {
            MakeNextPlayerMove();
        }

        if (state == PuzzleState.InPlay)
        {
            MakeNextPlayerMove();
        }
        else if (state == PuzzleState.Randomizing)
        {
            if (shuffleMovesRemaining > 0)
            {
                MakeNextShuffleMove();
            }
            else
            {
                state = PuzzleState.InPlay;
            }
        }
    }

    void StartShuffle()
    {
        state = PuzzleState.Randomizing;
        shuffleMovesRemaining = shuffleLength;
        _emptyTile.gameObject.SetActive(false);
        MakeNextShuffleMove();
    }

    void MakeNextShuffleMove()
    {
        Vector2Int[] offsets =
            {new Vector2Int(1, 0), new Vector2Int(-1, 0), new Vector2Int(0, 1), new Vector2Int(0, -1)};
        int randomIndex = Random.Range(0, offsets.Length);

        for (int i = 0; i < offsets.Length; i++)
        {
            Vector2Int offset = offsets[(randomIndex + i) % offsets.Length];
            if (offset != prevShuffleOffset * -1)
            {
                Vector2Int moveBlockCoord = _emptyTile.coord + offset;

                if (moveBlockCoord.x >= 0 && moveBlockCoord.x < _tilesPerLine && moveBlockCoord.y >= 0 &&
                    moveBlockCoord.y < _tilesPerLine)
                {
                    MoveBlock(_tiles[moveBlockCoord.x, moveBlockCoord.y], shuffleMoveDuration);
                    shuffleMovesRemaining--;
                    prevShuffleOffset = offset;
                    break;
                }
            }
        }
    }

    void CheckIfSolved()
    {
        foreach (TilesView block in _tiles)
        {
            if (!block.IsAtStartingCoord())
            {
                return;
            }
        }

        state = PuzzleState.Solved;
        _emptyTile.gameObject.SetActive(true);
    }
}