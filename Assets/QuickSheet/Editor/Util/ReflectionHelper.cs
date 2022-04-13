

using System;
using System.Collections.Generic;
using UnityEditor.Compilation;

namespace UnityQuickSheet
{
    public static class ReflectionHelper
    {

        /// <summary>
        /// a 是否继承自 b
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool AIsExtendsByB(Type a, Type b)
        {
            var baseType = a.BaseType;
            while (baseType != null)
            {
                if (baseType.Name == b.Name)
                {
                    return true;
                }
                else
                {
                    baseType = baseType.BaseType;
                }
            }

            return false;
        }

        /// <summary>
        /// 根据Attribute拿到所有的类 
        /// 针对"Assembly-CSharp"dll
        /// </summary>
        /// <typeparam name="T">Attribute</typeparam>
        /// <returns></returns>
        public static List<Type> FindAllScriptTypeByAttribute<T>() where T : Attribute
        {
            List<Type> result = new List<Type>();

            var types = System.Reflection.Assembly.Load("Assembly-CSharp").GetTypes();
            var targetT = typeof(T);
            foreach (var type in types)
            {
                var attrs = type.GetCustomAttributes(targetT, false);
                if (attrs.Length == 0)
                {
                    continue;
                }
                result.Add(type);
            }

            return result;
        }

        /// <summary>
        /// 查找对应Unity Assembly
        /// </summary>
        /// <param name="assemblyName"></param>
        /// <returns></returns>
        public static UnityEditor.Compilation.Assembly FindAssembly(string assemblyName)
        {
            UnityEngine.Debug.Log("== Player Assemblies ==");
            UnityEditor.Compilation.Assembly[] playerAssemblies =
                CompilationPipeline.GetAssemblies(AssembliesType.PlayerWithoutTestAssemblies);

            foreach (var assembly in playerAssemblies)
            {
                if (assembly.name == assemblyName)
                {
                    return assembly;
                }
            }
            return null;
        }
    }
}