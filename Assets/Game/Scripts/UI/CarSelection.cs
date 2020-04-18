using Game.Scripts.UI;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class CarSelection : MonoBehaviour
{
    public const string ScenePassingData = "ScenePassingData_CarSelection";

    [Range(0, 100)] public float rotationSpeed = 1.7f;
    [Range(10, 180)] public float wobblingIntensity = 30f;
    public GameObject rotatingCars;
    public UnityEvent onAsyncSceneLoad;


    private int _carIndex;
    private AsyncOperation _arenaScene;
    private bool _asyncSceneLoaded;

    private void Start()
    {
        _arenaScene = SceneManager.LoadSceneAsync("Game/Scenes/Arena");
        _arenaScene.allowSceneActivation = false;
    }

    private void Update()
    {
        HandleVisualRotation();
        HandleInput();
        HandleOnAsyncSceneLoadFeedback();
    }

    private void HandleVisualRotation()
    {
        var lastRotation = rotatingCars.transform.rotation.eulerAngles;
        rotatingCars.transform.rotation = Quaternion.Euler(
            0,
            lastRotation.y + rotationSpeed,
            Mathf.Sin(Time.time) * wobblingIntensity
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

    private void HandleOnAsyncSceneLoadFeedback()
    {
        if (_asyncSceneLoaded || _arenaScene.progress < 0.9f)
        {
            return;
        }

        _asyncSceneLoaded = true;
        onAsyncSceneLoad.Invoke();
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
        var scenePassingData = new GameObject(ScenePassingData).AddComponent<ScenePassingData>();
        scenePassingData.number = _carIndex;
        DontDestroyOnLoad(scenePassingData);
        _arenaScene.allowSceneActivation = true;
    }
}
