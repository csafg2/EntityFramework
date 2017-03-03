// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class KeyConvention :
        IPrimaryKeyConvention,
        IForeignKeyConvention,
        IForeignKeyRemovedConvention,
        IBaseTypeConvention
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual InternalRelationshipBuilder Apply(InternalRelationshipBuilder relationshipBuilder)
        {
            foreach (var property in relationshipBuilder.Metadata.Properties)
            {
                var propertyBuilder = property.Builder;
                propertyBuilder.ValueGenerated(ValueGenerated.Never, ConfigurationSource.Convention);
            }

            return relationshipBuilder;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void Apply(InternalEntityTypeBuilder entityTypeBuilder, ForeignKey foreignKey)
            => SetKeyValueGeneration(foreignKey.Properties, entityTypeBuilder.Metadata);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool Apply(InternalEntityTypeBuilder entityTypeBuilder, Key previousPrimaryKey)
        {
            if (previousPrimaryKey != null)
            {
                foreach (var property in previousPrimaryKey.Properties)
                {
                    property.Builder?.ValueGenerated(ValueGenerated.Never, ConfigurationSource.Convention);
                }
            }

            SetKeyValueGeneration(entityTypeBuilder.Metadata.FindPrimaryKey()?.Properties, entityTypeBuilder.Metadata);

            return true;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool Apply(InternalEntityTypeBuilder entityTypeBuilder, EntityType oldBaseType)
        {
            var pk = entityTypeBuilder.Metadata.FindPrimaryKey();
            if (pk != null)
            {
                SetKeyValueGeneration(pk.Properties, pk.DeclaringEntityType);
            }

            return true;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Property FindValueGeneratedOnAddProperty(
            [CanBeNull] IReadOnlyList<Property> properties, [NotNull] EntityType entityType)
        {
            Check.NotNull(entityType, nameof(entityType));

            if (properties == null)
            {
                return null;
            }

            if (entityType.FindPrimaryKey(properties) != null
                && properties.Count == 1)
            {
                var property = properties.First();
                if (!property.IsForeignKey())
                {
                    var propertyType = property.ClrType.UnwrapNullableType();
                    if ((propertyType.IsInteger()
                         && propertyType != typeof(byte))
                        || propertyType == typeof(Guid)
                        || propertyType == typeof(string)
                        || propertyType == typeof(byte[]))
                    {
                        return property;
                    }
                }
            }

            return null;
        }

        private void SetKeyValueGeneration(IReadOnlyList<Property> properties, EntityType entityType)
            => FindValueGeneratedOnAddProperty(properties, entityType)
                ?.Builder?.ValueGenerated(ValueGenerated.OnAdd, ConfigurationSource.Convention);
    }
}
