using System;
using UnityEngine;


public abstract record Result<T, E> {
    public abstract Result<U, E> Map<U>(Func<T, U> func);

    public abstract TResult Match<TResult>(Func<T, TResult> okFunc, Func<E, TResult> errFunc);

    public abstract T Unwrap();
}

public record Ok<T, E>(T value) : Result<T, E> {
    public override Result<U, E> Map<U>(Func<T, U> func) => new Ok<U, E>(func(value));

    public override TResult Match<TResult>(Func<T, TResult> okFunc, Func<E, TResult> errFunc) => okFunc(value);

    public override T Unwrap() => value;
}

public record Err<T, E>(E error) : Result<T, E> {
    public override Result<U, E> Map<U>(Func<T, U> func) => new Err<U, E>(error);

    public override TResult Match<TResult>(Func<T, TResult> okFunc, Func<E, TResult> errFunc) => errFunc(error);

    public override T Unwrap() {
        Debug.LogError(error.ToString());
        return default;
        ;
    }
}

