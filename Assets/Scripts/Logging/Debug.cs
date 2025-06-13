using System;
using System.Diagnostics;
using UnityEngine;
using Object = UnityEngine.Object;

/// <summary>
/// Global type with the same interface as UnityEditor.Debug, necessary for ability
/// to remove logging for release builds
/// </summary>
public static class Debug
{
    public const string ENABLE_CONDITION = "ENABLE_LOGS";
    
    /// <summary>
    ///   <para>In the Build Settings dialog there is a check box called "Development Build".</para>
    /// </summary>
    public static bool isDebugBuild => UnityEngine.Debug.isDebugBuild;
    
    /// <summary>
    ///   <para>Logs a message to the Unity Console.</para>
    /// </summary>
    /// <param name="message">String or object to be converted to string representation for display.</param>
    /// <param name="context">Object to which the message applies.</param>
    [Conditional(ENABLE_CONDITION)]
    public static void Log(object message) =>  UnityEngine.Debug.unityLogger.Log(LogType.Log, message);

    /// <summary>
    ///   <para>Logs a message to the Unity Console.</para>
    /// </summary>
    /// <param name="message">String or object to be converted to string representation for display.</param>
    /// <param name="context">Object to which the message applies.</param>
    [Conditional(ENABLE_CONDITION)]
    public static void Log(object message, Object context) => UnityEngine.Debug.unityLogger.Log(LogType.Log, message, context);

    /// <summary>
    ///   <para>Logs a formatted message to the Unity Console.</para>
    /// </summary>
    /// <param name="format">A composite format string.</param>
    /// <param name="args">Format arguments.</param>
    /// <param name="context">Object to which the message applies.</param>
    /// <param name="logType">Type of message e.g. warn or error etc.</param>
    /// <param name="logOptions">Option flags to treat the log message special.</param>
    [Conditional(ENABLE_CONDITION)]
    public static void LogFormat(string format, params object[] args) => UnityEngine.Debug.unityLogger.LogFormat(LogType.Log, format, args);

    /// <summary>
    ///   <para>Logs a formatted message to the Unity Console.</para>
    /// </summary>
    /// <param name="format">A composite format string.</param>
    /// <param name="args">Format arguments.</param>
    /// <param name="context">Object to which the message applies.</param>
    /// <param name="logType">Type of message e.g. warn or error etc.</param>
    /// <param name="logOptions">Option flags to treat the log message special.</param>
    [Conditional(ENABLE_CONDITION)]
    public static void LogFormat(Object context, string format, params object[] args) => UnityEngine.Debug.unityLogger.LogFormat(LogType.Log, context, format, args);

    /// <summary>
    ///   <para>A variant of Debug.Log that logs an error message to the console.</para>
    /// </summary>
    /// <param name="message">String or object to be converted to string representation for display.</param>
    /// <param name="context">Object to which the message applies.</param>
    public static void LogError(object message) => UnityEngine.Debug.unityLogger.Log(LogType.Error, message);

    /// <summary>
    ///   <para>A variant of Debug.Log that logs an error message to the console.</para>
    /// </summary>
    /// <param name="message">String or object to be converted to string representation for display.</param>
    /// <param name="context">Object to which the message applies.</param>
    public static void LogError(object message, Object context) => UnityEngine.Debug.unityLogger.Log(LogType.Error, message, context);

    /// <summary>
    ///   <para>Logs a formatted error message to the Unity console.</para>
    /// </summary>
    /// <param name="format">A composite format string.</param>
    /// <param name="args">Format arguments.</param>
    /// <param name="context">Object to which the message applies.</param>
    public static void LogErrorFormat(string format, params object[] args) => UnityEngine.Debug.unityLogger.LogFormat(LogType.Error, format, args);

    /// <summary>
    ///   <para>Logs a formatted error message to the Unity console.</para>
    /// </summary>
    /// <param name="format">A composite format string.</param>
    /// <param name="args">Format arguments.</param>
    /// <param name="context">Object to which the message applies.</param>
    public static void LogErrorFormat(Object context, string format, params object[] args) => UnityEngine.Debug.unityLogger.LogFormat(LogType.Error, context, format, args);

/// <summary>
    ///   <para>A variant of Debug.Log that logs an error message to the console.</para>
    /// </summary>
    /// <param name="context">Object to which the message applies.</param>
    /// <param name="exception">Runtime Exception.</param>
    public static void LogException(Exception exception) => UnityEngine.Debug.unityLogger.LogException(exception, (Object) null);

