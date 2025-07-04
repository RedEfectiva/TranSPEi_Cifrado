using System;

namespace TranSPEi_Cifrado.Domain.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class SensitiveDataAttribute : Attribute
    {
    }
}
