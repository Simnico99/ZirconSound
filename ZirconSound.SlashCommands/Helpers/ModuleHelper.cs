using System;
using System.Collections.Generic;
using System.Reflection;
using ZirconSound.ApplicationCommands.Interactions;

namespace ZirconSound.ApplicationCommands.Helpers
{
    internal static class ModuleHelper
    {
        public static IEnumerable<T1> GetInteractionModules<T1, T2>(Assembly assembly) where T1 : IInteractionGroup<T2>  where T2 : Attribute
        {
            var list = new List<T1>();
            foreach (var type in assembly.GetTypes())
            {
                if (type.IsSubclassOf(typeof(InteractionModule<IInteractionContext>)))
                    foreach (var method in type.GetMethods())
                    {
                        if (method.GetCustomAttributes(typeof(T2), false).Length > 0)
                        {
                            var toAddToGroup = (T1)Activator.CreateInstance(typeof(T1));
                            if (toAddToGroup != null)
                            {
                                toAddToGroup.Interaction = method.GetCustomAttribute<T2>();
                                toAddToGroup.Method = method;
                                toAddToGroup.Module = type;
                                list.Add(toAddToGroup);
                            }
                        }
                    }
            }

            return list;
        }
    }
}