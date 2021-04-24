﻿using System;
using BepInEx;
using BepInEx.Logging;
using BepInEx.Configuration;
using KKAPI;
using KKAPI.MainGame;
using UnityEngine;

namespace KK_DiscordRPC
{
    [BepInPlugin(GUID, "KK_DiscordRPC", Version)]
    [BepInDependency(KoikatuAPI.GUID, KoikatuAPI.VersionConst)]
    public class Koi_DiscordRPC : BaseUnityPlugin
    {
        public const string GUID = "varkaria.njaecha.DiscordRPC";
        public const string Version = "1.0";

        internal static DiscordRPC.RichPresence prsnc;
        internal new static ManualLogSource Logger;
        internal static HSceneProc hproc;

        private float CheckInterval;
        private float CoolDown;
        public long startStamp;
        internal static long currentStamp;
        private GameMode old_Gamemode;
        private ChaFile old_Character;

        private ConfigEntry<bool> configDisplayActivity;
        private ConfigEntry<string> configCustomActivityMessage;
        private ConfigEntry<bool> configDisplayTime;
        private ConfigEntry<bool> configResetTime;

        private ConfigEntry<string> configActivityMessageMaker;
        private ConfigEntry<string> configStateMessageMaker;
        private ConfigEntry<string> configBigImageMaker;

        private ConfigEntry<string> configBigImageStudio;

        private ConfigEntry<string> configBigImageMainGame;
        private ConfigEntry<string> configActivityMessageMainGame;
        private ConfigEntry<string> configStateMessageMainGame;

        private ConfigEntry<string> configBigImageHScene;
        private ConfigEntry<string> configActivityMessageHScene;
        private ConfigEntry<string> configStateMessageHScene;
        private ConfigEntry<string> configStateMessageHScene3P;
        private ConfigEntry<string> configFreeHTrue;
        private ConfigEntry<string> configFreeHFalse;
        private ConfigEntry<string> configDarknessTrue;
        private ConfigEntry<string> configDarknessFalse;

        private ConfigEntry<string> hModeAibzu;
        private ConfigEntry<string> hModeHoushi;
        private ConfigEntry<string> hModeSonyu;
        private ConfigEntry<string> hModeMasturbation;
        private ConfigEntry<string> hModePeeping;
        private ConfigEntry<string> hModeLesbian;
        private ConfigEntry<string> hModeHoushi3P;
        private ConfigEntry<string> hModeSonyu3P;
        private ConfigEntry<string> hModeHoushi3PMMF;
        private ConfigEntry<string> hModeSonyu3PMMF;

        private ConfigEntry<string> hTypeNormal;
        private ConfigEntry<string> hTypeWatching;
        private ConfigEntry<string> hTypeThreesome;

        private ConfigEntry<string> hSafetySafe;
        private ConfigEntry<string> hSafetyRisky;

