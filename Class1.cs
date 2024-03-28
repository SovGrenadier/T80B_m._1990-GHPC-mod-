using System;
using MelonLoader;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using GHPC.Weapons;
using UnityEngine;
using GHPC.Camera;
using GHPC.Player;
using GHPC.Vehicle;
using GHPC.Equipment;
using GHPC.Utility;
using GHPC;
using TMPro;
using NWH;
using NWH.VehiclePhysics;
using Reticle;
using GHPC.State;
using System.Collections;
using GHPC.Equipment.Optics;
using T80B;
using System.Resources;
using GHPC.Audio;
using UnityEngine.PlayerLoop;
using BehaviorDesigner.Runtime.Tasks.Unity.UnityGameObject;
using JetBrains.Annotations;

[assembly: MelonInfo(typeof(T80B90), "T-80B (90)", "1.0.0", "Schweiz & ATLAS")]
[assembly: MelonGame("Radian Simulations LLC", "GHPC")]

namespace T80B
{
    public class T80B90 : MelonMod
    {
        WeaponSystemCodexScriptable gun_2a46m1;

        AmmoClipCodexScriptable clip_codex_agona;
        AmmoType.AmmoClip clip_agona;
        AmmoCodexScriptable ammo_codex_agona;
        static public AmmoType ammo_agona;

        AmmoClipCodexScriptable clip_codex_3bm42;
        AmmoType.AmmoClip clip_3bm42;
        AmmoCodexScriptable ammo_codex_3bm42;
        AmmoType ammo_3bm42;

        AmmoClipCodexScriptable clip_codex_3bk29;
        AmmoType.AmmoClip clip_3bk29;
        AmmoCodexScriptable ammo_codex_3bk29;
        AmmoType ammo_3bk29;

        AmmoType ammo_9m111;
        AmmoType ammo_3bm32;
        AmmoType ammo_3of26;
        AmmoType ammo_3bk14m;

        ArmorType armor_superTextolite;
        ArmorCodexScriptable armor_codex_superTextolite;

        ArmorType armor_textolite;

        GameObject[] vic_gos;
        GameObject gameManager;
        PlayerInput playerManager;

