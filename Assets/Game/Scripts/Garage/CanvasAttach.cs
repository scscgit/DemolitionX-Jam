using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


///<summary>
///Every Function in this script is called by UI Events
///</summary>
public class CanvasAttach : MonoBehaviour
{
    #region singleton

    private static CanvasAttach instance;

    public static CanvasAttach Instance
    {
        get
        {
            if (instance == null) instance = GameObject.FindObjectOfType<CanvasAttach>() as CanvasAttach;
            return instance;
        }
    }

    #endregion

    public VehiclePhysics activePlayerVehicle;
    public Transform activeCamera;

    [Header("UI Menus")] public GameObject wheelsMenu;
    public GameObject configurationMenu;
    public GameObject steeringAssistancesMenu;
    public GameObject colorsMenu;
    public GameObject changePartsMenu;

    [Header("UI Sliders")] public Slider frontCamber;
    public Slider rearCamber;
    public Slider frontSuspensionDistances;
    public Slider rearSuspensionDistances;
    public Slider frontSuspensionDampers;
    public Slider rearSuspensionDampers;
    public Slider frontSuspensionSprings;
    public Slider rearSuspensionSprings;
    public Slider gearShiftingThreshold;
    public Slider clutchThreshold;

    [Header("UI Toggles")] public Toggle TCS;
    public Toggle ABS;
    public Toggle ESP;
    public Toggle SH;
    public Toggle counterSteering;
    public Toggle NOS;
    public Toggle turbo;
    public Toggle exhaustFlame;
    public Toggle revLimiter;
    public Toggle clutchMargin;

    [Header("UI InputFields")] public InputField maxSpeed;
    public InputField maxBrake;
    public InputField maxTorque;

    [Header("UI Dropdown Menus")] public Dropdown drivetrainMode;
    public Dropdown spoilerDropdown;
    public Dropdown wheelDropdown;
    public Dropdown wheelSizeDropdown;

    void Start()
    {
        // This vehicle is practically always disabled on start due to being a NetworkIdentity without any server
        activePlayerVehicle?.gameObject.SetActive(true);
        CheckUIs();
    }

    public void BackToCarSelection()
    {
        SceneManager.UnloadSceneAsync("Game/Scenes/Garage");
    }

    public void CheckUIs()
    {
        if (!Instance.activePlayerVehicle)
            return;

        frontCamber.value = Instance.activePlayerVehicle.FrontLeftWheelCollider.camber;
        rearCamber.value = Instance.activePlayerVehicle.RearLeftWheelCollider.camber;
        frontSuspensionDistances.value =
            Instance.activePlayerVehicle.FrontLeftWheelCollider.wheelCollider.suspensionDistance;
        rearSuspensionDistances.value =
            Instance.activePlayerVehicle.RearLeftWheelCollider.wheelCollider.suspensionDistance;
        frontSuspensionDampers.value =
            Instance.activePlayerVehicle.FrontLeftWheelCollider.wheelCollider.suspensionSpring.damper;
        rearSuspensionDampers.value =
            Instance.activePlayerVehicle.RearLeftWheelCollider.wheelCollider.suspensionSpring.damper;
        frontSuspensionSprings.value =
            Instance.activePlayerVehicle.FrontLeftWheelCollider.wheelCollider.suspensionSpring.spring;
        rearSuspensionSprings.value =
            Instance.activePlayerVehicle.RearLeftWheelCollider.wheelCollider.suspensionSpring.spring;
        gearShiftingThreshold.value = Instance.activePlayerVehicle.gearShiftingThreshold;
        clutchThreshold.value = Instance.activePlayerVehicle.clutchInertia;

        TCS.isOn = Instance.activePlayerVehicle.TCS;
        ABS.isOn = Instance.activePlayerVehicle.ABS;
        ESP.isOn = Instance.activePlayerVehicle.ESP;
        SH.isOn = Instance.activePlayerVehicle.steeringHelper;
        counterSteering.isOn = Instance.activePlayerVehicle.applyCounterSteering;
        NOS.isOn = Instance.activePlayerVehicle.useNOS;
        turbo.isOn = Instance.activePlayerVehicle.useTurbo;
        exhaustFlame.isOn = Instance.activePlayerVehicle.useExhaustFlame;
        revLimiter.isOn = Instance.activePlayerVehicle.useRevLimiter;
        clutchMargin.isOn = Instance.activePlayerVehicle.useClutchMarginAtFirstGear;

        maxSpeed.text = Instance.activePlayerVehicle.maxspeed.ToString();
        maxBrake.text = Instance.activePlayerVehicle.brakeTorque.ToString();
        maxTorque.text = Instance.activePlayerVehicle.engineTorque.ToString();

        switch (Instance.activePlayerVehicle._wheelTypeChoise)
        {
            case VehiclePhysics.WheelType.FWD:
                drivetrainMode.value = 0;
                break;

            case VehiclePhysics.WheelType.RWD:
                drivetrainMode.value = 1;
                break;

            case VehiclePhysics.WheelType.AWD:
                drivetrainMode.value = 2;
                break;

            case VehiclePhysics.WheelType.BIASED:
                drivetrainMode.value = 3;
                break;
        }

        wheelDropdown.value = Custamization.wheelIndex;
        wheelSizeDropdown.value = Custamization.wheelSizeIndex;
        spoilerDropdown.value = Custamization.spoilerIndex;
    }

