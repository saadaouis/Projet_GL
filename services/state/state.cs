// <copyright file="state.cs" company="EasySave">
// Copyright (c) EasySave. All rights reserved.
// </copyright>

using System.Text.Json;

namespace EasySave.Services.State
{
    /// <summary>
    /// State class for handling the state of the application.
    /// </summary>
    public class State
    {
        private readonly string statePath = "json/state.json";

        public void writeState(Dictionary<string, string> state)
        {
            string json = JsonSerializer.Serialize(state);
            File.WriteAllText(this.statePath, json);
        }
    }
}   