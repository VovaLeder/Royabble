using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StatisticsUIManager : MonoBehaviour
{

    [SerializeField] LogInUI logInUI;
    [SerializeField] LogOutUI logOutUI;
    [SerializeField] StatisticsUI statisticsUI;
    [SerializeField] ErrorUI errorUI;

    StatisticsManager statisticsManager;

    void Start()
    {
        statisticsManager = StatisticsManager.Instance;

        StartCoroutine(UpdateUser());

        logInUI.LogInBtn.onClick.AddListener(() =>
        {
            StartCoroutine(LogInButtonListener(statisticsManager.LogIn));
        });
        logInUI.CreateUserBtn.onClick.AddListener(() =>
        {
            StartCoroutine(LogInButtonListener(statisticsManager.CreateUser));
        });

        logOutUI.LogOutBtn.onClick.AddListener(() =>
        {
            statisticsManager.ResetUser();
            logInUI.Show();
            logOutUI.Hide();
        });
        logOutUI.ShowStatsBtn.onClick.AddListener(() =>
        {
            statisticsUI.Show();
            statisticsUI.SetStats(statisticsManager.currentUser);
        });
    }

    private IEnumerator LogInButtonListener(Func<string, string, IEnumerator> func)
    {
        yield return StartCoroutine(func(logInUI.GetUsername(), logInUI.GetPassword()));
        if (!statisticsManager.networkUnavailable)
        {
            if (statisticsManager.success)
            {
                logInUI.Hide();
                logOutUI.Show();
                logOutUI.SetLoggedUserText($"Выполнен вход: {statisticsManager.currentUser.username}");
            }
            else
            {
                errorUI.ShowErrorText(statisticsManager.errorMessage);
            }
        }
        else
        {
            logInUI.Hide();
            logOutUI.Hide();
            errorUI.Hide();
            statisticsUI.Hide();
        }
    }

    private IEnumerator UpdateUser()
    {
        yield return StartCoroutine(statisticsManager.UpdateUser());
        if (statisticsManager.LoggedIn())
        {
            logOutUI.Show();
            logOutUI.SetLoggedUserText($"Выполнен вход: {statisticsManager.currentUser.username}");
        }
        else
        {
            logInUI.Show();
        }

        if (statisticsManager.networkUnavailable)
        {
            logInUI.Hide();
            logOutUI.Hide();
            errorUI.Hide();
            statisticsUI.Hide();
        }
    }
}
