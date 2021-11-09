﻿using System;
using System.Linq;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Diagnostics;

namespace Caravela.Samples.Dirty
{
    public class DirtyAttribute : TypeAspect
    {
        private static readonly DiagnosticDefinition<INamedType> _mustHaveDirtyStateSetter = new
        ("MY001",
            Severity.Error,
            "The 'IDirty' interface is implemented manually on type '{0}', but the property 'DirtyState' does not have a property setter.");

        private static readonly DiagnosticDefinition<IProperty> _dirtyStateSetterMustBeProtected = new
        ("MY002",
            Severity.Error,
            "The setter of the '{0}' property must be have the 'protected' accessibility.");

        public override void BuildAspect(IAspectBuilder<INamedType> aspectBuilder)
        {
            // Implement the IDirty interface.
            if (!aspectBuilder.Target.ImplementedInterfaces.Any(i => i.Is(typeof(IDirty))))
            {
                aspectBuilder.Advices.ImplementInterface(aspectBuilder.Target, typeof(IDirty), OverrideStrategy.Ignore);
            }
            else
            {
                // If the type already implements IDirty, it must have a protected method called OnDirty, otherwise 
                // this is a contract violation, so we report an error.
                var dirtyStateProperty = aspectBuilder.Target.Properties
                    .Where(m => m.Name == nameof(this.DirtyState) && m.Parameters.Count == 0 &&
                                m.Type.Is(typeof(DirtyState)))
                    .SingleOrDefault();

                if (dirtyStateProperty?.SetMethod == null)
                {
                    aspectBuilder.Diagnostics.Report(_mustHaveDirtyStateSetter, aspectBuilder.Target);
                }
                else if (dirtyStateProperty.SetMethod.Accessibility != Accessibility.Protected)
                {
                    aspectBuilder.Diagnostics.Report(_dirtyStateSetterMustBeProtected, dirtyStateProperty);
                }
            }

            // Override all writable fields and automatic properties.
            var fieldsOrProperties = aspectBuilder.Target.Properties
                .Cast<IFieldOrProperty>()
                .Concat(aspectBuilder.Target.Fields)
                .Where(f => f.Writeability == Writeability.All);

            foreach (var fieldOrProperty in fieldsOrProperties)
            {
                aspectBuilder.Advices.OverrideFieldOrPropertyAccessors(fieldOrProperty, null, nameof(OverrideSetter));
            }

            // TODO: This aspect is not complete. We should normally not set DirtyState to Clean after the object has been initialized,
            // but this is not possible in the current version of Caravela.
        }

        [InterfaceMember] 
        public DirtyState DirtyState { get; protected set; }


        [Template]
        private void OverrideSetter()
        {
            meta.Proceed();

            if (this.DirtyState == DirtyState.Clean)
            {
                this.DirtyState = DirtyState.Dirty;
            }
        }
    }
}