using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace SharpFormat
{
    /// <summary>
    /// A class which represents a reader for a specific format.
    /// Base classes of this class may use <see cref="FormatCommandAttribute"/> and <see cref="FormatPropAttribute"/> 
    /// to define commands and properties.
    /// 
    /// See <see cref="ExampleReader"/> for an example on how to use the API.
    /// </summary>
    public class FormatReader
    {
        private static Dictionary<Type, ReaderInfo> registeredReaderInfos = new Dictionary<Type, ReaderInfo>();
        internal ReaderInfo info;

        protected FormatReader()
        {
            var t = GetType();

            if(registeredReaderInfos.ContainsKey(t))
            {
                info = registeredReaderInfos[t];
            }
            else
            {
                throw new Exception("Source code failure: use of un-registered FormatReader.");
            }
        }

        public static void RegisterReaderTypes(Assembly assembly)
        {
            var types = assembly.GetTypes();
            foreach(var t in types)
            {
                if(t.BaseType == typeof(FormatReader))
                {
                    RegisterReaderType(t);
                }
            }
        }

        public static void RegisterReaderType<T>() where T : FormatReader
            => RegisterReaderType(typeof(T));
        public static void RegisterReaderType(Type t)
        {
            if(registeredReaderInfos.ContainsKey(t))
            {
                return;
            }

            if(t.BaseType != typeof(FormatReader))
            {
                throw new Exception("Source code failure: types registering as a format reader must directly inherit FormatReader.");
            }

            var commandInfo = new ReaderInfo();

            var methods = t.GetMethods()
                .Where(m => m.CustomAttributes.Any(a => a.AttributeType == typeof(FormatCommandAttribute)));
            var props = t.GetProperties()
                .Where(p => p.CustomAttributes.Any(a => a.AttributeType == typeof(FormatPropAttribute)));

            foreach(var method in methods)
            {
                var name = method.GetCustomAttribute<FormatCommandAttribute>().Name;
                name = name ?? method.Name;

                if(!IsIdentifier(name))
                {
                    throw new Exception($"Source code error: invalid name for command: '{name}'");
                }

                var methodParameters = method.GetParameters();

                foreach(var param in methodParameters)
                {
                    if (!TypeExtensions.TypeIsSupported(param.ParameterType))
                    {
                        throw new Exception($"Source code error: invalid parameter type for command: {param.ParameterType}");
                    }
                    if (!IsIdentifier(param.Name))
                    {
                        throw new Exception($"Source code error: invalid name for command parameter: '{name}'");
                    }
                }

                var paramsMap = methodParameters.ToDictionary(p => p.Name, p => p);

                commandInfo.commands.Add(
                    name,
                    (reader, args) => MethodCall(reader, args, paramsMap, method, name)
                );
            }

            foreach(var prop in props)
            {
                var name = prop.GetCustomAttribute<FormatPropAttribute>().Name;
                name = name ?? prop.Name;

                if (!IsIdentifier(name))
                {
                    throw new Exception($"Source code error: invalid name for property command: '{name}'");
                }

                if(!TypeExtensions.TypeIsSupported(prop.PropertyType))
                {
                    throw new Exception($"Source code error: invalid type for property command: {prop.PropertyType}");
                }

                if(!prop.CanWrite)
                {
                    throw new Exception($"Source code error: no setter available for property command '{name}'");
                }

                commandInfo.setCommands.Add(name,
                    (reader, arg) => PropertyCall(reader, arg, prop, name)
                );
            }

            registeredReaderInfos.Add(t, commandInfo);
        }

        private static bool IsIdentifier(string str)
        {
            return str != null && str.Length != 0 &&
                (char.IsLetter(str[0]) || str[0] == '_') &&
                str.Skip(1).All(ch => char.IsLetterOrDigit(ch) || ch == '_');
        }

        private static void MethodCall(FormatReader reader, 
                                       Dictionary<string, EvaluableItem> args,
                                       Dictionary<string, ParameterInfo> paramsMap,
                                       MethodInfo method,
                                       string commandName)
        {
            var parameters = new object[paramsMap.Count];
            foreach (var info in paramsMap.Values)
            {
                if (info.HasDefaultValue)
                {
                    parameters[info.Position] = info.DefaultValue;
                }
            }

            foreach (var kvp in args)
            {
                var paramName = kvp.Key;
                var value = kvp.Value;

                if (!paramsMap.ContainsKey(paramName))
                {
                    throw new Exception($"Unexpected paramter '{paramName}'");
                }

                var info = paramsMap[paramName];

                try
                {
                    var getter = value.GetterForType(info.ParameterType);
                    parameters[info.Position] = getter();
                }
                catch (Exception ex)
                {
                    throw new Exception($"Error in parsing parameter '{paramName}' for command '{commandName}': {ex.Message}");
                }
            }

            try
            {
                method.Invoke(reader, parameters);
            }
            catch (TargetInvocationException ex)
            {
                throw new Exception($"Error in command '{commandName}': {ex.InnerException.Message}");
            }
            catch (Exception ex)
            {
                throw new Exception($"Error in command '{commandName}': {ex.Message}");
            }
        }
        private static void PropertyCall(FormatReader reader, 
                                         EvaluableItem arg,
                                         PropertyInfo prop,
                                         string commandName)
        {
            try
            {
                object argForProp;

                var getter = arg.GetterForType(prop.PropertyType);
                argForProp = getter();

                prop.SetMethod.Invoke(reader, new object[] { argForProp });
            }
            catch (TargetInvocationException ex)
            {
                throw new Exception($"Error in property command '{commandName}': {ex.InnerException.Message}");
            }
            catch (Exception ex)
            {
                throw new Exception($"Error in property command '{commandName}': {ex.Message}");
            }
        }

        public void Evaluate(string text)
        {
            FormatExecutor.FromText(text, this).Execute();
        }
    }
}