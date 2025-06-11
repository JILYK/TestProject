using UnityEngine;
using UnityEngine.UI;

public class UiButtons : MonoBehaviour
{
    [SerializeField] private Button _solveButton;
    [SerializeField] private Button _reloadButton;
    [SerializeField] private MahjongAutoSolver _startSolving;
    [SerializeField] private MahjongLevelGenerator _levelGenerator;

    private void Start()
    {
        _reloadButton.onClick.AddListener(ReloadLevel);
        _solveButton.onClick.AddListener(StartSolving);
    }

    private void ReloadLevel() => _levelGenerator.GenerateLevel();
    private void StartSolving() => _startSolving.StartSolving();
}