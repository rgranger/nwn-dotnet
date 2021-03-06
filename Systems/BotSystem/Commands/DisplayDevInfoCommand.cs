﻿using System.Threading.Tasks;
using Discord.Commands;

namespace NWN.Systems
{
    public static partial class BotSystem
    {
        public static async Task ExecuteDisplayDevInfoCommand(SocketCommandContext context)
        {
            await context.Channel.SendMessageAsync("Développements et tâches en cours\n" +
              "    - Système d'usure du matériel + recyclage + réparation\n" +
              "    - Système de transport de matières premières\n" +
              "    - Système d'arène PvE => construire combats intéressants et équilibrés + récompenses\n" +
              "    - Maps ?\n" +
              "    - Icônes pour dons customs\n" +
              "    - Ambiance sonore du module(musiques et sons)\n" +
              "    - Réfléchir à rencontres / combats / boss / donjons intéressants\n" +
              "    - Ecrire les descriptions de choses non décrites en jeu\n" +
              "    - Création de pnjs + background + description pour animation\n\n"
              );
        }
    }
}
