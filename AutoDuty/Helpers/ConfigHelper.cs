﻿using System;
using System.Reflection;
using ECommons.DalamudServices;

namespace AutoDuty.Helpers
{
    internal static class ConfigHelper
    {
        private const BindingFlags All = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;
        private static string ConfigType(FieldInfo field) => field.FieldType.ToString();

        internal static string GetConfig(string configName)
        {
            FieldInfo? field;
            if ((field = FindConfig(configName)) == null)
            {
                Svc.Log.Error($"Unable to find config: {configName}, please type /autoduty cfg list to see all available configs");
                return string.Empty;
            }
            else if (field.FieldType.ToString().Contains("System.Collections", StringComparison.InvariantCultureIgnoreCase) || field.FieldType.ToString().Contains("Dalamud.Plugin", StringComparison.InvariantCultureIgnoreCase))
                return string.Empty;
            else
                return field.GetValue(Plugin.Configuration)?.ToString() ?? string.Empty;
        }

        internal static bool ModifyConfig(string configName, string configValue)
        {
            FieldInfo? field;
            if ((field = FindConfig(configName)) == null)
            {
                Svc.Log.Error($"Unable to find config: {configName}, please type /autoduty cfg list to see all available configs");
                return false;
            }
            else if (field.FieldType.ToString().Contains("System.Collections", StringComparison.InvariantCultureIgnoreCase) || field.FieldType.ToString().Contains("Dalamud.Plugin", StringComparison.InvariantCultureIgnoreCase))
                return false;
            else
            {
                var configType = ConfigType(field);
                if (configType == "System.Boolean" && (configValue.ToLower().Equals("true") || configValue.ToLower().Equals("false")))
                    field.SetValue(Plugin.Configuration, bool.Parse(configValue));
                else if (configType == "System.Int32" && int.TryParse(configValue, out var i))
                    field.SetValue(Plugin.Configuration, i);
                else if (configType == "System.String")
                    field.SetValue(Plugin.Configuration, configValue);
                else
                {
                    Svc.Log.Error($"Unable to set config setting: {field.Name.Replace(">k__BackingField", "").Replace("<", "")}, value must be of type: {field.FieldType.ToString().Replace("System.", "")}");
                    return false;
                }
                Plugin.Configuration.Save();
            }
            return false;
        }

        internal static void ListConfig()
        {
            var i = Assembly.GetExecutingAssembly().GetType("AutoDuty.Windows.Configuration")?.GetFields(All);
            if (i == null) return;
            foreach (var field in i)
            {
                if (!field.FieldType.ToString().Contains("System.Collections",StringComparison.InvariantCultureIgnoreCase) && !field.FieldType.ToString().Contains("Dalamud.Plugin", StringComparison.InvariantCultureIgnoreCase) && !field.Name.Replace(">k__BackingField", "").Replace("<", "").Equals("Version",StringComparison.InvariantCultureIgnoreCase))
                    Svc.Log.Info($"{field.Name.Replace(">k__BackingField", "").Replace("<", "")} = {field.GetValue(Plugin.Configuration)} ({field.FieldType.ToString().Replace("System.", "")})");
            }
        }

        internal static FieldInfo? FindConfig(string configName)
        {
            var i = Assembly.GetExecutingAssembly().GetType("AutoDuty.Windows.Configuration")?.GetFields(All);
            foreach (var field in i!)
            {
                if (field.Name.Replace(">k__BackingField", "").Replace("<", "").Equals("Version", StringComparison.InvariantCultureIgnoreCase))
                    continue;
                if (field.Name.Replace(">k__BackingField", "").Replace("<", "").Equals(configName, StringComparison.InvariantCultureIgnoreCase))
                    return field;
                
            }
            return null;
        }
    }
}
