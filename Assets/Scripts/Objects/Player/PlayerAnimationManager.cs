using System.Collections;
using UnityEngine;

public class PlayerAnimationManager : MonoBehaviour
{
    public float playerDeathTime;
    private Animator anim;

    private void Awake()
    {
        this.anim = gameObject.GetComponent<Animator>();
        if (this.anim == null) Debug.Log("Cannot find 'Animator' component");
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Player collides with Toimato, Game Over
        if (other.gameObject.CompareTag("Tomato"))
        {
            //Instantiate(playerExplosion, other.transform.position, new Quaternion(0, 0, 0, 0));

            StartCoroutine(AnimationCoroutine("Player_Sleep-Fainting", this.playerDeathTime));
        }
    }

    private IEnumerator AnimationCoroutine(string animationName, float seconds)
    {
        this.anim.Play(animationName);
        yield return new WaitForSeconds(seconds);
        this.anim.Play("Idle");
        yield return null;
    }
}
