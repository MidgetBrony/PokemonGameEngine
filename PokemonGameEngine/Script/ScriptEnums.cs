﻿namespace Kermalis.PokemonGameEngine.Scripts
{
    public enum ScriptConditional : byte
    {
        Equal,
        GreaterEqual,
        LessEqual,
        NotEqual,
        Less,
        Greater
    }

    public enum ScriptMovement : byte
    {
        End,
        Face_S,
        Face_N,
        Face_W,
        Face_E,
        Face_SW,
        Face_SE,
        Face_NW,
        Face_NE,
        Walk_S,
        Walk_N,
        Walk_W,
        Walk_E,
        Walk_SW,
        Walk_SE,
        Walk_NW,
        Walk_NE,
        Run_S,
        Run_N,
        Run_W,
        Run_E,
        Run_SW,
        Run_SE,
        Run_NW,
        Run_NE
    }

    public enum ScriptCommand : ushort
    {
        End,
        GoTo,
        Call,
        Return,
        HealParty,
        GivePokemon,
        GivePokemonForm,
        GivePokemonFormItem,
        MoveObj,
        AwaitObjMovement,
        DetachCamera,
        AttachCamera,
        Delay,
        SetFlag,
        ClearFlag,
        Warp,
        Message,
        AwaitMessage,
        LockObj,
        UnlockObj,
        LockAllObjs,
        UnlockAllObjs,
        SetVar,
        AddVar,
        SubVar,
        MulVar,
        DivVar,
        RshftVar,
        LshiftVar,
        AndVar,
        OrVar,
        XorVar,
        RandomizeVar,
        GoToIf,
        GoToIfFlag,
        BufferSpeciesName,
        WildBattle,
        AwaitBattle,
        MessageNoClose,
        SetMessageCanClose,
        UnloadObj,
    }
}
