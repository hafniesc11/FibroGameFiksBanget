using UnityEngine;
using System.Collections;

public class s_QuizManager : MonoBehaviour
{
    public GameObject quizPanel;
    public GameObject maskotBenar;
    public GameObject maskotSalah;
    public GameObject panelPenjelasan;
    
    void Start(){
        quizPanel.SetActive(false);
        maskotBenar.SetActive(false);
        maskotSalah.SetActive(false);
    }

    public void ShowQuiz(){
        quizPanel.SetActive(true);
    }

   public void JawabanBenar(){
    quizPanel.SetActive(false);
    maskotBenar.SetActive(true);
    }

    public void BukaPenjelasan(){
    maskotBenar.SetActive(false);
    panelPenjelasan.SetActive(true);
    }

    IEnumerator Benar(){
        quizPanel.SetActive(false);
        maskotBenar.SetActive(true);
        yield return new WaitForSeconds(2f);
        maskotBenar.SetActive(false);
        panelPenjelasan.SetActive(true);
}

    public void JawabanSalah(){
        quizPanel.SetActive(false);
        maskotSalah.SetActive(true);
    }
    public void CobaLagi(){
        maskotSalah.SetActive(false);
        quizPanel.SetActive(true);
    }
}
