using UnityEngine;

public class PlayerAnimationEvents : MonoBehaviour
{
    public ParticleSystem footstepParticles;
    public ParticleSystem landParticles;
    public ParticleSystem jumpParticles;
    public TrailRenderer handTrail;
    
    void Start()
    {
        // Make sure it's off by default
        if (handTrail != null) handTrail.emitting = false;
    }
    
    public void StartSwingTrail()
    {
        if (handTrail != null)
        {
            handTrail.Clear();
            handTrail.emitting = true;
            handTrail.AddPosition(handTrail.transform.position);
        }
    }
    
    public void EndSwingTrail()
    {
        if (handTrail != null)
        {
            handTrail.emitting = false;
        }
    }
    
    public void PlayFootstep()
    {
        if (footstepParticles)
        {
            footstepParticles.Play();
        }
    }

    public void PlayLanding()
    {
        if (landParticles)
        {
            landParticles.Play();
        }
    }

    public void PlayJump()
    {
        if (jumpParticles)
        {
            jumpParticles.Play();
        }
    }
}
