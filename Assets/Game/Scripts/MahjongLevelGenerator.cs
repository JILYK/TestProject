using System.Collections.Generic;
using UnityEngine;

public class MahjongLevelGenerator : MonoBehaviour
{
    [Header("Размеры поля")] [SerializeField]
    private int _width = 10;

    [SerializeField] private int _height = 10;
    [SerializeField] private int _layers = 3;

    [Header("Ресурсы")] [SerializeField] private GameObject _tilePrefab;
    [SerializeField] private List<Sprite> _tileSprites;
    [SerializeField] private MahjongGameController _controller;

    [Header("Условия генерации")] [Tooltip("Минимальное количество различных путей к победе")] [SerializeField]
    private int _minSolutions = 2;

    private float _cellW = 1f;
    private float _cellH = 1f;

    private Tile[,,] _tiles;
    public Tile[,,] Tiles => _tiles;
    public int Width => _width;
    public int Height => _height;
    public int LayerCount => _layers;

    private void Awake() => CacheTileSize();

    public void GenerateLevel()
    {
        int attempts = 0;
        while (true)
        {
            attempts++;


            var allPos = CollectAllPositions();
            var removalOrder = new List<(int x, int y, int l)>();


            var poolCopy = new List<(int, int, int)>(allPos);
            if (!BuildGuaranteedOrder(poolCopy, removalOrder))
                continue;


            if (!HasAtLeastNSolutions(removalOrder, _minSolutions))
                continue;


            InstantiateLevel(removalOrder);
            CenterLevel();

            _controller.ShowNotification(false);
            break;
        }
    }


    private List<(int, int, int)> CollectAllPositions()
    {
        var list = new List<(int, int, int)>();
        for (int l = 0; l < _layers; l++)
        for (int x = 0; x < _width - l; x++)
        for (int y = 0; y < _height - l; y++)
            list.Add((x, y, l));

        if (list.Count % 2 == 1) list.RemoveAt(list.Count - 1);
        return list;
    }


    private bool BuildGuaranteedOrder(
        List<(int, int, int)> pool,
        List<(int, int, int)> order)
    {
        if (pool.Count == 0) return true;

        var free = CollectFree(pool);
        if (free.Count < 2) return false;


        for (int i = 0; i < free.Count; i++)
        {
            for (int j = i + 1; j < free.Count; j++)
            {
                var a = free[i];
                var b = free[j];

                pool.Remove(a);
                pool.Remove(b);
                order.Add(a);
                order.Add(b);

                if (BuildGuaranteedOrder(pool, order))
                    return true;
                order.RemoveRange(order.Count - 2, 2);
                pool.Add(a);
                pool.Add(b);
            }
        }

        return false;
    }


    private List<(int, int, int)> CollectFree(List<(int, int, int)> pool)
    {
        var list = new List<(int, int, int)>();
        foreach (var p in pool)
            if (IsFree(p, pool))
                list.Add(p);
        return list;
    }

