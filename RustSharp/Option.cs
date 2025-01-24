using System;
using UnityEngine;


/// <summary>
/// An Option is a way to wrap your data that helps with null safety by intentionally treating it as data that only optionally exists.
/// <para>Options are centered around your inner data type being in one of two "states", those being Some(<typeparamref name="T"/>) or None.</para>
/// <para>The Option class provides various different utilities focused on safe checks of your inner data that allow you maneuver around nulls easier.</para>
/// </summary>
/// <typeparam name="T"></typeparam>
public abstract class Option<T>
{
    /// <summary>
    /// Initializes or changes the state of an Option to Some(<paramref name="value"/>)
    /// </summary>
    /// <param name="value">The new inner value of the Option</param>
    /// <returns>The newly updated Option</returns>
    public static Option<T> Some(T value) => new SomeOption(value);
    /// <summary>
    /// Initializes or changes the state of an Option to None
    /// </summary>
    /// <returns>The newly updated Option</returns>
    public static Option<T> None() => new NoneOption();

    /// <summary>
    /// True if the optional value exists
    /// </summary>
    public abstract bool IsSome { get; }
    /// <summary>
    /// True if the optional value does not exist
    /// </summary>
    public abstract bool IsNone { get; }
    /// <summary>
    /// Bypasses any checks and returns the inner data regardless of whether it exists or not
    /// <para>Using this often is generally discouraged unless you are 100% sure the Unwrap will not return None</para>
    /// </summary>
    /// <returns>Either the inner data or throws an error</returns>
    public abstract T Unwrap();
    /// <summary>
    /// A safer version of Unwrap that returns the inner data if it exists or uses the provided <paramref name="defaultValue"/> as a substitute.
    /// </summary>
    /// <param name="defaultValue">A value that the Unwrap will default to in the case of calling Unwrap on a None</param>
    /// <returns>Either the inner data or the <paramref name="defaultValue"/> if None</returns>
    public abstract T UnwrapOr(T defaultValue);
    /// <summary>
    /// Performs a <paramref name="mapper"/> action that "maps" the inner data into a new value of <typeparamref name="TResult"/>
    /// </summary>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="mapper"></param>
    /// <returns>An Option with the <typeparamref name="TResult"/> inner data type</returns>
    public abstract Option<TResult> Map<TResult>(Func<T, TResult> mapper);
    /// <summary>
    /// Performs an action on the inner value if it exists otherwise does nothing
    /// </summary>
    /// <param name="action">Action to be performed on the existant value</param>
    /// <returns>Whether the operation happened or not</returns>
    public abstract bool IfSomeDo(Action<T> action);

    /// <summary>
    /// Performs an action if no value exists otherwise does nothing
    /// </summary>
    /// <param name="action">Action to be performed on a non-existant value</param>
    /// <returns>Whether the operation happened or not</returns>
    public abstract bool IfNoneDo(Action action);

    private sealed class SomeOption : Option<T>
    {
        private readonly T _value;

        public SomeOption(T value) => _value = value;

        public override bool IsSome => true;
        public override bool IsNone => false;

        public override T Unwrap() => _value;

        public override T UnwrapOr(T defaultValue) => _value;

        public override Option<TResult> Map<TResult>(Func<T, TResult> mapper) => Option<TResult>.Some(mapper(_value));

        public override bool IfSomeDo(Action<T> action) { action(_value); return true; }

        public override bool IfNoneDo(Action action) => false;
    }

    private sealed class NoneOption : Option<T>
    {
        public override bool IsSome => false;
        public override bool IsNone => true;

        public override T Unwrap() 
        {
            Debug.LogError("Called Unwrap on a None");
            throw new InvalidOperationException("No value present");
        }

        public override T UnwrapOr(T defaultValue) => defaultValue;

        public override Option<TResult> Map<TResult>(Func<T, TResult> mapper) => Option<TResult>.None();

        public override bool IfSomeDo(Action<T> action) => false;

        public override bool IfNoneDo(Action action) { action(); return true; }
    }
}
