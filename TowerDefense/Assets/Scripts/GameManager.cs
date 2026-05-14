using UnityEngine;

public class GameManager : MonoBehaviour
{
    #region Singleton Setup
    public static GameManager Instance;

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
        else if(Instance != this)
        {
            Destroy(this);
        }
    }
    #endregion

    #region Panic Button Behaviour
    public PanicButtonBehavior panicButtonType = PanicButtonBehavior.Standard;
    #endregion
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //ensures this sticks between levels
        DontDestroyOnLoad(gameObject);
    }

}