    /// <summary>
    ///   <para>A variant of Debug.Log that logs an error message to the console.</para>
    /// </summary>
    /// <param name="context">Object to which the message applies.</param>
    /// <param name="exception">Runtime Exception.</param>
    public static void LogException(Exception exception, Object context) => UnityEngine.Debug.unityLogger.LogException(exception, context);
    
    /// <summary>
    ///   <para>A variant of Debug.Log that logs a warning message to the console.</para>
    /// </summary>
    /// <param name="message">String or object to be converted to string representation for display.</param>
    /// <param name="context">Object to which the message applies.</param>
    [Conditional(ENABLE_CONDITION)]
    public static void LogWarning(object message) => UnityEngine.Debug.unityLogger.Log(LogType.Warning, message);

    /// <summary>
    ///   <para>A variant of Debug.Log that logs a warning message to the console.</para>
    /// </summary>
    /// <param name="message">String or object to be converted to string representation for display.</param>
    /// <param name="context">Object to which the message applies.</param>
    [Conditional(ENABLE_CONDITION)]
    public static void LogWarning(object message, Object context) => UnityEngine.Debug.unityLogger.Log(LogType.Warning, message, context);

    /// <summary>
    ///   <para>Logs a formatted warning message to the Unity Console.</para>
    /// </summary>
    /// <param name="format">A composite format string.</param>
    /// <param name="args">Format arguments.</param>
    /// <param name="context">Object to which the message applies.</param>
    [Conditional(ENABLE_CONDITION)]
    public static void LogWarningFormat(string format, params object[] args) => UnityEngine.Debug.unityLogger.LogFormat(LogType.Warning, format, args);

    /// <summary>
    ///   <para>Logs a formatted warning message to the Unity Console.</para>
    /// </summary>
    /// <param name="format">A composite format string.</param>
    /// <param name="args">Format arguments.</param>
    /// <param name="context">Object to which the message applies.</param>
    [Conditional(ENABLE_CONDITION)]
    public static void LogWarningFormat(Object context, string format, params object[] args) => UnityEngine.Debug.unityLogger.LogFormat(LogType.Warning, context, format, args);
    
    /// <summary>
    ///   <para>Assert a condition and logs an error message to the Unity console on failure.</para>
    /// </summary>
    /// <param name="condition">Condition you expect to be true.</param>
    /// <param name="context">Object to which the message applies.</param>
    /// <param name="message">String or object to be converted to string representation for display.</param>
    [Conditional("UNITY_ASSERTIONS")]
    public static void Assert(bool condition)
    {
      if (condition)
        return;
      UnityEngine.Debug.unityLogger.Log(LogType.Assert, (object) "Assertion failed");
    }

    /// <summary>
    ///   <para>Assert a condition and logs an error message to the Unity console on failure.</para>
    /// </summary>
    /// <param name="condition">Condition you expect to be true.</param>
    /// <param name="context">Object to which the message applies.</param>
    /// <param name="message">String or object to be converted to string representation for display.</param>
    [Conditional("UNITY_ASSERTIONS")]
    public static void Assert(bool condition, Object context)
    {
      if (condition)
        return;
      UnityEngine.Debug.unityLogger.Log(LogType.Assert, (object) "Assertion failed", context);
    }

    /// <summary>
    ///   <para>Assert a condition and logs an error message to the Unity console on failure.</para>
    /// </summary>
    /// <param name="condition">Condition you expect to be true.</param>
    /// <param name="context">Object to which the message applies.</param>
    /// <param name="message">String or object to be converted to string representation for display.</param>
    [Conditional("UNITY_ASSERTIONS")]
    public static void Assert(bool condition, object message)
    {
      if (condition)
        return;
      UnityEngine.Debug.unityLogger.Log(LogType.Assert, message);
    }

