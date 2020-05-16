using Mirror;
using UnityEngine;
using UnityEngine.UI;

public class ShopMenu : MonoBehaviour
{
    public CarShopData[] datas => ShopInventory.Core.cars;
    public int SelectedIndex;
    public int PreviewIndex;
    public static CarShopData selected;
    public CarShopData preview;

    public Text price;
    public Button Selected;
    public Text SelectionText;

    private bool initialized = false;

    // Start is called before the first frame update
    void Start()
    {
        if (!initialized)
            Initialized();
    }

    private void OnEnable()
    {
        if (!initialized)
            Initialized();
    }

    public void Initialized()
    {
        if (NetworkClient.isConnected)
            return;
        if (datas != null && datas.Length > 0)
        {
            for (int i = 0; i < datas.Length; i++)
            {
                if (!PlayerPrefs.HasKey(datas[i].GameName))
                    PlayerPrefs.SetInt(datas[i].GameName, datas[i].isPurchased ? 1 : 0);
                else
                    datas[i].isPurchased = PlayerPrefs.GetInt(datas[i].GameName) == 1 ? true : false;
            }
            if (PlayerPrefs.HasKey("SelectedCar"))
                SelectedIndex = PlayerPrefs.GetInt("SelectedCar");
            else
                PlayerPrefs.SetInt("SelectedCar", SelectedIndex);
            PlayerPrefs.Save();
            selected = datas[SelectedIndex];
            selected.Car.SetActive(true);
            CoreManager.Core.CurrentCar = selected.NetCar;
            initialized = true;
        }
        else
        {
            Debug.LogError("Hold There bro... " +
                "Set atleast one element to datas field in editor inspector.. " +
                "I dont like to fill ur console with any more error.. " +
                "Script Auto Disabled..");
            enabled = false;
        }
    }

    public void ShopOpen()
    {
        PreviewIndex = SelectedIndex;
        preview = selected;
        preview.Car.SetActive(true);
    }
    public void ShopClose()
    {
        preview.Car.SetActive(false);
        selected.Car.SetActive(true);
        CoreManager.Core.CurrentCar = selected.NetCar;
    }

    public void Previous_Next(int value)
    {
        PreviewIndex += value;
        if (PreviewIndex > datas.Length - 1)
        {
            PreviewIndex = 0;
        }
        if (PreviewIndex < 0)
        {
            PreviewIndex = datas.Length - 1;
        }
        preview.Car.SetActive(false);
        preview = datas[PreviewIndex];
        preview.Car.SetActive(true);
    }

    public void Select_Purchase()
    {
        if (!preview.isPurchased)
        {
            preview.isPurchased = true;
            datas[PreviewIndex].isPurchased = true;
            PlayerPrefs.SetInt(selected.GameName, selected.isPurchased ? 1 : 0);
        }
        selected = preview;
        SelectedIndex = PreviewIndex;
        PlayerPrefs.SetInt("SelectedCar", SelectedIndex);
    }

    // Update is called once per frame
    void Update()
    {
        if (preview.Car == selected.Car)
        {
            Selected.interactable = false;
            SelectionText.text = "SELECTED";
        }
        else
        {
            if (Selected.interactable == false)
                Selected.interactable = true;
            SelectionText.text = "SELECT";
        }
        if (preview.isPurchased)
        {
            price.text = "PURCHASED";
        }
        else
        {
            SelectionText.text = "BUY";
            price.text = "$" + preview.price;
        }

    }

    private void OnDisable()
    {
        PlayerPrefs.Save();
    }

#if UNITY_EDITOR
    [ContextMenu("Do")]
    public void ResetSaves()
    {
        PlayerPrefs.DeleteAll();
    }
#endif
}


