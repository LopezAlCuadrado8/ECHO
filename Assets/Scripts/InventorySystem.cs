using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InventorySystem : MonoBehaviour
{
    [Serializable]
    public class Slot
    {
        public ThrowableData data;
        public int count = 5;
        public int maxCount = 10;
    }

    [Header("Slots (Small, Medium, Large)")]
    [SerializeField] private Slot[] slots = new Slot[3];
    [SerializeField] private int activeSlot = 0;

    [Header("Lanzamiento")]
    [SerializeField] private GameObject throwablePrefab;
    [SerializeField] private Transform throwOrigin;   // desde dónde sale el objeto
    [SerializeField] private float baseThrowForce = 12f;
    [SerializeField] private float upwardAngle = 15f; // grados hacia arriba

    [Header("Refs")]
    [SerializeField] private PlayerController playerController;

    public event Action<int> OnSlotChanged;
    public event Action<int, int> OnSlotCountChanged;   // slot, count

    public ThrowableData ActiveData => slots[activeSlot].data;
    public int ActiveCount => slots[activeSlot].count;
    public int ActiveSlotIndex => activeSlot;

    // Input
    private PlayerInputActions input;

    private void Awake()
    {
        input = new PlayerInputActions();

        input.Player.Throw.performed += _ => Throw();
        input.Player.CycleItem.performed += _ => CycleSlot();
    }

    private void OnEnable() => input.Player.Enable();
    private void OnDisable() => input.Player.Disable();

    private void Start()
    {
        OnSlotChanged?.Invoke(activeSlot);
        OnSlotCountChanged?.Invoke(activeSlot, slots[activeSlot].count);
    }

    private void CycleSlot()
    {
        activeSlot = (activeSlot + 1) % slots.Length;
        OnSlotChanged?.Invoke(activeSlot);
    }

    public void Throw()
    {
        var slot = slots[activeSlot];
        if (slot.count <= 0 || slot.data == null) return;

        // ¿Hay objeto en inventario?
        slot.count--;
        OnSlotCountChanged?.Invoke(activeSlot, slot.count);

        // Dirección del lanzamiento: hacia donde mira el player + ángulo arriba
        float facing = playerController != null && playerController.FacingRight ? 1f : -1f;
        Vector2 dir = Quaternion.Euler(0, 0, facing > 0 ? upwardAngle : 180f - upwardAngle) * Vector2.right;

        float force = baseThrowForce * slot.data.throwForceMultiplier;
        Vector2 velocity = dir * force;

        // Instanciar
        Vector3 spawnPos = throwOrigin != null ? throwOrigin.position : transform.position;
        var go = Instantiate(throwablePrefab, spawnPos, Quaternion.identity);
        var throwable = go.GetComponent<Throwable>();
        throwable.Launch(slot.data, velocity);
    }

    public void AddItem(ThrowableData data, int amount)
    {
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i].data == data)
            {
                slots[i].count = Mathf.Min(slots[i].count + amount, slots[i].maxCount);
                OnSlotCountChanged?.Invoke(i, slots[i].count);
                return;
            }
        }
    }
}