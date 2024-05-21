using Common.Interfaces;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Trickster
{
    internal class ModConfig : IConfigurable
    {
        public event EventHandler<ConfigChangedEventArgs> ConfigChanged;

        [DefaultValue(true)]
        public bool EnableMod { get; set; }

        public ModConfig() => InitializeDefaultConfig();
        private void OnConfigChanged(string propertyName, object oldValue, object newValue)
        {
            ConfigChanged?.Invoke(this, new ConfigChangedEventArgs(propertyName, oldValue, newValue));
        }
        public void InitializeDefaultConfig(string category = null)
        {
            PropertyInfo[] properties = GetType().GetProperties();

            foreach (PropertyInfo property in properties)
            {
                DefaultValueAttribute defaultValueAttribute = (DefaultValueAttribute)property.GetCustomAttribute(typeof(DefaultValueAttribute));
                if (defaultValueAttribute != null)
                {
                    object defaultValue = defaultValueAttribute.Value;

                    if (property.PropertyType == typeof(KeybindList) && defaultValue is SButton)
                    {
                        defaultValue = new KeybindList((SButton)defaultValue);
                    }

                    if (category != null && defaultValueAttribute.Category != category)
                    {
                        continue;
                    }


                    // Handle BlacklistedLocations default value
                    if (property.Name == "BlacklistedLocations")
                    {
                        defaultValue ??= new List<string>();
                    }

                    OnConfigChanged(property.Name, property.GetValue(this), defaultValue);
                    property.SetValue(this, defaultValue);
                }
            }
        }

        public void SetConfig(string propertyName, object value)
        {
            PropertyInfo property = GetType().GetProperty(propertyName);
            if (property != null)
            {
                try
                {
                    object convertedValue = Convert.ChangeType(value, property.PropertyType);
                    OnConfigChanged(property.Name, property.GetValue(this), convertedValue);
                    property.SetValue(this, convertedValue);
                }
                catch (Exception ex)
                {
                    ModEntry.ModMonitor.Log($"Error setting property '{propertyName}': {ex.Message}", LogLevel.Error);
                }
            }
            else
            {
                ModEntry.ModMonitor.Log($"Property '{propertyName}' not found in config.", LogLevel.Error);
            }
        }

    }

    [AttributeUsage(AttributeTargets.Property)]
    internal class DefaultValueAttribute : Attribute
    {
        public object Value { get; }
        public string Category { get; }

        public DefaultValueAttribute(object value, string category = null)
        {
            Value = value;
            Category = category;
        }
    }

    internal class ConfigChangedEventArgs : EventArgs
    {
        public string ConfigName { get; }
        public object OldValue { get; }
        public object NewValue { get; }

        public ConfigChangedEventArgs(string configName, object oldValue, object newValue)
        {
            ConfigName = configName;
            OldValue = oldValue;
            NewValue = newValue;
        }
    }
}