        private void Awake()
        {
            Logger = base.Logger;
            GameAPI.RegisterExtraBehaviour<SceneGameController>(GUID);

            AcceptableValueBase pictures = new AcceptableValueList<string>("logo_main", "logo_main_alt", "logo_studio", "sliders");

            configDisplayActivity = Config.Bind("_General_", "Display Activity", true, "Whether or not to display your activity (Ingame, Studio or Maker).");
            configCustomActivityMessage = Config.Bind("_General_", "Custom Activity Message", "Ingame", "Message to be displayed when display Activity is turned off.");
            configDisplayTime = Config.Bind("_General_", "Display Time", true, "Whether or not to display the elapsed time.");
            configResetTime = Config.Bind("_General_", "Reset Elapsed Time", true, "Whether or not to reset the elapsed time when chaning activity and/or loading diffrent character in maker.");

            configActivityMessageMaker = Config.Bind("CharacterMaker", "Activity Message", "Maker (<maker_sex>)", "Activity message to display when in maker. Keywords: <maker_sex>");
            configStateMessageMaker = Config.Bind("CharacterMaker", "State Message", "Editing: <chara_name>", "State message to display when editing a character in maker. Keywords: <chara_name>, <chara_nickname>");
            configBigImageMaker = Config.Bind("CharacterMaker", "Image", "sliders", new ConfigDescription("Displayed image when in Maker.", pictures));

            configBigImageStudio = Config.Bind("CharaStudio", "Image", "logo_studio", new ConfigDescription("Displayed image when in CharaStudio.", pictures));

            configBigImageMainGame = Config.Bind("Main Game", "Image", "logo_main", new ConfigDescription("Displayed image when in MainGame (everything but Maker and Studio).", pictures));
            configActivityMessageMainGame = Config.Bind("Main Game", "Activity Message", "Ingame", "Actvity message to display when in main game but not H-Scene.");
            configStateMessageMainGame = Config.Bind("Main Game", "State Message", "Menu/Roaming", "State message to display when in main game but not H-Scene.");

            configBigImageHScene = Config.Bind("H-Scene", "Image", "logo_main_alt", new ConfigDescription("Displayed image when in H-Scene.", pictures));
            configActivityMessageHScene = Config.Bind("H-Scene", "Activity Message", "H-Scene <hscene_type>", "Activity message to display when in H-Scene. Keywords: <is_freeH>, <is_darkness>, <heroine_name>, <heroine_nickname>, <hscene_type>");
            configStateMessageHScene = Config.Bind("H-Scene", "State Message", "<hscene_mode> <heroine_name>", "State message to display when in H-Scene. Keywords: <hscene_mode>, <heroine_name>, <heroine_nickname>, <heroine_experience>, <heroine_safety>");
            configStateMessageHScene3P = Config.Bind("H-Scene", "State Message Two Girls", "<hscene_mode> <heroine_name> and <secondary_name>", "State message to display when in H-Scene with two Girls. Keywords: <hscene_mode>, <heroine_name>, <heroine_nickname>, <heroine_experience>, <heroine_safety>, <secondary_name>, <secondary_nickname>, <secondary_experience>, <secondary_safety> ");

            configFreeHFalse = Config.Bind("H-Scene Keywords", "FreeH False Keywordvalue", "", "Value of <is_freeH> when NOT in FreeH-Mode");
            configFreeHTrue = Config.Bind("H-Scene Keywords", "FreeH True Keywordvalue", "(Free H)", "Value of <is_freeH> when in FreeH-Mode");
            configDarknessFalse = Config.Bind("H-Scene Keywords", "Darkness False Keywordvalue", "", "Value of <is_darkness> when NOT in a darkness scene");
            configDarknessTrue = Config.Bind("H-Scene Keywords", "Darkness True Keywordvalue", "(Darkness)", "Value of <is_darkness> when in a darkness scene");

            hModeAibzu = Config.Bind("H-Scene Keywords", "Mode Caress Keywordvalue", "Caressing", "Value of <hscene_mode> when carassing in normal H-Scene");
            hModeHoushi = Config.Bind("H-Scene Keywords", "Mode Service Keywordvalue", "Serviced by", "Value of <hscene_mode> when getting serviced in normal H-Scene");
            hModeSonyu = Config.Bind("H-Scene Keywords", "Mode Penetration Keywordvalue", "Doing", "Value of <hscene_mode> when penetrating in normal H-Scene");
            hModeMasturbation = Config.Bind("H-Scene Keywords", "Mode Masturbation Keywordvalue", "Watching", "Value of <hscene_mode> when watching a masturbation H-Scene");
            hModePeeping = Config.Bind("H-Scene Keywords", "Mode Peeping Keywordvalue", "Peeping on", "Value of <hscene_mode> when peeping in Storymode");
            hModeLesbian = Config.Bind("H-Scene Keywords", "Mode Lesbian Keywordvalue", "Watching", "Value of <hscene_mode> when watching a lesbian H-Scene");
            hModeHoushi3P = Config.Bind("H-Scene Keywords", "Mode Service Threesome Keywordvalue", "Serviced by", "Value of <hscene_mode> when getting serviced in threesome H-Scene");
            hModeSonyu3P = Config.Bind("H-Scene Keywords", "Mode Penetration Threesome Keywordvalue", "Doing", "Value of <hscene_mode> when penetrating in threesome H-Scene");
            hModeHoushi3PMMF = Config.Bind("H-Scene Keywords", "Mode Service MMF Keywordvalue", "Serviced by", "Value of <hscene_mode> when getting serviced in MMF (darkness) H-Scene");
            hModeSonyu3PMMF = Config.Bind("H-Scene Keywords", "Mode Penetration MMF Keywordvalue", "Doing", "Value of <hscene_mode> when penetrating in MMF (darkness) H-Scene");

            hTypeNormal = Config.Bind("H-Scene Keywords", "Type Normal Keywordvalue", "", "Value of <hscene_type> when in a normal H-Scene (Caress, Service, Penetration)");
            hTypeWatching = Config.Bind("H-Scene Keywords", "Type Watching Keywordvalue", "(Spectating)", "Value of <hscene_type> when in a H-Scene where the player is not involved");
            hTypeThreesome = Config.Bind("H-Scene Keywords", "Type Threesome Keywordvalue", "(Threesome)", "Value of <hscene_type> when in a threesome H-Scene (Threesome, Darkness)");
            hSafetySafe = Config.Bind("H-Scene Keywords", "Safety Safe Keywordvalue", "Safe", "Value of <heroine_safety> and <secondary_safety> when the associated character is on a safe day");
            hSafetyRisky = Config.Bind("H-Scene Keywords", "Safety Risky Keywordvalue", "Risky", "Value of <heroine_safety> and <secondary_safety> when the associated character is on a risky day");

            var handlers = new DiscordRPC.EventHandlers();
            DiscordRPC.Initialize(
                "835112124295806987",
                ref handlers,
                false,
                "643270");

            startStamp = (long)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
            old_Character = null;
            currentStamp = startStamp;
            CheckInterval = 3;
            CheckStatus();
            Logger.LogInfo("Discord Rich Presence started");
        }
        private void Update()
        {
            if (CoolDown > 0)
                CoolDown -= Time.deltaTime;
            else
            {
                CoolDown = CheckInterval;
                CheckStatus();
            }
        }
        void CheckStatus()
        {
            if (configDisplayActivity.Value is true)
            {
                switch (KoikatuAPI.GetCurrentGameMode())
                {
                    case GameMode.Unknown:
                        SetStatus("Unkown Gamemode", "Koikatsu", startStamp, "main", "Unknown");
                        old_Gamemode = KoikatuAPI.GetCurrentGameMode();
                        break;
                    case GameMode.Maker:
                        MakerStatus();
                        old_Gamemode = KoikatuAPI.GetCurrentGameMode();
                        break;
                    case GameMode.Studio:
                        StudioStatus();
                        old_Gamemode = KoikatuAPI.GetCurrentGameMode();
                        break;
                    case GameMode.MainGame:
                        MainGameStatus();
                        old_Gamemode = KoikatuAPI.GetCurrentGameMode();
                        break;
                    default:
                        SetStatus("Playing something", "Koikatsu", startStamp, "logo_main", "Main game");
                        break;
                }
            }
            else SetStatus(null, configCustomActivityMessage.Value, startStamp, "logo_main", "Koikatsu");
        }
        void MakerStatus()
        {
            Boolean loaded = KKAPI.Maker.MakerAPI.InsideAndLoaded;
            if (loaded is true)
            {
                ChaFile character = KKAPI.Maker.MakerAPI.LastLoadedChaFile;
                string activity = configActivityMessageMaker.Value;
                string sex_string = "unknown";
                Int32 sex = KKAPI.Maker.MakerAPI.GetMakerSex();
                switch (sex) 
                {
                    case 0:
                        sex_string = "male";
                        break;
                    case 1:
                        sex_string = "female";
                        break;
                }
                activity = activity.Replace("<maker_sex>", sex_string);
                if (KoikatuAPI.GetCurrentGameMode() != old_Gamemode)
                {
                    currentStamp = (long)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
                }
                if (old_Character is null)
                {
                    currentStamp = (long)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
                    old_Character = character;
                }
                if (character.charaFileName != old_Character.charaFileName)
                {
                    currentStamp = (long)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
                }
                string state = configStateMessageMaker.Value;
                state = state.Replace("<chara_name>", character.parameter.fullname);
                state = state.Replace("<chara_nickname>", character.parameter.nickname);
                SetStatus(activity, state, currentStamp, configBigImageMaker.Value, "CharacterMaker");

                old_Character = character;
            }
            else
            {
                SetStatus("In Charactermaker", "Koikatsu", startStamp, configBigImageMaker.Value, "CharacterMaker");
            }
        }
        void StudioStatus()
        {
            SetStatus("In Studio", "CharaStudio", startStamp, configBigImageStudio.Value, "CharaStudio");
        }
        void MainGameStatus()
        {
            string activity = configActivityMessageMainGame.Value;
            string state = configStateMessageMainGame.Value;
            string image = configBigImageMainGame.Value;
            if (KoikatuAPI.GetCurrentGameMode() != old_Gamemode)
            {
                currentStamp = (long)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
            }
            if (KKAPI.MainGame.GameAPI.InsideHScene is true)
            {
                if (hproc)
                {
                    image = configBigImageHScene.Value;
                    activity = configActivityMessageHScene.Value;

                    HFlag hflag = hproc.flags;
                    System.Collections.Generic.List<SaveData.Heroine> females = hproc.dataH.lstFemale;
                    SaveData.Heroine heroine = females[0];
                    SaveData.Heroine secondary = null;
                    string secondaryExperience = "";
                    string secondarySafety = "";
                    string freeH = "";
                    string darkness = "";
                    string heroineExperience = "";
                    string mode = "";
                    string type = "";
                    string safety = "";
                    if (females.Count > 1)
                    {
                        state = configStateMessageHScene3P.Value;
                        secondary = females[1];
                        switch (secondary.HExperience)
                        {
                            case SaveData.Heroine.HExperienceKind.初めて:
                                secondaryExperience = "First Time";
                                break;
                            case SaveData.Heroine.HExperienceKind.不慣れ:
                                secondaryExperience = "Inexperienced";
                                break;
                            case SaveData.Heroine.HExperienceKind.慣れ:
                                secondaryExperience = "Experienced";
                                break;
                            case SaveData.Heroine.HExperienceKind.淫乱:
                                secondaryExperience = "Lewd";
                                break;
                            default:
                                secondaryExperience = "";
                                break;
                        }
                        switch (HFlag.GetMenstruation(secondary.MenstruationDay))
                        {
                            case HFlag.MenstruationType.安全日:
                                secondarySafety = hSafetySafe.Value;
                                break;
                            case HFlag.MenstruationType.危険日:
                                secondarySafety = hSafetyRisky.Value;
                                break;
                        }
                    }
                    else
                    {
                        state = configStateMessageHScene.Value;
                    }
                    
                    switch (hproc.dataH.isFreeH)
                    {
                        case true:
                            freeH = configFreeHTrue.Value;
                            break;
                        case false:
                            freeH = configFreeHFalse.Value;
                            break;
                    }
                    switch (hproc.dataH.isDarkness)
                    {
                        case true:
                            darkness = configDarknessTrue.Value;
                            break;
                        case false:
                            darkness = configDarknessFalse.Value;
                            break;
                    }
                    switch (heroine.HExperience)
                    {
                        case SaveData.Heroine.HExperienceKind.初めて:
                            heroineExperience = "First Time";
                            break;
                        case SaveData.Heroine.HExperienceKind.不慣れ:
                            heroineExperience = "Inexperienced";
                            break;
                        case SaveData.Heroine.HExperienceKind.慣れ:
                            heroineExperience = "Experienced";
                            break;
                        case SaveData.Heroine.HExperienceKind.淫乱:
                            heroineExperience = "Lewd";
                            break;
                        default:
                            heroineExperience = "";
                            break;
                    }
                    switch (hproc.flags.mode)
                    {
                        case HFlag.EMode.aibu:
                            mode = hModeAibzu.Value;
                            type = hTypeNormal.Value;
                            break;
                        case HFlag.EMode.houshi:
                            mode = hModeHoushi.Value;
                            type = hTypeNormal.Value;
                            break;
                        case HFlag.EMode.sonyu:
                            mode = hModeSonyu.Value;
                            type = hTypeNormal.Value;
                            break;
                        case HFlag.EMode.masturbation:
                            mode = hModeMasturbation.Value;
                            type = hTypeWatching.Value;
                            break;
                        case HFlag.EMode.lesbian:
                            mode = hModeLesbian.Value;
                            type = hTypeWatching.Value;
                            break;
                        case HFlag.EMode.peeping:
                            mode = hModePeeping.Value;
                            type = hTypeWatching.Value;
                            break;
                        case HFlag.EMode.houshi3P:
                            mode = hModeHoushi3P.Value;
                            type = hTypeThreesome.Value;
                            break;
                        case HFlag.EMode.sonyu3P:
                            mode = hModeSonyu3P.Value;
                            type = hTypeThreesome.Value;
                            break;
                        case HFlag.EMode.houshi3PMMF:
                            mode = hModeHoushi3PMMF.Value;
                            type = hTypeThreesome.Value;
                            break;
                        case HFlag.EMode.sonyu3PMMF:
                            mode = hModeSonyu3PMMF.Value;
                            type = hTypeThreesome.Value;
                            break;
                    }
                    switch (HFlag.GetMenstruation(heroine.MenstruationDay))
                    {
                        case HFlag.MenstruationType.安全日:
                            safety = hSafetySafe.Value;
                            break;
                        case HFlag.MenstruationType.危険日:
                            safety = hSafetyRisky.Value;
                            break;
                    }
                   
                    activity = activity.Replace("<is_freeH>", freeH);
                    activity = activity.Replace("<is_darkness>", darkness);
                    activity = activity.Replace("<heroine_name>", heroine.Name);
                    activity = activity.Replace("<heroine_nickname>", heroine.nickname);
                    activity = activity.Replace("<hscene_type>", type);

                    state = state.Replace("<heroine_name>", heroine.Name);
                    state = state.Replace("<heroine_nickname>", heroine.nickname);
                    state = state.Replace("<heroine_experience>", heroineExperience);
                    state = state.Replace("<heroine_safety>", safety);
                    if (secondary != null)
                    {
                        state = state.Replace("<secondary_name>", secondary.Name);
                        state = state.Replace("<secondary_nickname>", secondary.nickname);
                        state = state.Replace("<secondary_experience>", secondaryExperience);
                        state = state.Replace("<secondary_safety>", secondarySafety);
                    }
                    state = state.Replace("<hscene_mode>", mode);

                }
            }   
            SetStatus(activity, state, currentStamp, image, "Main Game");
        }

        void SetStatus(string activity, string state, long timestamp, string img, string img_text)
        {
            if (configResetTime.Value is false) timestamp = startStamp;
            if (configDisplayTime.Value is false) timestamp = 0;
            
            prsnc.details = activity;
            prsnc.state = state;
            prsnc.startTimestamp = timestamp;
            prsnc.largeImageKey = img;
            prsnc.largeImageText = img_text;
            prsnc.smallImageKey = null;
            prsnc.smallImageText = null;
            prsnc.partySize = 0;
            prsnc.partyMax = 0;
            DiscordRPC.UpdatePresence(ref prsnc);
        }
        private void OnStartH(HSceneProc proc, bool freeH)
        {
            Logger.LogInfo("H-Scence started");
            hproc = proc;
        }
    }
    public class SceneGameController: GameCustomFunctionController
    {
        protected override void OnStartH(HSceneProc proc, bool freeH)
        {
            Console.WriteLine("H Started");
            Koi_DiscordRPC.hproc = proc;
            Koi_DiscordRPC.currentStamp = (long)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
        }
    }
}