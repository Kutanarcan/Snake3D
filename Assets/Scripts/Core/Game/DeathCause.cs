namespace Game.Core
{
    // What was in the cell the head walked into. Core stops here on purpose: it
    // names the fact and not the reaction, so telling a wall apart from the
    // snake's own body costs the presentation a table row rather than a rule.
    public enum DeathCause
    {
        None = 0,
        Bounds = 1,
        Ground = 2,
        Snake = 3
    }
}