    public void OpenMenu(GameObject activeMenu)
    {
        if (activeMenu.activeInHierarchy)
        {
            activeMenu.SetActive(false);
            return;
        }

        wheelsMenu.SetActive(false);
        configurationMenu.SetActive(false);
        steeringAssistancesMenu.SetActive(false);
        colorsMenu.SetActive(false);
        changePartsMenu.SetActive(false);

        activeMenu.SetActive(true);
    }

    public void CloseAllMenus()
    {
        wheelsMenu.SetActive(false);
        configurationMenu.SetActive(false);
        steeringAssistancesMenu.SetActive(false);
        colorsMenu.SetActive(false);
        changePartsMenu.SetActive(false);
    }

    public void SetCustomizationMode(bool state)
    {
        if (!Instance.activePlayerVehicle)
            return;

        Custamization.SetCustomizationMode(Instance.activePlayerVehicle, state);

        if (state)
            CheckUIs();
    }

    public void SetFrontCambersBySlider(Slider slider)
    {
        Custamization.SetFrontCambers(Instance.activePlayerVehicle, slider.value);
    }

    public void SetRearCambersBySlider(Slider slider)
    {
        Custamization.SetRearCambers(Instance.activePlayerVehicle, slider.value);
    }

    public void TogglePreviewSmokeByToggle(Toggle toggle)
    {
        Custamization.SetSmokeParticle(Instance.activePlayerVehicle, toggle.isOn);
    }

    public void TogglePreviewExhaustFlameByToggle(Toggle toggle)
    {
        Custamization.SetExhaustFlame(Instance.activePlayerVehicle, toggle.isOn);
    }

    public void SetSmokeColorByColorPicker(ColorSlider color)
    {
        Custamization.SetSmokeColor(Instance.activePlayerVehicle, 0, color.color);
    }

    public void SetHeadlightColorByColorPicker(ColorSlider color)
    {
        Custamization.SetHeadlightsColor(Instance.activePlayerVehicle, color.color);
    }

    int wheelIndex;
    public void ChangeWheelsByDropdown(Dropdown dropdown)
    {
        Custamization.ChangeWheels(Instance.activePlayerVehicle, dropdown.value,
            Custamization.wheelSizeIndex, Custamization.tyreIndex);
    }

    public void ChangeTyreTextureByDropdown(Dropdown dropdown)
    {
        Custamization.ChangeWheels(Instance.activePlayerVehicle, Custamization.wheelIndex,
            Custamization.wheelSizeIndex, dropdown.value);
    }

