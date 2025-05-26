// <copyright file="App.axaml.cs" company="EasySave">
// Copyright (c) EasySave. All rights reserved.
// </copyright>

using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;

namespace EasySave
{
    /// <summary>
    /// Extension methods for accessing services.
    /// </summary>
    public static class ServiceExtensions
    {
        public static T GetService<T>() where T : class
        {
            return App.ServiceProvider.GetRequiredService<T>();
        }
    }
}
