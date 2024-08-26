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
using System.Text.RegularExpressions;
using Il2CppAssets.Scripts.Unity.UI_New.InGame.Stats;
using Il2CppAssets.Scripts.Unity.UI_New.GameOver;

[assembly: MelonInfo(typeof(unbalanced_random_towers_and_projectiles.Main), unbalanced_random_towers_and_projectiles.ModHelperData.Name, unbalanced_random_towers_and_projectiles.ModHelperData.Version, unbalanced_random_towers_and_projectiles.ModHelperData.RepoOwner)]
[assembly: MelonGame("Ninja Kiwi", "BloonsTD6")]
namespace unbalanced_random_towers_and_projectiles
{
    public class Main : BloonsTD6Mod
    {

        //static TextMeshProUGUI infoDisplay;
        static GameObject upgradeTreeButton;
        static TowerToSimulation lastSelected;
        static System.Collections.Generic.List<TowerModel> allTowers = new System.Collections.Generic.List<TowerModel>();
        static string toWrite = "Info";


        public override void OnApplicationStart()
        {
            base.OnApplicationStart();
            Console.WriteLine("unbalanced_random_towers_and_projectiles loaded.");
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

        static void Write(string t)
        {
            //toWrite = t;
            if (upgradeTreeButton == null)
            {
                //upgradeTreeButton = GameObject.Find("UpgradeTreeButton");
                upgradeTreeButton = GameObject.Find("RoundPanel");
                //upgradeTreeButton = GameObject.Find("TowerSelected");

            }
            //Console.WriteLine("upgradeTreeButton: " + (upgradeTreeButton==null));
            if (upgradeTreeButton != null)
            {
                //var pos = upgradeTreeButton.transform.position;
                //upgradeTreeButton.transform.position = new Vector3(1000f, pos.y, pos.z);
                if (t.Contains("-"))
                {
                    //upgradeTreeButton.GetComponentInChildren<TextMeshProUGUI>().text = t.Substring(t.Length - 3) + "-" + t.Substring(0,t.Length - 3);
                } else
                {
                    //upgradeTreeButton.GetComponentInChildren<TextMeshProUGUI>().text = t;
                }
                
                //upgradeTreeButton.GetComponentInChildren<NK_TextMeshProUGUI>().text = t;
            }

        }

        static bool wasLoaded = false;
        static float timer = 0;

        public override void OnUpdate()
        {
            base.OnUpdate();

            bool inAGame = InGame.instance != null && InGame.instance.bridge != null;

            if (inAGame)
            {
                timer += UnityEngine.Time.deltaTime;
            }
            else
            {
                timer = 0;
            }

            if (!wasLoaded && inAGame)
            {
                InGameLoaded(InGame.instance);
            }
            wasLoaded = inAGame;

            if (inAGame)
            {

                if (InGame.instance.inputManager.SelectedTower != null)
                {
                    //Console.WriteLine("writing:");
                    try
                    {
                        lastSelected = InGame.instance.inputManager.SelectedTower;
                        var a = lastSelected.tower.model.Cast<TowerModel>().GetBehavior<AttackModel>().weapons[0].projectile.name;
                        //Console.WriteLine(a);
                        if(!a.ToLower().Contains("projectilemodel"))
                            Write(a);
                    }
                    catch(Exception e)
                    {
                        //Write(e.Message);
                        Write("Round");
                    }
                }
                else
                {
                    Write("Round");
                }



            }
        }


        static string[] blacklist =
        {
            "EngineerMonkey",
            "SniperMonkey",
            "BananaFarm",
        };

        //static Model temp;
        static Model randomTower(float price, string orig)
        {
            //Console.WriteLine("called randomTower with " + price + " " + margin);
            if (price == 0) return null;
            allTowers.Shuffle();
            foreach (var item in allTowers)
            {
                if (item.name.ToLower().Contains("beast")) continue;
                if (item.name != orig && !blacklist.Any(item.name.Contains) && !Regex.IsMatch(item.name, "DartlingGunner-4..") && !Regex.IsMatch(item.name, "DartlingGunner-5.."))                                      
                {
                    Console.WriteLine("new value: " + item.cost);
                    return item;
                }
            }
            //Console.WriteLine("failed");
            return randomTower(price,orig);
        }


        [HarmonyPatch(typeof(Tower), nameof(Tower.Initialise))]
        internal class Tower_Initialise
        {

            [HarmonyPrefix]
            internal static bool Prefix(ref Tower __instance, ref Model modelToUse)
            {
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
                    //random tower
                    var temp = randomTower(modelToUse.Cast<TowerModel>().cost, modelToUse.Cast<TowerModel>().name).Duplicate();
                    if (temp != null)
                        modelToUse = temp;

                    //random projectile
                    //modelToUse = modelToUse.Duplicate();
                    temp = randomTower(modelToUse.Cast<TowerModel>().cost, modelToUse.Cast<TowerModel>().name);
                    //Console.WriteLine("name: " + modelToUse.Cast<TowerModel>().name + " new name: " + temp.Cast<TowerModel>().name);
                    var newproj = temp.Cast<TowerModel>().GetBehavior<AttackModel>().weapons[0].projectile;
                    if (temp != null)
                        foreach(var attackmodel in modelToUse.Cast<TowerModel>().GetBehaviors<AttackModel>())
                        {
                            foreach(var weapon in attackmodel.weapons)
                            {
                                weapon.projectile = newproj.Duplicate();
                                weapon.projectile.name = temp.Cast<TowerModel>().name;
                            }
                        }

                }
                catch (Exception e)
                {
                    Console.WriteLine("failed: " + e.Message);
                }
                return true;
            }
        }



        public void InGameLoaded(InGame inGame)
        {
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
                    //Console.WriteLine(tower.name + " " + tower.cost);
                }
            }

            allTowers = new System.Collections.Generic.List<TowerModel>();
            foreach (var item in inGame.GetGameModel().towers)
            {
                if (!item.IsHero())
                {
                    //Console.WriteLine("added " + item.name + " " + item.cost);
                    allTowers.Add(item);
                }

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
                //random tower
                var modelToUse = randomTower(newBaseTowerModel.cost, newBaseTowerModel.name).Cast<TowerModel>().Duplicate();


                //random projectile
                var random = randomTower(modelToUse.cost, modelToUse.name).Cast<TowerModel>().Duplicate();

                var newproj = random.Cast<TowerModel>().GetBehavior<AttackModel>().weapons[0].projectile;
                if (random != null)
                    foreach (var attackmodel in modelToUse.Cast<TowerModel>().GetBehaviors<AttackModel>())
                    {
                        foreach (var weapon in attackmodel.weapons)
                        {
                            weapon.projectile = newproj.Duplicate();
                            weapon.projectile.name = random.Cast<TowerModel>().name;
                        }
                    }



                if (random != null)
                    tower.SetTowerModel(modelToUse);
                tower.SetNextTargetType();

            }
            catch (Exception e)
            {
                Console.WriteLine("OnTowerUpgraded failed: " + e.Message);
            }
        }





    }

}