Steps:
1. Move all units, so all further skills will be applied to units on their new positions (e.g. Move, Fly). It is possible that unit do not have move command => those units fill not move on this step.
2. Get other actions outcome:
    1. Find ALL automatic actions (e.g. Regeneration, Auras)
    2. Find 1 action per unit except move (e.g. MeleeAttack, Fireball)
3. Get all effects outcome
4. Execute all outcomes

Ability effects:

- If ability should add an effect for next turns so players will be prepared (e.g. Burn), then ability should only add an effect to the outcome
- If ability adds an effect that should be executed at the same turn (e.g. Haste), then ability should return effect outcome as well
