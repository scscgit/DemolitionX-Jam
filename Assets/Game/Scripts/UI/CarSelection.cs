using Game.Scripts.Network;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using Mirror;
public class CarSelection : MonoBehaviour
{
    public const string ScenePassingData = "ScenePassingData_CarSelection";

    [Range(0, 100)] public float rotationSpeed = 1.7f;
    [Range(10, 180)] public float wobblingIntensity = 30f;
    public GameObject rotatingCars;
    public UnityEvent disableReady;
    public UnityEvent enableReady;

    private int _carIndex;
    private GameNetworkPlayer _game;

    private void Awake()
    {
        _game = FindObjectOfType<GameNetworkPlayer>();
        if (ReferenceEquals(_game, null))
        {
            Debug.LogError("Launched CarSelection without the active game scene. You won't be able to start.");
        }
    }

    private void Update()
    {
        HandleVisualRotation();
        HandleInput();
    }

    private void HandleVisualRotation()
    {
        var lastRotation = rotatingCars.transform.rotation.eulerAngles;
        rotatingCars.transform.rotation = Quaternion.Euler(
            0,
            lastRotation.y + rotationSpeed,
            Mathf.Sin(Time.deltaTime) * wobblingIntensity
        );
    }

    private void HandleInput()
    {
        var leftKeyPressed = Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A);
        var rightKeyPressed = Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D);
        var enterKeyPressed = Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter);
        if (leftKeyPressed && !rightKeyPressed)
        {
            Left();
        }
        else if (rightKeyPressed && !leftKeyPressed)
        {
            Right();
        }
        else if (enterKeyPressed)
        {
            EnterGame();
        }
    }

    public void Left()
    {
        SetCarActive(false);
        var count = rotatingCars.transform.childCount;
        _carIndex = ((_carIndex - 1) % count + count) % count;
        SetCarActive(true);
    }

    public void Right()
    {
        SetCarActive(false);
        _carIndex = (_carIndex + 1) % rotatingCars.transform.childCount;
        SetCarActive(true);
    }

    private void SetCarActive(bool active)
    {
        rotatingCars.transform.GetChild(_carIndex).gameObject.SetActive(active);
    }

    public void EnterGame()
    {
        SceneManager.UnloadSceneAsync("Game/Scenes/CarSelection");
        _game.SelectedCar(_carIndex);
    }
}
