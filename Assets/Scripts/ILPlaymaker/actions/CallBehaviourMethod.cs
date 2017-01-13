
using System;
using System.Reflection;
using UnityEngine;
using System.Collections.Generic;
using Object = UnityEngine.Object;
using HutongGames.PlayMaker;

[ActionCategory(ActionCategory.ScriptControl)]
[HutongGames.PlayMaker.Tooltip("Call a method in a behaviour.")]
public class CallBehaviourMethod : FsmStateAction
{
	[RequiredField]
	[HutongGames.PlayMaker.Tooltip("The game object that owns the behaviour.")]
	public FsmOwnerDefault gameObject;

	[RequiredField]
	[UIHint(UIHint.Behaviour)]
	[HutongGames.PlayMaker.Tooltip("The behaviour that contains the method.")]
	public FsmString behaviourName;

	[RequiredField]
	[UIHint(UIHint.Method)]
	[HutongGames.PlayMaker.Tooltip("The name of the method to invoke.")]
	public FsmString methodName;

	[HutongGames.PlayMaker.Tooltip("Method paramters. NOTE: these must match the method's signature!")]
	public FsmVar[] parameters;

	[ActionSection("Store Result")]

	[UIHint(UIHint.Variable)]
	[HutongGames.PlayMaker.Tooltip("Store the result of the method call.")]
	public FsmVar storeResult;

	[HutongGames.PlayMaker.Tooltip("Repeat every frame.")]
	public bool everyFrame;

	private FsmObject cachedBehaviour;
	private FsmString cachedMethodName;
	private Type cachedType;
	private MethodInfo cachedMethodInfo;
	private ParameterInfo[] cachedParameterInfo;
	private object[] parametersArray;
	private string errorString;

	public override void Reset()
	{
		//behaviour = null;
		methodName = null;
		parameters = null;
		storeResult = null;
		everyFrame = false;
	}

	public override void OnEnter()
	{
		parametersArray = new object[parameters.Length];

		DoMethodCall();

		if (!everyFrame)
		{
			Finish();
		}
	}

	public override void OnUpdate()
	{
		DoMethodCall();
	}

	private void DoMethodCall()
	{
		if (behaviourName.Value == null)
		{
			Finish();
			return;
		}

		if (NeedToUpdateCache())
		{
			if (!DoCache())
			{
				Debug.LogError(errorString);
				Finish();
				return;
			}
		}

		object result = null;
		if (cachedParameterInfo.Length == 0)
		{
			result = cachedMethodInfo.Invoke(cachedBehaviour.Value, null);
		}
		else
		{
			for (var i = 0; i < parameters.Length; i++)
			{
				var parameter = parameters[i];
				parameter.UpdateValue();
				parametersArray[i] = parameter.GetValue();
			}

			result = cachedMethodInfo.Invoke(cachedBehaviour.Value, parametersArray);
		}

		if (!storeResult.IsNone)
		{
			storeResult.SetValue(result);
		}
	}

	// TODO: Move tests to helper function in core
	private bool NeedToUpdateCache()
	{
		return 	gameObject == null ||  behaviourName == null || string.IsNullOrEmpty(behaviourName.Value) ||
				cachedBehaviour == null || cachedMethodName == null || // not cached yet
				cachedBehaviour.Value == null || 
				cachedBehaviour.Value.GetType().Name != behaviourName.Value ||
				((MonoBehaviour)cachedBehaviour.Value).gameObject != Fsm.GetOwnerDefaultTarget(gameObject) ||
				cachedMethodName.Value != methodName.Value ||   // methodName value changed
				cachedMethodName.Name != methodName.Name;       // methodName variable name changed
	}

