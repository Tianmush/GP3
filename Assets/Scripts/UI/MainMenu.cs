using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void StartGame()
    {
        // ȷ����Ϸ��������������ȷ
        string gameSceneName = "1"; // ��Ϸ������������
        SceneManager.LoadScene(gameSceneName);
    }
}