        public IEnumerator Convert(GameState _)
        {
            vic_gos = GameObject.FindGameObjectsWithTag("Vehicle");
            gameManager = GameObject.Find("_APP_GHPC_");
            playerManager = gameManager.GetComponent<PlayerInput>();

            foreach (GameObject armour in GameObject.FindGameObjectsWithTag("Penetrable"))
            {
                if (armour == null) continue;

                VariableArmor texolitePlate = armour.GetComponent<VariableArmor>();

                if (texolitePlate == null) continue;
                if (texolitePlate.Unit == null) continue;
                if (texolitePlate.Unit.FriendlyName != "T-80B") continue;
                if (texolitePlate.Name != "glacis glass textolite layers") continue;

                FieldInfo armorPlate = typeof(VariableArmor).GetField("_armorType", BindingFlags.NonPublic | BindingFlags.Instance);
                armorPlate.SetValue(texolitePlate, armor_codex_superTextolite);

                MelonLogger.Msg("Sucessfully configured hull armour.");
            }

            foreach (GameObject vic_go in vic_gos)
            {
                Vehicle vic = vic_go.GetComponent<Vehicle>();

                if (vic == null) continue;
                if (vic.FriendlyName != "T-80B") continue;

                GameObject ammo_3bm42_vis = null;
                GameObject ammo_agona_vis = null;
                GameObject ammo_3bk29_vis = null;

                // generate visual models 
                if (ammo_3bm42_vis == null)
                {
                    ammo_3bm42_vis = GameObject.Instantiate(ammo_3bm32.VisualModel);
                    ammo_3bm42_vis.name = "3BM42 visual";
                    ammo_3bm42.VisualModel = ammo_3bm42_vis;
                    ammo_3bm42.VisualModel.GetComponent<AmmoStoredVisual>().AmmoType = ammo_3bm42;
                    ammo_3bm42.VisualModel.GetComponent<AmmoStoredVisual>().AmmoScriptable = ammo_codex_3bm42;
                }

                if (ammo_agona_vis == null)
                {
                    ammo_agona_vis = GameObject.Instantiate(ammo_3of26.VisualModel);
                    ammo_agona_vis.name = "agona visual";
                    ammo_agona.VisualModel = ammo_agona_vis;
                    ammo_agona.VisualModel.GetComponent<AmmoStoredVisual>().AmmoType = ammo_agona;
                    ammo_agona.VisualModel.GetComponent<AmmoStoredVisual>().AmmoScriptable = ammo_codex_agona;
                }

                if (ammo_3bk29_vis == null)
                {
                    ammo_3bk29_vis = GameObject.Instantiate(ammo_3bk14m.VisualModel);
                    ammo_3bk29_vis.name = "3bk29 visual";
                    ammo_3bk29.VisualModel = ammo_3bk29_vis;
                    ammo_3bk29.VisualModel.GetComponent<AmmoStoredVisual>().AmmoType = ammo_3bk29;
                    ammo_3bk29.VisualModel.GetComponent<AmmoStoredVisual>().AmmoScriptable = ammo_codex_3bk29;
                }

                MelonLogger.Msg("Sucessfully loaded vis models.");

                vic._friendlyName = "T-80B (90)";

                // convert weapon system and FCS, especially big thanks to Atlas here

                LoadoutManager loadoutManager = vic.GetComponent<LoadoutManager>();
                WeaponsManager weaponsManager = vic.GetComponent<WeaponsManager>();
                WeaponSystemInfo mainGunInfo = weaponsManager.Weapons[0];
                WeaponSystem mainGun = mainGunInfo.Weapon;

                mainGun.Feed.ReloadDuringMissileTracking = true;

                MethodInfo reparent_awake = typeof(Reparent).GetMethod("Awake", BindingFlags.Instance | BindingFlags.NonPublic);
                mainGunInfo.Name = "125mm gun 2A46M-1";
                FieldInfo codex = typeof(WeaponSystem).GetField("CodexEntry", BindingFlags.NonPublic | BindingFlags.Instance);
                codex.SetValue(mainGun, gun_2a46m1);

                loadoutManager.LoadedAmmoTypes[0] = clip_codex_3bm42;
                loadoutManager.LoadedAmmoTypes[2] = clip_codex_agona;
                loadoutManager.LoadedAmmoTypes[1] = clip_codex_3bk29;

                //Improve the 1G42 Sight

                FireControlSystem fcs = vic.GetComponentInChildren<FireControlSystem>();
                UsableOptic day_optic = Util.GetDayOptic(fcs);
                UsableOptic night_optic = day_optic.slot.LinkedNightSight.PairedOptic;

                day_optic.RotateAzimuth = true;
                day_optic.slot.DefaultFov = 21.2267f;
                day_optic.slot.OtherFovs = new float[] 
                {
                    19.68f, 17.1333f,
                    15.5867f, 13.8133f, 12.2667f,
                    11.4933f, 10.72f, 9.9467f, 9.1733f, 8.4f, 7.2f, 6.5f
                };
                mainGunInfo.FCS.MainOptic.slot.VibrationBlurScale = 0.0f;
                mainGunInfo.FCS.MainOptic.slot.VibrationShakeMultiplier = 0.0f;
                fcs.gameObject.AddComponent<fcsupdate>();
                
                // Modify carousel loadout

                FieldInfo nonFixedAmmoClipCountsByRack = typeof(GHPC.Weapons.LoadoutManager).GetField("_nonFixedAmmoClipCountsByRack", BindingFlags.NonPublic | BindingFlags.Instance);
                List<int[]> AMMO = nonFixedAmmoClipCountsByRack.GetValue(loadoutManager) as List<int[]>;
                AMMO[0] = new int[] { 16, 6, 6 };
                MelonLogger.Msg(AMMO[0][0]);
                nonFixedAmmoClipCountsByRack.SetValue(loadoutManager, new List<int[]>());

                // [0] = ap [1] = heat [2] = he/atgm
                // carousel is 28 rnds 
                loadoutManager.RackLoadouts[0].AmmoCounts = new int[] { 16, 6, 6 };
                for (int i = 0; i < 2; i++)
                {
                    GHPC.Weapons.AmmoRack rack = loadoutManager.RackLoadouts[i].Rack;
                    rack.ClipTypes[0] = clip_codex_3bm42.ClipType;
                    rack.ClipTypes[1] = clip_codex_3bk29.ClipType;
                    rack.ClipTypes[2] = clip_codex_agona.ClipType;
                    Util.EmptyRack(rack);
                }
                loadoutManager.SpawnCurrentLoadout();

                PropertyInfo roundInBreech = typeof(AmmoFeed).GetProperty("AmmoTypeInBreech");
                roundInBreech.SetValue(mainGun.Feed, null);

                MethodInfo refreshBreech = typeof(AmmoFeed).GetMethod("Start", BindingFlags.Instance | BindingFlags.NonPublic);
                refreshBreech.Invoke(mainGun.Feed, new object[] { });

                MelonLogger.Msg("Configured Ammo Count.");

                // attach guidance computer
                GameObject guidance_computer_obj = new GameObject("guidance computer");
                guidance_computer_obj.transform.parent = vic.transform;
                guidance_computer_obj.AddComponent<MissileGuidanceUnit>();

                guidance_computer_obj.AddComponent<Reparent>();
                Reparent reparent = guidance_computer_obj.GetComponent<Reparent>();
                reparent.NewParent = vic_go.transform.Find("T80B_rig/HULL/TURRET").gameObject.transform;
                typeof(Reparent).GetMethod("Awake", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(reparent, new object[] { });

                MissileGuidanceUnit computer = guidance_computer_obj.GetComponent<MissileGuidanceUnit>();
                computer.AimElement = mainGunInfo.FCS.AimTransform;
                mainGun.GuidanceUnit = computer;

                MelonLogger.Msg("Success attaching Guid. Comp.");

                // update ballistics computer
                MethodInfo registerAllBallistics = typeof(LoadoutManager).GetMethod("RegisterAllBallistics", BindingFlags.Instance | BindingFlags.NonPublic);
                registerAllBallistics.Invoke(loadoutManager, new object[] { });

                MelonLogger.Msg("Sucessfully loaded mod.");
            }

            yield break;
        }

        public class fcsupdate : MonoBehaviour
        {
            void Update()
            {
                FireControlSystem fcs = GetComponent<FireControlSystem>();               

                if (fcs.CurrentAmmoType == ammo_agona)
                {
                    fcs.DynamicLead = false;
                }
                else
                {
                    fcs.DynamicLead = true;
                }

            }
        }

        
        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            if (sceneName == "MainMenu2_Scene" || sceneName == "LOADER_MENU" || sceneName == "MainMenu2-1_Scene" || sceneName == "LOADER_INITIAL" || sceneName == "t64_menu") return;

            if (ammo_3bm42 == null)
            {
                foreach (AmmoCodexScriptable s in Resources.FindObjectsOfTypeAll(typeof(AmmoCodexScriptable)))
                {
                    if (s.AmmoType.Name == "9M111 Fagot")
                    {
                        ammo_9m111 = s.AmmoType;
                    }

                    if (s.AmmoType.Name == "3BM32 APFSDS-T")
                    {
                        ammo_3bm32 = s.AmmoType;
                    }

                    if (s.AmmoType.Name == "3BK14M HEAT-FS-T")
                    {
                        ammo_3bk14m = s.AmmoType;
                    }
                    if (s.AmmoType.Name == "3OF26 HEF-FS-T")
                    {
                        ammo_3of26 = s.AmmoType;
                    }
                }

                
                foreach (ArmorCodexScriptable s in Resources.FindObjectsOfTypeAll(typeof(ArmorCodexScriptable)))
                {
                    if (s.ArmorType.Name == "glass textolite x2")
                    {
                        armor_textolite = s.ArmorType;
                    }
                }
                
                var composite_optimizations_agona = new List<AmmoType.ArmorOptimization>() { };
                var composite_optimizations_3bk29 = new List<AmmoType.ArmorOptimization>() { };

                string[] composite_names = new string[] 
                {
                    "ARAT-1 Armor Codex",
                    "BRAT-M5 Armor Codex",
                    "PRAT Armor Codex",
                    "kontakt-1 armour"
                };

                foreach (ArmorCodexScriptable s in Resources.FindObjectsOfTypeAll<ArmorCodexScriptable>())
                {
                    if (composite_names.Contains(s.name))
                    {
                        AmmoType.ArmorOptimization optimization_agona = new AmmoType.ArmorOptimization();
                        optimization_agona.Armor = s;
                        optimization_agona.RhaRatio = 0.0f;
                        composite_optimizations_agona.Add(optimization_agona);

                        AmmoType.ArmorOptimization optimization_3bk29 = new AmmoType.ArmorOptimization();
                        optimization_3bk29.Armor = s;
                        optimization_3bk29.RhaRatio = 0.0f;
                        composite_optimizations_3bk29.Add(optimization_3bk29);
                    }

                    if (composite_optimizations_agona.Count == composite_names.Length) break;
                }                
                // 2a46m-1
                gun_2a46m1 = ScriptableObject.CreateInstance<WeaponSystemCodexScriptable>();
                gun_2a46m1.name = "gun_2a46m-1";
                gun_2a46m1.CaliberMm = 125;
                gun_2a46m1.FriendlyName = "125mm Gun 2A46M-1";
                gun_2a46m1.Type = WeaponSystemCodexScriptable.WeaponType.LargeCannon;

                // 3bm42 
                ammo_3bm42 = new AmmoType();
                Util.ShallowCopy(ammo_3bm42, ammo_3bm32);
                ammo_3bm42.Name = "3BM42 Манго APFSDS-T";
                ammo_3bm42.Caliber = 125;
                ammo_3bm42.RhaPenetration = 625;
                ammo_3bm42.MuzzleVelocity = 1700f;
                ammo_3bm42.Mass = 4.85f;

                ammo_codex_3bm42 = ScriptableObject.CreateInstance<AmmoCodexScriptable>();
                ammo_codex_3bm42.AmmoType = ammo_3bm42;
                ammo_codex_3bm42.name = "ammo_3bm42";

                clip_3bm42 = new AmmoType.AmmoClip
                {
                    Capacity = 1,
                    Name = "3BM42 Манго APFSDS-T",
                    MinimalPattern = new AmmoCodexScriptable[1]
                };
                clip_3bm42.MinimalPattern[0] = ammo_codex_3bm42;

                clip_codex_3bm42 = ScriptableObject.CreateInstance<AmmoClipCodexScriptable>();
                clip_codex_3bm42.name = "clip_3bm42";
                clip_codex_3bm42.ClipType = clip_3bm42;

                // agona AT missile
                ammo_agona = new AmmoType();
                Util.ShallowCopy(ammo_agona, ammo_9m111);
                ammo_agona.Name = "9M128 Агона BLATGM";
                ammo_agona.Caliber = 125;
                ammo_agona.RhaPenetration = 775000;
                ammo_agona.MuzzleVelocity = 350;
                ammo_agona.Mass = 23.2f;
                ammo_agona.ArmingDistance = 75;
                ammo_agona.DetonateSpallCount = 20;
                ammo_agona.SpiralPower = 0f;
                ammo_agona.TntEquivalentKg = 4.6f;
                ammo_agona.TurnSpeed = 0.15f;
                ammo_agona.SpiralAngularRate = 0f;
                ammo_agona.RangedFuseTime = 12.5f;
                ammo_agona.MaximumRange = 5000;
                ammo_agona.MaxSpallRha = 18f;
                ammo_agona.MinSpallRha = 1f;
                ammo_agona.SpallMultiplier = 6;
                ammo_agona.ArmorOptimizations = composite_optimizations_agona.ToArray();
                ammo_agona.CertainRicochetAngle = 3f;
                ammo_agona.ShotVisual = ammo_9m111.ShotVisual;
                ammo_agona.Guidance = AmmoType.GuidanceType.Saclos;

                ammo_codex_agona = ScriptableObject.CreateInstance<AmmoCodexScriptable>();
                ammo_codex_agona.AmmoType = ammo_agona;
                ammo_codex_agona.name = "ammo_agona";

                clip_agona = new AmmoType.AmmoClip
                {
                    Capacity = 1,
                    Name = "9M128 Агона BLATGM",
                    MinimalPattern = new AmmoCodexScriptable[1]
                };
                clip_agona.MinimalPattern[0] = ammo_codex_agona;

                clip_codex_agona = ScriptableObject.CreateInstance<AmmoClipCodexScriptable>();
                clip_codex_agona.name = "clip_agona";
                clip_codex_agona.ClipType = clip_agona;

                // 3bk29

                ammo_3bk29 = new AmmoType();
                Util.ShallowCopy(ammo_3bk29, ammo_3bk14m);
                ammo_3bk29.Name = "3BK29 Брейк HEATFS";
                ammo_3bk29.Caliber = 125;
                ammo_3bk29.RhaPenetration = 800f;
                ammo_3bk29.MuzzleVelocity = 1200;
                ammo_3bk29.Mass = 22.0f;
                ammo_3bk29.TntEquivalentKg = 3.4f;
                ammo_3bk29.CertainRicochetAngle = 0f;
                ammo_3bk29.MaxSpallRha = 15f;
                ammo_3bk29.MinSpallRha = 1f;
                ammo_3bk29.SpallMultiplier = 1.35f;
                ammo_3bk29.ArmorOptimizations = composite_optimizations_3bk29.ToArray();

                ammo_codex_3bk29 = ScriptableObject.CreateInstance<AmmoCodexScriptable>();
                ammo_codex_3bk29.AmmoType = ammo_3bk29;
                ammo_codex_3bk29.name = "ammo_3bk29";

                clip_3bk29 = new AmmoType.AmmoClip
                {
                    Capacity = 1,
                    Name = "3BK29 Брейк HEATFS",
                    MinimalPattern = new AmmoCodexScriptable[1]
                };
                clip_3bk29.MinimalPattern[0] = ammo_codex_3bk29;

                clip_codex_3bk29 = ScriptableObject.CreateInstance<AmmoClipCodexScriptable>();
                clip_codex_3bk29.name = "clip_3bk29";
                clip_codex_3bk29.ClipType = clip_3bk29;

                MelonLogger.Msg("Sucessfully defined ammunition stats.");
             
                armor_superTextolite = new ArmorType();
                Util.ShallowCopy(armor_superTextolite, armor_textolite);
                armor_superTextolite.RhaeMultiplierCe = 2.6f; //2.6
                armor_superTextolite.RhaeMultiplierKe = 1.8f; //1.8
                armor_superTextolite.Name = "super textolite";

                armor_codex_superTextolite = ScriptableObject.CreateInstance<ArmorCodexScriptable>();
                armor_codex_superTextolite.name = "super textolite";
                armor_codex_superTextolite.ArmorType = armor_superTextolite;
                
            }

            StateController.RunOrDefer(GameState.GameReady, new GameStateEventHandler(Convert), GameStatePriority.Lowest);
        }
    }
}