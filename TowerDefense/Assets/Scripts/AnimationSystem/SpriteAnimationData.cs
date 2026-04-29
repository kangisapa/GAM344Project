using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SpriteAnimationData", menuName = "Scriptable Objects/SpriteAnimationData")]
public class SpriteAnimationData : ScriptableObject
{
    [Min(0), Tooltip("This should be set to the default animation that loops, the animatior will return to this animation once any other is finished")]
    public int idleAnimation = 0;

    [Tooltip("Different objects that use this may have different animations based on index," +
    "refer to its documentation to know which animation should be in which index")]
    public List<Animation> animations;
}

[Serializable]
public struct Animation
{
    public List<Sprite> animationSprites;
    public int framesPerSecond;
    public bool looping;
}

