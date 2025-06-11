using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MahjongAutoSolver : MonoBehaviour
{
    [Header("References")] private MahjongLevelGenerator levelGenerator;
    private MahjongGameController controller;


    [Header("Delays (sec)")] [SerializeField]
    private float previewSeconds = 0.1f;

    [SerializeField] private float pairPause = 0.1f;


    private Coroutine routine;
    private SpriteRenderer[] previewRenderers;

    private void Start()
    {
        levelGenerator = GetComponent<MahjongLevelGenerator>();
        controller = GetComponent<MahjongGameController>();
    }

    public void StartSolving()
    {
        if (routine == null) routine = StartCoroutine(Solve());
    }

    public void StopSolving()
    {
        if (routine != null)
        {
            StopCoroutine(routine);
            routine = null;
            RestorePreviewColor();
        }
    }


    private IEnumerator Solve()
    {
        yield return null;

        while (true)
        {
            List<Tile> free = CollectFreeTiles();
            if (free.Count < 2) break;


            Tile a = null, b = null;
            for (int i = 0; i < free.Count && a == null; i++)
            {
                for (int j = i + 1; j < free.Count; j++)
                {
                    if (free[i].Sprite == free[j].Sprite)
                    {
                        a = free[i];
                        b = free[j];
                        break;
                    }
                }
            }

            if (a == null) break;


            controller.SelectTile(a);


            HighlightBlue(b);


            yield return new WaitForSeconds(previewSeconds);


            RestorePreviewColor();
            controller.SelectTile(b);


            yield return new WaitForSeconds(pairPause);
        }

        routine = null;
    }


    private List<Tile> CollectFreeTiles()
    {
        var list = new List<Tile>();
        for (int l = 0; l < levelGenerator.LayerCount; l++)
        for (int x = 0; x < levelGenerator.Width; x++)
        for (int y = 0; y < levelGenerator.Height; y++)
        {
            Tile t = levelGenerator.Tiles[x, y, l];
            if (t != null && !t.IsBlocked) list.Add(t);
        }

        return list;
    }


    private void HighlightBlue(Tile t)
    {
        var renderers = new List<SpriteRenderer>();

        var rMain = t.GetComponent<SpriteRenderer>();
        if (rMain != null)
        {
            renderers.Add(rMain);
            rMain.color = Color.blue;
        }

        if (t.transform.childCount > 0)
        {
            var rSprite = t.transform.GetChild(0).GetComponent<SpriteRenderer>();
            if (rSprite != null)
            {
                renderers.Add(rSprite);
                rSprite.color = Color.blue;
            }
        }

        previewRenderers = renderers.ToArray();
    }


    private void RestorePreviewColor()
    {
        if (previewRenderers == null) return;
        foreach (var r in previewRenderers)
            if (r != null)
                r.color = Color.white;
        previewRenderers = null;
    }
}