	private bool DoCache()
	{
		errorString = string.Empty;

		GameObject go = Fsm.GetOwnerDefaultTarget(gameObject);
		if (go == null || behaviourName == null || string.IsNullOrEmpty(behaviourName.Value))
		{
			errorString += "Invalid GameObject or Behaviour script.\n";
			Finish();
			return false;
		}

		Type objectType = ReflectionUtils.GetGlobalType(behaviourName.Value);
		Object behav = go.GetComponent(objectType);

		if(behav == null){
			errorString += "Missing Component "+ behaviourName.Value +" for game object "+go.name +".\n";
			behaviourName = null;
			methodName = null;
			Finish();
			return false;
		}

		cachedBehaviour = new FsmObject(behav);//behaviourName);
		cachedMethodName = new FsmString(methodName);

		cachedType = objectType;

		var types = new List<Type>(parameters.Length);
		foreach (var each in parameters)
		{
			types.Add(each.RealType);
		}

		#if NETFX_CORE
		var methods = cachedType.GetTypeInfo().GetDeclaredMethods(methodName.Value);
		foreach (var method in methods)
		{
		if (TestMethodSignature(method, types))
		{
		cachedMethodInfo = method;
		}
		}
		#else
		cachedMethodInfo = cachedType.GetMethod(methodName.Value, types.ToArray());

		#endif
		if (cachedMethodInfo == null)
		{
			errorString += "Invalid Method Name or Parameters: " + methodName.Value + "\n";
			Finish();
			return false;
		}

		cachedParameterInfo = cachedMethodInfo.GetParameters();

		return true;
	}

	#if NETFX_CORE
	private bool TestMethodSignature(MethodInfo method, List<Type> parameterTypes)
	{
	if (method == null) return false;
	var methodParameters = method.GetParameters();
	if (methodParameters.Length != parameterTypes.Count) return false;
	for (var i = 0; i < methodParameters.Length; i++)
	{
	if (!ReferenceEquals(methodParameters[i].ParameterType, parameterTypes[i]))
	{
	return false;
	}
	}
	return true;
	}
	#endif

	public override string ErrorCheck()
	{
		/* We could only error check if when we recache,
     * however NeedToUpdateCache() is not super robust
     * So for now we just recache every frame in editor
     * Need to test editor perf...
    if (!NeedToUpdateCache())
    {
        return errorString; // last error message
    }*/

		if (Application.isPlaying)
		{
			return errorString; // last error message
		}

		errorString = string.Empty;
		if (!DoCache())
		{
			return errorString;
		}

		if (parameters.Length != cachedParameterInfo.Length)
		{
			return "Parameter count does not match method.\nMethod has " + cachedParameterInfo.Length + " parameters.\nYou specified " + parameters.Length + " paramaters.";
		}

		for (var i = 0; i < parameters.Length; i++)
		{
			var p = parameters[i];
			var paramType = p.RealType;
			var paramInfoType = cachedParameterInfo[i].ParameterType;
			if (!ReferenceEquals(paramType, paramInfoType))
			{
				return "Parameters do not match method signature.\nParameter " + (i + 1) + " (" + paramType + ") should be of type: " + paramInfoType;
			}
		}

		if (ReferenceEquals(cachedMethodInfo.ReturnType, typeof(void)))
		{
			if (!string.IsNullOrEmpty(storeResult.variableName))
			{
				return "Method does not have return.\nSpecify 'none' in Store Result.";
			}
		}
		else if (!ReferenceEquals(cachedMethodInfo.ReturnType, storeResult.RealType))
		{
			return "Store Result is of the wrong type.\nIt should be of type: " + cachedMethodInfo.ReturnType;
		}

		return string.Empty;
	}

	#if UNITY_EDITOR
	public override string AutoName()
	{
		var name = methodName + "(";
		for (int i = 0; i < parameters.Length; i++)
		{
			var param = parameters[i];
			name += ActionHelpers.GetValueLabel(param.NamedVar);
			if (i < parameters.Length - 1)
			{
				name += ",";
			}
		}
		name += ")";

		if (!storeResult.IsNone)
		{
			name = storeResult.variableName + "=" + name;
		}

		return name;
	}
	#endif
}
