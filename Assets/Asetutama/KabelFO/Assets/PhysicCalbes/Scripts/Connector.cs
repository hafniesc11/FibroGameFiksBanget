using System.Collections;
using UnityEngine;
using NaughtyAttributes;


namespace HPhysic
{
    [RequireComponent(typeof(Rigidbody))]
    public class Connector : MonoBehaviour
    {
        public enum ConType { Male, Female }
        public enum CableColor { White, Red, Green, Yellow, Blue, Cyan, Magenta }

        [field: Header("Settings")]

        [field: SerializeField] public ConType ConnectionType { get; private set; } = ConType.Male;
        [field: SerializeField, OnValueChanged(nameof(UpdateConnectorColor))] public CableColor ConnectionColor { get; private set; } = CableColor.White;

        [SerializeField] private bool makeConnectionKinematic = false;
        private bool _wasConnectionKinematic;

        [SerializeField] private bool hideInteractableWhenIsConnected = false;
        [SerializeField] private bool allowConnectDifrentCollor = false;
        [SerializeField] private MediaConverterController mediaConverter;

        [field: SerializeField] public Connector ConnectedTo { get; private set; }


        [Header("Object to set")]
        [SerializeField, Required] private Transform connectionPoint;
        [SerializeField] private MeshRenderer collorRenderer;
        [SerializeField] private ParticleSystem sparksParticle;


        private FixedJoint _fixedJoint;
        public Rigidbody Rigidbody { get; private set; }

        public Vector3 ConnectionPosition => connectionPoint ? connectionPoint.position : transform.position;
        public Quaternion ConnectionRotation => connectionPoint ? connectionPoint.rotation : transform.rotation;
        public Quaternion RotationOffset => connectionPoint ? connectionPoint.localRotation : Quaternion.Euler(Vector3.zero);
        public Vector3 ConnectedOutOffset => connectionPoint ? connectionPoint.right : transform.right;

        public bool IsConnected => ConnectedTo != null;
        public bool IsConnectedRight => IsConnected && ConnectionColor == ConnectedTo.ConnectionColor;
        [SerializeField] private float disconnectDistance = 0.15f;
        private UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grabInteractable;
        private bool isTryingToConnect = false;

private void Update()
{
    if (IsConnected && ConnectedTo != null)
    {
        float distance = Vector3.Distance(
            ConnectionPosition,
            ConnectedTo.ConnectionPosition
        );

        if (distance > disconnectDistance)
        {
            Disconnect();
        }
    }
}
private Connector candidateConnector;
private void OnTriggerEnter(Collider other)
{
    if (isTryingToConnect) return;
    if (IsConnected) return;

    if (grabInteractable != null && grabInteractable.isSelected)
        return;

    Connector otherConnector = other.GetComponent<Connector>();
    if (otherConnector == null) return;

    if (!CanConnect(otherConnector)) return;

    UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable otherGrab = otherConnector.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
    if (otherGrab != null && otherGrab.isSelected)
        return;

    candidateConnector = otherConnector;
    isTryingToConnect = true;

    Connect(otherConnector);
}

private void OnTriggerExit(Collider other)
{
    if (other == null) return;

    Connector otherConnector = other.GetComponent<Connector>();
    if (otherConnector == null) return;

    if (candidateConnector == otherConnector)
    {
        candidateConnector = null;
        isTryingToConnect = false;
    }
}

        private void Awake()
        {
            Rigidbody = gameObject.GetComponent<Rigidbody>();
            grabInteractable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
        }

        private void Start()
        {
            UpdateConnectorColor();

            if (ConnectedTo != null)
            {
                Connector t = ConnectedTo;
                ConnectedTo = null;
                Connect(t);
            }
        }

    
        private void OnDisable() => Disconnect();

