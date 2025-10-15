using Fusion;
using TMPro;
using UnityEngine;

public class Player : NetworkBehaviour
{
    [SerializeField] private Ball _prefabBall;
    [SerializeField] private PhysxBall _prefabPhysxBall;
    public Material _material;


    [Networked] private TickTimer delay { get; set; }
    [Networked]
    public bool spawnedProjectile { get; set; }
    [Networked] private TickTimer messageLife { get; set; }
    [Networked]
    public NetworkObject HeldItem { get; set; }

    private NetworkCharacterController _cc;
    private Vector3 _forward;
    [SerializeField] private Transform holdPoint;
    public bool IsHolding(NetworkObject item)
    {
        return HeldItem == item;
    }
    private void Awake()
    {
        _cc = GetComponent<NetworkCharacterController>();
        _forward = transform.forward;
        _material = GetComponentInChildren<MeshRenderer>().material;
    }


    public override void FixedUpdateNetwork()
    {
        if (GetInput(out NetworkInputData data))
        {
            data.direction.Normalize();
            _cc.Move(5 * data.direction * Runner.DeltaTime);

            if (data.direction.sqrMagnitude > 0)
                _forward = data.direction;

            if (HasStateAuthority && delay.ExpiredOrNotRunning(Runner))
            {
                /*if (data.buttons.IsSet(NetworkInputData.MOUSEBUTTON0))
                {
                    delay = TickTimer.CreateFromSeconds(Runner, 0.5f);
                    Runner.Spawn(_prefabBall,
                      transform.position + _forward,
                      Quaternion.LookRotation(_forward),
                      Object.InputAuthority,
                      (runner, o) =>
                      {
                          // Initialize the Ball before synchronizing it
                          o.GetComponent<Ball>().Init();
                          spawnedProjectile = !spawnedProjectile;
                      });
                }
                else if (data.buttons.IsSet(NetworkInputData.MOUSEBUTTON1))
                {
                    delay = TickTimer.CreateFromSeconds(Runner, 0.5f);
                    Runner.Spawn(_prefabPhysxBall,
                      transform.position + _forward,
                      Quaternion.LookRotation(_forward),
                      Object.InputAuthority,
                      (runner, o) =>
                      {
                          spawnedProjectile = !spawnedProjectile;
                          o.GetComponent<PhysxBall>().Init(10 * _forward);
                      });
                }*/
            }
            if (Object.HasInputAuthority && data.buttons.IsSet(NetworkInputData.INTERACT))
            {
                TryInteract();
            }
            if (HeldItem != null)
            {
                // 🔹 jeśli przedmiot nie jest na stole, trzymamy go w ręce
                Table table = HeldItem.GetComponent<Table>();
                if (table == null || table.HeldItem != HeldItem)
                {
                    HeldItem.transform.position = holdPoint.position;
                    HeldItem.transform.rotation = holdPoint.rotation;
                }
            }


        }

    }
    private ChangeDetector _changeDetector;

    public override void Spawned()
    {
        _changeDetector = GetChangeDetector(ChangeDetector.Source.SimulationState);
    }
    public override void Render()
    {
        /*foreach (var change in _changeDetector.DetectChanges(this))
        {
            switch (change)
            {
                case nameof(spawnedProjectile):
                    _material.color = Color.white;
                    break;
            }
        }
        _material.color = Color.Lerp(_material.color, Color.blue, Time.deltaTime);
       */
    }
    private void Update()
    {
       /* if (Object.HasInputAuthority && Input.GetKeyDown(KeyCode.R))
        {
            RPC_SendMessage("Hey Mate!");
        }*/
    }
    private TMP_Text _messages;

    /* [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority, HostMode = RpcHostMode.SourceIsHostPlayer)]
     public void RPC_SendMessage(string message, RpcInfo info = default)
     {
         RPC_RelayMessage(message, info.Source);
     }

     [Rpc(RpcSources.StateAuthority, RpcTargets.All, HostMode = RpcHostMode.SourceIsServer)]
     public void RPC_RelayMessage(string message, PlayerRef messageSource)
     {
         if (_messages == null)
             _messages = FindObjectOfType<TMP_Text>();

         if (messageSource == Runner.LocalPlayer)
         {
             message = $"You said: {message}\n";
         }
         else
         {
             message = $"Some other player said: {message}\n";
         }

         _messages.text += message;

     }*/

    [SerializeField] private float interactRange = 2f; // zwiększ dystans
    [SerializeField] private float interactHeight = 2f;
    [SerializeField] private float interactRadius = 0.5f;
    private IInteractable currentTarget;

    private void TryInteract()
    {
        Vector3 start = transform.position + Vector3.up * interactHeight;

        if (Physics.SphereCast(start, interactRadius, _forward, out RaycastHit hit, interactRange))
        {
            if (hit.collider.TryGetComponent(out IInteractable interactable))
            {
                // Jeśli trzymasz przedmiot i trafiasz w stół
                if (HeldItem != null && interactable is Table table)
                {
                    table.RPC_SetHeldItem(Object, HeldItem);
                    HeldItem = null;
                    return;
                }

                // Jeśli trzymasz przedmiot, a to nie stół
                if (HeldItem != null)
                {
                    Drop();
                    return;
                }

                // Jeśli nie trzymasz przedmiotu, podnieś go
                interactable.Interact(this);

                if (interactable.CanBePickedUp && hit.collider.TryGetComponent(out NetworkObject netObj))
                {
                    Pickup(netObj);
                }
            }
        }
    }



    public void Pickup(NetworkObject item)
    {
        if (HeldItem != null) return; // już trzymasz coś

        HeldItem = item;

        // ustaw obiekt w rękach gracza
        HeldItem.transform.position = holdPoint.position;
        HeldItem.transform.rotation = holdPoint.rotation;
        Collider playerCollider = GetComponent<Collider>();
        Collider itemCollider = HeldItem.GetComponent<Collider>();
        if (playerCollider != null && itemCollider != null)
        {
            Physics.IgnoreCollision(playerCollider, itemCollider, true);
        }
    }

    public void Drop()
    {
        if (HeldItem == null) return;

        // SphereCast przed graczem, żeby sprawdzić stół
        Vector3 start = transform.position + Vector3.up * interactHeight;
        float dropRange = 2f;
        if (Physics.SphereCast(start, interactRadius, _forward, out RaycastHit hit, dropRange))
        {
            if (hit.collider.TryGetComponent(out Table table))
            {
                table.RPC_SetHeldItem(Object, HeldItem);
                HeldItem = null;
                return;
            }
        }

        // Standardowe odłożenie przedmiotu
        Collider playerCollider = GetComponent<Collider>();
        Collider itemCollider = HeldItem.GetComponent<Collider>();
        if (playerCollider != null && itemCollider != null)
            Physics.IgnoreCollision(playerCollider, itemCollider, false);

        HeldItem.transform.position = holdPoint.position + transform.forward * 0.5f;
        HeldItem.transform.rotation = Quaternion.identity;
        HeldItem = null;
    }
    public void ClearHeldItem()
    {
        if (HeldItem == null) return;

        // przywróć kolizję z graczem jeśli wcześniej wyłączona
        Collider playerCollider = GetComponent<Collider>();
        Collider itemCollider = HeldItem.GetComponent<Collider>();
        if (playerCollider != null && itemCollider != null)
            Physics.IgnoreCollision(playerCollider, itemCollider, false);

        HeldItem = null;
    }
    private void OnDrawGizmos()
    {
        Debug.DrawRay(transform.position + Vector3.up * interactHeight, _forward * interactRange, Color.red, 0.1f);
    }
}