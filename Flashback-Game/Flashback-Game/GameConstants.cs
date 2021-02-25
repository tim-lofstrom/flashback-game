using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Flashback_Game
{
    class GameConstants
    {
        //camera constants
        public const float CameraHeight = 25000.0f;
        public const float PlayfieldSizeX = 30000f;
        public const float PlayfieldSizeY = 46000f;
        //asteroid constants
        public const int NumAsteroids = 20;

        public const float AsteroidMinSpeed = 800.0f;
        public const float AsteroidMaxSpeed = 1600.0f;

        public const float AsteroidSpeedAdjustment = 5.0f;

        public const float AsteroidBoundingSphereScale = 0.95f;  //95% size
        public const float ShipBoundingSphereScale = 0.5f;  //50% size

        public const int NumBullets = 10;
        public const float BulletSpeedAdjustment = 200.0f;

        public const int ShotPenalty = 1;
        public const int DeathPenalty = 100;
        public const int WarpPenalty = 50;
        public const int KillBonus = 25000000;
    }
}
