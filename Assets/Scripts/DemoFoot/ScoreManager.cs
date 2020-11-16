using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{

    public int goalRedScore;
    public int goalYellowScore;

    public void OnEnterColliderRed()
    {
        goalYellowScore++;
    }

    public void OnEnterColliderYellow()
    {
        goalRedScore++;
    }


    // Start is called before the first frame update
    void Start()
    {
        goalRedScore = 0;
        goalYellowScore = 0;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
