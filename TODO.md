## Add documentation for the following:
- IProjOwnedByBoss<T>.
- Easings.
- Utilities? Theres a lot of these, so maybe just descriptions of which each files contain?
- Balancing.
- Cutscenes.
- Metaballs.
- Shaders, including the auto-recompilation feature.
- The hooking features.
- Looped sounds.
- Verlets

## Mark following changes as obsolete next release:
- public static void SetUniversalRumble(float strength, float angularVariance = TwoPi, Vector2? shakeDirection = null)
   -> public static ShakeInfo SetUniversalRumble(float strength, float angularVariance = TwoPi, Vector2? shakeDirection = null, float shakeStrengthDissipationIncrement = 0.2f).