    public void ChangeWheelSizeIndexByDropDown(Dropdown dropdown) {

        Custamization.ChangeWheels(Instance.activePlayerVehicle, Custamization.wheelIndex,
            dropdown.value, Custamization.tyreIndex);

    }

    public void SetFrontSuspensionTargetsBySlider(Slider slider)
    {
        Custamization.SetFrontSuspensionsTargetPos(Instance.activePlayerVehicle, slider.value);
    }

    public void SetRearSuspensionTargetsBySlider(Slider slider)
    {
        Custamization.SetRearSuspensionsTargetPos(Instance.activePlayerVehicle, slider.value);
    }

    public void SetAllSuspensionTargetsByButton(float strength)
    {
        Custamization.SetAllSuspensionsTargetPos(Instance.activePlayerVehicle, strength);
    }

    public void SetFrontSuspensionDistancesBySlider(Slider slider)
    {
        Custamization.SetFrontSuspensionsDistances(Instance.activePlayerVehicle, slider.value);
    }

    public void SetRearSuspensionDistancesBySlider(Slider slider)
    {
        Custamization.SetRearSuspensionsDistances(Instance.activePlayerVehicle, slider.value);
    }

    public void SetGearShiftingThresholdBySlider(Slider slider)
    {
        Custamization.SetGearShiftingThreshold(Instance.activePlayerVehicle, Mathf.Clamp(slider.value, .5f, .95f));
    }

    public void SetClutchThresholdBySlider(Slider slider)
    {
        Custamization.SetClutchThreshold(Instance.activePlayerVehicle, Mathf.Clamp(slider.value, .1f, .9f));
    }

    public void SetDriveTrainModeByDropdown(Dropdown dropdown)
    {
        switch (dropdown.value)
        {
            case 0:
                Custamization.SetDrivetrainMode(Instance.activePlayerVehicle, VehiclePhysics.WheelType.FWD);
                break;

            case 1:
                Custamization.SetDrivetrainMode(Instance.activePlayerVehicle, VehiclePhysics.WheelType.RWD);
                break;

            case 2:
                Custamization.SetDrivetrainMode(Instance.activePlayerVehicle, VehiclePhysics.WheelType.AWD);
                break;

            case 3:
                Custamization.SetDrivetrainMode(Instance.activePlayerVehicle, VehiclePhysics.WheelType.BIASED);
                break;
        }
    }

    public void SetNOSByToggle(Toggle toggle)
    {
        Custamization.SetNOS(Instance.activePlayerVehicle, toggle.isOn);
    }

    public void SetTurboByToggle(Toggle toggle)
    {
        Custamization.SetTurbo(Instance.activePlayerVehicle, toggle.isOn);
    }

    public void SetExhaustFlameByToggle(Toggle toggle)
    {
        Custamization.SetUseExhaustFlame(Instance.activePlayerVehicle, toggle.isOn);
    }

    public void SetRevLimiterByToggle(Toggle toggle)
    {
        Custamization.SetRevLimiter(Instance.activePlayerVehicle, toggle.isOn);
    }

    public void SetClutchMarginByToggle(Toggle toggle)
    {
        Custamization.SetClutchMargin(Instance.activePlayerVehicle, toggle.isOn);
    }

    public void SetFrontSuspensionsSpringForceBySlider(Slider slider)
    {
        Custamization.SetFrontSuspensionsSpringForce(Instance.activePlayerVehicle,
            Mathf.Clamp(slider.value, 10000f, 100000f));
    }

    public void SetRearSuspensionsSpringForceBySlider(Slider slider)
    {
        Custamization.SetRearSuspensionsSpringForce(Instance.activePlayerVehicle,
            Mathf.Clamp(slider.value, 10000f, 100000f));
    }

    public void SetFrontSuspensionsSpringDamperBySlider(Slider slider)
    {
        Custamization.SetFrontSuspensionsSpringDamper(Instance.activePlayerVehicle,
            Mathf.Clamp(slider.value, 1000f, 10000f));
    }

