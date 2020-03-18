using SoulsFormats;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Irregulator
{
    class Irregulator
    {
        private readonly Random rand;

        public Irregulator(string seed)
        {
            if (seed == string.Empty)
                rand = new Random();
            else
                rand = new Random(seed.GetHashCode());
        }

        public void Randomize(Dictionary<string, PARAM64> paramDict,
            bool doArmor, bool doWeapons, bool[] weaponStyle, bool doRings, bool doGoods, bool doSpells, bool doBullets, bool bulletsPlus, bool doHumans, bool doOthers, bool doTesting)
        {
            if (doBullets)
            {
                PARAM64 param = paramDict["Bullet"];
                RandomizeAll(param.Rows, bulletsPlus);
            }

            if (doRings)
            {
                PARAM64 param = paramDict["EquipParamAccessory"];
                var rings = param.Rows.Where(row => (byte)row["accessoryCategory"].Value == 0 && row.ID < 900000);
                RandomizeSome(rings, false, "weight", "refId0");
            }

            if (doGoods)
            {
                PARAM64 goods = paramDict["EquipParamGoods"];
                var usable = goods.Rows.Where(row => (byte)row["goodsType"].Value == 0 &&           //Is a consumable (?) type good
                                              row.ID >= 240 && !(row.ID >= 712 && row.ID <= 747) && //Not boss souls or estus
                                              (int)row["refId1"].Value > 0);                        //Has an effect

                // Doing these sets of parameters together ensures an item can be used properly
                // for example, an effect that needs a text box will open that text box
                string[] goodsLockedParams = {
                    "refId1",
                    "refCategory",
                    "opmeMenuType",
                    "yesNoDialogMessageId",
                    "isEnhance",
                    "useBulletMaxNum",
                    "useHpCureMaxNum",
                    "isEnchantLeftHand",
                    "isApplySpecialEffect",
                    "refVirtualWepId",
                    "replaceItemId_BySpEffect",
                    "replaceTriggerSpEffectId",
                    "reinforceParamWeapon"
                };
                RandomizeSomeTogether(usable, goodsLockedParams);
                RandomizeSome(usable, false, "sfxVariationId", "goodsUseAnim"); // These do not affect the effect
            }

            if (doArmor)
            {
                PARAM64 param = paramDict["EquipParamProtector"];
                var valid = param.Rows.Where(row => row.ID >= 1000000);
                RandomizeSome(valid, false, "weight", "residentSpEffectId1", "residentSpEffectId2", "residentSpEffectId3", "resistPoison", "resistBlood", "resistCurse", "resistFrost", "Poise",
                    "PhysDamageCutRate", "SlashDamageCutRate", "StrikeDamageCutRate", "ThrustDamageCutRate", "MagicDamageCutRate", "FireDamageCutRate", "ThunderDamageCutRate", "DarkDamageCutRate");
            }

            if (doWeapons)
            {
                bool flatten = weaponStyle[1];  // Chose Balanced
                bool separate = weaponStyle[2]; // Chose No Shields

                RandomizeAll(paramDict["HitEffectSfxParam"].Rows);
                PARAM64 param = paramDict["EquipParamWeapon"];

                // Get rid of test and ghost stuff
                var valid = param.Rows.Where(row => row.ID >= 100000 && row.ID < 30000000 && !(row.ID >= 409900 && row.ID <= 933900));
                RandomizeSome(valid, false, "weight", "correctStrength", "correctAgility", "corretMagic", "corretFaith", "physGuardCutRate", "magGuardCutRate", "fireGuardCutRate", "thunGuardCutRate",
                    "residentSpEffectId0", "residentSpEffectId1", "residentSpEffectId2", "parryDamageLife", "atkBasePhysics", "atkBaseMagic", "atkBaseFire", "atkBaseThunder", "atkBaseDark",
                    "atkBaseStamina", "saWeaponDamage", "saDurability", "guardAngle", "staminaGuardDef", "properStrength", "properAgility", "properMagic", "properFaith", "correctLuck");
                for (int i = 0; i < 24; i++)
                    RandomizeOne<int>(valid, "HitSfx" + i, true);
                for (int i = 0; i < 8; i++)
                    RandomizeOne<int>(valid, "weaponVfx" + i, true);

                if (separate) // If wanting to remove shield movesets from the weapon's pool
                {
                    var weapons = valid.Where(row => weaponCats.Contains((byte)row["weaponCategory"].Value));
                    RandomizeSome(weapons, false, 
                        "wepmotionCategory", "guardmotionCategory", "spAtkCategory", "wepmotionOneHandId", "wepmotionBothHandId", "swordArtId", "wepAbsorpPosId");

                    var shields = valid.Where(row => shieldCats.Contains((byte)row["weapnCategory"].Value));
                    RandomizeSome(weapons, false,
                        "wepmotionCategory", "guardmotionCategory", "spAtkCategory", "wepmotionOneHandId", "wepmotionBothHandId", "swordArtId", "wepAbsorpPosId");
                }
                else
                {
                    var weapons = valid.Where(row => swingableCats.Contains((byte)row["weaponCategory"].Value));
                    RandomizeSome(weapons, flatten,
                        "wepmotionCategory", "guardmotionCategory", "spAtkCategory", "wepmotionOneHandId", "wepmotionBothHandId", "swordArtId", "wepAbsorpPosId");
                }

                var bows = valid.Where(row => bowCats.Contains((byte)row["weaponCategory"].Value));
                RandomizeSome(bows, false,
                    "wepmotionCategory", "guardmotionCategory", "spAtkCategory", "wepmotionOneHandId", "wepmotionBothHandId", "swordArtId", "wepAbsorpPosId");

                var catalysts = valid.Where(row => catalystCats.Contains((byte)row["weaponCategory"].Value));
                RandomizeSome(catalysts, false, 
                    "wepmotionCategory", "guardmotionCategory", "spAtkCategory", "wepmotionOneHandId", "wepmotionBothHandId", "swordArtId", "wepAbsorpPosId");
            }

            if (doSpells)
            {
                PARAM64 param = paramDict["Magic"];
                RandomizeSome(param.Rows, false,
                    "refIdFpCost1", "refIdSpCost1", "sfxVariationId", "slotLength", "requirementIntellect", "requirementFaith", "analogDexterityMin", "analogDexterityMax", "spEffectCategory",
                    "refType", "CastSfx1", "CastSfx2", "CastSfx3", "refIdFpCost2", "refIdSpCost2", "refIdFpCost3", "refIdSpCost3", "refIdFpCost4", "refIdSpCost4");
                RandomizePair<byte, int>(param.Rows, "refCategory1", "refId1");
                RandomizePair<byte, int>(param.Rows, "refCategory2", "refId2");
                RandomizePair<byte, int>(param.Rows, "refCategory3", "refId3");
                RandomizePair<byte, int>(param.Rows, "refCategory4", "refId4");
            }

            if (doHumans)
            {
                PARAM64 param = paramDict["CharaInitParam"];
                RandomizeSome(param.Rows, false, "equip_Helm", "equip_Armor", "equip_Gaunt", "equip_Leg", "equip_Wep_Right", "equip_Subwep_Right", "equip_Wep_Left", "equip_Subwep_Left",
                    "equip_Accessory1", "equip_Accessory2", "equip_Accessory3", "equip_Accessory4", "bodyScaleHead", "bodyScaleBreast", "BodyScaleAbdomen", "BodyScaleArm", "BodyScaleLeg");
            }

            //{
            //    PARAM64 param = paramDict["NpcParam"];
            //    foreach (var cell in param.Rows[0].Cells)
            //    {
            //        if (cell.Name != "teamType" && !cell.Name.StartsWith("ModelDispMask"))
            //            RandomizeSome(param.Rows, cell.Name);
            //    }
            //}

            if (doOthers)
            {
                //TEXTURE
                RandomizeAll(paramDict["DecalParam"].Rows); // Like bloody footprints?
                RandomizeAll(paramDict["PhantomParam"].Rows); // Phantom Texture
                RandomizeAll(paramDict["WetAspectParam"].Rows); // Liquid Colors
                RandomizeAll(paramDict["Wind"].Rows); // Wind directions and strengths?
                                                      
                RandomizeAll(paramDict["HitEffectSfxParam"].Rows);
                RandomizeAll(paramDict["ObjectMaterialSfxParam"].Rows);
                RandomizeAll(paramDict["HitEffectSfxConceptParam"].Rows);
                RandomizeAll(paramDict["ModelSfxParam"].Rows);

                //SOUND
                RandomizeAll(paramDict["FootSfxParam"].Rows);
                RandomizeAll(paramDict["HitEffectSeParam"].Rows);
                RandomizeAll(paramDict["SeMaterialConvertParam"].Rows);

                IEnumerable<PARAM64.Row> breakables = paramDict["ObjectParam"].Rows.Where(row =>
                            (short)row["HP"].Value > 0 && !(bool)row["isAnimBreak"].Value);
                RandomizeAll(breakables, true);


                //ENEMY ATTACLS
                String[] some = {"KnockbackDist", "HitStopTime", "spEffect0", "spEffect1", "spEffect2", "spEffect3", "spEffect4",
                                                 "AtkPhys", "AtkMag", "AtkFire", "AtkThun", "AtkStam", "GuardAtkRate", "GuardBreakRate", "AtkSuperArmor",
                                                 "AtkThrowEscape", "damageLevel", "mapHitType", "AtkAttribute", "atkPowForSfxSe", "atkDirForSfxSe"};
                
                PARAM64 param = paramDict["AtkParam_Npc"];
                List<long> oldIDs = new List<long>();
                oldIDs.Add(0);
                for (int i = 3; i >= 0; i--)
                {
                    String toCheck = "Hit" + i + "_Radius";

                    var xhit = param.Rows.Where(row => (float)row[toCheck].Value > 0 && !oldIDs.Contains(row.ID));
                    RandomizeSome(xhit, false, some);

                    List<long> newIDs = xhit.Select(row => (long)row.ID).ToList();
                    Console.Out.WriteLine("Randomized " + newIDs.Count + " attacks with " + (i + 1) + " hits.");

                    oldIDs.AddRange(newIDs);
                }
            }

            if (doTesting)
            {
                //TODO:
                //SHIELDS AS WEAPONS
                //SWAMP NO HURT ENEMY
                //spEffectParam, row 4001-4004, set effectTargetFriend to no
                
                
                // AFAICT There's no way to stop the ground from affecting the NPCs
                // Turn off for Ash and Stone to protect shrine at least?
                IEnumerable<PARAM64.Row> materials = paramDict["HitMtrlParam"].Rows.Where(row => !dangerFloors.Contains(row.ID));
               
                string[] effect0 = { "spEffectIdOnHit0", "spEffectIdOnHit3", "spEffectIdOnHit4", "spEffectIdOnHit5", "spEffectIdOnHit6", "spEffectIdOnHit7", "spEffectIdOnHit8", "spEffectIdOnHit9" };
                string[] effect1 = { "spEffectIdOnHit1", "newSpType0", "spEffectIdOnHit10", "spEffectIdOnHit11", "spEffectIdOnHit12", "spEffectIdOnHit13", "spEffectIdOnHit14", "spEffectIdOnHit15", "spEffectIdOnHit16" };
                string[] effect2 = { "spEffectIdOnHit2", "FootEffectHeightType0", "FootEffectHeightType1"};
                string[] unlocked = { "HitMtrlType0", "HitMtrlType1", "HitMtrlType2" };
                RandomizeSomeTogether(materials, effect0);
                RandomizeSomeTogether(materials, effect1);
                RandomizeSomeTogether(materials, effect2);
                RandomizeSome(materials, false, unlocked);

                bool done = true;

                if (done)
                {
                    

                    //BREAKABLES
                    

                    //BALANCE
                    // This block randomizes enemy attack effects
                    // but we can only randomize between attack entries with the same number of attacks
                    
                }
            }
        }

        private static byte[] weaponCats = { 0, 1, 2, 3, 4, 5, 6, 7, 9 };
        private static byte[] shieldCats = { 12 };
        private static byte[] swingableCats = { 0, 1, 2, 3, 4, 5, 6, 7, 9, 12 };
        private static byte[] bowCats = { 10, 11 };
        private static byte[] ammoCats = { 13, 14 };
        private static byte[] catalystCats = { 8 };
        private static long[] dangerFloors = { 37, 38, 39 };

        private void RandomizeOne<T>(IEnumerable<PARAM64.Row> rows, string param, bool plusMode = false)
        {
            if (plusMode)
            {
                List<T> options = rows.Select(row => (T)row[param].Value).GroupBy(val => val).Select(group => group.First()).ToList();
                foreach (PARAM64.Row row in rows)
                    row[param].Value = options.GetRandom(rand);
            }
            else
            {
                List<T> options = rows.Select(row => (T)row[param].Value).ToList();
                foreach (PARAM64.Row row in rows)
                    row[param].Value = options.PopRandom(rand);
            }
        }

        private void RandomizePair<T1, T2>(IEnumerable<PARAM64.Row> rows, string param1, string param2)
        {
            List<(T1, T2)> options = rows.Select(row => ((T1)row[param1].Value, (T2)row[param2].Value)).ToList();
            foreach (PARAM64.Row row in rows)
            {
                (T1 val1, T2 val2) = options.PopRandom(rand);
                row[param1].Value = val1;
                row[param2].Value = val2;
            }
        }

        private void RandomizeSomeTogether(IEnumerable<PARAM64.Row> rows, string[] paramNames)
        {
            List<PARAM64.Row> options = rows.ToList();
            foreach (PARAM64.Row row in rows)
            {
                PARAM64.Row sampleRow = options.PopRandom(rand);
                foreach (PARAM64.Cell cell in sampleRow.Cells)
                    Console.Out.WriteLine(cell.Name);
                foreach (string param in paramNames) 
                {
                    Console.Out.WriteLine(row[param]);
                    row[param].Value = sampleRow[param].Value;
                }
            }
        }

        private void RandomizeSome(IEnumerable<PARAM64.Row> rows, bool plusMode = false, params string[] paramNames)
        {
            foreach (string paramName in paramNames)
            {
                Console.Out.WriteLine(paramName);
                PARAM64.Cell cell = rows.First().Cells.Find(c => c.Name == paramName);

                if (cell.Type == "u8" || cell.Type == "x8")
                    RandomizeOne<byte>(rows, cell.Name, plusMode);
                else if (cell.Type == "s8")
                    RandomizeOne<sbyte>(rows, cell.Name, plusMode);
                else if (cell.Type == "u16" || cell.Type == "x16")
                    RandomizeOne<ushort>(rows, cell.Name, plusMode);
                else if (cell.Type == "s16")
                    RandomizeOne<short>(rows, cell.Name, plusMode);
                else if (cell.Type == "u32" || cell.Type == "x32")
                    RandomizeOne<uint>(rows, cell.Name, plusMode);
                else if (cell.Type == "s32")
                    RandomizeOne<int>(rows, cell.Name, plusMode);
                else if (cell.Type == "f32")
                    RandomizeOne<float>(rows, cell.Name, plusMode);
                else if (cell.Type == "b8" || cell.Type == "b32")
                    RandomizeOne<bool>(rows, cell.Name, plusMode);
                else if (cell.Type != "dummy8")
                    throw null;
            }
        }

        private void RandomizeAll(IEnumerable<PARAM64.Row> rows, bool plusMode = false)
        {
            foreach (PARAM64.Cell cell in rows.First().Cells)
            {
                if (cell.Type == "u8" || cell.Type == "x8")
                    RandomizeOne<byte>(rows, cell.Name, plusMode);
                else if (cell.Type == "s8")
                    RandomizeOne<sbyte>(rows, cell.Name, plusMode);
                else if (cell.Type == "u16" || cell.Type == "x16")
                    RandomizeOne<ushort>(rows, cell.Name, plusMode);
                else if (cell.Type == "s16")
                    RandomizeOne<short>(rows, cell.Name, plusMode);
                else if (cell.Type == "u32" || cell.Type == "x32")
                    RandomizeOne<uint>(rows, cell.Name, plusMode);
                else if (cell.Type == "s32")
                    RandomizeOne<int>(rows, cell.Name, plusMode);
                else if (cell.Type == "f32")
                    RandomizeOne<float>(rows, cell.Name, plusMode);
                else if (cell.Type == "b8" || cell.Type == "b32")
                    RandomizeOne<bool>(rows, cell.Name, plusMode);
                else if (cell.Type != "dummy8")
                    throw null;
            }
        }
    }
}
