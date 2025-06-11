using System.Collections.Generic;
using UnityEngine;

public class MahjongGameController : MonoBehaviour
{
    [SerializeField] private MahjongLevelGenerator _levelGenerator;

    private Tile _firstSelected;
    private readonly List<Tile> _allTiles = new();
    [SerializeField] private GameObject notificationObject;


    public void ResetTiles()
    {
        _allTiles.Clear();
        _firstSelected = null;
    }


    public void ShowNotification(bool show)
    {
        notificationObject.SetActive(show);
    }


    public void RegisterTile(Tile tile) => _allTiles.Add(tile);

    public void SelectTile(Tile tile)
    {
        if (tile.IsBlocked) return;


        if (_firstSelected == null)
        {
            _firstSelected = tile;
            tile.SetSelected(true);
            return;
        }


        if (_firstSelected == tile)
        {
            tile.SetSelected(false);
            _firstSelected = null;
            return;
        }


        if (_firstSelected.Sprite == tile.Sprite)
        {
            RemovePair(_firstSelected, tile);
            _firstSelected = null;
        }
        else
        {
            _firstSelected.SetSelected(false);
            _firstSelected = tile;
            tile.SetSelected(true);
        }
    }


    private void RemovePair(Tile a, Tile b)
    {
        _levelGenerator.ClearTileAt(a);
        _levelGenerator.ClearTileAt(b);

        Destroy(a.gameObject);
        Destroy(b.gameObject);
        _allTiles.Remove(a);
        _allTiles.Remove(b);

        UpdateBlocks();
        CheckWin();

        if (_allTiles.Count > 0 && !HasAvailableMoves())
        {
            ShowNotification(true);
        }

    }


    private void UpdateBlocks()
    {
        foreach (Tile t in _allTiles)
            t.SetBlocked(_levelGenerator.IsTileBlocked(t));
    }


    private void CheckWin()
    {
        if (_allTiles.Count == 0)
        {
            Debug.Log("Победа");
            ShowNotification(true);
        }
        
    }

    private bool HasAvailableMoves()
    {
        for (int i = 0; i < _allTiles.Count; i++)
        {
            for (int j = i + 1; j < _allTiles.Count; j++)
            {
                Tile t1 = _allTiles[i];
                Tile t2 = _allTiles[j];
                if (!t1.IsBlocked && !t2.IsBlocked && t1.Sprite == t2.Sprite)
                {
                    return true;
                }
            }
        }

        return false;
    }


    public void SolveAutomatically()
    {
        bool progress = true;
        while (progress)
        {
            progress = false;
            for (int i = 0; i < _allTiles.Count; i++)
            {
                for (int j = i + 1; j < _allTiles.Count; j++)
                {
                    Tile t1 = _allTiles[i];
                    Tile t2 = _allTiles[j];
                    if (!t1.IsBlocked && !t2.IsBlocked && t1.Sprite == t2.Sprite)
                    {
                        RemovePair(t1, t2);
                        progress = true;
                        break;
                    }
                }

                if (progress) break;
            }
        }
    }
}