using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Flower Shop Quest")]
    public int flowerShopState = 0;  // 0: first visit | 1: needs money/negotiation done | 2: bought bouquet
    public int flowerPrice = 100;
    public int playerMoney = 0;
    public bool hasBouquet = false;
    public bool needToCallFriend = false;

    [Header("Inventory (Items)")]
    public bool hasMetalPipe = false;
    public bool hasFlower = false;
    public int bottleCount = 0;

    [Header("Story Progress")]
    public bool hoboQTEFinished = false; 
    public bool friendCalled = false;    

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void AddMoney(int amount)
    {
        playerMoney += amount;
        if (ActionPanelController.Instance != null)
        {
            ActionPanelController.Instance.UpdateInventoryUI();
        }
    }

    public void AddBottle()
    {
        bottleCount++;
    }

    public void SellBottles()
{
    if (bottleCount > 0)
    {
        AddMoney(bottleCount * 2);
        bottleCount = 0;
    }
}

    public void SetHasMetalPipe(bool value) 
    {
        hasMetalPipe = value;
    }

    /// <summary>
    /// Checks whether the player currently holds the specified required item.
    /// </summary>
    public bool HasItem(RequiredItem item)
    {
        switch (item)
        {
            case RequiredItem.MetalPipe: 
                return hasMetalPipe;
            case RequiredItem.Flower: 
                return hasFlower;
            case RequiredItem.BeerBottle: 
                return bottleCount > 0; 
            case RequiredItem.MoneyForFlowers:
               if (flowerShopState == 0) return true; // Свободный вход для первого диалога (без денег)
               if (flowerShopState == 1) return playerMoney >= flowerPrice; // Жесткая проверка денег для второго захода
               return false; // Магазин закрыт навсегда (если flowerShopState >= 2)
            case RequiredItem.None:
                return true;
            default: 
                return false;
        }
    }
}
