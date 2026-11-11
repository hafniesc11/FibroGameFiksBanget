using UnityEngine;
using System.Collections;

public class FindProblemTrigger : MonoBehaviour
{
    public GameFlowController gameFlow;

    bool selesai = false;

    private void OnTriggerEnter(Collider other)
    {
        if (selesai) return;

        if (!other.CompareTag("Player"))
            return;

        selesai = true;

        StartCoroutine(TampilkanObjective());
    }

    IEnumerator TampilkanObjective()
    {
        yield return new WaitForSeconds(0.5f);

        gameFlow.FoundProblemArea();
    }
}