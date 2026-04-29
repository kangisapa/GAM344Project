using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteAnimationSystem : MonoBehaviour
{
    private List<Animation> assetAnimations;

    private SpriteRenderer spriteRenderer;
    private int activeAnimationIndex;
    private int defaultIndex;


    private Coroutine currentAnimationCoroutine;

    public void InitializeAnimationSystem(SpriteAnimationData animationImports, SpriteRenderer renderer)
    {
        spriteRenderer = renderer;
        defaultIndex = animationImports.idleAnimation;
        assetAnimations = new List<Animation>();
        foreach (Animation anim in animationImports.animations)
        {
            assetAnimations.Add(anim);
        }
        PlayAnimation(0);
    }


    public void PlayAnimation(int index)
    {
        if(index > assetAnimations.Count - 1)
        {
            Debug.LogWarning($"{gameObject.name} tried to play an animation that wasn't available of index {index}", gameObject);
            return;
        }

        if(currentAnimationCoroutine != null)
        {
            StopCoroutine(currentAnimationCoroutine);
        }

        if (assetAnimations[index].looping)
        {
            currentAnimationCoroutine = StartCoroutine(AnimationLoop(index));
        }
        else
        {
            currentAnimationCoroutine = StartCoroutine(AnimationOneShot(index));
        }

    }

    private IEnumerator AnimationLoop(int index)
    {
        Animation toPlay = assetAnimations[index];

        WaitForSeconds waitTime = new WaitForSeconds(1f / toPlay.framesPerSecond);

        int frame = 0;
        while(true)
        {
            spriteRenderer.sprite = toPlay.animationSprites[frame];
            frame++;
            if(frame > toPlay.animationSprites.Count - 1)
            {
                frame = 0;
            }
            yield return waitTime;
        }
    }

    private IEnumerator AnimationOneShot(int index)
    {
        Animation toPlay = assetAnimations[index];

        WaitForSeconds waitTime = new WaitForSeconds(1f / toPlay.framesPerSecond);

        for (int i = 0; i < toPlay.animationSprites.Count; i++)
        {
            spriteRenderer.sprite = toPlay.animationSprites[i];
            yield return waitTime;
        }
        PlayAnimation(defaultIndex);
    }
}

