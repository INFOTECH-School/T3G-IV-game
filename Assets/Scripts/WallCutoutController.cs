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
        
        // Raise the target slightly so we aim for the chest/head, not the feet (optional but helps)
        Vector3 targetPos = playerPos + Vector3.up * 1.0f; 
        
        Vector3 dir = targetPos - transform.position;
        float distance = dir.magnitude;

        // Use SphereCastAll instead of RaycastAll
        // Origin, Radius, Direction, Distance, LayerMask
        RaycastHit[] hits = Physics.SphereCastAll(transform.position, castRadius, dir, distance, wallLayer);


        // STEP 3: Apply holes to current hits
        foreach (RaycastHit hit in hits)
        {
            Renderer rend = hit.collider.GetComponent<Renderer>();
            
            if (rend != null)
            {
                rend.GetPropertyBlock(_propBlock);
                
                // Always send the raw Player Position (feet) to the shader for the center of the hole
                _propBlock.SetVector(PosID, playerPos);
                _propBlock.SetFloat(SizeID, cutoutSize);
                _propBlock.SetFloat(FalloffID, falloffSize);
                
                rend.SetPropertyBlock(_propBlock);

                _previousRenderers.Add(rend);
            }
        }
    }
}