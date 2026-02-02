using System.Collections.Generic;
using UnityEngine;

public class WallCutoutController : MonoBehaviour
{
    [Header("Settings")]
    public LayerMask wallLayer;
    public float cutoutSize = 1.5f;
    public float falloffSize = 0.5f;
    
    [Header("Detection Precision")]
    [Tooltip("How thick the detection beam is. Higher = holes stay open longer at edges.")]
    public float castRadius = 0.05f; // Radius of the "Thick Beam"
    
    [Tooltip("How far behind the camera to start the cast. Helps detect walls the camera is already inside.")]
    public float castBackwardsDistance = 0.5f; 

    private Camera _camera;
    
    // List to track walls from the previous frame
    private List<Renderer> _previousRenderers = new List<Renderer>();
    private MaterialPropertyBlock _propBlock;

    private static readonly int PosID = Shader.PropertyToID("_CutoutPosition");
    private static readonly int SizeID = Shader.PropertyToID("_CutoutSize");
    private static readonly int FalloffID = Shader.PropertyToID("_FalloffSize");

    void Awake()
    {
        _camera = GetComponent<Camera>();
        _propBlock = new MaterialPropertyBlock();
    }

    void Update()
    {
        // STEP 1: RESET renderers from the last frame
        foreach (Renderer rend in _previousRenderers)
        {
            if (rend == null) continue;

            rend.GetPropertyBlock(_propBlock);
            _propBlock.SetFloat(SizeID, 0f); // Close the hole
            rend.SetPropertyBlock(_propBlock);
        }
        
        _previousRenderers.Clear();

        
        // STEP 2: Calculate "Thick Beam" (SphereCast)
        Vector3 playerPos = GameManager.Instance.Player.transform.position;
        Vector3 targetPos = playerPos + Vector3.up * 1.0f; // Aim for chest/head
        
        Vector3 cameraPos = transform.position;
        Vector3 dirVector = targetPos - cameraPos;
        float distanceToPlayer = dirVector.magnitude;
        Vector3 direction = dirVector.normalized;

        // --- THE FIX ---
        // Move the start point backwards. If we start INSIDE a wall, Physics.SphereCast ignores it.
        // By backing up, we ensure we start outside the wall collider.
        Vector3 startPoint = cameraPos - (direction * castBackwardsDistance);
        
        // We must increase the total distance of the cast to account for the backup
        float totalCastDistance = distanceToPlayer + castBackwardsDistance;

        // Use SphereCastAll with the new StartPoint and TotalDistance
        RaycastHit[] hits = Physics.SphereCastAll(startPoint, castRadius, direction, totalCastDistance, wallLayer);


        // STEP 3: Apply holes to current hits
        foreach (RaycastHit hit in hits)
        {
            Renderer rend = hit.collider.GetComponent<Renderer>();
            
            if (rend != null)
            {
                rend.GetPropertyBlock(_propBlock);
                
                // Send Player Position (feet) for the hole center
                _propBlock.SetVector(PosID, playerPos);
                _propBlock.SetFloat(SizeID, cutoutSize);
                _propBlock.SetFloat(FalloffID, falloffSize);
                
                rend.SetPropertyBlock(_propBlock);

                _previousRenderers.Add(rend);
            }
        }
    }
}