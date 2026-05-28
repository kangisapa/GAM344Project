using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// modified tower that has the capacity to handle multiplle firing sprites
/// </summary>
public class DirectionAnimatedTower : Tower
{

    protected override IEnumerator AttackCreepsInRange(List<Creep> targetableCreeps, WaitForSeconds firingDelayWait, WaitForSeconds postFiringDelayWait)
    {
        animationSystem.PlayAnimation(ChooseAnimationIndex(targetableCreeps[0].transform));
        AssignDamage(targetableCreeps);
        yield return firingDelayWait;
        DealDamage(targetableCreeps);
        audioManager.PlaySFX(audioManager.basicAttackSFX);
        yield return postFiringDelayWait;
    }

    private const int upIndex = 1;
    private const int downIndex = 2;
    private const int leftIndex = 3;
    private const int rightIndex = 4;

    private int ChooseAnimationIndex(Transform referenceCreep)
    {
        float angle = CustomMathLibrary.AngleBetweenVector2Positions(transform.position, referenceCreep.position);
        return angle switch
        {
            _ when CustomMathLibrary.AngleInRange(new(315, 45), angle) => rightIndex,
            _ when CustomMathLibrary.AngleInRange(new(45, 135), angle) => upIndex,
            _ when CustomMathLibrary.AngleInRange(new(135, 225), angle) => leftIndex,
            _ => downIndex // technically between 225 and 315. Since its the last one and if none of the others are true, then it has to be this
        };
    }
}
