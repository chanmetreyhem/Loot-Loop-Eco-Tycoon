using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;
[RequireComponent(typeof(LineRenderer))]    
public class LoopDetector : MonoBehaviour
{
    private LineRenderer lineRenderer;
    private List<Vector2> points = new List<Vector2>();
    private bool isDrawing = false;

    [Header("Settings")]
    public float minPointDistance = 0.2f;
    public float closeLoopDistance = 0.5f;

    [Header("Puzzle Detection")]
    public LayerMask blockLayer;

    private GridManager gridManager;
    private ScoreSystem scoreSystem; // Reference to track player currency profiles
    void Start()
    {
        gridManager = FindFirstObjectByType<GridManager>();
        scoreSystem = FindFirstObjectByType<ScoreSystem>();

        // Dynamically instantiate clean visual trails for player inputs
        lineRenderer = gameObject.GetComponent<LineRenderer>();
        lineRenderer.startWidth = 0.1f;
        lineRenderer.endWidth = 0.1f;
        lineRenderer.positionCount = 0;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = Color.cyan;
        lineRenderer.endColor = Color.cyan;
    }

    void Update()
    {
        HandleInput();
    }

    void HandleInput()
    {
        var pointer = Pointer.current;
        if (pointer == null) return;

        // 1. Initial Touch / Press down
        if (pointer.press.wasPressedThisFrame)
        {
            isDrawing = true;
            points.Clear();
            lineRenderer.positionCount = 0;
            AddPoint(GetPointerWorldPosition(pointer));
        }

        // 2. Dragging across screen
        if (isDrawing && pointer.press.isPressed)
        {
            Vector2 currentPos = GetPointerWorldPosition(pointer);
            if (points.Count > 0 && Vector2.Distance(currentPos, points[points.Count - 1]) > minPointDistance)
            {
                AddPoint(currentPos);
                CheckForLoop();
            }
        }

        // 3. User lifts touch finger up
        if (pointer.press.wasReleasedThisFrame)
        {
            isDrawing = false;
            lineRenderer.positionCount = 0;
            points.Clear();
        }
    }

    Vector2 GetPointerWorldPosition(Pointer pointer)
    {
        Vector2 screenPos = pointer.position.ReadValue();
        // Uses Camera near clip plane to dynamically handle different screen aspect ratios without translation errors
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, Camera.main.nearClipPlane));
        return new Vector2(worldPos.x, worldPos.y);
    }

    void AddPoint(Vector2 point)
    {
        points.Add(point);
        lineRenderer.positionCount = points.Count;
        // Sets position slightly forward on the Z axis (-1f) to ensure the drawn trail is always rendered on top of blocks
        lineRenderer.SetPosition(points.Count - 1, new Vector3(point.x, point.y, -1f));
    }

    // Evaluates mathematical intersection points to detect a completed loop boundary
    void CheckForLoop()
    {
        if (points.Count < 5) return;

        Vector2 firstPoint = points[0];
        Vector2 lastPoint = points[points.Count - 1];

        if (Vector2.Distance(lastPoint, firstPoint) < closeLoopDistance)
        {
            isDrawing = false;
            OnLoopCompleted();
        }
    }

    void OnLoopCompleted()
    {
        Debug.Log("Loop detected via New Input System! Checking for captured grid blocks...");

        Vector2 center = Vector2.zero;
        foreach (Vector2 p in points) center += p;
        center /= points.Count;

        float maxDistance = 0f;
        foreach (Vector2 p in points)
        {
            float dist = Vector2.Distance(center, p);
            if (dist > maxDistance) maxDistance = dist;
        }

        // Broad-phase physics scan using overlapping geometries
        Collider2D[] caughtBlocks = Physics2D.OverlapCircleAll(center, maxDistance, blockLayer);
        List<GameObject> blocksToRemove = new List<GameObject>();
        string targetedTag = "";
        bool isValidLoop = true;

        foreach (Collider2D col in caughtBlocks)
        {
            // Raycasting filter to confirm item is physically locked inside the polygon outline
            if (IsPointInPolygon(col.transform.position, points))
            {
                if (targetedTag == "") targetedTag = col.gameObject.tag;
                // Invalid loop condition: Player mixed multiple resource types inside a single circle
                else if (col.gameObject.tag != targetedTag)
                {
                    isValidLoop = false;
                    break;
                }
                blocksToRemove.Add(col.gameObject);
            }
        }

        // Validation Rule: Must encompass 3 or more matching elements
        if (isValidLoop && blocksToRemove.Count >= 3)
        {
            Debug.Log("Valid Match! Exploded " + blocksToRemove.Count + " blocks of type: " + targetedTag);

            // Allocate score profile modifications 
            if (scoreSystem != null)
            {
                scoreSystem.AddLootAndCoins(targetedTag, blocksToRemove.Count);
            }

            foreach (GameObject block in blocksToRemove)
            {
                if (gridManager != null) gridManager.RemoveBlockFromGrid(block);
                Destroy(block);
            }

            if (gridManager != null) gridManager.ClearAndRefill();
        }
        else
        {
            Debug.Log("Invalid Loop! Ensure you circled at least 3 matching items without mixing types.");
        }

        lineRenderer.positionCount = 0;
        points.Clear();
    }

    // Cross-product polygon checking logic to distinguish bounds mapping
    bool IsPointInPolygon(Vector2 p, List<Vector2> poly)
    {
        bool inside = false;
        for (int i = 0, j = poly.Count - 1; i < poly.Count; j = i++)
        {
            if (((poly[i].y > p.y) != (poly[j].y > p.y)) &&
                (p.x < (poly[j].x - poly[i].x) * (p.y - poly[i].y) / (poly[j].y - poly[i].y) + poly[i].x))
            {
                inside = !inside;
            }
        }
        return inside;
    }
}
