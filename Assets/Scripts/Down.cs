using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Down : MonoBehaviour
{
    [SerializeField] GameObject replayButton;
    // Start is called before the first frame update
    void Start()
    {
        replayButton.SetActive(false);
        Time.timeScale = 1;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Time.timeScale = 0;
        replayButton.SetActive(true);
    }

    public void Replay()
    {
        SceneManager.LoadScene("Game");
    }
}
