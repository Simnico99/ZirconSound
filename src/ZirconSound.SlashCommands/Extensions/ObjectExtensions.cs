namespace ZirconSound.ApplicationCommands.Extensions;

internal static class ObjectExtensions
{
    public static bool HasProperty(this object obj, string propertyName) => obj.GetType().GetProperty(propertyName) != null;

}
