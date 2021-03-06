﻿namespace WpfApp
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Windows;
    using System.Windows.Data;
    using IoC;

    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    public class DataProvider: ObjectDataProvider
    {
        // Design Time Container
        private static readonly Lazy<IContainer> ContainerDesignTime 
            = new Lazy<IContainer>(() => Container.Create().Using<ConfigurationDesignTime>());

        public object Tag { get; set; }

        protected override void BeginQuery()
        {
            base.BeginQuery();
            if (Application.Current is App app)
            {
                OnQueryFinished(app.Container.Resolve<object>(ObjectType, Tag));
            }
            else
            {
                OnQueryFinished(ContainerDesignTime.Value.Resolve<object>(ObjectType, Tag));
            }
        }
    }
}