    public void SetRearSuspensionsSpringDamperBySlider(Slider slider)
    {
        Custamization.SetRearSuspensionsSpringDamper(Instance.activePlayerVehicle,
            Mathf.Clamp(slider.value, 1000f, 10000f));
    }

    public void SetMaximumSpeedByInputField(InputField inputField)
    {
        Custamization.SetMaximumSpeed(Instance.activePlayerVehicle, StringToFloat(inputField.text, 200f));
        inputField.text = Instance.activePlayerVehicle.maxspeed.ToString();
    }

    public void SetMaximumTorqueByInputField(InputField inputField)
    {
        Custamization.SetMaximumTorque(Instance.activePlayerVehicle, StringToFloat(inputField.text, 2000f));
        inputField.text = Instance.activePlayerVehicle.engineTorque.ToString();
    }

    public void SetMaximumBrakeByInputField(InputField inputField)
    {
        Custamization.SetMaximumBrake(Instance.activePlayerVehicle, StringToFloat(inputField.text, 2000f));
        inputField.text = Instance.activePlayerVehicle.brakeTorque.ToString();
    }

    public void SetESP(Toggle toggle)
    {
        Custamization.SetESP(Instance.activePlayerVehicle, toggle.isOn);
    }

    public void SetABS(Toggle toggle)
    {
        Custamization.SetABS(Instance.activePlayerVehicle, toggle.isOn);
    }

    public void SetTCS(Toggle toggle)
    {
        Custamization.SetTCS(Instance.activePlayerVehicle, toggle.isOn);
    }

    public void SetSH(Toggle toggle)
    {
        Custamization.SetSH(Instance.activePlayerVehicle, toggle.isOn);
    }

    public void SetSHStrength(Slider slider)
    {
        Custamization.SetSHStrength(Instance.activePlayerVehicle, slider.value);
    }

    public void SaveStats()
    {
        Custamization.SaveStats(Instance.activePlayerVehicle);
    }

    public void LoadStats()
    {
        Custamization.LoadStats(Instance.activePlayerVehicle);
        CheckUIs();
    }

    private float StringToFloat(string stringValue, float defaultValue)
    {
        float result = defaultValue;
        float.TryParse(stringValue, out result);
        return result;
    }

    public void SetCarBodyColorByColorPicker(ColorSlider colorSlider)
    {
        Custamization.SetCarBodyColor(Instance.activePlayerVehicle, colorSlider.color);
    }

    public void SetPaintMetalnessByColorPicker(ColorSlider colorSlider)
    {
        Custamization.SetPaintMetalness(Instance.activePlayerVehicle, colorSlider.metalness);
    }

    public void SetPaintGlossinessByColorPicker(ColorSlider colorSlider)
    {
        Custamization.SetPaintGlossiness(Instance.activePlayerVehicle, colorSlider.glossiness);
    }

    public void SetCarPoilerByDropdown(Dropdown dropdown)
    {
        Custamization.SetSpoiler(Instance.activePlayerVehicle, dropdown.value);
    }

    public void SetRimColorBySlider(ColorSlider colorSlider)
    {
        Custamization.SetRimColor(Instance.activePlayerVehicle, colorSlider.color);
    }

    public void SetRimMetalnessByColorPicker(ColorSlider colorSlider)
    {
        Custamization.SetRIMPaintMetalness(instance.activePlayerVehicle, colorSlider.metalness);
    }

    public void SetRimGlossinessByColorPicker(ColorSlider colorSlider)
    {
        Custamization.SetRIMPaintGlossiness(instance.activePlayerVehicle, colorSlider.glossiness);
    }

    public void SetCarFrontBumperByDropdown(Dropdown dropdown)
    {
        Custamization.SetFrontBumper(Instance.activePlayerVehicle, dropdown.value);
    }

    public void SetTyreTextureByDropdown(Dropdown dropdown)
    {

    }
}
