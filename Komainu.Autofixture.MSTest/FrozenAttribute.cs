using AutoFixture;
using AutoFixture.Kernel;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Komainu.AutoFixture.MSTest
{
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
    public sealed class FrozenAttribute : CustomizeAttribute
    {
        public FrozenAttribute() : this(Matching.ExactType)
        {
        }

        public FrozenAttribute(Matching by)
        {
            By = by;
        }

        public Matching By { get; }

        public override ICustomization GetCustomization(ParameterInfo parameter)
        {
            if (parameter == null)
                throw new ArgumentNullException(nameof(parameter));

            return FreezeByCriteria(parameter);
        }

        private ICustomization FreezeByCriteria(ParameterInfo parameter)
        {
            var type = parameter.ParameterType;
            var name = parameter.Name;

            var filter = new Filter(ByEqual(parameter))
                .Or(ByExactType(type))
                .Or(ByBaseType(type))
                .Or(ByImplementedInterfaces(type))
                .Or(ByPropertyName(type, name))
                .Or(ByParameterName(type, name))
                .Or(ByFieldName(type, name));

            return new FreezeOnMatchCustomization(parameter, filter);
        }

        private static IRequestSpecification ByEqual(object target) =>
            new EqualRequestSpecification(target);

        private IRequestSpecification ByExactType(Type type) =>
            ShouldMatchBy(Matching.ExactType)
                ? new OrRequestSpecification(
                    new ExactTypeSpecification(type),
                    new SeedRequestSpecification(type))
                : NoMatch();

        private IRequestSpecification ByBaseType(Type type) =>
            ShouldMatchBy(Matching.DirectBaseType)
                ? new AndRequestSpecification(
                    new InverseRequestSpecification(
                        new ExactTypeSpecification(type)),
                    new DirectBaseTypeSpecification(type))
                : NoMatch();

        private IRequestSpecification ByImplementedInterfaces(Type type) =>
            ShouldMatchBy(Matching.ImplementedInterfaces)
                ? new AndRequestSpecification(
                    new InverseRequestSpecification(
                        new ExactTypeSpecification(type)),
                    new ImplementedInterfaceSpecification(type))
                : NoMatch();

        private IRequestSpecification ByParameterName(Type type, string name) =>
            ShouldMatchBy(Matching.ParameterName)
                ? new ParameterSpecification(
                    new ParameterTypeAndNameCriterion(
                        DerivesFrom(type),
                        IsNamed(name)))
                : NoMatch();

        private IRequestSpecification ByPropertyName(Type type, string name) =>
             ShouldMatchBy(Matching.PropertyName)
                ? new PropertySpecification(
                    new PropertyTypeAndNameCriterion(
                        DerivesFrom(type),
                        IsNamed(name)))
                : NoMatch();

        private IRequestSpecification ByFieldName(Type type, string name) =>
             ShouldMatchBy(Matching.FieldName)
                ? new FieldSpecification(
                    new FieldTypeAndNameCriterion(
                        DerivesFrom(type),
                        IsNamed(name)))
                : NoMatch();


        private bool ShouldMatchBy(Matching criteria)
        {
            return By.HasFlag(criteria);
        }

        private static IRequestSpecification NoMatch() =>
            new FalseRequestSpecification();

        private static Criterion<Type> DerivesFrom(Type type)=>
            new Criterion<Type>(
                type,
                new DerivesFromTypeComparer());

        private static Criterion<string> IsNamed(string name) =>
            new Criterion<string>(
                name,
                StringComparer.OrdinalIgnoreCase);

        private class Filter : IRequestSpecification
        {
            private readonly IRequestSpecification _criteria;

            public Filter(IRequestSpecification criteria)
            {
                _criteria = criteria;
            }

            public Filter Or(IRequestSpecification condition) =>
                new Filter(new OrRequestSpecification(
                    _criteria,
                    condition));

            public bool IsSatisfiedBy(object request) =>
                _criteria.IsSatisfiedBy(request);
        }

        private class DerivesFromTypeComparer : IEqualityComparer<Type>
        {
            public bool Equals(Type x, Type y)
            {
                if (y == null && x == null)
                    return true;

                if (y == null)
                    return false;

                return y.GetTypeInfo().IsAssignableFrom(x);
            }

            public int GetHashCode(Type obj) => 0;
        }
    }
}