    [Conditional("UNITY_ASSERTIONS")]
    public static void Assert(bool condition, string message)
    {
      if (condition)
        return;
      UnityEngine.Debug.unityLogger.Log(LogType.Assert, (object) message);
    }

    /// <summary>
    ///   <para>Assert a condition and logs an error message to the Unity console on failure.</para>
    /// </summary>
    /// <param name="condition">Condition you expect to be true.</param>
    /// <param name="context">Object to which the message applies.</param>
    /// <param name="message">String or object to be converted to string representation for display.</param>
    [Conditional("UNITY_ASSERTIONS")]
    public static void Assert(bool condition, object message, Object context)
    {
      if (condition)
        return;
      UnityEngine.Debug.unityLogger.Log(LogType.Assert, message, context);
    }

    [Conditional("UNITY_ASSERTIONS")]
    public static void Assert(bool condition, string message, Object context)
    {
      if (condition)
        return;
      UnityEngine.Debug.unityLogger.Log(LogType.Assert, (object) message, context);
    }

    /// <summary>
    ///   <para>Assert a condition and logs a formatted error message to the Unity console on failure.</para>
    /// </summary>
    /// <param name="condition">Condition you expect to be true.</param>
    /// <param name="format">A composite format string.</param>
    /// <param name="args">Format arguments.</param>
    /// <param name="context">Object to which the message applies.</param>
    [Conditional("UNITY_ASSERTIONS")]
    public static void AssertFormat(bool condition, string format, params object[] args)
    {
      if (condition)
        return;
      UnityEngine.Debug.unityLogger.LogFormat(LogType.Assert, format, args);
    }

    /// <summary>
    ///   <para>Assert a condition and logs a formatted error message to the Unity console on failure.</para>
    /// </summary>
    /// <param name="condition">Condition you expect to be true.</param>
    /// <param name="format">A composite format string.</param>
    /// <param name="args">Format arguments.</param>
    /// <param name="context">Object to which the message applies.</param>
    [Conditional("UNITY_ASSERTIONS")]
    public static void AssertFormat(
      bool condition,
      Object context,
      string format,
      params object[] args)
    {
      if (condition)
        return;
      UnityEngine.Debug.unityLogger.LogFormat(LogType.Assert, context, format, args);
    }

    /// <summary>
    ///   <para>A variant of Debug.Log that logs an assertion message to the console.</para>
    /// </summary>
    /// <param name="message">String or object to be converted to string representation for display.</param>
    /// <param name="context">Object to which the message applies.</param>
    [Conditional("UNITY_ASSERTIONS")]
    public static void LogAssertion(object message) => UnityEngine.Debug.unityLogger.Log(LogType.Assert, message);

    /// <summary>
    ///   <para>A variant of Debug.Log that logs an assertion message to the console.</para>
    /// </summary>
    /// <param name="message">String or object to be converted to string representation for display.</param>
    /// <param name="context">Object to which the message applies.</param>
    [Conditional("UNITY_ASSERTIONS")]
    public static void LogAssertion(object message, Object context) => UnityEngine.Debug.unityLogger.Log(LogType.Assert, message, context);

    /// <summary>
    ///   <para>Logs a formatted assertion message to the Unity console.</para>
    /// </summary>
    /// <param name="format">A composite format string.</param>
    /// <param name="args">Format arguments.</param>
    /// <param name="context">Object to which the message applies.</param>
    [Conditional("UNITY_ASSERTIONS")]
    public static void LogAssertionFormat(string format, params object[] args) => UnityEngine.Debug.unityLogger.LogFormat(LogType.Assert, format, args);

    /// <summary>
    ///   <para>Logs a formatted assertion message to the Unity console.</para>
    /// </summary>
    /// <param name="format">A composite format string.</param>
    /// <param name="args">Format arguments.</param>
    /// <param name="context">Object to which the message applies.</param>
    [Conditional("UNITY_ASSERTIONS")]
    public static void LogAssertionFormat(Object context, string format, params object[] args) => UnityEngine.Debug.unityLogger.LogFormat(LogType.Assert, context, format, args);
}
