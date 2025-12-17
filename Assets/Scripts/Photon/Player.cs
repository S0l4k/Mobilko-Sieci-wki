using Fusion;
using UnityEngine;

public class Player : NetworkBehaviour
{
    [Networked] public NetworkObject HeldItem { get; private set; }

    [SerializeField] private Transform holdPoint;
    [SerializeField] private float interactRange = 2f;
    [SerializeField] private float interactHeight = 1.2f;
    [SerializeField] private float moveSpeed = 5f;


    private Vector3 _forward;
    private NetworkCharacterController _cc;
    private NetworkButtons _previousButtons;

    private Vector3 _interpPosition;
    private Quaternion _interpRotation;

    private void Awake()
    {
        _cc = GetComponent<NetworkCharacterController>();
        _forward = transform.forward;

        _interpPosition = transform.position;
        _interpRotation = transform.rotation;
    }

    // ---------------- NETWORK UPDATE ----------------
    public override void FixedUpdateNetwork()
    {
        if (GetInput(out NetworkInputData data))
        {
            // ---------------- RUCH ----------------
            Vector3 moveDir = Vector3.zero;

            // PC: WASD
            moveDir += data.direction;

            // Mobile: joystick
            moveDir += new Vector3(data.mobileDirection.x, 0f, data.mobileDirection.y);

            if (moveDir.sqrMagnitude > 0f)
            {
                moveDir.Normalize();
                _cc.Move(moveSpeed * moveDir * Runner.DeltaTime);
                _forward = moveDir;
                // Aktualizacja animacji
             
            }
            float speed = moveDir.magnitude;
            // ---------------- INTERACT ----------------
            bool interactPressed = data.buttons.WasPressed(_previousButtons, NetworkInputData.INTERACT) || data.interact;
            if (Object.HasInputAuthority && interactPressed)
                TryInteract();

            _previousButtons = data.buttons;
        }

        // ---------------- TRZYMANE PRZEDMIOTY (SERVER) ----------------
        if (HeldItem != null && Object.HasStateAuthority)
        {
            HeldItem.transform.position = holdPoint.position;
            HeldItem.transform.rotation = holdPoint.rotation;
        }

        // ---------------- INTERPOLACJA CLIENT ----------------
        if (!Object.HasStateAuthority)
        {
            _interpPosition = Vector3.Lerp(_interpPosition, _cc.transform.position, 0.2f);
            _interpRotation = Quaternion.Slerp(_interpRotation, Quaternion.LookRotation(_forward), 0.2f);

            transform.position = _interpPosition;
            transform.rotation = _interpRotation;
        }
        else
        {
            _interpPosition = transform.position;
            _interpRotation = transform.rotation;
        }
    }

    // ---------------- CLIENT SIDE INTERACT ----------------
    private void TryInteract()
    {
        Vector3 start = transform.position + Vector3.up * interactHeight;
        if (!Physics.Raycast(start, _forward, out var hit, interactRange))
            return;

        if (hit.collider.TryGetComponent<NetworkObject>(out var obj))
        {
            RPC_RequestInteract(obj);
        }
    }

    // ---------------- SERVER-SIDE INTERACT LOGIC ----------------
    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    private void RPC_RequestInteract(NetworkObject obj)
    {
        if (obj == null) return;

        // 1) Przedmiot na ziemi
        if (obj.TryGetComponent<KitchenItem>(out var item))
        {
            if (item.CanBePickedUp)
            {
                RPC_Pickup(obj);
                return;
            }
        }

        // 2) Interact z IInteractable (stoły, urządzenia itd.)
        if (obj.TryGetComponent<IInteractable>(out var interactable))
        {
            interactable.Interact(this);
        }
    }

    // ---------------- PICKUP ----------------
    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_Pickup(NetworkObject item)
    {
        if (HeldItem != null || item == null) return;

        HeldItem = item;

        if (item.TryGetComponent<Rigidbody>(out var rb))
        {
            rb.isKinematic = true;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        if (TryGetComponent(out Collider pCol) && item.TryGetComponent(out Collider iCol))
            Physics.IgnoreCollision(pCol, iCol, true);

        Debug.Log($"[Player] Podniesiono {item.name}");
    }

    // ---------------- DROP ----------------
    public void Drop()
    {
        if (HeldItem == null) return;

        Vector3 dropPos = transform.position + _forward * 1f + Vector3.up * 0.3f;
        RPC_DropItem(HeldItem, dropPos, Quaternion.identity);
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_DropItem(NetworkObject item, Vector3 pos, Quaternion rot)
    {
        if (item == null) return;

        if (item.TryGetComponent<Rigidbody>(out var rb))
            rb.isKinematic = false;

        item.transform.position = pos;
        item.transform.rotation = rot;

        if (TryGetComponent(out Collider pCol) && item.TryGetComponent(out Collider iCol))
            Physics.IgnoreCollision(pCol, iCol, false);

        if (HeldItem == item)
            HeldItem = null;

        Debug.Log($"[Player] Upuszczono {item.name}");
    }

    // ---------------- POURING LIQUID ----------------
    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_PourLiquidToStation(NetworkObject glassObj, NetworkObject liquidObj)
    {
        var glass = glassObj?.GetComponent<KitchenItem>();
        var liquid = liquidObj?.GetComponent<KitchenItem>();
        if (glass == null || liquid == null) return;

        switch (liquid.Variant)
        {
            case ItemVariant.Vodka: glass.Variant = ItemVariant.GlassWithVodka; break;
            case ItemVariant.Plazma: glass.Variant = ItemVariant.GlassWithPlazma; break;
            case ItemVariant.Poison: glass.Variant = ItemVariant.GlassWithPoison; break;
            case ItemVariant.Blood: glass.Variant = ItemVariant.GlassWithBlood; break;
            case ItemVariant.Magma: glass.Variant = ItemVariant.GlassWithMagma; break;
            default: return;
        }

        Runner.Despawn(liquidObj);

        var table = glassObj.GetComponentInParent<PouringStation>();
        table?.ReceiveItem(glassObj);
    }

    // ---------------- PLACE ON TABLE ----------------
    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_PlaceOnTable(NetworkObject tableObj, NetworkObject item)
    {
        if (item == null || tableObj == null) return;

        var table = tableObj.GetComponent<Table>();
        if (table != null && table.HeldItem == null)
        {
            table.ReceiveItem(item);
            HeldItem = null;
            Debug.Log($"[Player] Odłożono {item.name} na {table.name}");
            return;
        }

        // Nie udało się — drop
        Vector3 dropPos = transform.position + _forward + Vector3.up * 0.3f;
        RPC_DropItem(item, dropPos, Quaternion.identity);
    }

    public void ClearHeldItem()
    {
        HeldItem = null;
    }
    private void OnDrawGizmosSelected()
    {
        // Kolor zasięgu interakcji
        Gizmos.color = Color.yellow;
        Vector3 start = transform.position + Vector3.up * interactHeight;
        Gizmos.DrawLine(start, start + _forward * interactRange);
        Gizmos.DrawWireSphere(start + _forward * interactRange, 0.2f);

        // Kierunek patrzenia
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, transform.position + _forward);

        // Hold point
        if (holdPoint != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(holdPoint.position, 0.1f);
        }
    }
}

