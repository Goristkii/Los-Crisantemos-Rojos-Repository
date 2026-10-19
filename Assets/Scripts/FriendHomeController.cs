using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

public class FriendHomeController : MonoBehaviour
{
    [Header("Portraits")]
    [FormerlySerializedAs("portraitBasic")]
    public GameObject friendPortraitBasic;
    public GameObject friendPortraitCash;
    public GameObject portraitGame;       // Друг с геймпадом (слева)
    public GameObject portraitPlayerGame; // Главный герой с геймпадом (справа)

    [Header("QTE Game")]
    public GameObject qteGameObject;

    [Header("Dialogues")]
    [TextArea(2, 4)] public string[] introThoughts;
    [FormerlySerializedAs("linesBeforeGame")]
    [TextArea(2, 4)] public string[] preGameDialogue;

    [Header("Post-Game Dialogues")]
    [TextArea(2, 4)] public string[] winDialogueBeforeCash;
    [TextArea(2, 4)] public string[] winDialogueWithCash;
    [TextArea(2, 4)] public string[] loseDialogueBeforeCash;
    [TextArea(2, 4)] public string[] loseDialogueWithCash;

    [Header("On Scene End")]
    public UnityEvent onSceneFinished;

    void Start()
    {
        StartApartmentScene();
    }

    public void StartApartmentScene()
    {
        if (ActionPanelController.Instance != null)
        {
            ActionPanelController.Instance.gameObject.SetActive(false);
        }

        // Прячем ВСЕ портреты и мини-игру перед мыслями главного героя
        if (friendPortraitBasic != null) friendPortraitBasic.SetActive(false);
        if (friendPortraitCash != null) friendPortraitCash.SetActive(false);
        if (portraitGame != null) portraitGame.SetActive(false);
        if (portraitPlayerGame != null) portraitPlayerGame.SetActive(false);
        if (qteGameObject != null) qteGameObject.SetActive(false);

        if (MessageManager.Instance != null)
        {
            MessageManager.Instance.ShowMessages(introThoughts, OnIntroThoughtsFinished);
        }
        else
        {
            OnIntroThoughtsFinished();
        }
    }

    private void OnIntroThoughtsFinished()
    {
        // Включаем обычный портрет друга
        if (friendPortraitBasic != null) friendPortraitBasic.SetActive(true);

        if (MessageManager.Instance != null)
        {
            MessageManager.Instance.ShowMessages(preGameDialogue, StartQTE);
        }
        else
        {
            StartQTE();
        }
    }

    private void StartQTE()
    {
        Action startAction = () =>
        {
            // Прячем базовый портрет
            if (friendPortraitBasic != null) friendPortraitBasic.SetActive(false);
            
            // Включаем оба игровых портрета по бокам экрана
            if (portraitGame != null) portraitGame.SetActive(true);
            if (portraitPlayerGame != null) portraitPlayerGame.SetActive(true);

            if (qteGameObject != null) qteGameObject.SetActive(true);
        };

        if (GlobalFader.Instance != null)
        {
            GlobalFader.Instance.FadeOutAndIn(startAction);
        }
        else
        {
            startAction();
        }
    }

    public void OnGameWon()
    {
        PlayPostGameSequence(winDialogueBeforeCash, winDialogueWithCash, OnWinCashDialogueComplete);
    }

    public void OnGameLost()
    {
        PlayPostGameSequence(loseDialogueBeforeCash, loseDialogueWithCash, OnLoseCashDialogueComplete);
    }

    private void OnWinCashDialogueComplete()
    {
        // Give player 80 money after win dialogue finishes
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddMoney(80);
        }

        FinishScene();
    }

    private void OnLoseCashDialogueComplete()
    {
        // Give player 80 money after lose dialogue finishes
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddMoney(80);
        }

        FinishScene();
    }

    private void PlayPostGameSequence(string[] beforeCash, string[] withCash, Action onCashDialogueComplete)
    {
        Action postGameAction = () =>
        {
            // Прячем QTE и игровые портреты
            if (qteGameObject != null) qteGameObject.SetActive(false);
            if (portraitPlayerGame != null) portraitPlayerGame.SetActive(false);
            if (portraitGame != null) portraitGame.SetActive(false);

            // Убеждаемся, что friendPortraitBasic активен, а friendPortraitCash неактивен
            if (friendPortraitBasic != null) friendPortraitBasic.SetActive(true);
            if (friendPortraitCash != null) friendPortraitCash.SetActive(false);

            if (MessageManager.Instance != null)
            {
                MessageManager.Instance.ShowMessages(beforeCash, () =>
                {
                    // Меняем портреты в середине разговора
                    if (friendPortraitBasic != null) friendPortraitBasic.SetActive(false);
                    if (friendPortraitCash != null) friendPortraitCash.SetActive(true);

                    // Запускаем вторую часть диалога (с деньгами)
                    MessageManager.Instance.ShowMessages(withCash, onCashDialogueComplete);
                });
            }
            else
            {
                onCashDialogueComplete?.Invoke();
            }
        };

        if (GlobalFader.Instance != null)
        {
            GlobalFader.Instance.FadeOutAndIn(postGameAction);
        }
        else
        {
            postGameAction();
        }
    }

    public void StartPostGameDialogue() => OnGameWon();

    private void FinishScene()
    {
        // Выключаем портреты перед переходом
        if (friendPortraitBasic != null) friendPortraitBasic.SetActive(false);
        if (friendPortraitCash != null) friendPortraitCash.SetActive(false);

        // Возвращаем панель действий
        if (ActionPanelController.Instance != null)
        {
            ActionPanelController.Instance.gameObject.SetActive(true);
        }

        // Запускаем переход
        onSceneFinished?.Invoke();
    }
}