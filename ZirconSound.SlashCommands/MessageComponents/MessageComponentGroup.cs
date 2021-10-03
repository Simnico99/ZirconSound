using System;
using System.Reflection;
using ZirconSound.ApplicationCommands.Interactions;

namespace ZirconSound.ApplicationCommands.MessageComponents
{
    public abstract class MessageComponentGroup : MessageComponentGroup<MessageComponentAttribute>, IInteractionGroup
    { }

    public class MessageComponentGroup<T> : IInteractionGroup<T> where T : MessageComponentAttribute
    {
        public T Interaction { get; set; }
        public MethodInfo Method { get; set; }
        public Type Module { get; set; }
    }
}