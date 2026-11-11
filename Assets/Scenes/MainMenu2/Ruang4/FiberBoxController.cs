using UnityEngine;

public class FiberBoxController : MonoBehaviour
{
    public Animator animator;

    public void OpenBox(){
        animator.ResetTrigger("Tutup");
        animator.SetTrigger("Buka");
    }

    public void CloseBox(){
        animator.ResetTrigger("Buka");
        animator.SetTrigger("Tutup");
    }
}