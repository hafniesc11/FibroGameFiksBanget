using UnityEngine;

public class DoorController : MonoBehaviour
{
    public Transform doorLeft;
    public Transform doorRight;

    public float openSpeed = 2f;

    public float leftOpenAngle = -80f;
    public float rightOpenAngle = 80f;

    public GameObject openDoorUI;

    private bool playerNear = false;
    private bool isOpen = false;

    private Quaternion leftOpenRot;
    private Quaternion rightOpenRot;

    void Start()
    {
        leftOpenRot = Quaternion.Euler(
            doorLeft.eulerAngles.x,
            doorLeft.eulerAngles.y + leftOpenAngle,
            doorLeft.eulerAngles.z);

        rightOpenRot = Quaternion.Euler(
            doorRight.eulerAngles.x,
            doorRight.eulerAngles.y + rightOpenAngle,
            doorRight.eulerAngles.z);

        openDoorUI.SetActive(false);
    }

    void Update()
    {
        if (isOpen)
        {
            doorLeft.rotation = Quaternion.Lerp(
                doorLeft.rotation,
                leftOpenRot,
                Time.deltaTime * openSpeed);

            doorRight.rotation = Quaternion.Lerp(
                doorRight.rotation,
                rightOpenRot,
                Time.deltaTime * openSpeed);
        }
    }

    public void OpenDoor()
    {
        if (playerNear)
        {
            isOpen = true;
            openDoorUI.SetActive(false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerNear = true;
            openDoorUI.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerNear = false;
            openDoorUI.SetActive(false);
        }
    }
}