using System.Collections;
using UnityEngine;
using UnityEngine.UI;

class NewsScript : MonoBehaviour
{
    private string URL = "https://drive.google.com/uc?export=download&id=1MxS9pUuUeWSIgQtN1CZa9xRXKR59g2uq";
    private string News;
    [SerializeField] private Text NewsText;
    [SerializeField] private GameObject loadInd;

    void Start() => StartCoroutine(LoadTextData(URL));

    private void CheckNews()
    {
        loadInd.SetActive(false);
        NewsText.text = News;
    }

    private IEnumerator LoadTextData(string url)
    {
        WWW www = new WWW(url);
        yield return www;
        News = www.text;
        CheckNews();
    }

    public void newsReload()
    {
        NewsText.text = null;
        loadInd.SetActive(true);
        StartCoroutine(LoadTextData(URL));
    }
}
