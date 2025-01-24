using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;
using System.Linq;
using System.Linq.Expressions;
using System.Collections.Concurrent;

/// <summary>
/// Extension for the Delegate class that helps cut down on the cost of repeated calls to DynamicInvoke
/// <para>Keeps track of lambda's that have previously been constructed for a particular delegate and saves them for use later should they be invoked again</para>
/// <para>This only saves on performance in places where you would you would be continuously using DynamicInvoke to invoke particular methods</para>
/// </summary>
public static class DynamicInvoker
{
    private static readonly ConcurrentDictionary<Delegate, Func<object[], object>> _cache = new ConcurrentDictionary<Delegate, Func<object[], object>>();

    /// <summary>
    /// Invokes the stored lambda for a <paramref name="del"/> if it has already been constructed, otherwise constructs, invokes, and caches it for later.
    /// </summary>
    /// <param name="del">The delegate attempting to be invoked.</param>
    /// <param name="args">Arguments to invoke the delegate with.</param>
    /// <returns>The constructed or retrieved lambda</returns>
    public static object InvokeOrDynamic(this Delegate del, object[] args)
    {
        var invoker = _cache.GetOrAdd(del, CreateInvoker);
        return invoker(args);
    }

    private static Func<object[], object> CreateInvoker(Delegate del)
    {
        var argsParameter = Expression.Parameter(typeof(object[]), "args");

        var parameters = del.Method.GetParameters()
            .Select((p, i) => Expression.Convert(
                Expression.ArrayIndex(argsParameter, Expression.Constant(i)),
                p.ParameterType
            )).ToArray();

        var instance = del.Target == null ? null : Expression.Constant(del.Target);
        var call = Expression.Call(instance, del.Method, parameters);

        if (del.Method.ReturnType == typeof(void))
        {
            var lambda = Expression.Lambda<Action<object[]>>(
                Expression.Block(call, Expression.Constant(null, typeof(object))),
                argsParameter
            );

            Action<object[]> action = lambda.Compile();
            return args =>
            {
                action(args);
                return null;
            };
        }
        else
        {
            var lambda = Expression.Lambda<Func<object[], object>>(
                Expression.Convert(call, typeof(object)),
                argsParameter
            );

            return lambda.Compile();
        }
    }
}

public class ChannelManager : MonoBehaviour
{
    /// <summary>
    /// The Channel Hub asset itself enforces it existance so we can always trust it exists.
    /// </summary>
    private static ChannelHub channelHub = ChannelHub.Instance;
    private static bool preRuntimeListenersRegistered;

    public static void RegisterListener<T>(Action<T> receiver, MonoBehaviour ownerObject, string listen_on)
    {
        Register(receiver.Method, ownerObject, listen_on);
    }

    public static bool ReceiverIsValid(Delegate receiver, object[] messages)
    {
        var rcvParams = receiver.Method.GetParameters();

        if (rcvParams.Length != messages.Length)
        {
            return false;
        }

        for (int i = 0; i < rcvParams.Length; i++)
        {
            if (rcvParams[i].ParameterType != messages[i].GetType())
            {
                return false;
            }
        }

        return true;
    }

    public static void Send(string channel, params object[] messages)
    {
        if (!preRuntimeListenersRegistered) RegisterPreRuntimeListeners();

        var receivers = channelHub.channels.Find(_channel => _channel.name == channel).receivers;
        foreach (var receiver in receivers)
        {
            if (ReceiverIsValid(receiver, messages))
            {
                receiver.InvokeOrDynamic(messages);
            }
        }
    }

    private static void RegisterPreRuntimeListeners()
    {
        var monoBehaviours = FindObjectsOfType<MonoBehaviour>();

        foreach (var monoBehavior in monoBehaviours)
        {
            Register(monoBehavior);
        }

        preRuntimeListenersRegistered = true;
    }

    private static void Register(MonoBehaviour monoBehaviour)
    {
        var receivingMethods = monoBehaviour.GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic)
            .Where(m => m.GetCustomAttributes(typeof(ChannelReceiver), false).Length > 0);
        foreach (var method in receivingMethods)
        {
            var attribute = (ChannelReceiver)method.GetCustomAttributes(typeof(ChannelReceiver), false).First();
            
            var channelName = attribute.boundChannelName;

            var types = Array.ConvertAll(method.GetParameters(), p => p.ParameterType);
            var delegateType = Expression.GetDelegateType(types.Concat(new[] { method.ReturnType }).ToArray());

            var callback = Delegate.CreateDelegate(delegateType, monoBehaviour, method);

            channelHub.channels.Find(channel => channel.name == channelName).receivers.Add(callback);
        }
    }

    private static void Register(MethodInfo method, MonoBehaviour monoBehaviour, string channelName)
    {
        var type = method.GetParameters()[0].ParameterType;
        var callback = Delegate.CreateDelegate(typeof(Action<>).MakeGenericType(type), monoBehaviour, method);

        channelHub.channels.Find(channel => channel.name == channelName).receivers.Add(callback);
    }
}
