using System.Collections.Generic;
using _Main.Scripts.Settings;

namespace _Main.Scripts.Match
{
    public class MatchUnitsModel
    {
        private readonly Dictionary<ulong, PlayerSide> _sidesByUnits;
        private readonly Dictionary<PlayerSide, List<ulong>> _unitListBySides;

        private int _currentRoundNumber;
        private int _maxRoundNumber;
        private int _maxRoundTime;

        private PlayerSide _currentTurn;

        public MatchUnitsModel(ulong[] unitIdsSide1, ulong[] unitIdsSide2)
        {
            _sidesByUnits = new Dictionary<ulong, PlayerSide>();
            foreach (var baseUnit in unitIdsSide1)
                _sidesByUnits.Add(baseUnit, PlayerSide.Side1);
            
            foreach (var baseUnit in unitIdsSide2)
                _sidesByUnits.Add(baseUnit, PlayerSide.Side2);

            _unitListBySides = new Dictionary<PlayerSide, List<ulong>>();

            if (!_unitListBySides.ContainsKey(PlayerSide.Side1))
                _unitListBySides.Add(PlayerSide.Side1, new List<ulong>());
            _unitListBySides[PlayerSide.Side1].AddRange(unitIdsSide1);
            
            if (!_unitListBySides.ContainsKey(PlayerSide.Side2))
                _unitListBySides.Add(PlayerSide.Side2, new List<ulong>());
            _unitListBySides[PlayerSide.Side2].AddRange(unitIdsSide2);
        }
        
        public PlayerSide GetUnitSide(ulong baseUnit) =>
            _sidesByUnits.GetValueOrDefault(baseUnit);
    }
}