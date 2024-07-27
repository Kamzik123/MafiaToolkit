using System;
using System.Diagnostics;
using System.Globalization;
using System.Windows.Forms;
using Toolkit.Mathematics;
using Utils.Discord;
using Utils.Logging;

namespace Utils.Settings
{
    public class ToolkitSettings
    {
        public static Matrix44 PrevMatrix = new();
        public static float PrevFloat = 0.0f;
        public static int CurrentKeyFrame = 0;
        public static int[] FloatKeyFrameIndex = new int[]
        {
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,

            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,

            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,

            //0,
            //0,
            //0,

            1,
        };

        public static string[] AssetPaths = new string[]
        {
            @"G:\Mods\Mafia II\Subway\Subway_Anims\Subway_00_Body_Anim.txt",
            @"G:\Mods\Mafia II\Subway\Subway_Anims\Subway_00_Front_Axle_Anim.txt",
            @"G:\Mods\Mafia II\Subway\Subway_Anims\Subway_00_Rear_Axle_Anim.txt",
            @"G:\Mods\Mafia II\Subway\Subway_Anims\Subway_00_Front_Door_Anim.txt",
            @"G:\Mods\Mafia II\Subway\Subway_Anims\Subway_00_Rear_Door_Anim.txt",
            @"G:\Mods\Mafia II\Subway\Subway_Anims\Subway_01_Body_Anim.txt",
            @"G:\Mods\Mafia II\Subway\Subway_Anims\Subway_01_Front_Axle_Anim.txt",
            @"G:\Mods\Mafia II\Subway\Subway_Anims\Subway_01_Rear_Axle_Anim.txt",
            @"G:\Mods\Mafia II\Subway\Subway_Anims\Subway_00_Front_Door_Anim.txt",
            @"G:\Mods\Mafia II\Subway\Subway_Anims\Subway_00_Rear_Door_Anim.txt",
            @"G:\Mods\Mafia II\Subway\Subway_Anims\Subway_02_Body_Anim.txt",
            @"G:\Mods\Mafia II\Subway\Subway_Anims\Subway_02_Front_Axle_Anim.txt",
            @"G:\Mods\Mafia II\Subway\Subway_Anims\Subway_02_Rear_Axle_Anim.txt",
            @"G:\Mods\Mafia II\Subway\Subway_Anims\Subway_00_Front_Door_Anim.txt",
            @"G:\Mods\Mafia II\Subway\Subway_Anims\Subway_00_Rear_Door_Anim.txt",

            @"G:\Mods\Mafia II\Subway\Subway_Anims\Subway_03_Body_Anim.txt",
            @"G:\Mods\Mafia II\Subway\Subway_Anims\Subway_03_Front_Axle_Anim.txt",
            @"G:\Mods\Mafia II\Subway\Subway_Anims\Subway_03_Rear_Axle_Anim.txt",
            @"G:\Mods\Mafia II\Subway\Subway_Anims\Subway_03_Front_Door_Anim.txt",
            @"G:\Mods\Mafia II\Subway\Subway_Anims\Subway_03_Rear_Door_Anim.txt",
            @"G:\Mods\Mafia II\Subway\Subway_Anims\Subway_04_Body_Anim.txt",
            @"G:\Mods\Mafia II\Subway\Subway_Anims\Subway_04_Front_Axle_Anim.txt",
            @"G:\Mods\Mafia II\Subway\Subway_Anims\Subway_04_Rear_Axle_Anim.txt",
            @"G:\Mods\Mafia II\Subway\Subway_Anims\Subway_03_Front_Door_Anim.txt",
            @"G:\Mods\Mafia II\Subway\Subway_Anims\Subway_03_Rear_Door_Anim.txt",
            @"G:\Mods\Mafia II\Subway\Subway_Anims\Subway_05_Body_Anim.txt",
            @"G:\Mods\Mafia II\Subway\Subway_Anims\Subway_05_Front_Axle_Anim.txt",
            @"G:\Mods\Mafia II\Subway\Subway_Anims\Subway_05_Rear_Axle_Anim.txt",
            @"G:\Mods\Mafia II\Subway\Subway_Anims\Subway_03_Front_Door_Anim.txt",
            @"G:\Mods\Mafia II\Subway\Subway_Anims\Subway_03_Rear_Door_Anim.txt",

            @"G:\Mods\Mafia II\Subway\Subway_Anims\Subway_06_Body_Anim.txt",
            @"G:\Mods\Mafia II\Subway\Subway_Anims\Subway_06_Front_Axle_Anim.txt",
            @"G:\Mods\Mafia II\Subway\Subway_Anims\Subway_06_Rear_Axle_Anim.txt",
            @"G:\Mods\Mafia II\Subway\Subway_Anims\Subway_06_Front_Door_Anim.txt",
            @"G:\Mods\Mafia II\Subway\Subway_Anims\Subway_06_Rear_Door_Anim.txt",
            @"G:\Mods\Mafia II\Subway\Subway_Anims\Subway_07_Body_Anim.txt",
            @"G:\Mods\Mafia II\Subway\Subway_Anims\Subway_07_Front_Axle_Anim.txt",
            @"G:\Mods\Mafia II\Subway\Subway_Anims\Subway_07_Rear_Axle_Anim.txt",
            @"G:\Mods\Mafia II\Subway\Subway_Anims\Subway_06_Front_Door_Anim.txt",
            @"G:\Mods\Mafia II\Subway\Subway_Anims\Subway_06_Rear_Door_Anim.txt",
            @"G:\Mods\Mafia II\Subway\Subway_Anims\Subway_08_Body_Anim.txt",
            @"G:\Mods\Mafia II\Subway\Subway_Anims\Subway_08_Front_Axle_Anim.txt",
            @"G:\Mods\Mafia II\Subway\Subway_Anims\Subway_08_Rear_Axle_Anim.txt",
            @"G:\Mods\Mafia II\Subway\Subway_Anims\Subway_06_Front_Door_Anim.txt",
            @"G:\Mods\Mafia II\Subway\Subway_Anims\Subway_06_Rear_Door_Anim.txt",

            @"G:\Mods\Mafia II\Subway\Subway_Anims\Subway_09_Body_Anim.txt",
            @"G:\Mods\Mafia II\Subway\Subway_Anims\Subway_09_Front_Axle_Anim.txt",
            @"G:\Mods\Mafia II\Subway\Subway_Anims\Subway_09_Rear_Axle_Anim.txt",
            @"G:\Mods\Mafia II\Subway\Subway_Anims\Subway_09_Front_Door_Anim.txt",
            @"G:\Mods\Mafia II\Subway\Subway_Anims\Subway_09_Rear_Door_Anim.txt",
            @"G:\Mods\Mafia II\Subway\Subway_Anims\Subway_10_Body_Anim.txt",
            @"G:\Mods\Mafia II\Subway\Subway_Anims\Subway_10_Front_Axle_Anim.txt",
            @"G:\Mods\Mafia II\Subway\Subway_Anims\Subway_10_Rear_Axle_Anim.txt",
            @"G:\Mods\Mafia II\Subway\Subway_Anims\Subway_09_Front_Door_Anim.txt",
            @"G:\Mods\Mafia II\Subway\Subway_Anims\Subway_09_Rear_Door_Anim.txt",
            @"G:\Mods\Mafia II\Subway\Subway_Anims\Subway_11_Body_Anim.txt",
            @"G:\Mods\Mafia II\Subway\Subway_Anims\Subway_11_Front_Axle_Anim.txt",
            @"G:\Mods\Mafia II\Subway\Subway_Anims\Subway_11_Rear_Axle_Anim.txt",
            @"G:\Mods\Mafia II\Subway\Subway_Anims\Subway_09_Front_Door_Anim.txt",
            @"G:\Mods\Mafia II\Subway\Subway_Anims\Subway_09_Rear_Door_Anim.txt",

            @"G:\Mods\Mafia II\Subway\Subway_Anims\Subway_12_Body_Anim.txt",
            @"G:\Mods\Mafia II\Subway\Subway_Anims\Subway_12_Front_Axle_Anim.txt",
            @"G:\Mods\Mafia II\Subway\Subway_Anims\Subway_12_Rear_Axle_Anim.txt",
            @"G:\Mods\Mafia II\Subway\Subway_Anims\Subway_12_Front_Door_Anim.txt",
            @"G:\Mods\Mafia II\Subway\Subway_Anims\Subway_12_Rear_Door_Anim.txt",
            @"G:\Mods\Mafia II\Subway\Subway_Anims\Subway_13_Body_Anim.txt",
            @"G:\Mods\Mafia II\Subway\Subway_Anims\Subway_13_Front_Axle_Anim.txt",
            @"G:\Mods\Mafia II\Subway\Subway_Anims\Subway_13_Rear_Axle_Anim.txt",
            @"G:\Mods\Mafia II\Subway\Subway_Anims\Subway_12_Front_Door_Anim.txt",
            @"G:\Mods\Mafia II\Subway\Subway_Anims\Subway_12_Rear_Door_Anim.txt",
            @"G:\Mods\Mafia II\Subway\Subway_Anims\Subway_14_Body_Anim.txt",
            @"G:\Mods\Mafia II\Subway\Subway_Anims\Subway_14_Front_Axle_Anim.txt",
            @"G:\Mods\Mafia II\Subway\Subway_Anims\Subway_14_Rear_Axle_Anim.txt",
            @"G:\Mods\Mafia II\Subway\Subway_Anims\Subway_12_Front_Door_Anim.txt",
            @"G:\Mods\Mafia II\Subway\Subway_Anims\Subway_12_Rear_Door_Anim.txt",

            @"G:\Mods\Mafia II\Subway\Subway_Anims\Subway_15_Body_Anim.txt",
            @"G:\Mods\Mafia II\Subway\Subway_Anims\Subway_15_Front_Axle_Anim.txt",
            @"G:\Mods\Mafia II\Subway\Subway_Anims\Subway_15_Rear_Axle_Anim.txt",
            @"G:\Mods\Mafia II\Subway\Subway_Anims\Subway_15_Front_Door_Anim.txt",
            @"G:\Mods\Mafia II\Subway\Subway_Anims\Subway_15_Rear_Door_Anim.txt",
            @"G:\Mods\Mafia II\Subway\Subway_Anims\Subway_16_Body_Anim.txt",
            @"G:\Mods\Mafia II\Subway\Subway_Anims\Subway_16_Front_Axle_Anim.txt",
            @"G:\Mods\Mafia II\Subway\Subway_Anims\Subway_16_Rear_Axle_Anim.txt",
            @"G:\Mods\Mafia II\Subway\Subway_Anims\Subway_15_Front_Door_Anim.txt",
            @"G:\Mods\Mafia II\Subway\Subway_Anims\Subway_15_Rear_Door_Anim.txt",
            @"G:\Mods\Mafia II\Subway\Subway_Anims\Subway_17_Body_Anim.txt",
            @"G:\Mods\Mafia II\Subway\Subway_Anims\Subway_17_Front_Axle_Anim.txt",
            @"G:\Mods\Mafia II\Subway\Subway_Anims\Subway_17_Rear_Axle_Anim.txt",
            @"G:\Mods\Mafia II\Subway\Subway_Anims\Subway_15_Front_Door_Anim.txt",
            @"G:\Mods\Mafia II\Subway\Subway_Anims\Subway_15_Rear_Door_Anim.txt",

            //@"G:\Mods\Mafia II\Subway\Subway_Anims\Subway_01_Body_Anim.txt",
            //@"G:\Mods\Mafia II\Subway\Subway_Anims\Subway_Camera_Anim.txt",
            //@"G:\Mods\Mafia II\Subway\Subway_Anims\Subway_Camera_Anim.txt",

            @"G:\Mods\Mafia II\Subway\Subway_Anims\Subway_Main_Sound_Intensity.txt",
        };

