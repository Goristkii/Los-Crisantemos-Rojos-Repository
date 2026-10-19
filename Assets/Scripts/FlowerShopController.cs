using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FlowerShopController : MonoBehaviour
{
    [Header("Локации для выхода")]
    public GameObject shopPanel;
    public GameObject homePanel;

    [Header("UI Elements")]
    public GameObject actionPanelRoot;

    [Header("Portrait")]
    [SerializeField] private Image girlPortrait;
    [SerializeField] private Sprite girlBasic;
    [SerializeField] private Sprite girlTouched;
    [SerializeField] private Sprite girlAngry;

    [Header("Options UI")]
    [SerializeField] private GameObject optionsContainer;
    [SerializeField] private Button btnOption1;
    [SerializeField] private Button btnOption2;
    [SerializeField] private Button btnOption3;
    [SerializeField] private TextMeshProUGUI txtOption1;
    [SerializeField] private TextMeshProUGUI txtOption2;
    [SerializeField] private TextMeshProUGUI txtOption3;

    [Header("State 0 — First Visit")]
    [TextArea] public string[] interiorThoughts;
    [TextArea] public string[] introDialog;
    [TextArea] public string[] opt1_Dialog; // Neutral haggle failure ($100)
    [TextArea] public string[] opt2_Dialog; // Successful haggle ($90)
    [TextArea] public string[] opt3_Dialog; // Terrible haggle ($110)

    [Header("State 1 — Returning with Money")]
    [TextArea] public string[] buyBouquetDialog;

    private Coroutine initCoroutine;
    private bool isExitPhase = false;

    // -------------------------------------------------------------------------
    // Initialization
    // -------------------------------------------------------------------------

    private void Awake()
    {
        btnOption1.onClick.AddListener(OnOption1Clicked);
        btnOption2.onClick.AddListener(OnOption2Clicked);
        btnOption3.onClick.AddListener(OnOption3Clicked);
    }

    private void OnEnable()
    {

    

        if (actionPanelRoot != null)
        {
            actionPanelRoot.SetActive(false);
        }


        isExitPhase = false;
        optionsContainer.SetActive(false);

        if (girlPortrait != null && girlBasic != null)
        {
            girlPortrait.sprite = girlBasic;
        }

        if (initCoroutine != null)
        {
            StopCoroutine(initCoroutine);
        }

        initCoroutine = StartCoroutine(InitializeShopRoutine());
    }

    private void OnDisable()
    {
        if (initCoroutine != null)
        {
            StopCoroutine(initCoroutine);
            initCoroutine = null;
        }
    }

    private IEnumerator InitializeShopRoutine()
    {
        yield return new WaitForSeconds(1.5f);

        if (GameManager.Instance == null)
        {
            Debug.LogWarning("[FlowerShopController] GameManager instance not found.");
            yield break;
        }

        if (GameManager.Instance.flowerShopState == 0)
        {
            // First visit: play interior thoughts, then intro dialog
            if (interiorThoughts != null && interiorThoughts.Length > 0)
            {
                MessageManager.Instance.ShowMessages(interiorThoughts, () =>
                {
                    MessageManager.Instance.ShowMessages(introDialog, OnIntroComplete_State0);
                });
            }
            else
            {
                MessageManager.Instance.ShowMessages(introDialog, OnIntroComplete_State0);
            }
        }
        else if (GameManager.Instance.flowerShopState == 1)
        {
            // Second visit: negotiation grid stays hidden. Automatically play purchase dialogue.
            optionsContainer.SetActive(false);
            PlayPurchaseDialog();
        }
    }

    // -------------------------------------------------------------------------
    // Intro Callbacks
    // -------------------------------------------------------------------------

    private void OnIntroComplete_State0()
    {
        txtOption1.text = "- Rosa siempre me hacía un descuento.";
        txtOption2.text = "- Antes había una mujer aquí. Rosa. La tía Rosa, la llamaban todos.";
        txtOption3.text = "- Vamos, ayúdame con esto. Lo estoy comprando para mi madre.";

        btnOption1.gameObject.SetActive(true);
        btnOption2.gameObject.SetActive(true);
        btnOption3.gameObject.SetActive(true);

        optionsContainer.SetActive(true);
    }

    private void OnIntroComplete_State1()
    {
        txtOption1.text = $"Buy bouquet for {GameManager.Instance.flowerPrice}";

        btnOption1.gameObject.SetActive(true);
        btnOption2.gameObject.SetActive(false);
        btnOption3.gameObject.SetActive(false);

        optionsContainer.SetActive(true);
    }

    // -------------------------------------------------------------------------
    // State 0 Options Click Handlers
    // -------------------------------------------------------------------------

    private void OnOption1Clicked()
    {
        if (GameManager.Instance.flowerShopState == 0)
        {
            if (isExitPhase)
            {
                // Player clicked "I'll come back later"
                GameManager.Instance.flowerShopState = 1;
                ExitShop();
            }
            else
            {
                // Option 1: Neutral haggle failure ($100)
                optionsContainer.SetActive(false);
                GameManager.Instance.flowerPrice = 100;
                MessageManager.Instance.ShowMessages(opt1_Dialog, OnOptionDialogFinished);
            }
        }
        else if (GameManager.Instance.flowerShopState == 1)
        {
            // State 1: Buy bouquet
            optionsContainer.SetActive(false);
            PlayPurchaseDialog();
        }
    }

    private void OnOption2Clicked()
    {
        // Option 2: Successful haggle ($90)
        optionsContainer.SetActive(false);

        if (girlPortrait != null && girlTouched != null)
        {
            girlPortrait.sprite = girlTouched;
        }

        GameManager.Instance.flowerPrice = 90;
        MessageManager.Instance.ShowMessages(opt2_Dialog, OnOptionDialogFinished);
    }

    private void OnOption3Clicked()
    {
        // Option 3: Terrible haggle ($110)
        optionsContainer.SetActive(false);

        if (girlPortrait != null && girlAngry != null)
        {
            girlPortrait.sprite = girlAngry;
        }

        GameManager.Instance.flowerPrice = 110;
        MessageManager.Instance.ShowMessages(opt3_Dialog, OnOptionDialogFinished);
    }

    // -------------------------------------------------------------------------
    // Shared Callback After Any Option Dialog Finishes
    // -------------------------------------------------------------------------

    private void OnOptionDialogFinished()
    {
        if (girlPortrait != null && girlBasic != null)
        {
            girlPortrait.sprite = girlBasic;
        }

        isExitPhase = true;

        txtOption1.text = "- Volvere pronto.";
        btnOption1.gameObject.SetActive(true);
        btnOption2.gameObject.SetActive(false);
        btnOption3.gameObject.SetActive(false);

        optionsContainer.SetActive(true);
    }

    // -------------------------------------------------------------------------
    // State 1 Buy Bouquet Callback
    // -------------------------------------------------------------------------

    private void OnBuyBouquetComplete()
    {
        GameManager.Instance.playerMoney -= GameManager.Instance.flowerPrice;
        if (ActionPanelController.Instance != null)
        {
            ActionPanelController.Instance.UpdateInventoryUI();
        }

        GameManager.Instance.hasFlower = true;
        GameManager.Instance.hasBouquet = true;
        GameManager.Instance.flowerShopState = 2; // Locks shop permanently
        ExitShop();
    }

    // -------------------------------------------------------------------------
    // Exit Shop
    // -------------------------------------------------------------------------

    private void ExitShop()
    {
        // If we exit after the first phase, signal the phone call
        if (GameManager.Instance.flowerShopState == 1)
        {
            GameManager.Instance.needToCallFriend = true;
        }

        if (GlobalFader.Instance != null)
        {
            GlobalFader.Instance.FadeToLocation(shopPanel, homePanel, () =>
            {
                if (actionPanelRoot != null)
                {
                    actionPanelRoot.SetActive(true);
                }
            });
        }
        else
        {
            // Disable shop panel and enable home panel
            if (shopPanel != null) shopPanel.SetActive(false);
            if (homePanel != null) homePanel.SetActive(true);

            if (actionPanelRoot != null)
            {
                actionPanelRoot.SetActive(true);
            }
        }
    }

    // -------------------------------------------------------------------------
    // Purchase Dialog (Dynamic Price Replacement)
    // -------------------------------------------------------------------------

    private void PlayPurchaseDialog()
    {
        if (buyBouquetDialog == null) return;

        string priceStr = (GameManager.Instance != null) ? GameManager.Instance.flowerPrice.ToString() : "0";
        string[] processedDialog = new string[buyBouquetDialog.Length];

        for (int i = 0; i < buyBouquetDialog.Length; i++)
        {
            processedDialog[i] = buyBouquetDialog[i] != null
                ? buyBouquetDialog[i].Replace("{PRICE}", priceStr)
                : string.Empty;
        }

        MessageManager.Instance.ShowMessages(processedDialog, OnBuyBouquetComplete);
    }
}
