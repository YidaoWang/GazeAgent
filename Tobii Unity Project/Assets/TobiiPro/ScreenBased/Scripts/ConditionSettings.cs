using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum MediaCondition
{
    A = 0, F = 1, N = 2, Other = 255,
}

public enum CursorCondition
{
    C = 0, N = 1, Other = 255
}

public class ConditionSettings : MonoBehaviour
{
    public delegate void OnConditionChangeEventHandler(MediaCondition media, CursorCondition cursor);
    public event OnConditionChangeEventHandler OnConditionChange;

    public static MediaCondition MediaCondition { get; set; }
    public static CursorCondition CursorCondition { get; set; }

    void Start()
    {
        AOnClick();
    }

    public void AOnClick()
    {
        ChangeCondition(MediaCondition.A, CursorCondition.N);
    }

    public void FOnClick()
    {
        ChangeCondition(MediaCondition.F, CursorCondition.N);
    }

    public void NOnClick()
    {
        ChangeCondition(MediaCondition.N, CursorCondition.N);
    }

    public void ACOnClick()
    {
        ChangeCondition(MediaCondition.A, CursorCondition.C);
    }

    public void FCOnClick()
    {
        ChangeCondition(MediaCondition.F, CursorCondition.C);
    }

    public void COnClick()
    {
        ChangeCondition(MediaCondition.N, CursorCondition.C);
    }

    void ChangeCondition(MediaCondition mediaCondition, CursorCondition cursorCondition)
    {
        MediaCondition = mediaCondition;
        CursorCondition = CursorCondition;
        OnConditionChange?.Invoke(mediaCondition, cursorCondition);
    }
}