        public static (bool pos, bool rot, bool f)[] AssetTypes = new (bool pos, bool rot, bool f)[]
        {
            (true, true, false),
            (true, true, false),
            (true, true, false),
            (true, false, false),
            (true, false, false),
            (true, true, false),
            (true, true, false),
            (true, true, false),
            (true, false, false),
            (true, false, false),
            (true, true, false),
            (true, true, false),
            (true, true, false),
            (true, false, false),
            (true, false, false),

            (true, true, false),
            (true, true, false),
            (true, true, false),
            (true, false, false),
            (true, false, false),
            (true, true, false),
            (true, true, false),
            (true, true, false),
            (true, false, false),
            (true, false, false),
            (true, true, false),
            (true, true, false),
            (true, true, false),
            (true, false, false),
            (true, false, false),

            (true, true, false),
            (true, true, false),
            (true, true, false),
            (true, false, false),
            (true, false, false),
            (true, true, false),
            (true, true, false),
            (true, true, false),
            (true, false, false),
            (true, false, false),
            (true, true, false),
            (true, true, false),
            (true, true, false),
            (true, false, false),
            (true, false, false),

            (true, true, false),
            (true, true, false),
            (true, true, false),
            (true, false, false),
            (true, false, false),
            (true, true, false),
            (true, true, false),
            (true, true, false),
            (true, false, false),
            (true, false, false),
            (true, true, false),
            (true, true, false),
            (true, true, false),
            (true, false, false),
            (true, false, false),

            (true, true, false),
            (true, true, false),
            (true, true, false),
            (true, false, false),
            (true, false, false),
            (true, true, false),
            (true, true, false),
            (true, true, false),
            (true, false, false),
            (true, false, false),
            (true, true, false),
            (true, true, false),
            (true, true, false),
            (true, false, false),
            (true, false, false),

            (true, true, false),
            (true, true, false),
            (true, true, false),
            (true, false, false),
            (true, false, false),
            (true, true, false),
            (true, true, false),
            (true, true, false),
            (true, false, false),
            (true, false, false),
            (true, true, false),
            (true, true, false),
            (true, true, false),
            (true, false, false),
            (true, false, false),

            //(true, true, false),
            //(true, true, false),
            //(true, true, false),

            (false, false, true),

            //(false, false, false),
            //(false, false, false),
            //(false, false, false),
            //(false, false, false),
            //(false, false, false),
            //(false, false, false),
            //(false, false, false),
            //(false, false, false),
            //(false, false, false),
            //(false, false, false),
            //(false, false, false),
            //(false, false, false),
            //(false, false, false),
            //(false, false, false),
            //(false, false, false),
            //(false, false, false),
        };

