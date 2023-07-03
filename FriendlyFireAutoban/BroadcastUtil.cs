using PluginAPI.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FriendlyFireAutoban
{
    using Server = PluginAPI.Core.Server;

    internal class BroadcastUtil
    {

        public static void ServerBroadcast(string message, ushort duration = 5)
        {
            Server.SendBroadcast(message, duration);
        }

        public static void MapBroadcast(string message, ushort duration = 5)
        {
            //Map.Broadcast(new Exiled.API.Features.Broadcast(message, duration), false);
            Map.Broadcast(duration, message);
        }

        public static void PlayerBroadcast(Player player, string message, ushort duration = 5)
        {
            //player.Broadcast(new Exiled.API.Features.Broadcast(message, 10), true);
            player.SendBroadcast(message, duration);
        }
    }
}
