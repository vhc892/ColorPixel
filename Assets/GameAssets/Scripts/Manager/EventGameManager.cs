using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventGameManager : MonoBehaviour
{
    public static EventGameManager Instance;
    private const string FirstEventPlayKey = "HasPlayedEventTutorial";
    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public EventGame currentEventGame;

    public void PlayEvent(int index)
    {
        if (index < 0 || index >= DatabaseManager.Instance.eventDatabase.Length) return;
        EventGame[] eventGames = DatabaseManager.Instance.eventDatabase;
        if (currentEventGame == null)
        {
            currentEventGame = Instantiate(eventGames[index], Vector3.zero, Quaternion.identity).GetComponent<EventGame>();
        }
        else if (currentEventGame != null && eventGames[index].name != currentEventGame.name)
        {
            Destroy(currentEventGame.gameObject);
            currentEventGame = Instantiate(eventGames[index], Vector3.zero, Quaternion.identity).GetComponent<EventGame>();
        }
        InputHandler.Instance.SwitchToEventInput();
        UIManager.Instance.ShowEventGameplayUI();
        currentEventGame.gameObject.SetActive(true);

        if (PlayerPrefs.GetInt(FirstEventPlayKey, 0) == 0)
        {
            currentEventGame.StartTutorial();
            PlayerPrefs.SetInt(FirstEventPlayKey, 1);
            PlayerPrefs.Save();
        }
    }

    public void PlayEvent()
    {
        if (currentEventGame == null) return;
        InputHandler.Instance.SwitchToEventInput();
        UIManager.Instance.ShowEventGameplayUI();
        currentEventGame.gameObject.SetActive(true);
    }

    public SpriteRenderer GetEventBG()
    {
        if (currentEventGame == null) return null;
        return currentEventGame.event_BG;
    }

    public void HideEvent()
    {
        if (currentEventGame == null) return;
        currentEventGame.gameObject.SetActive(false);
    }
}