        public static (float x, float y, float z)[] AssetPosOffsets = new (float x, float y, float z)[]
        {
            (0.0f, 0.0f, -0.1f),
            (0.0f, 0.0f, 0.0f),
            (0.0f, 0.0f, 0.0f),
            (0.0f, 0.0f, 0.0f),
            (0.0f, 0.0f, 0.0f),
            (0.0f, 0.0f, -0.1f),
            (0.0f, 0.0f, 0.0f),
            (0.0f, 0.0f, 0.0f),
            (0.0f, 0.0f, 0.0f),
            (0.0f, 0.0f, 0.0f),
            (0.0f, 0.0f, -0.1f),
            (0.0f, 0.0f, 0.0f),
            (0.0f, 0.0f, 0.0f),
            (0.0f, 0.0f, 0.0f),
            (0.0f, 0.0f, 0.0f),

            (0.0f, 0.0f, -0.1f),
            (0.0f, 0.0f, 0.0f),
            (0.0f, 0.0f, 0.0f),
            (0.0f, 0.0f, 0.0f),
            (0.0f, 0.0f, 0.0f),
            (0.0f, 0.0f, -0.1f),
            (0.0f, 0.0f, 0.0f),
            (0.0f, 0.0f, 0.0f),
            (0.0f, 0.0f, 0.0f),
            (0.0f, 0.0f, 0.0f),
            (0.0f, 0.0f, -0.1f),
            (0.0f, 0.0f, 0.0f),
            (0.0f, 0.0f, 0.0f),
            (0.0f, 0.0f, 0.0f),
            (0.0f, 0.0f, 0.0f),

            (0.0f, 0.0f, -0.1f),
            (0.0f, 0.0f, 0.0f),
            (0.0f, 0.0f, 0.0f),
            (0.0f, 0.0f, 0.0f),
            (0.0f, 0.0f, 0.0f),
            (0.0f, 0.0f, -0.1f),
            (0.0f, 0.0f, 0.0f),
            (0.0f, 0.0f, 0.0f),
            (0.0f, 0.0f, 0.0f),
            (0.0f, 0.0f, 0.0f),
            (0.0f, 0.0f, -0.1f),
            (0.0f, 0.0f, 0.0f),
            (0.0f, 0.0f, 0.0f),
            (0.0f, 0.0f, 0.0f),
            (0.0f, 0.0f, 0.0f),

            (0.0f, 0.0f, -0.1f),
            (0.0f, 0.0f, 0.0f),
            (0.0f, 0.0f, 0.0f),
            (0.0f, 0.0f, 0.0f),
            (0.0f, 0.0f, 0.0f),
            (0.0f, 0.0f, -0.1f),
            (0.0f, 0.0f, 0.0f),
            (0.0f, 0.0f, 0.0f),
            (0.0f, 0.0f, 0.0f),
            (0.0f, 0.0f, 0.0f),
            (0.0f, 0.0f, -0.1f),
            (0.0f, 0.0f, 0.0f),
            (0.0f, 0.0f, 0.0f),
            (0.0f, 0.0f, 0.0f),
            (0.0f, 0.0f, 0.0f),

            (0.0f, 0.0f, -0.1f),
            (0.0f, 0.0f, 0.0f),
            (0.0f, 0.0f, 0.0f),
            (0.0f, 0.0f, 0.0f),
            (0.0f, 0.0f, 0.0f),
            (0.0f, 0.0f, -0.1f),
            (0.0f, 0.0f, 0.0f),
            (0.0f, 0.0f, 0.0f),
            (0.0f, 0.0f, 0.0f),
            (0.0f, 0.0f, 0.0f),
            (0.0f, 0.0f, -0.1f),
            (0.0f, 0.0f, 0.0f),
            (0.0f, 0.0f, 0.0f),
            (0.0f, 0.0f, 0.0f),
            (0.0f, 0.0f, 0.0f),

            (0.0f, 0.0f, -0.1f),
            (0.0f, 0.0f, 0.0f),
            (0.0f, 0.0f, 0.0f),
            (0.0f, 0.0f, 0.0f),
            (0.0f, 0.0f, 0.0f),
            (0.0f, 0.0f, -0.1f),
            (0.0f, 0.0f, 0.0f),
            (0.0f, 0.0f, 0.0f),
            (0.0f, 0.0f, 0.0f),
            (0.0f, 0.0f, 0.0f),
            (0.0f, 0.0f, -0.1f),
            (0.0f, 0.0f, 0.0f),
            (0.0f, 0.0f, 0.0f),
            (0.0f, 0.0f, 0.0f),
            (0.0f, 0.0f, 0.0f),

            //(0.0f, 0.0f, 0.96f),
            //(0.0f, 0.0f, 1.0f),
            //(0.0f, 0.0f, 1.0f),

            (0.0f, 0.0f, 1.0f),
        };

