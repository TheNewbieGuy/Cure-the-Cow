using UnityEngine;
using UnityEngine.InputSystem;

public class Interaction : MonoBehaviour
{
    [Header("Interaction")]
    public float interactDistance = 3f;
    public LayerMask interactLayer;

    [Header("Dragging")]
    public float dragSmoothness = 15f;
    public float heldDistance = 1f;

    [Header("Placement")]
    public float placementDistance = 2f;
    public float dropHeight = 0.15f;
    public float pickupCooldown = 0.25f;
    
    [Header("Outline")]
    public Color defaultOutlineColor = Color.black;
    public Color hoverOutlineColor = Color.white;

    private PickupObject currentHoveredObject;
    private Camera cam;

    
    private readonly System.Collections.Generic.Dictionary<PickupObject, float> pickupCooldowns
        = new System.Collections.Generic.Dictionary<PickupObject, float>();

    private PickupObject heldObject;

    public PickupObject HeldObject
    {
        get { return heldObject; }
    }

    void Start()
    {
        cam = Camera.main;
    }

    void Update()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            if (heldObject == null)
            {
                TryPickup();
            }
            else if (!TryGiveToCustomer())
            {
                // Only fall through to normal grid placement if we
                // didn't hand this off as a cure attempt.
                TryPlace();
            }
        }

        UpdateDraggedObject();

        UpdateCursor();
        UpdateHoverOutline();
    }
    void UpdateHoverOutline()
    {
        PickupObject closestPickup = null;

        float closestDistance = Mathf.Infinity;

        Vector2 mousePos = Mouse.current.position.ReadValue();

        PickupObject[] pickups = FindObjectsOfType<PickupObject>();

        foreach (PickupObject pickup in pickups)
        {
            // Held object should never highlight
            if (pickup == heldObject)
            {
                SetOutlineColor(
                    pickup,
                    defaultOutlineColor
                );

                continue;
            }

            Vector3 screenPos =
                cam.WorldToScreenPoint(pickup.transform.position);

            if (screenPos.z < 0f)
                continue;

            float dist = Vector2.Distance(
                mousePos,
                new Vector2(screenPos.x, screenPos.y)
            );

            // Must be within interaction range
            float worldDist = Vector3.Distance(
                cam.transform.position,
                pickup.transform.position
            );

            if (worldDist > interactDistance)
                continue;

            // Optional cursor range limit
            if (dist > 100f)
                continue;

            if (dist < closestDistance)
            {
                closestDistance = dist;
                closestPickup = pickup;
            }
        }

        // Reset previous outline
        if (currentHoveredObject != null &&
            currentHoveredObject != closestPickup)
        {
            SetOutlineColor(
                currentHoveredObject,
                defaultOutlineColor
            );
        }

        currentHoveredObject = closestPickup;

        // Highlight closest valid object
        if (currentHoveredObject != null)
        {
            SetOutlineColor(
                currentHoveredObject,
                hoverOutlineColor
            );
        }
    }
    void SetOutlineColor(PickupObject obj, Color color)
    {
        Transform[] children =
            obj.GetComponentsInChildren<Transform>(true);

        foreach (Transform child in children)
        {
            if (child.CompareTag("Outline"))
            {
                Renderer renderer =
                    child.GetComponent<Renderer>();

                if (renderer != null)
                {
                    renderer.material.color = color;
                }
            }
        }
    }

    void TryPickup()
    {
        Ray ray = cam.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (Physics.Raycast(ray, out RaycastHit hit, interactDistance, interactLayer))
        {
            PickupObject pickup = hit.collider.GetComponent<PickupObject>();

            if (pickup != null)
            {
                if (pickup.currentSpot != null)
                {
                    pickup.currentSpot.currentObject = null;
                    pickup.currentSpot = null;
                }

                if (pickupCooldowns.TryGetValue(pickup, out float cooldownEnd))
                {
                    if (Time.time < cooldownEnd)
                        return;
                }

                heldObject = pickup;

                Rigidbody rb = heldObject.GetComponent<Rigidbody>();
                SFXManager.Instance.PlaySFX("Pick Up");

                if (rb != null)
                {
                    rb.isKinematic = true;
                    rb.linearVelocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
                }

                
            }
        }
    }

    /// <summary>
    /// If the player is holding a givable item and clicks on the current
    /// customer, hand it to CustomerManager as a cure attempt instead of
    /// treating it as a normal grid placement. Returns true whenever the
    /// item was handed off (right or wrong cure) or the item isn't givable
    /// at all is NOT included here - see the isGivable check below, which
    /// returns false so non-givable items fall through to TryPlace().
    /// </summary>
    bool TryGiveToCustomer()
    {
        // Items not flagged as givable (tools, decorations, etc.) should
        // never be treated as a cure attempt - let them fall through to
        // normal placement instead.
        if (!heldObject.isGivable)
            return false;

        Ray ray = cam.ScreenPointToRay(Mouse.current.position.ReadValue());

        // Exclude interactLayer so the item currently being held (which is
        // dragged right in front of the camera every frame) doesn't block
        // the ray before it reaches the customer behind it.
        if (!Physics.Raycast(ray, out RaycastHit hit, interactDistance, ~interactLayer))
            return false;

        // CustomerData lives on the customer root - GetComponentInParent
        // covers cases where the collider is on a child (e.g. a hitbox).
        CustomerData customerData =
            hit.collider.GetComponentInParent<CustomerData>();

        if (customerData == null)
            return false;

        if (CustomerManager.Instance == null)
            return false;

        // TryGiveCure returns true for any *accepted* cure attempt - right
        // or wrong - and only false if the attempt wasn't valid at all
        // (e.g. not in Inspection state). Either way, a valid attempt
        // uses up the item.
        bool cureAccepted =
            CustomerManager.Instance.TryGiveCure(heldObject);

        if (cureAccepted)
        {
            Destroy(heldObject.gameObject);
            heldObject = null;
        }

        return true;
    }

    void TryPlace()
    {
        PlacementSpot[] spots = FindObjectsOfType<PlacementSpot>();

        PlacementSpot bestSpot = null;

        float closestScreenDistance = Mathf.Infinity;

        Vector2 mousePos = Mouse.current.position.ReadValue();

        foreach (PlacementSpot spot in spots)
        {
            if (!spot.CanPlace(heldObject))
                continue;

            // Convert world position to screen position
            Vector3 screenPos = cam.WorldToScreenPoint(spot.transform.position);

            // Ignore spots behind camera
            if (screenPos.z < 0f)
                continue;

            float dist = Vector2.Distance(
                mousePos,
                new Vector2(screenPos.x, screenPos.y)
            );

            // Optional max placement range
            float worldDist = Vector3.Distance(
                heldObject.transform.position,
                spot.transform.position
            );

            if (worldDist > placementDistance)
                continue;

            if (dist < closestScreenDistance)
            {
                closestScreenDistance = dist;
                bestSpot = spot;
            }
        }
        if (closestScreenDistance > 300f)
            return;

        if (bestSpot != null)
        {
            Vector3 dropPosition =
                bestSpot.transform.position +
                Vector3.up * dropHeight;

            heldObject.transform.position = dropPosition;
            heldObject.transform.rotation = bestSpot.transform.rotation;

            bestSpot.currentObject = heldObject;
            heldObject.currentSpot = bestSpot;

            Rigidbody rb = heldObject.GetComponent<Rigidbody>();

            if (rb != null)
            {
                rb.isKinematic = false;

                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }

// Start pickup cooldown
            pickupCooldowns[heldObject] = Time.time + pickupCooldown;

            heldObject = null;
        }
    }

    

    void UpdateDraggedObject()
    {
        if (heldObject == null)
            return;

        Vector2 mousePos = Mouse.current.position.ReadValue();

        Vector3 screenPoint = new Vector3(
            mousePos.x,
            mousePos.y,
            heldDistance
        );

        Vector3 worldPoint = cam.ScreenToWorldPoint(screenPoint);

        heldObject.transform.position = Vector3.Lerp(
            heldObject.transform.position,
            worldPoint,
            Time.deltaTime * dragSmoothness
        );
    }

    void UpdateCursor()
    {
        if (heldObject != null)
        {
            CursorManager.Instance.SetCursor("Grab");
            return;
        }

        Ray ray = cam.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (Physics.Raycast(ray, out RaycastHit hit, interactDistance, interactLayer))
        {
            PickupObject pickup = hit.collider.GetComponent<PickupObject>();

            if (pickup != null)
            {
                CursorManager.Instance.SetCursor("Hover");
                return;
            }
        }

        CursorManager.Instance.SetCursor("Default");
    }
}