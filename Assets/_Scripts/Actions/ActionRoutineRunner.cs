using System.Collections;
using UnityEngine;

public class ActionRoutineRunner : MonoBehaviour
{
    public Coroutine Run(IEnumerator routine)
    {
        return StartCoroutine(routine);
    }

    public void Stop(Coroutine routine)
    {
        if (routine == null)
            return;

        StopCoroutine(routine);
    }

    public void StopAllActionRoutines()
    {
        StopAllCoroutines();
    }
}