        public static int CurrentAsset = 0;

        private static IniFile ini;

        //Discord keys;
        public static bool DiscordEnabled;
        public static bool DiscordElapsedTimeEnabled;
        public static bool DiscordStateEnabled;
        public static bool DiscordDetailsEnabled;
        public static string CustomStateText;

        //ModelViewer keys;
        public static bool VSync;
        public static float ScreenDepth;
        public static float ScreenNear;
        public static float CameraSpeed;
        public static string TexturePath;
        public static bool Experimental;
        public static bool bNavigation;
        public static bool UseMIPS;
        public static float FieldOfView;

        //Model Exporting keys;
        public static int Format;
        public static string ExportPath;

        //Misc vars;
        private static long ElapsedTime;
        private static DiscordController controller;
        public static bool LoggingEnabled;
        public static int Language;
        public static int SerializeSDSOption;
        public static bool bUseOodleCompression;
        public static float CompressionRatio;
        public static bool DecompileLUA;
        public static bool EnableLuaHelper;
        public static bool bBackupEnabled;
        public static bool AddTimeDataBackup;
        public static bool UseSDSToolFormat;
        public static int IndexMemorySizePerBuffer;
        public static int VertexMemorySizePerBuffer;
        public static bool CookCollisions;
        public static bool CheckForUpdates;
        public static bool SkipGameSelector;
        public static int DefaultGame;

