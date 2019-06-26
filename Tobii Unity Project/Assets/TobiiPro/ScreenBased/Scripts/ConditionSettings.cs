using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum MediaCondition 
{
    A, F, N
}

public enum CursorCondition
{
    C, N
}

public class ConditionSettings : MonoBehaviour
{ 
    public delegate void OnConditionChangeEventHandler(MediaCondition media, CursorCondition cursor);
    public event OnConditionChangeEventHandler OnConditionChange;

    void Start()
    {
        AOnClick();
    }

    public void AOnClick()
    {
        OnConditionChange?.Invoke(MediaCondition.A, CursorCondition.N);
    }

    public void FOnClick()
    {
        OnConditionChange?.Invoke(MediaCondition.F, CursorCondition.N);
    }

    public void NOnClick()
    {
        OnConditionChange?.Invoke(MediaCondition.N, CursorCondition.N);
    }

    public void ACOnClick()
    {
        OnConditionChange?.Invoke(MediaCondition.A, CursorCondition.C);
    }

    public void FCOnClick()
    {
        OnConditionChange?.Invoke(MediaCondition.F, CursorCondition.C);
    }

    public void COnClick()
    {
        OnConditionChange?.Invoke(MediaCondition.N, CursorCondition.C);
    }
}
