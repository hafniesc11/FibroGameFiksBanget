using UnityEngine;

public class RJ45 : MonoBehaviour
{
    public Transform snapPoint;
    public MediaConverterController mediaConverter;

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("UTP"))
        {
            // Snap ke port
            other.transform.position = snapPoint.position;
            other.transform.rotation = snapPoint.rotation;

            // Matikan physics
            Rigidbody rb = other.GetComponent<Rigidbody>();
            if(rb != null)
                rb.isKinematic = true;

            // Kasih tahu media converter kalau kabel sudah terpasang
            mediaConverter.KabelUTPMasuk();
        }
}

private void OnTriggerExit(Collider other)
{
    if (other.CompareTag("UTP"))
    {
        mediaConverter.KabelUTPKeluar();
    }
}
}
