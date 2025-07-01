namespace TranSPEi_Cifrado.Domain.Interfaces.External;

/// <summary>
/// This is the interface to use in the TDPLus applications
/// </summary>
public interface ILoggerService
{
    /// <summary>
    /// Set the trace id for the logs messages
    /// </summary>
    /// <param name="traceid">an unique string</param>
    void SetTraceId(string? traceid);

    /// <summary>
    /// Set the trace id user manager for the logs messages
    /// </summary>
    /// <param name="userManagerId">The user manager ID</param>
    void SetUserManagerId(string? userManagerId);

    #region trace
    // Trace: Most verbose level. Used for development and seldom enabled in production.
    /// <summary>
    /// Write a TRACE register with a simple message
    /// </summary>
    /// <param name="message">The message</param>
    void Trace(string message);

    /// <summary>
    /// Write a TRACE register with message and exception
    /// </summary>
    /// <param name="message">The message</param>
    /// <param name="ex">The exception</param>
    void Trace(string message, Exception ex);

    /// <summary>
    /// Write a TRACE register with a message and detail
    /// </summary>
    /// <param name="message">The message</param>
    /// <param name="detail">An object to detail the message</param>
    void Trace(string message, object detail);
    #endregion

    #region debug
    // Debug: Debugging the application behavior from internal events of interest.
    /// <summary>
    /// Write a DEBUG register with a simple message
    /// </summary>
    /// <param name="message">The message</param>
    void Debug(string message);

    /// <summary>
    /// Write a DEBUG register with message and exception
    /// </summary>
    /// <param name="message">The message</param>
    /// <param name="ex">The exception</param>
    void Debug(string message, Exception ex);

    /// <summary>
    /// Write a DEBUG register with a message and detail
    /// </summary>
    /// <param name="message">The message</param>
    /// <param name="detail">An object to detail the message</param>
    void Debug(string message, object detail);
    #endregion

    #region info
    // Info: Information that highlights progress or application lifetime events.
    /// <summary>
    /// Write a INFO register with a simple message
    /// </summary>
    /// <param name="message">The message</param>
    void Info(string message);

    /// <summary>
    /// Write a INFO register with message and exception
    /// </summary>
    /// <param name="message">The message</param>
    /// <param name="ex">The exception</param>
    void Info(string message, Exception ex);

    /// <summary>
    /// Write a INFO register with a message and detail
    /// </summary>
    /// <param name="message">The message</param>
    /// <param name="detail">An object to detail the message</param>
    void Info(string message, object detail);
    #endregion

    #region warn
    // Warn: Warnings about validation issues or temporary failures that can be recovered.
    /// <summary>
    /// Write a WARN register with a simple message
    /// </summary>
    /// <param name="message">The message</param>
    void Warn(string message);

    /// <summary>
    /// Write a WARN register with message and exception
    /// </summary>
    /// <param name="message">The message</param>
    /// <param name="ex">The exception</param>
    void Warn(string message, Exception ex);

    /// <summary>
    /// Write a WARN register with a message and detail
    /// </summary>
    /// <param name="message">The message</param>
    /// <param name="detail">An object to detail the message</param>
    void Warn(string message, object detail);
    #endregion

    #region error
    // Error: Errors where functionality has failed or Exception have been caught.
    /// <summary>
    /// Write a ERROR register with a simple message
    /// </summary>
    /// <param name="message">The message</param>
    void Error(string message);

    /// <summary>
    /// Write a ERROR register with message and exception
    /// </summary>
    /// <param name="message">The message</param>
    /// <param name="ex">The exception</param>
    void Error(string message, Exception ex);

    /// <summary>
    /// Write a ERROR register with a message and detail
    /// </summary>
    /// <param name="message">The message</param>
    /// <param name="detail">An object to detail the message</param>
    void Error(string message, object detail);
    #endregion

    #region fatal
    // Fatal: Most critical level.Application is about to abort.
    /// <summary>
    /// Write a FATAL register with a simple message
    /// </summary>
    /// <param name="message">The message</param>
    void Fatal(string message);

    /// <summary>
    /// Write a FATAL register with message and exception
    /// </summary>
    /// <param name="message">The message</param>
    /// <param name="ex">The exception</param>
    void Fatal(string message, Exception ex);

    /// <summary>
    /// Write a FATAL register with a message and detail
    /// </summary>
    /// <param name="message">The message</param>
    /// <param name="detail">An object to detail the message</param>
    void Fatal(string message, object detail);
    #endregion
}
