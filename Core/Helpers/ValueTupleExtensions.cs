using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime;
using System.Runtime.CompilerServices;

namespace SharpParser.Helpers
{
    public static class ValueTupleExtensions
    {
        public const int MaxTypes = 8;

        public static TupleTypeInfo[] UnderlyingTypes(Type t)
        {
            if (t == typeof(ValueTuple))
                return new TupleTypeInfo[0];

            IEnumerable<TupleTypeInfo> AddNameInfo(IEnumerable<Type> types)
            {
                return types.Select(
                    (a, i) => new TupleTypeInfo {
                        Type = a,
                        Name = (
                            a.GetCustomAttributes()
                             .FirstOrDefault(at => at.GetType() == typeof(TupleElementNamesAttribute)
                        ) as TupleElementNamesAttribute)?.TransformNames?[i]
                    }
                );
            }

            if (!t.IsConstructedGenericType)
                return null;

            var genT = t.GetGenericTypeDefinition();
            var args = t.GetGenericArguments();

            //Console.WriteLine(args.ToArray());

            if (genT == typeof(ValueTuple<>)
             || genT == typeof(ValueTuple<,>)
             || genT == typeof(ValueTuple<,,>)
             || genT == typeof(ValueTuple<,,,>)
             || genT == typeof(ValueTuple<,,,,>)
             || genT == typeof(ValueTuple<,,,,,>)
             || genT == typeof(ValueTuple<,,,,,,>))
                return AddNameInfo(args).ToArray();

            if (genT == typeof(ValueTuple<,,,,,,,>))
            {
                var rest = args[args.Length - 1];
                return AddNameInfo(args.Take(MaxTypes - 1)).Concat(UnderlyingTypes(rest)).ToArray();
            }

            return null;
        }

        private static Type TupleTypeForGroup(IEnumerable<Type> elementTypes)
        {
            Type type;
            switch(elementTypes.Count())
            {
                case 0:
                    return typeof(ValueTuple);

                case 1:
                    type = typeof(ValueTuple<>);
                    break;

                case 2:
                    type = typeof(ValueTuple<,>);
                    break;

                case 3:
                    type = typeof(ValueTuple<,,>);
                    break;

                case 4:
                    type = typeof(ValueTuple<,,,>);
                    break;

                case 5:
                    type = typeof(ValueTuple<,,,,>);
                    break;

                case 6:
                    type = typeof(ValueTuple<,,,,,>);
                    break;

                case 7:
                    type = typeof(ValueTuple<,,,,,,,>);
                    break;

                default:
                    throw new Exception("Invalid list of element types.");
            }

            return type.MakeGenericType(elementTypes.ToArray());
        }
        public static Type TupleType(IEnumerable<Type> elementTypes)
        {
            var count = elementTypes.Count();

            if(count < MaxTypes)
            {
                return TupleTypeForGroup(elementTypes);
            }

            return typeof(ValueTuple<,,,,,,,>).MakeGenericType(
                elementTypes.ElementAt(0),
                elementTypes.ElementAt(1),
                elementTypes.ElementAt(2),
                elementTypes.ElementAt(3),
                elementTypes.ElementAt(4),
                elementTypes.ElementAt(5),
                elementTypes.ElementAt(6),
                elementTypes.ElementAt(7),
                TupleType(elementTypes.Skip(MaxTypes))
            );
        }
        public static Type TupleType(params Type[] elementTypes)
            => TupleType(elementTypes);
        
        public static object ToTuple(IEnumerable<object> elements)
        {
            var count = elements.Count();
            var elementTypes = elements.Select(x => x.GetType());
            var type = TupleType(elementTypes);

            if (count < MaxTypes)
            {
                return Activator.CreateInstance(
                    type, 
                    elements.ToArray()
                );
            }

            return Activator.CreateInstance(
                type,
                elements.Take(MaxTypes - 1).Concat(
                    new object[] { ToTuple(elements.Skip(MaxTypes)) }
                ).ToArray()
            );
        }
    }
}