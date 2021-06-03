using UnityEngine;
using System.Collections;
using System;

public class PlayBandInvoke {
	
	public static void Invoke(MonoBehaviour target, Action func, float delaySeconds)
	{
		target.StartCoroutine(_Invoke(func, delaySeconds));
	}

	public static void Invoke<T1>(MonoBehaviour target, Action<T1> func, float delaySeconds, T1 param1)
	{
		target.StartCoroutine(_Invoke(func, delaySeconds, param1));
	}

	public static void Invoke<T1, T2>(MonoBehaviour target, Action<T1, T2> func, float delaySeconds, T1 param1, T2 param2)
	{
		target.StartCoroutine(_Invoke(func, delaySeconds, param1, param2));
	}

	public static void Invoke<T1, T2, T3>(MonoBehaviour target, Action<T1, T2, T3> func, float delaySeconds, T1 param1, T2 param2, T3 param3)
	{
		target.StartCoroutine(_Invoke(func, delaySeconds, param1, param2, param3));
	}

	public static void Invoke<T1, T2, T3, T4>(MonoBehaviour target, Action<T1, T2, T3, T4> func, float delaySeconds, T1 param1, T2 param2, T3 param3, T4 param4)
	{
		target.StartCoroutine(_Invoke(func, delaySeconds, param1, param2, param3, param4));
	}




	
	private static IEnumerator _Invoke(Action func, float delaySeconds)
	{
		yield return new WaitForSeconds(delaySeconds);
		func();
	}
	private static IEnumerator _Invoke<T1>(Action<T1> func, float delaySeconds, T1 param1)
	{
		yield return new WaitForSeconds(delaySeconds);
		func(param1);
	}
	private static IEnumerator _Invoke<T1, T2>(Action<T1, T2> func, float delaySeconds, T1 param1, T2 param2)
	{
		yield return new WaitForSeconds(delaySeconds);
		func(param1, param2);
	}
	private static IEnumerator _Invoke<T1, T2, T3>(Action<T1, T2, T3> func, float delaySeconds, T1 param1, T2 param2, T3 param3)
	{
		yield return new WaitForSeconds(delaySeconds);
		func(param1, param2, param3);
	}
	private static IEnumerator _Invoke<T1, T2, T3, T4>(Action<T1, T2, T3, T4> func, float delaySeconds, T1 param1, T2 param2, T3 param3, T4 param4)
	{
		yield return new WaitForSeconds(delaySeconds);
		func(param1, param2, param3, param4);
	}
}
