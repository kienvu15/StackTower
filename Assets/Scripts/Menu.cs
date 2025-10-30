using TMPro;
using UnityEngine;

public class Menu : MonoBehaviour
{
    public GameObject scoreInGame;
    public GameObject bestScoreInMenu;

    public GameObject StartPanel;

    public TheStack theStack;
    void Start()
    {
        bestScoreInMenu.GetComponent<TextMeshProUGUI>().text = PlayerPrefs.GetInt("score").ToString();
    }

    public void StartGame()
    {
        bestScoreInMenu.SetActive(false);
        StartPanel.SetActive(false);

        scoreInGame.SetActive(true);
        theStack.gameStart = true;
    }
}