    private bool IsFree((int x, int y, int l) p, List<(int, int, int)> pool)
    {
        int x = p.x, y = p.y, l = p.l;


        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                if (pool.Contains((x + dx, y + dy, l + 1)))
                    return false;
            }
        }


        bool left = pool.Contains((x - 1, y, l));
        bool right = pool.Contains((x + 1, y, l));


        return !(left && right);
    }


    private struct VTile
    {
        public int x, y, l, pairId;
    }


    private bool HasAtLeastNSolutions(List<(int, int, int)> order, int limit)
    {
        int n = order.Count;
        int pairCount = n / 2;

        var tiles = new VTile[n];
        var posToIndex = new Dictionary<(int, int, int), int>(n);

        for (int i = 0; i < n; i++)
        {
            var p = order[i];
            tiles[i] = new VTile { x = p.Item1, y = p.Item2, l = p.Item3, pairId = i / 2 };
            posToIndex[p] = i;
        }


        var pairIndices = new (int, int)[pairCount];
        for (int pid = 0; pid < pairCount; pid++)
            pairIndices[pid] = (pid * 2, pid * 2 + 1);

        var alive = new bool[n];
        for (int i = 0; i < n; i++) alive[i] = true;

        int solutions = 0;

        bool DFS()
        {
            if (solutions >= limit) return true;

            var freePairs = new List<int>();
            for (int pid = 0; pid < pairCount; pid++)
            {
                var (i1, i2) = pairIndices[pid];
                if (!alive[i1]) continue;
                if (!alive[i2]) continue;

                if (TileBlocked(i1) || TileBlocked(i2))
                    continue;
                freePairs.Add(pid);
            }

            if (freePairs.Count == 0) return false;
            foreach (int pid in freePairs)
            {
                var (i1, i2) = pairIndices[pid];
                alive[i1] = alive[i2] = false;

                bool noTilesLeft = true;
                for (int k = 0; k < n; k++)
                    if (alive[k])
                    {
                        noTilesLeft = false;
                        break;
                    }

                if (noTilesLeft)
                {
                    solutions++;
                    alive[i1] = alive[i2] = true;
                    if (solutions >= limit) return true;
                }
                else
                {
                    if (DFS()) return true;
                    alive[i1] = alive[i2] = true;
                }
            }

            return false;
        }


        bool TileBlocked(int idx)
        {
            var t = tiles[idx];
            if (Roof(t)) return true;
            bool left = Side(t, -1);
            bool right = Side(t, +1);
            return left && right;
        }

        bool Roof(VTile t)
        {
            for (int dx = -1; dx <= 1; dx++)
            for (int dy = -1; dy <= 1; dy++)
                if (FindAlive(t.x + dx, t.y + dy, t.l + 1) != -1)
                    return true;
            return false;
        }

        bool Side(VTile t, int dir)
        {
            return FindAlive(t.x + dir, t.y, t.l) != -1;
        }

        int FindAlive(int x, int y, int l)
        {
            if (l < 0 || l >= _layers) return -1;
            if (x < 0 || y < 0 || x >= _width - l || y >= _height - l) return -1;
            int idx;
            if (posToIndex.TryGetValue((x, y, l), out idx) && alive[idx])
                return idx;
            return -1;
        }


        DFS();
        return solutions >= limit;
    }

    private void InstantiateLevel(List<(int, int, int)> removalOrder)
    {
        _controller.ResetTiles();
        foreach (Transform c in transform) Destroy(c.gameObject);
        _tiles = new Tile[_width, _height, _layers];


        int nPairs = removalOrder.Count / 2;
        var pairSprites = new List<Sprite>(nPairs);
        for (int i = 0; i < nPairs; i++)
            pairSprites.Add(_tileSprites[UnityEngine.Random.Range(0, _tileSprites.Count)]);
        Shuffle(pairSprites);


        var spritePool = new List<Sprite>(removalOrder.Count);
        foreach (var s in pairSprites)
        {
            spritePool.Add(s);
            spritePool.Add(s);
        }

        for (int i = 0; i < removalOrder.Count; i++)
        {
            var p = removalOrder[removalOrder.Count - 1 - i];
            Sprite spr = spritePool[i];


            Vector3 localPos = new(
                (p.Item1 + p.Item3 * 0.5f) * _cellW,
                (p.Item2 + p.Item3 * 0.5f) * _cellH,
                -p.Item3 * 0.1f);

            var go = Instantiate(_tilePrefab, Vector3.zero, Quaternion.identity, transform);
            go.transform.localPosition = localPos;

            var tile = go.GetComponent<Tile>();
            tile.Init(spr, p.Item1, p.Item2, p.Item3, false);

            _tiles[p.Item1, p.Item2, p.Item3] = tile;
            _controller.RegisterTile(tile);
        }

        _controller.SendMessage("UpdateBlocks");
    }

    private void CacheTileSize()
    {
        if (_tilePrefab == null) return;

        var temp = Instantiate(_tilePrefab);
        var sr = temp.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            _cellW = sr.bounds.size.x;
            _cellH = sr.bounds.size.y;
        }

        Destroy(temp);
    }

    private void Shuffle<T>(IList<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int j = UnityEngine.Random.Range(i, list.Count);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }


    public void ClearTileAt(Tile t)
    {
        if (_tiles[t.X, t.Y, t.Layer] == t)
            _tiles[t.X, t.Y, t.Layer] = null;
    }

    public bool IsTileBlocked(Tile t)
    {
        int x = t.X, y = t.Y, l = t.Layer;

        if (HasRoof(x, y, l))
            return true;

        bool left = x > 0 && _tiles[x - 1, y, l] != null;
        bool right = x + 1 < _width - l && _tiles[x + 1, y, l] != null;
        return left && right;
    }

    private bool HasRoof(int x, int y, int l)
    {
        int nl = l + 1;
        if (nl >= _layers) return false;


        return _tiles[x, y, nl] != null ||
               (x > 0 && _tiles[x - 1, y, nl] != null) ||
               (y > 0 && _tiles[x, y - 1, nl] != null) ||
               (x > 0 && y > 0 && _tiles[x - 1, y - 1, nl] != null);
    }


    private void CenterLevel()
    {
        float minX = float.MaxValue, maxX = float.MinValue;
        float minY = float.MaxValue, maxY = float.MinValue;

        foreach (Transform child in transform)
        {
            Vector3 pos = child.localPosition;
            if (pos.x < minX) minX = pos.x;
            if (pos.x > maxX) maxX = pos.x;
            if (pos.y < minY) minY = pos.y;
            if (pos.y > maxY) maxY = pos.y;
        }

        Vector3 center = new Vector3((minX + maxX) / 2f, (minY + maxY) / 2f, 0f);

        foreach (Transform child in transform)
        {
            child.localPosition -= center;
        }
    }
}