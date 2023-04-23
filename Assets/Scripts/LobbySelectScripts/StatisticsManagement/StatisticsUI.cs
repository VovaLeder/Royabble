using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StatisticsUI : MonoBehaviour
{
    [SerializeField] private Button closeBtn;
    [SerializeField] private TextMeshProUGUI usernameText;
    [SerializeField] private TextMeshProUGUI gamesPlayedStat;
    [SerializeField] private TextMeshProUGUI gamesWonStat;
    [SerializeField] private TextMeshProUGUI earnedPointsStat;
    [SerializeField] private TextMeshProUGUI spentPointsStat;
    [SerializeField] private TextMeshProUGUI wordsComposedStat;
    [SerializeField] private TextMeshProUGUI playersKilledStat;

    private void Awake()
    {
        Hide();
    }

    private void Start()
    {
        closeBtn.onClick.AddListener(() =>
        {
            Hide();
        });
    }

    public void SetStats(UserData user)
    {
        usernameText.text = $"�������� ����: {user.username}";
        gamesPlayedStat.text = $"������� ���: {user.gamesPlayed}";
        gamesWonStat.text = $"�������� ���: {user.gamesWon}";
        earnedPointsStat.text = $"���������� �����: {user.earnedPoints}";
        spentPointsStat.text = $"��������� �����: {user.spentPoints}";
        wordsComposedStat.text = $"���������� ����: {user.wordsComposed}";
        playersKilledStat.text = $"��������� �������: {user.playersKilled}";
    }

    public void Show()
    {
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