        // Update vars
        public static float CurrentVersion = 1.0f;
        public static readonly float Version = 2.33f;

        public static void ReadINI()
        {
            // Set to invariant culture, some cultures may have issues with ini
            CultureInfo CurrentCulture = CultureInfo.CurrentCulture;
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

            ini = new IniFile();
            ElapsedTime = (int)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;

            TexturePath = ReadKey("TexturePath", "ModelViewer");
            CustomStateText = ReadKey("CustomStateText", "Discord", "Developing mods.");
            bool.TryParse(ReadKey("Enabled", "Discord", "True"), out DiscordEnabled);
            bool.TryParse(ReadKey("ElapsedTimeEnabled", "Discord", "True"), out DiscordElapsedTimeEnabled);
            bool.TryParse(ReadKey("StateEmabled", "Discord", "True"), out DiscordStateEnabled);
            bool.TryParse(ReadKey("DetailsEnabled", "Discord", "True"), out DiscordDetailsEnabled);
            int.TryParse(ReadKey("SerializeOption", "SDS", "0"), out SerializeSDSOption);
            bool.TryParse(ReadKey("UseOodleCompression", "SDS", "1"), out bUseOodleCompression);
            float.TryParse(ReadKey("CompressionRatio", "SDS", "0.9"), out CompressionRatio);
            bool.TryParse(ReadKey("VSync", "ModelViewer", "True"), out VSync);
            bool.TryParse(ReadKey("UseMIPS", "ModelViewer", "True"), out UseMIPS);
            float.TryParse(ReadKey("ScreenDepth", "ModelViewer", "10000"), out ScreenDepth);
            float.TryParse(ReadKey("ScreenNear", "ModelViewer", "1"), out ScreenNear);
            float.TryParse(ReadKey("CameraSpeed", "ModelViewer", "100"), out CameraSpeed);
            bool.TryParse(ReadKey("EnableExperimental", "ModelViewer", "0"), out Experimental);
            bool.TryParse(ReadKey("EnableNavigation", "ModelViewer", "0"), out bNavigation);
            float.TryParse(ReadKey("FieldOfView", "ModelViewer", "60"), out FieldOfView);
            bool.TryParse(ReadKey("Logging", "Misc", "True"), out LoggingEnabled);
            int.TryParse(ReadKey("Language", "Misc", "0"), out Language);
            int.TryParse(ReadKey("Format", "Exporting", "0"), out Format);
            int.TryParse(ReadKey("DefaultGame", "Misc", "0"), out DefaultGame);
            bool.TryParse(ReadKey("BackupEnabled", "SDS", "True"), out bBackupEnabled);
            bool.TryParse(ReadKey("AddTimeDataBackup", "SDS", "True"), out AddTimeDataBackup);
            bool.TryParse(ReadKey("DecompileLUA", "SDS", "False"), out DecompileLUA);
            bool.TryParse(ReadKey("UseSDSToolFormat", "SDS", "False"), out UseSDSToolFormat);
            bool.TryParse(ReadKey("CookCollisions", "SDS", "True"), out CookCollisions);
            bool.TryParse(ReadKey("EnableLUAHelper", "LUA", "False"), out EnableLuaHelper);
            bool.TryParse(ReadKey("CheckForUpdates", "Misc", "True"), out CheckForUpdates);
            bool.TryParse(ReadKey("SkipGameSelector", "Misc", "False"), out SkipGameSelector);
            int.TryParse(ReadKey("IndexMemorySizePerBuffer", "SDS", "945000"), out IndexMemorySizePerBuffer);
            int.TryParse(ReadKey("VertexMemorySizePerBuffer", "SDS", "6000000 "), out VertexMemorySizePerBuffer);
            ExportPath = ReadKey("ModelExportPath", "Directories", Application.StartupPath);
            float.TryParse(ReadKey("CurrentVersion", "Update", Version.ToString()), out CurrentVersion);

            Log.LoggingEnabled = LoggingEnabled;

            if (DiscordEnabled)
            {
                InitRichPresence();
            }

            // Reset culture
            CultureInfo.CurrentCulture = CurrentCulture;
        }

