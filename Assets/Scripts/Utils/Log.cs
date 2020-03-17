public static class Log
{
    public static void Print(string print, params object[] args)
        => UnityEngine.Debug.Log(string.Format(print, args));

    public static void Print(string print)
        => UnityEngine.Debug.Log(string.Format(print));

    public static void Warn(string print, params object[] args)
        => UnityEngine.Debug.LogWarning(string.Format(print, args));

    public static void Err(string print, params object[] args)
        => UnityEngine.Debug.LogError(string.Format(print, args));
}