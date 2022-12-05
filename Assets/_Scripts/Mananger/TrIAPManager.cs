using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.UI;
using TMPro;

public class TrIAPManager : MonoBehaviour, IStoreListener
{
    static TrIAPManager _instance;
    public static TrIAPManager xInstance { get { return _instance; } }

    static IStoreController m_StoreController; // The Unity Purchasing system.
    static IExtensionProvider m_StoreExtensionProvider;
    ITransactionHistoryExtensions m_TransactionHistoryExtensions;

    const int _numIAP = 4;

    TrIAPData[] _datas;

    [SerializeField] TextMeshProUGUI[] _txtNumberStamina;
    [SerializeField] Text[] _txtPriceStamina;
    [SerializeField] GameObject _goGuard;

    #region init
    void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        _datas = TrCSVParser.zParse();
        InitializePurchasing();
        _goGuard.SetActive(false);
    }

    string yGetPriceInfo(string id)
    {
        string result = m_StoreController.products.WithID(id).metadata.localizedPriceString;
        return result;
    }

    bool IsIntialized()
    {
        return m_StoreController != null && m_StoreExtensionProvider != null;
    }

    void InitializePurchasing()
    {
        if (IsIntialized()) return;

        var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());

        //Add products that will be purchasable and indicate its type.
        for (int i = 0; i < _datas.Length; i++)
        {
            builder.AddProduct(_datas[i]._id, ProductType.Consumable);
        }


        UnityPurchasing.Initialize(this, builder);
    }

    public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
    {
        Debug.Log("In-App Purchasing successfully initialized");
        m_StoreController = controller;

        for (int i = 0; i < _datas.Length; i++)
        {
            TrIAPData data = _datas[i];
            _txtNumberStamina[i].text = data._number.ToString();
#if PLATFORM_ANDROID
            _txtPriceStamina[i].text = yGetPriceInfo(data._id);
#endif
#if PLATFORM_iOS
            _txtPriceStamina[i].text = string.Format("{0}{1}", "$", data._price);
#endif
        }
        //#if PLAY
    }

    public void OnInitializeFailed(InitializationFailureReason error)
    {
        Debug.Log($"In-App Purchasing initialize failed: {error}");
    }
    #endregion
    public void xBuyStamina(int serialNumber)
    {
        _goGuard.SetActive(true);
        TrAudio_UI.xInstance.zzPlay_ClickButtonSmall();
        for (int i = 0; i < _datas.Length; i++)
        {
            if (_datas[i]._serialNumber == serialNumber)
            {
                m_StoreController.InitiatePurchase(_datas[i]._id);
                break;
            }
        }
    }

    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
    {
        //Retrieve the purchased product
        var product = args.purchasedProduct;

        string res = product.definition.id;

        //Add the purchased product to the players inventory
        for (int i = 0; i < _datas.Length; i++)
        {
            if (_datas[i]._id == res)
            {
                yAddStamina(_datas[i]._number);
                break;
            }
        }

        Debug.Log($"Purchase Complete - Product: {product.definition.id}");
        _goGuard.SetActive(false);
        //We return Complete, informing IAP that the processing on our side is done and the transaction can be closed.
        return PurchaseProcessingResult.Complete;
    }

    public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
    {
        Debug.Log($"Purchase failed - Product: '{product.definition.id}', PurchaseFailureReason: {failureReason}");
        _goGuard.SetActive(false);
    }



    void yAddStamina(int num)
    {
        DatabaseManager._myDatas.stamina += num;
        StaminaManager.xInstance.zFullStamina();
        StaminaManager.xInstance.zSetStaminaOnUI();
        DatabaseManager.xInstance.zSetStamina();
    }
}