        public static string ReadKey(string key, string section, string defaultValue = null)
        {
            if (!ini.KeyExists(key, section))
            {
                ini.Write(key, defaultValue, section);
            }
            else
            {
                return ini.Read(key, section);
            }

            return defaultValue;
        }

        public static void WriteKey(string key, string section, string defaultValue)
        {
            ini.Write(key, defaultValue, section);
        }

        public static void WriteDirectoryKey(string key, string value)
        {
            WriteKey(key, "Directories", value);
        }

        public static string ReadDirectoryKey(string key)
        {
            return ReadKey(key, "Directories", "");
        }

        private static void InitRichPresence()
        {
            controller = new DiscordController();
            controller.Initialize();
            controller.presence = new DiscordRPC.RichPresence()
            {
                smallImageKey = "",
                smallImageText = "",
                largeImageKey = "main_art",
                largeImageText = "",
                startTimestamp = ElapsedTime
            };
            UpdateRichPresence("Using the Game Explorer.");
        }

        public static void UpdateRichPresence(string details = null)
        {
            if (!DiscordEnabled)
            {
                controller?.Shutdown();
                controller = null;
            }
            else
            {
                if (controller == null)
                {
                    InitRichPresence();
                }

                details = ""; //don't like current imp.
                string detailsLine = string.IsNullOrEmpty(details) ? CustomStateText : details;
                controller.presence.state = DiscordStateEnabled ? detailsLine : null;
                string vString = Debugger.IsAttached ? "DEBUG " : "RELEASE ";
                vString += Version;
                controller.presence.details = DiscordDetailsEnabled ? vString : null;
                controller.presence.startTimestamp = DiscordElapsedTimeEnabled ? ElapsedTime : 0;

                controller.UpdatePresence();
            }
        }
    }
}