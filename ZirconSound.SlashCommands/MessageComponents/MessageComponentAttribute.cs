using System;

namespace ZirconSound.ApplicationCommands.MessageComponents
{
    [AttributeUsage(AttributeTargets.Method)]
    public class MessageComponentAttribute : Attribute
    {
        public MessageComponentAttribute(string id) => Id = id;

        public string Id { get; }
    }
}
