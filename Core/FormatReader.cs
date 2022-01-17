using SharpParser.Helpers;
using SharpParser.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace SharpParser
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

            if(!registeredReaderInfos.ContainsKey(t))
            {
                PreRegisterReaderType(t);
            }

            info = registeredReaderInfos[t];
        }

        /// <summary>
        /// Register all of the reflection information needed for the format reader types within the given assembly.
        /// Use this to cut down on the costs of registering this information during the first constructor call of the assembly's reader types.
        /// </summary>
        public static void PreRegisterReaderTypes(Assembly assembly)
        {
            var types = assembly.GetTypes();
            foreach(var t in types)
            {
                if(t.BaseType == typeof(FormatReader))
                {
                    PreRegisterReaderType(t);
                }
            }
        }

        /// <summary>
        /// Register all of the reflection information needed for the format reader type, passed via type parameter <typeparamref name="T"/>.
        /// Use this to cut down on the costs of registering this information during the first constructor call of the given reader type.
        /// </summary>
        public static void PreRegisterReaderType<T>() 
            => PreRegisterReaderType(typeof(T));

        /// <summary>
        /// Register all of the reflection information needed for the given format reader type.
        /// Use this to cut down on the costs of registering this information during the first constructor call of the given reader type.
        /// </summary>
        public static void PreRegisterReaderType(Type t)
        {
            if(registeredReaderInfos.ContainsKey(t))
            {
                return;
            }

            if(t.BaseType != typeof(FormatReader))
            {
                throw new Exception($"Source code failure: type {t} registering as a format reader must directly inherit FormatReader.");
            }

            var commandInfo = new ReaderInfo();

            var methods = t.GetMethods()
                .Where(m => m.CustomAttributes.Any(a => a.AttributeType == typeof(FormatCommandAttribute)));
            var props = t.GetProperties()
                .Where(p => p.CustomAttributes.Any(a => a.AttributeType == typeof(FormatPropAttribute)));

            foreach (var method in methods)
            {
                var commandName = method.GetCustomAttribute<FormatCommandAttribute>().Name;
                commandName = commandName ?? method.Name;

                if (!IsIdentifier(commandName))
                {
                    throw new Exception($"Source code error: invalid name for command: '{commandName}'");
                }

                var methodParameters = method.GetParameters();

                foreach (var param in methodParameters)
                {
                    if (!TypeExtensions.TypeIsSupported(param.ParameterType))
                    {
                        throw new Exception($"Source code error: invalid parameter type for format command: {param.ParameterType} for parameter '{param.Name}' of command '{t.FullName}.{commandName}'");
                    }
                    if (!IsIdentifier(param.Name))
                    {
                        throw new Exception($"Source code error: invalid name for command parameter: '{param.Name}' of command '{t.FullName}.{commandName}'");
                    }
                }

                var paramsMap = methodParameters.ToDictionary(p => p.Name, p => p);

                commandInfo.commands.Add(
                    commandName,
                    new Command
                    {
                        Explicit = (reader, args) => ExplicitMethodCall(reader, args, paramsMap, method, commandName),
                        NonExplicit = (reader, args) => NonExplicitMethodCall(reader, args, method, commandName),
                        IsVoid = method.ReturnType == typeof(void)
                    }
                );
            }

            foreach(var prop in props)
            {
                var name = prop.GetCustomAttribute<FormatPropAttribute>().Name;
                name = name ?? prop.Name;

                if (!IsIdentifier(name))
                {
                    throw new Exception($"Source code error: invalid name for property command: '{t.FullName}.{name}'");
                }

                if(!TypeExtensions.TypeIsSupported(prop.PropertyType))
                {
                    throw new Exception($"Source code error: invalid type for property command: {prop.PropertyType} for command '{t.FullName}.{name}'");
                }

                if(!prop.CanWrite)
                {
                    throw new Exception($"Source code error: no setter available for property command '{t.FullName}.{name}'");
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

        private static ValueInterface ExplicitMethodCall(FormatReader reader, Dictionary<string, ValueInterface> args, Dictionary<string, ParameterInfo> paramsMap, MethodInfo method, string commandName)
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
                return new ValueInterface(method.Invoke(reader, parameters));
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
        private static ValueInterface NonExplicitMethodCall(FormatReader reader, ValueInterface[] args, MethodInfo method, string commandName)
        {
            var parameters = new object[args.Length];
            var methodParamTypes = method.GetParameters();

            for (int i = 0; i < args.Length; i++)
            {
                parameters[i] = args[i].GetterForType(methodParamTypes[i].ParameterType)();
            }

            try
            {
                return new ValueInterface(method.Invoke(reader, parameters));
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

        private static ValueInterface PropertyCall(FormatReader reader, ValueInterface arg, PropertyInfo prop, string commandName)
        {
            try
            {
                object argForProp;

                var getter = arg.GetterForType(prop.PropertyType);
                argForProp = getter();

                prop.SetMethod.Invoke(reader, new object[] { argForProp });

                return ValueInterface.Void;
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

        public virtual void BeforeEval() {}
        public virtual void AfterEval() {}

        public void Evaluate(string text, string sourceName = "UNKNOWN")
        {
            BeforeEval();
            FormatExecutor.FromText(text, sourceName, this).Execute();
            AfterEval();
        }
    }
}