using System;

public class ActionEvent
{
    public static Action<int> OnIncreaseDamage;
    public static Action<int> OnIncreaseArmor;
    public static Action<Group> OnSelectGroup;
}