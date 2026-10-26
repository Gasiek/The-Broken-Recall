using System.Collections;
using UnityEngine;

public class ActionRoutineRunner : MonoBehaviour
{
    public Coroutine Run(IEnumerator routine)
    {
        return StartCoroutine(routine);
    }

    public void StopAllActionRoutines()
    {
        StopAllCoroutines();
    }
}
