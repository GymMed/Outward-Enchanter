using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OutwardEnchanter.Managers
{
    public class CharacterEnchanterManager
    {
        private static CharacterEnchanterManager _instance;

        private Character _mainCharacter;

        private CharacterEnchanterManager()
        {
            MainCharacter = Global.Lobby.PlayersInLobby[0].ControlledCharacter;
        }

        public static CharacterEnchanterManager Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new CharacterEnchanterManager();

                return _instance;
            }
        }

        public Character MainCharacter { get => _mainCharacter; set => _mainCharacter = value; }
    }
}
