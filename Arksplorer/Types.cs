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
            Type = "TameDinos",
            Description = "Tamed Dino",
            NormalSearch = "Map='#' OR Tribe='#' OR Tamer='#' OR Imprinter='#' OR Creature='#' OR Name='#'",
            WildcardSearch = "Map LIKE '*#*' OR Tribe LIKE '*#*' OR Tamer LIKE '*#*' OR Imprinter LIKE '*#*' OR Creature LIKE '*#*' OR Name LIKE '*#*'"
        };

        public static MetaData WildMetadata = new()
        {
            Type = "WildDinos",
            Description = "Wild Dino",
            NormalSearch = "Map='#' OR Creature='#'",
            WildcardSearch = "Map LIKE '*#*' OR Creature LIKE '*#*'"
        };

        public static MetaData SurvivorMetadata = new()
        {
            Type = "Survivors",
            Description = "Survivor",
            NormalSearch = "Map='#' OR Steam='#' OR Name='#' OR Tribe='#'",
            WildcardSearch = "Map LIKE '*#*' OR Steam LIKE '*#*' OR Name LIKE '*#*' OR Tribe LIKE '*#*'",
        };
    }
}
