using Shard.API.Models;
using Shard.Shared.Core;

namespace Shard.API.Services
{
    public class CombatService
    {
        private readonly Dictionary<string, List<UnitSpecification>> _sameSystemUnits;
        private readonly Dictionary<string, List<UnitSpecification>> _samePlanetUnits;

        public CombatService()
        {
            _sameSystemUnits = new Dictionary<string, List<UnitSpecification>>();
            _samePlanetUnits = new Dictionary<string, List<UnitSpecification>>();
        }

        public void AddSystem(string system)
        {
            if (!_sameSystemUnits.ContainsKey(system))
            {
                _sameSystemUnits.Add(system, new List<UnitSpecification>());
            }
        }

        public void AddPlanet(string planet)
        {
            if (!_samePlanetUnits.ContainsKey(planet))
            {
                _samePlanetUnits.Add(planet, new List<UnitSpecification>());
            }
        }

        public CombatUnit createCombatUnit(string Id, string Type, string System, string Planet, string DestinationSystem, string DestinationPlanet)
        {
            CombatUnit combatUnit = new CombatUnit(Id, Type, System, Planet, DestinationSystem, DestinationPlanet);

            /*if (System != null)
            {
                _sameSystemUnits[System].Add(combatUnit);
            }

            if (Planet != null)
            {
                _samePlanetUnits[Planet].Add(combatUnit);
            }*/

            return combatUnit;
        }

        public Boolean canFire(UnitSpecification unit, int time)
        {
            CombatUnit combatUnit = (CombatUnit)unit;

            return time % combatUnit.Cooldown == 0;
        }
    }
}
