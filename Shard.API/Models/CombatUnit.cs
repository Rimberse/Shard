namespace Shard.API.Models
{
    public class CombatUnit : UnitSpecification
    {
        public int Health { get; set; }
        public int Damage { get; set; }
        public int Cooldown { get; set; }

        public CombatUnit()
        {
            Id = Guid.NewGuid().ToString();
            Type = Guid.NewGuid().ToString();
            System = Guid.NewGuid().ToString();
            Planet = Guid.NewGuid().ToString();
            DestinationSystem = System;
            DestinationPlanet = Planet;
            Health = 0;
            Damage = 0;
            Cooldown = 0;
        }
        public CombatUnit(string Id, string Type, string System, string DestinationSystem, string Planet, string DestinationPlanet)
        {
            this.Id = Id == null ? Guid.NewGuid().ToString() : Id;
            this.Type = Type;
            this.System = System;
            this.DestinationSystem = DestinationSystem == null ? System : DestinationSystem;
            this.Planet = Planet;
            this.DestinationPlanet = DestinationPlanet == null ? Planet : DestinationPlanet;

            if (Type == "bomber")
            {
                Health = 50;
                Damage = 400;
                Cooldown = 60;
            }
            else if (Type == "cruiser")
            {
                Health = 400;
                Damage = 10000;
                Cooldown = 6;
            }
            else if (Type == "fighter")
            {
                Health = 80;
                Damage = 10;
                Cooldown = 6;
            }
        }
    }
}
