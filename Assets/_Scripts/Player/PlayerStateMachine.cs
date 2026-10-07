using UnityEngine;
public class PlayerStateMachine : MonoBehaviour
{
    public PlayerState CurrentState { get; private set; } = PlayerState.Normal;

    public bool CanMove => CurrentState != PlayerState.Dashing && CurrentState != PlayerState.Dead;

    public bool CanAct => CurrentState == PlayerState.Normal;

    public bool CanDash => CurrentState == PlayerState.Normal;

    public void StartActing()
    {
        if (!CanAct)
        {
            return;
        }

        CurrentState = PlayerState.Acting;
    }

    public void FinishActing()
    {
        if (CurrentState != PlayerState.Acting)
        {
            return;
        }

        CurrentState = PlayerState.Normal;
    }

    public void StartDashing()
    {
        if (!CanDash)
        {
            return;
        }

        CurrentState = PlayerState.Dashing;
    }

    public void FinishDashing()
    {
        if (CurrentState != PlayerState.Dashing)
        {
            return;
        }

        CurrentState = PlayerState.Normal;
    }

    public void Die()
    {
        CurrentState = PlayerState.Dead;
    }
}
