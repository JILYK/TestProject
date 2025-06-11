using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class Tile : MonoBehaviour
{
    [SerializeField] private Sprite _sprite;
    [SerializeField] private int _x;
    [SerializeField] private int _y;
    [SerializeField] private int _layer;
    [SerializeField] private bool _blocked;

    private bool _isSelected;

    private SpriteRenderer _tileRenderer;
    private SpriteRenderer _spriteRenderer;
    private MahjongGameController _controller;

    public Sprite Sprite => _sprite;
    public int X => _x;
    public int Y => _y;
    public int Layer => _layer;
    public bool IsBlocked => _blocked;

    private void Awake()
    {
        _controller = FindObjectOfType<MahjongGameController>();

        _tileRenderer = GetComponent<SpriteRenderer>();
        if (transform.childCount > 0)
            _spriteRenderer = transform.GetChild(0).GetComponent<SpriteRenderer>();
    }

    public void Init(Sprite sprite, int x, int y, int layer, bool blocked)
    {
        _sprite = sprite;
        _x = x;
        _y = y;
        _layer = layer;
        _blocked = blocked;

        if (_spriteRenderer != null)
        {
            _spriteRenderer.sprite = _sprite;
            _spriteRenderer.sortingOrder = _layer + 1;
        }

        if (_tileRenderer != null)
            _tileRenderer.sortingOrder = _layer;

        UpdateAppearance();
    }

    public void SetBlocked(bool blocked)
    {
        _blocked = blocked;
        UpdateAppearance();
    }

    public void SetSelected(bool selected)
    {
        _isSelected = selected;
        UpdateAppearance();
    }

    private void UpdateAppearance()
    {
        Color color;

        if (_isSelected)
        {
            color = Color.green;
        }
        else if (_blocked)
        {
            color = new Color(0.85f, 0.85f, 0.85f, 1f);
        }
        else
        {
            color = Color.white;
        }

        if (_tileRenderer != null)
            _tileRenderer.color = color;

        if (_spriteRenderer != null)
            _spriteRenderer.color = color;
    }

    private void OnMouseDown()
    {
        if (_blocked) return;
        _controller.SelectTile(this);
    }
}