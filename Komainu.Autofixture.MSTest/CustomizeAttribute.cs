using AutoFixture;
using System;
using System.Reflection;

namespace Komainu.AutoFixture.MSTest
{
    public abstract class CustomizeAttribute : Attribute
    {
        public abstract ICustomization GetCustomization(ParameterInfo parameter);
    }
}