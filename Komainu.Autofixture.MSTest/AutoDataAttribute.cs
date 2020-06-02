using AutoFixture;
using AutoFixture.Kernel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace Komainu.AutoFixture.MSTest
{
    public class AutoDataAttribute : Attribute, ITestDataSource
    {
        private readonly Lazy<IFixture> _fixtureLazy;

        private IFixture Fixture => _fixtureLazy.Value;

        public AutoDataAttribute() : this(() => new Fixture())
        {
        }

        protected AutoDataAttribute(Func<IFixture> fixtureFactory)
        {
            if (fixtureFactory == null)
                throw new ArgumentNullException(nameof(fixtureFactory));

            _fixtureLazy = new Lazy<IFixture>(fixtureFactory, LazyThreadSafetyMode.PublicationOnly);
        }

        public string DisplayName { get; private set; }

        public virtual IEnumerable<object[]> GetData(MethodInfo method)
        {
            if (method == null)
                throw new ArgumentNullException("method");

            var specimens = new List<object>();
            foreach (var p in method.GetParameters())
            {
                CustomizeFixture(p);

                var specimen = Resolve(p);
                specimens.Add(specimen);
            }

            return new[] { specimens.ToArray() };
        }

        private void CustomizeFixture(ParameterInfo p)
        {
            var customizeAttributes = p.GetCustomAttributes(typeof(CustomizeAttribute), false)
                .OfType<CustomizeAttribute>()
                .OrderBy(x => x, new CustomizeAttributeComparer());

            foreach (var ca in customizeAttributes)
            {
                var c = ca.GetCustomization(p);
                Fixture.Customize(c);
            }
        }

        private object Resolve(ParameterInfo p)
        {
            var context = new SpecimenContext(this.Fixture);
            return context.Resolve(p);
        }

        public string GetDisplayName(MethodInfo method, object[] data)
        {
            if (!string.IsNullOrWhiteSpace(DisplayName))
                return DisplayName;

            if (data != null)
                return string.Format(CultureInfo.CurrentCulture, "{0} ({1})", method.Name, string.Join(",", data));

            return null;
        }
    }
}
