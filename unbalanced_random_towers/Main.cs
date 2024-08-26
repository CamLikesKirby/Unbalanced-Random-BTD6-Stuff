﻿using MelonLoader;
using Harmony;
using Il2CppAssets.Scripts.Unity.UI_New.InGame.Races;
using Il2CppAssets.Scripts.Simulation.Towers.Weapons;
using Il2CppAssets.Scripts.Simulation;
using Il2CppAssets.Scripts.Unity.UI_New.InGame;
using Il2CppAssets.Scripts.Unity.UI_New.Main;
using Il2CppAssets.Scripts.Simulation.Bloons;
using Il2CppAssets.Scripts.Models.Towers;

using Il2CppAssets.Scripts.Unity;



using Il2CppAssets.Scripts.Simulation.Towers;

using Il2CppAssets.Scripts.Utils;

using Il2CppSystem.Collections;
using Il2CppAssets.Scripts.Unity.UI_New.Popups;
using Il2CppAssets.Scripts.Unity.Bridge;
using Il2CppAssets.Scripts.Models.Towers.Behaviors;
using Il2CppAssets.Scripts.Simulation.Objects;
using Il2CppAssets.Scripts.Models;

using Il2CppAssets.Scripts.Models.Towers.Behaviors.Attack;
using System;
using UnityEngine;
//
using BTD_Mod_Helper.Extensions;
using Il2CppAssets.Scripts.Models.Towers.Behaviors.Abilities.Behaviors;
using Il2CppAssets.Scripts.Unity.Scenes;
using Il2CppAssets.Scripts.Models.Towers.Upgrades;
using Il2CppAssets.Scripts.Models.Towers.Behaviors.Abilities;
using Il2CppAssets.Scripts.Simulation.Towers.Projectiles.Behaviors;
using Il2CppAssets.Scripts.Models.Towers.Projectiles.Behaviors;
using BTD_Mod_Helper;
using BTD_Mod_Helper.Api.ModOptions;
using System.Text.RegularExpressions;
using Il2CppAssets.Scripts.Unity.UI_New.GameOver;
using unbalanced_random_towers;

[assembly: MelonInfo(typeof(unbalanced_random_towers.Main), unbalanced_random_towers.ModHelperData.Name, unbalanced_random_towers.ModHelperData.Version, unbalanced_random_towers.ModHelperData.RepoOwner)]
[assembly: MelonGame("Ninja Kiwi", "BloonsTD6")]
namespace unbalanced_random_towers
{
    public class Main : BloonsTD6Mod
    {

        static ModSettingDouble defaultmargin = new ModSettingDouble(1.2f)
        {
            displayName = "margin",
            isSlider = false
        };

            

        //static bool loaded = false;
        //public override void OnInGameLoaded(InGame inGame)
        public void InGameLoaded(InGame inGame)
        {
            //if (loaded) return;
            //loaded = true;
            MelonLogger.Msg("fixing costs");
            foreach (var tower in inGame.GetGameModel().towers)
            {
                if (tower.name.Contains("-"))
                {
                    float cost = tower.cost;
                    foreach (var up in tower.appliedUpgrades)
                    {
                        cost += inGame.GetGameModel().upgradesByName[up].cost;
                    }
                    tower.cost = cost;
                    MelonLogger.Msg(tower.name + " " + tower.cost);
                }
            }
            MelonLogger.Msg("building tower list");
            allTowers = new System.Collections.Generic.List<TowerModel>();
            foreach (var item in inGame.GetGameModel().towers)
            {
                if (!item.IsHero())
                {
                    //MelonLogger.Msg("added " + item.name + " " + item.cost);
                    allTowers.Add(item);
                }
                    
            }
        }


        [HarmonyPatch(typeof(SummaryScreen), "RetryLastRound")]
        public class adfsdasafa
        {

            [HarmonyPrefix]
            internal static bool Prefix()
            {
                //MelonLogger.Msg("RetryLastRound");
                timer = 0;
                return true;
            }
        }



        public override void OnApplicationStart()
        {
            base.OnApplicationStart();
            MelonLogger.Msg("unbalanced_random_towers loaded.");
        }

        static System.Collections.Generic.List<TowerModel> allTowers = new System.Collections.Generic.List<TowerModel>();

        //static Model temp; 
        static Model randomTower(float price, string orig)
        {
            MelonLogger.Msg("called randomTower with " + price + " ");
            //MelonLogger.Msg("allTowers count: " + allTowers.Count);
            if (price == 0) return null;
            allTowers.Shuffle();
            foreach (var item in allTowers)
            {
                if (item.name.ToLower().Contains("beast")) continue;
                if(item.name != orig && !Regex.IsMatch(item.name, "DartlingGunner-4..") && !Regex.IsMatch(item.name, "DartlingGunner-5.."))
                {
                    MelonLogger.Msg("new tower: " + item.name);
                    MelonLogger.Msg("new value: " + item.cost);
                    return item;
                }
            }
            MelonLogger.Msg("failed");
            return randomTower(price,orig);
        }


        [HarmonyPatch(typeof(Tower), nameof(Tower.Initialise))]
        internal class Tower_Initialise
        {

            [HarmonyPrefix]
            internal static bool Prefix(ref Tower __instance, ref Model modelToUse)
            {
                //modelToUse.Cast<TowerModel>().IsHero()
                //MelonLogger.Msg("Tower.Initialise timer:" + timer);
                if (timer < 1)
                {
                    return true;
                }
                if (Regex.IsMatch(modelToUse.name, "DartlingGunner-4..") || Regex.IsMatch(modelToUse.name, "DartlingGunner-5.."))
                {
                    return true;
                }

                try
                {
                    //MelonLogger.Msg("name: " + modelToUse.Cast<TowerModel>().name + " cost: " + modelToUse.Cast<TowerModel>().cost);
                    var temp = randomTower(modelToUse.Cast<TowerModel>().cost, modelToUse.Cast<TowerModel>().name);
                    if (temp != null)
                        modelToUse = temp;

                }
                catch (Exception e)
                {
                    MelonLogger.Msg("failed: " + e.Message);
                }
                return true;
            }
        }



        static float timer = 0;
        static bool wasLoaded = false;
        public override void OnUpdate()
        {
            base.OnUpdate();
            bool inAGame = InGame.instance != null && InGame.instance.bridge != null;
            if(!wasLoaded && inAGame)
            {
                InGameLoaded(InGame.instance);
            }
            wasLoaded = inAGame;
            if (inAGame)
            {
                timer += UnityEngine.Time.deltaTime;
            }
            else
            {
                timer = 0;
            }

        }

        public override void OnTowerUpgraded(Tower tower, string upgradeName, TowerModel newBaseTowerModel)
        {
            if (timer < 1)
            {
                return;
            }
            if (Regex.IsMatch(tower.model.name, "DartlingGunner-4..") || Regex.IsMatch(tower.model.name, "DartlingGunner-5.."))
            {
                return;
            }
            try
            {
                //MelonLogger.Msg("name: " + newBaseTowerModel.name + " cost: " + newBaseTowerModel.cost);
                var temp = randomTower(newBaseTowerModel.cost, newBaseTowerModel.name).Cast<TowerModel>();
                if (temp != null)
                    tower.SetTowerModel(temp);
                tower.SetNextTargetType();

            }
            catch (Exception e)
            {
                MelonLogger.Msg("OnTowerUpgraded failed: " + e.Message);
            }
        }





    }

}