using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Resistance.Core
{
    public static class Rule
    {

        public static RoleCount GetRoleCount(int playerCount)
        {
            switch (playerCount)
            {
                case 5:
                    return new RoleCount(3, 2);
                case 6:
                    return new RoleCount(4, 2);
                case 7:
                    return new RoleCount(4, 3);
                case 8:
                    return new RoleCount(5, 3);
                case 9:
                    return new RoleCount(6, 3);
                case 10:
                    return new RoleCount(7, 3);
                default:
                    return new RoleCount(playerCount, 0);
            }
        }

        public static int SelectMemberCount(int playerCount, int stage)
        {
            switch (playerCount)
            {
                case 5:
                    switch (stage)
                    {
                        case 1:
                            return 2;
                        case 2:
                            return 3;
                        case 3:
                            return 2;
                        case 4:
                            return 3;
                        case 5:
                            return 3;
                        default:
                            return playerCount;
                    }
                case 6:
                    switch (stage)
                    {
                        case 1:
                            return 2;
                        case 2:
                            return 3;
                        case 3:
                            return 4;
                        case 4:
                            return 3;
                        case 5:
                            return 4;
                        default:
                            return playerCount;
                    }
                case 7:
                    switch (stage)
                    {
                        case 1:
                            return 2;
                        case 2:
                            return 3;
                        case 3:
                            return 3;
                        case 4:
                            return 3;
                        case 5:
                            return 4;
                        default:
                            return playerCount;
                    }
                case 8:
                case 9:
                case 10:
                    switch (stage)
                    {
                        case 1:
                            return 3;
                        case 2:
                            return 4;
                        case 3:
                            return 4;
                        case 4:
                            return 5;
                        case 5:
                            return 5;
                        default:
                            return playerCount;
                    }
                default:
                    return playerCount;
            }
        }

        public static bool ResultDetection(int playerCount, int stage, int failCount)
        {
            switch (playerCount)
            {
                case 7:
                case 8:
                case 9:
                case 10:
                    switch (stage)
                    {
                        case 4:
                            return failCount <= 1;
                        default:
                            return failCount == 0;
                    }
                default:
                    return failCount == 0;
            }
        }

        public static int DrowCardCount(int playerCount)
        {
            switch (playerCount)
            {
                case 5:
                case 6:
                    return 1;

                case 7:
                case 8:
                case 9:
                    return 2;

                case 10:
                    return 3;

                default:
                    return 0;
            }
        }

    }
}