        public void SetAsConnectedTo(Connector secondConnector)
        {
            ConnectedTo = secondConnector;
            _wasConnectionKinematic = secondConnector.Rigidbody.isKinematic;
            UpdateInteractableWhenIsConnected();
        }
        public void Connect(Connector secondConnector)
        {
            if (secondConnector == null)
            {
                Debug.LogWarning("Attempt to connect null");
                return;
            }

            if (IsConnected)
                Disconnect(secondConnector);

            secondConnector.transform.rotation = ConnectionRotation * secondConnector.RotationOffset;
            secondConnector.transform.position = ConnectionPosition - (secondConnector.ConnectionPosition - secondConnector.transform.position);

            _fixedJoint = gameObject.AddComponent<FixedJoint>();
            _fixedJoint.connectedBody = secondConnector.Rigidbody;
            _fixedJoint.breakForce = Mathf.Infinity;
            _fixedJoint.breakTorque = Mathf.Infinity;

            secondConnector.SetAsConnectedTo(this);
            _wasConnectionKinematic = secondConnector.Rigidbody.isKinematic;
            if (makeConnectionKinematic)
                secondConnector.Rigidbody.isKinematic = true;
            ConnectedTo = secondConnector;
            if (mediaConverter != null){
                Debug.Log("FO Masuk Port");
                mediaConverter.KabelFOMasuk();
            }
            // === NETWORK LOGIC ===
NetworkPort myPort = GetComponent<NetworkPort>();
NetworkPort otherPort = secondConnector.GetComponent<NetworkPort>();

if (myPort != null && otherPort != null)
{
    myPort.ConnectTo(otherPort);
    Debug.Log($"[NETWORK CONNECT] {myPort.name} -> {otherPort.name}");
}
//             Debug.Log(
//     $"[CONNECTED] {gameObject.name} ({ConnectionType}) -> {secondConnector.gameObject.name} ({secondConnector.ConnectionType})"
// );

            // sparks on inncretc connection
            if (incorrectSparksC == null && sparksParticle && IsConnected && !IsConnectedRight)
            {
                incorrectSparksC = IncorrectSparks();
                StartCoroutine(incorrectSparksC);
            }

            // disable outline on select
            UpdateInteractableWhenIsConnected();
            isTryingToConnect = false;
        }
public void Disconnect(Connector onlyThis = null)
{
    if (ConnectedTo == null || (onlyThis != null && onlyThis != ConnectedTo))
        return;

    Destroy(_fixedJoint);

    // reset velocity agar tidak terpental
    Rigidbody.linearVelocity = Vector3.zero;
    Rigidbody.angularVelocity = Vector3.zero;

    Connector toDisconnect = ConnectedTo;
    // === NETWORK DISCONNECT ===
NetworkPort myPort = GetComponent<NetworkPort>();
NetworkPort otherPort = ConnectedTo ? ConnectedTo.GetComponent<NetworkPort>() : null;

if (myPort != null && otherPort != null)
{
    myPort.Disconnect();
    Debug.Log($"[NETWORK DISCONNECT] {myPort.name}");
}
    ConnectedTo = null;
    if (mediaConverter != null){
            Debug.Log("FO Keluar Port");
        mediaConverter.KabelFOKeluar();
    }

    // kembalikan kinematic state
    if (makeConnectionKinematic)
        toDisconnect.Rigidbody.isKinematic = _wasConnectionKinematic;

    // recursive disconnect aman
    toDisconnect.Disconnect(this);

    if (sparksParticle)
    {
        sparksParticle.Stop();
        sparksParticle.Clear();
    }

    isTryingToConnect = false;
    candidateConnector = null;

    UpdateInteractableWhenIsConnected();
}

        private void UpdateInteractableWhenIsConnected()
        {
            if (hideInteractableWhenIsConnected)
            {
                if (TryGetComponent(out Collider collider))
                    collider.enabled = !IsConnected;
            }
        }


        private IEnumerator incorrectSparksC;
        private IEnumerator IncorrectSparks()
        {
            while (incorrectSparksC != null && sparksParticle && IsConnected && !IsConnectedRight)
            {
                sparksParticle.Play();

                yield return new WaitForSeconds(Random.Range(0.6f, 0.8f));
            }
            incorrectSparksC = null;
        }

        private void UpdateConnectorColor()
        {
            if (collorRenderer == null)
                return;

            Color color = MaterialColor(ConnectionColor);
            MaterialPropertyBlock probs = new();
            collorRenderer.GetPropertyBlock(probs);
            probs.SetColor("_Color", color);
            collorRenderer.SetPropertyBlock(probs);
        }

        private Color MaterialColor(CableColor cableColor) => cableColor switch
        {
            CableColor.White => Color.white,
            CableColor.Red => Color.red,
            CableColor.Green => Color.green,
            CableColor.Yellow => Color.yellow,
            CableColor.Blue => Color.blue,
            CableColor.Cyan => Color.cyan,
            CableColor.Magenta => Color.magenta,
            _ => Color.clear
        };


        public bool CanConnect(Connector secondConnector) =>
            this != secondConnector
            && !this.IsConnected && !secondConnector.IsConnected
            && this.ConnectionType != secondConnector.ConnectionType
            && (this.allowConnectDifrentCollor || secondConnector.allowConnectDifrentCollor || this.ConnectionColor == secondConnector.ConnectionColor);
    }

    
}