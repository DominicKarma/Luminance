# State Machines
Luminance provides a framework for the implementation and handling of behavior states via a concept known as a pushdown automata, which can conceptually be thought of like a memory stack that holds local data about a given state.

With this system, it is possible to easily seamlessly interrupt one state, transition to another, and then returning to the original state with the local data being undisturbed by simply pushing a new state on top of the stack.
An example of this could be a teleport animation. Instead of integrating the teleport into some attack state directly, you can simply transition to the teleport state, making sure that the ``KeepOldStateInStack`` parameter in the ``RegisterTransition`` method is true, and
wait for the teleport state to conclude. When it does, it will be popped from the stack and the executing state will be whatever attack was happening before, with the original data intact.

---

All states are represented via a generic ``IState``, with the generic type typically referring to some enum that holds the collection of possible states. The implementation of ``IState`` is responsible for holding data local to each state, such as
timers. For convenience, Luminance provides a simplified ``EntityAIState`` with a simpler integer timer.

---

State transitions are handled via the stack. Whenever the current state completes, it is removed from the top of the stack, and the state below it is run. This is handled primarily via the ``RegisterTransition`` method, like so:
```cs
StateMachine.RegisterTransition(OriginalState, NewState, KeepOldStateInStack, () =>
{
    return StateTransitionCondition();
});
```

---

In cases where transitions must be able to take over another transition for *any* original state, such as with a boss that's waiting for its current attack to finish before entering a second phase state, you would instead want to use the following method:

```cs
StateMachine.AddTransitionStateHijack(originalState =>
{
    if (HijackCondition)
        return NewState;

    return originalState;
}, originalState =>
{
    ThingsToDoAfterTheTransition();
});
```

> [!Note]
> Transition methods should be called only once on the object that holds the state machine, similar to loading methods in other contexts.

---

Lastly, it is possible to modify the ``StateStack`` itself of the state machine if necessary, though this is generally not recommended.
