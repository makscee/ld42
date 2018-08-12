using System;
using System.Collections;
using UnityEngine;

public enum InterpolationType
{
    Square, InvSquare, Linear
}

public static class Utils
{
    public static int IntX(this Vector3 v)
    {
        return (int)v.x;
    }

    public static float ToFloat(this string s)
    {
        if (string.IsNullOrEmpty(s)) return 0f;
        float f;
        var r = float.TryParse(s, out f);
        if (!r) f = 0f;
        return f;
    }

    public static Color ChangeAlpha(this Color c, float alpha)
    {
        c.a = alpha;
        return c;
    }

    public static float Interpolate(float from, float to, float over, float t, InterpolationType type = InterpolationType.Linear)
    {
        var delta = Time.deltaTime;
        if (t + delta > over)
        {
            delta = Math.Max(0, over - t);
        }
        var segment = to - from;
        var unit = t / over;
        var val = 0f;
        var deltaUnit = (t + delta) / over;
        switch (type)
        {
            case InterpolationType.Linear:
                val = delta / over;
                break;
            case InterpolationType.Square:
                val = SquareVal(deltaUnit) - SquareVal(unit);
                break;
            case InterpolationType.InvSquare:
                val = InvSquareVal(deltaUnit) - InvSquareVal(unit);
                break;
        }
        return val * segment;
    }

    private static float InvSquareVal(float t)
    {
        return 1 - (1 - t) * (1 - t);
    }

    private static float SquareVal(float t)
    {
        return t * t;
    }

    public static void Animate(Vector3 from, Vector3 to, float over, Action<Vector3> onChange, MonoBehaviour obj = null, bool fullValue = false, float delay = 0f, InterpolationType type = InterpolationType.Linear)
    {
        obj = obj == null ? CameraScript.Instance : obj;
        obj.StartCoroutine(Animation(from, to, over, onChange, fullValue, delay, type));
    }

    public static void Animate(Color from, Color to, float over, Action<Color> onChange, MonoBehaviour obj = null, bool fullValue = false, float delay = 0f, InterpolationType type = InterpolationType.Linear)
    {
        obj = obj == null ? CameraScript.Instance : obj;
        var fromVec = new Vector3(from.r, from.g, from.b);
        var toVec = new Vector3(to.r, to.g, to.b);
        obj.StartCoroutine(Animation(fromVec, toVec, over, v => onChange(new Color(v.x, v.y, v.z)), fullValue, delay, type));
    }

    public static void Animate(float from, float to, float over, Action<float> onChange, MonoBehaviour obj = null, bool fullValue = false, float delay = 0f, InterpolationType type = InterpolationType.Linear)
    {
        obj = obj == null ? CameraScript.Instance : obj;
        obj.StartCoroutine(Animation(new Vector3(from, 0), new Vector3(to, 0), over, v => onChange(v.x), fullValue, delay, type));
    }

    private static IEnumerator Animation(Vector3 from, Vector3 to, float over, Action<Vector3> action, bool fullValue, float delay, InterpolationType type)
    {
        yield return new WaitForSeconds(delay);
        var t = 0f;
        var result = from;
        while (t < over)
        {
            var x = Interpolate(from.x, to.x, over, t, type);
            var y = Interpolate(from.y, to.y, over, t, type);
            var z = Interpolate(from.z, to.z, over, t, type);
            var temp = new Vector3(x, y, z);
            result += temp;
            action(fullValue ? result : temp);
            t += Time.deltaTime;
            yield return null;
        }
        action(fullValue ? to : to - result);
    }

    public static void InvokeDelayed(Action a, float delay, MonoBehaviour obj = null, bool repeat = false)
    {
        obj = obj == null ? CameraScript.Instance : obj;
        obj.StartCoroutine(Delay(a, delay, repeat));
    }

    private static IEnumerator Delay(Action a, float delay, bool repeat)
    {
        yield return new WaitForSeconds(delay);
        a();
        while (repeat)
        {
            yield return new WaitForSeconds(delay);
            a();
        }
    }
}