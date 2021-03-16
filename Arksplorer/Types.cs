using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arksplorer
{
    /// <summary>
    /// Type names must match ArksplorerData.json names
    /// </summary>
    public static class Types
    {
        // Really want to tag static variants of these into the class definitions for ArkEntities, but, no luck so far (without them becoming duplicating strings)
        public static MetaData TameMetadata = new()
        {
            JsonClassType = typeof(TameDino),
            ArkEntityType = "TameDinos",
            Description = "Tamed Dino",
            IncludesCreatures = true,
            IncludesLevel = true,
            NormalSearch = "Map='#' OR Tribe='#' OR Tamer='#' OR Imprinter='#' OR Creature='#' OR Name='#' OR CreatureId='#'",
            WildcardSearch = "Map LIKE '*#*' OR Tribe LIKE '*#*' OR Tamer LIKE '*#*' OR Imprinter LIKE '*#*' OR Creature LIKE '*#*' OR Name LIKE '*#*' OR CreatureId LIKE '*#*'"
        };

        public static MetaData WildMetadata = new()
        {
            JsonClassType = typeof(WildDino),
            ArkEntityType = "WildDinos",
            Description = "Wild Dino",
            IncludesCreatures = true,
            IncludesLevel = true,
            NormalSearch = "Map='#' OR Creature='#' OR CreatureId='#'",
            WildcardSearch = "Map LIKE '*#*' OR Creature LIKE '*#*' OR CreatureId LIKE '*#*'"
        };

        public static MetaData SurvivorMetadata = new()
        {
            JsonClassType = typeof(Survivor),
            ArkEntityType = "Survivors",
            Description = "Survivor",
            IncludesCreatures = false,
            IncludesLevel = true,
            NormalSearch = "Map='#' OR Steam='#' OR Name='#' OR Tribe='#'",
            WildcardSearch = "Map LIKE '*#*' OR Steam LIKE '*#*' OR Name LIKE '*#*' OR Tribe LIKE '*#*'"
        };
    }
}
