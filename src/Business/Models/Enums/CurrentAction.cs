namespace IsometricGame.Logic.Enums
{
    public enum CurrentAction
    {
        // Actions on click: deselect selected unit, select another unit on cell
        None,
        // Actions on click: move shadow to selected cell
        Move,
        // Actions on click: Specify ability action point
        UseAbility
    }
}