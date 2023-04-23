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
        usernameText.text = $"Выполнен вход: {user.username}";
        gamesPlayedStat.text = $"Сыграно игр: {user.gamesPlayed}";
        gamesWonStat.text = $"Выиграно игр: {user.gamesWon}";
        earnedPointsStat.text = $"Заработано очков: {user.earnedPoints}";
        spentPointsStat.text = $"Потрачено очков: {user.spentPoints}";
        wordsComposedStat.text = $"Составлено слов: {user.wordsComposed}";
        playersKilledStat.text = $"Побеждено игроков: {user.playersKilled}";
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
