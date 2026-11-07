using System;
using System.Collections.Generic;

namespace WatchTower
{
    /// <summary>
    /// Crea la configuración editable de un perfil: una entrada por cada control y sensor que
    /// tiene el perfil, con los ajustes guardados en el XML o, si no los hay, los de por defecto.
    /// </summary>
    public static class ProfileConfigBuilder
    {
        /// <param name="profileFileName">Archivo del perfil, con extensión (por ejemplo "verano.json").</param>
        /// <param name="content">Controles y sensores que tiene el perfil ahora mismo.</param>
        /// <param name="stored">Ajustes guardados para ese perfil; null si no hay ninguno.</param>
        public static ProfileConfig Build(string profileFileName, ProfileContent content, ProfileConfig stored)
        {
            ProfileConfig config = new ProfileConfig { FileName = profileFileName };

            AddItems(config.Controls, content.Controls, stored?.Controls);
            AddItems(config.Sensors, content.Sensors, stored?.Sensors);

            return config;
        }

        // Añade a 'target' un ajuste por cada elemento del perfil. El nombre es siempre el actual
        // del perfil; los ajustes salen del XML si hay uno con el mismo Identifier.
        private static void AddItems(List<ItemConfig> target, List<LoggableItem> profileItems, List<ItemConfig> storedItems)
        {
            foreach (LoggableItem profileItem in profileItems)
            {
                ItemConfig item = new ItemConfig
                {
                    Identifier = profileItem.Identifier,
                    Name = profileItem.Name
                };

                ItemConfig stored = FindByIdentifier(storedItems, profileItem.Identifier);
                if (stored != null)
                {
                    item.IncludedInLog = stored.IncludedInLog;
                    item.IntervalSeconds = stored.IntervalSeconds;
                    item.Threshold = stored.Threshold;
                }

                target.Add(item);
            }
        }

        // Los identificadores se comparan exactamente, igual que hace el plugin.
        private static ItemConfig FindByIdentifier(List<ItemConfig> items, string identifier)
        {
            if (items == null) return null;

            foreach (ItemConfig item in items)
            {
                if (string.Equals(item.Identifier, identifier, StringComparison.Ordinal))
                    return item;
            }

            return null;
        }
    }
}