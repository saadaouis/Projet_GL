// <copyright file="ServiceExtensions.cs" company="EasySave">
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
        /// <summary>
        /// Gets the service.
        /// </summary>
        /// <typeparam name="T">The type of the service.</typeparam>
        /// <returns>The service.</returns>
        public static T GetService<T>() 
        where T : class
        {
            return App.ServiceProvider!.GetRequiredService<T>();
        }
    }
}
