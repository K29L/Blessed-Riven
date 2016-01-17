﻿using System;
using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Constants;
using EloBuddy.SDK.Enumerations;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;
using EloBuddy.SDK.Rendering;
using SharpDX;

namespace BlessedRiven
{
    class Program
    {
        private const string IsFirstR = "RivenFengShuiEngine";
        public static bool EnableR;
        private const string IsSecondR = "rivenizunablade";
        public static Menu menu, combo, clear, harass, misc, draw, Skin;
        private static readonly AIHeroClient _Player = ObjectManager.Player;
        private static SpellDataInst Flash;
        public static Spell.Active Q = new Spell.Active(SpellSlot.Q);
        public static Spell.Active E = new Spell.Active(SpellSlot.E, 325);

        public static Spell.Skillshot R = new Spell.Skillshot(SpellSlot.R, 900, SkillShotType.Cone, 250, 1600, 45)
        {
            MinimumHitChance = HitChance.High,
            AllowedCollisionCount = -1
        };
        public static int QStack = 1;
        private static bool forceQ;
        private static bool forceW;
        private static bool forceR;
        private static bool forceR2;
        private static bool forceItem;
        private static float LastQ;
        private static float LastW;
        private static float LastR;
        private static AttackableUnit QTarget;
        public static Item Hydra;
        


        public static Spell.Active W = new Spell.Active(SpellSlot.W,(uint)(70 + ObjectManager.Player.BoundingRadius + 120));

        public static uint WRange
        {
            get
            {
                return (uint)
                        (70 + ObjectManager.Player.BoundingRadius +
                         (ObjectManager.Player.HasBuff("RivenFengShuiEngine") ? 195 : 120));
            }
        }

        private static bool Dind
        {
            get { return draw["Dind"].Cast<CheckBox>().CurrentValue; }
        }

        private static bool DrawCB
        {
            get { return draw["DrawCB"].Cast<CheckBox>().CurrentValue; }
        }

        private static bool KillstealW
        {
            get { return misc["killstealw"].Cast<CheckBox>().CurrentValue; }
        }

        private static bool KillstealR
        {
            get { return misc["killstealr"].Cast<CheckBox>().CurrentValue; }
        }

        private static bool DrawAlwaysR
        {
            get { return draw["DrawAlwaysR"].Cast<CheckBox>().CurrentValue; }
        }

        private static bool DrawUseFastCombo
        {
            get { return draw["DrawUseFastCombo"].Cast<CheckBox>().CurrentValue; }
        }

        private static bool DrawFH
        {
            get { return draw["DrawFH"].Cast<CheckBox>().CurrentValue; }
        }

        private static bool DrawHS
        {
            get { return draw["DrawHS"].Cast<CheckBox>().CurrentValue; }
        }

        private static bool DrawBT
        {
            get { return draw["DrawBT"].Cast<CheckBox>().CurrentValue; }
        }

        private static bool UseFastCombo
        {
            get { return combo["UseFastCombo"].Cast<KeyBind>().CurrentValue; }
        }

        private static bool BurstCombo
        {
            get { return harass["burst"].Cast<KeyBind>().CurrentValue; }
        }
        private static bool FastHarassCombo
        {
            get { return harass["fastharass"].Cast<KeyBind>().CurrentValue; }
        }

        private static bool AlwaysR
        {
            get { return combo["AlwaysR"].Cast<KeyBind>().CurrentValue; }
        }

        private static bool AutoShield
        {
            get { return misc["AutoShield"].Cast<CheckBox>().CurrentValue; }
        }

        private static bool Shield
        {
            get { return misc["Shield"].Cast<CheckBox>().CurrentValue; }
        }

        private static bool KeepQ
        {
            get { return misc["KeepQ"].Cast<CheckBox>().CurrentValue; }
        }

        private static int QD
        {
            get { return misc["QD"].Cast<Slider>().CurrentValue; }
        }

        private static int QLD
        {
            get { return misc["QLD"].Cast<Slider>().CurrentValue; }
        }

        private static int AutoW
        {
            get { return misc["AutoW"].Cast<Slider>().CurrentValue; }
        }

        private static bool ComboW
        {
            get { return combo["ComboW"].Cast<CheckBox>().CurrentValue; }
        }

        private static bool RMaxDam
        {
            get { return misc["RMaxDam"].Cast<CheckBox>().CurrentValue; }
        }

        private static bool RKillable
        {
            get { return combo["RKillable"].Cast<CheckBox>().CurrentValue; }
        }

        private static int LaneW
        {
            get { return clear["LaneW"].Cast<Slider>().CurrentValue; }
        }

        private static bool LaneE
        {
            get { return clear["LaneE"].Cast<CheckBox>().CurrentValue; }
        }

        private static bool WInterrupt
        {
            get { return misc["WInterrupt"].Cast<CheckBox>().CurrentValue; }
        }

        private static bool Qstrange
        {
            get { return misc["Qstrange"].Cast<CheckBox>().CurrentValue; }
        }

        private static bool FirstHydra
        {
            get { return misc["FirstHydra"].Cast<CheckBox>().CurrentValue; }
        }

        private static bool LaneQ
        {
            get { return clear["LaneQ"].Cast<CheckBox>().CurrentValue; }
        }

        private static bool Youmu
        {
            get { return misc["youmu"].Cast<CheckBox>().CurrentValue; }
        }


        private static void Main()
        {
            Loading.OnLoadingComplete += OnGameLoad;
        }

        public static void LaneClearAfterAa(Obj_AI_Base target)
        {
            var unitHp = target.Health - Player.Instance.GetAutoAttackDamage(target, true);
            if (unitHp > 0)
            {
                if (clear["LaneQ"].Cast<CheckBox>().CurrentValue && Q.IsReady())
                {
                    Player.CastSpell(SpellSlot.Q, target.Position);
                    return;
                }
                if (clear["LaneW"].Cast<CheckBox>().CurrentValue && W.IsReady() &&
                    W.IsInRange(target))
                {
                    Player.CastSpell(SpellSlot.W);
                    return;
                }
            }
            else
            {
                System.Collections.Generic.List<Obj_AI_Minion> minions = EntityManager.MinionsAndMonsters.GetLaneMinions(EntityManager.UnitTeam.Enemy,
                    Player.Instance.Position, Q.Range).Where(a => a.NetworkId != target.NetworkId).ToList();
                if (clear["LaneQ"].Cast<CheckBox>().CurrentValue && Q.IsReady() && minions.Any())
                {
                    Player.CastSpell(SpellSlot.Q, minions[0].Position);
                    return;
                }
                minions = minions.Where(a => a.Distance(Player.Instance) < W.Range).ToList();
                if (clear["LaneW"].Cast<CheckBox>().CurrentValue && W.IsReady() &&
                    W.IsInRange(target) && minions.Any())
                {
                    Player.CastSpell(SpellSlot.W);
                    return;
                }
            }
        }

        //private static void Main() => CustomEvents.Game.OnGameLoad += OnGameLoad;

        public static void ComboAfterAa(Obj_AI_Base target)
        {
            if (Player.Instance.HasBuff("RivenFengShuiEngine") && R.IsReady() &&
                combo["AlwaysR"].Cast<CheckBox>().CurrentValue)
            {
                if (Player.Instance.CalculateDamageOnUnit(target, DamageType.Physical, DamageHandler.RDamage(target)) + Player.Instance.GetAutoAttackDamage(target, true) > target.Health)
                {
                    R.Cast(target);
                    return;
                }
            }
            if (combo["ComboW"].Cast<CheckBox>().CurrentValue && W.IsReady() &&
                W.IsInRange(target))
            {
                if (Hydra != null && Hydra.IsReady())
                {
                    Hydra.Cast();
                    return;
                }
                Player.CastSpell(SpellSlot.W);
                return;
            }
            if (combo["ComboQ"].Cast<CheckBox>().CurrentValue && Q.IsReady())
            {
                Player.CastSpell(SpellSlot.Q, target.Position);
                return;
            }
            if (Hydra != null && Hydra.IsReady())
            {
                Hydra.Cast();
            }
        }

        private static void OnGameLoad(EventArgs args)
        {
            Hacks.RenderWatermark = false;
            if (_Player.ChampionName != "Riven") return;
            Flash = ObjectManager.Player.Spellbook.Spells.FirstOrDefault(a => a.Name.ToLower().Contains("summonerflash"));
            Chat.Print("Blessed Riven Loaded", Color.Firebrick);

            OnMenuLoad();

            Game.OnUpdate += OnGameUpdate;

            Game.OnTick += OnTick;
            Drawing.OnDraw += Drawing_OnDraw;
            //todo: add damage indicator
            Drawing.OnEndScene += Drawing_OnEndScene;
            Obj_AI_Base.OnProcessSpellCast += OnCast;
            Obj_AI_Base.OnSpellCast += OnDoCast;
            Obj_AI_Base.OnPlayAnimation += OnPlay;
        }

        //Interrupter2.OnInterruptableTarget += Interrupt;

        
        private static bool HasTitan()
        {
            //(Items.HasItem(3748) && Items.CanUseItem(3748));
            var id = ObjectManager.Player.InventoryItems.FirstOrDefault(a => a.Id == (ItemId) 3748);
            if (id == null || !((new Item(id.Id, 300)).IsReady()))
            {
                return false;
            }
            return true;
        }

        private static void OnGameUpdate(EventArgs args)
        {
            if (checkSkin())
            {
                Player.SetSkinId(SkinId());
            }
        }

        public static int SkinId()
        {
            return Skin["skin.Id"].Cast<Slider>().CurrentValue;
        }
        public static bool checkSkin()
        {
            return Skin["checkSkin"].Cast<CheckBox>().CurrentValue;
        }

        private static void CastTitan()
        {
            var id = ObjectManager.Player.InventoryItems.FirstOrDefault(a => a.Id == (ItemId) 3748);
            if (id != null)
            {
                var HydraItem = new Item(id.Id, 300);
                if (HydraItem.IsReady())
                {
                    HydraItem.Cast();
                    Orbwalker.ResetAutoAttack();
                }
            }
        }
        private static readonly float _barLength = 104;
        private static readonly float _xOffset = 2;
        private static readonly float _yOffset = 9;
        private static void Drawing_OnEndScene(EventArgs args)
        {
            if (_Player.IsDead)
                return;
            if (!Dind) return;
            foreach (var aiHeroClient in EntityManager.Heroes.Enemies)
            {
                if (!aiHeroClient.IsHPBarRendered || !aiHeroClient.VisibleOnScreen) continue;

                var pos = new Vector2(aiHeroClient.HPBarPosition.X + _xOffset, aiHeroClient.HPBarPosition.Y + _yOffset);
                var fullbar = (_barLength) * (aiHeroClient.HealthPercent / 100);
                var damage = (_barLength) *
                                 ((getComboDamage(aiHeroClient) / aiHeroClient.MaxHealth) > 1
                                     ? 1
                                     : (getComboDamage(aiHeroClient) / aiHeroClient.MaxHealth));
                Line.DrawLine(System.Drawing.Color.Aqua, 9f, new Vector2(pos.X, pos.Y),
                    new Vector2(pos.X + (damage > fullbar ? fullbar : damage), pos.Y));
                Line.DrawLine(System.Drawing.Color.Black, 9, new Vector2(pos.X + (damage > fullbar ? fullbar : damage) - 2, pos.Y), new Vector2(pos.X + (damage > fullbar ? fullbar : damage) + 2, pos.Y));
            }
        }


        
        private static Item HasHydra()
        {

            var hydraId =
                ObjectManager.Player.InventoryItems.FirstOrDefault(
                    it => it.Id == ItemId.Ravenous_Hydra_Melee_Only || it.Id == ItemId.Tiamat_Melee_Only);
            if (hydraId != null)
            {
                return new Item(hydraId.Id, 300);
            }
            return null;
        }

        //private static  => Items.CanUseItem(3077) && Items.HasItem(3077) ? 3077 : Items.CanUseItem(3074) && Items.HasItem(3074) ? 3074 : 0;
        private static void OnDoCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (!sender.IsMe) return;
            var target = args.Target as Obj_AI_Base;

            // Hydra
            if (args.SData.Name.ToLower().Contains("itemtiamatcleave"))
            {
                Orbwalker.ResetAutoAttack();
                if (W.IsReady())
                {
                    var target2 = TargetSelector.GetTarget(W.Range, DamageType.Physical);
                    if (target2 != null || Orbwalker.ActiveModesFlags != Orbwalker.ActiveModes.None)
                    {
                        Player.CastSpell(SpellSlot.W);
                    }
                }
                return;
            }

            //W
            if (args.SData.Name.ToLower().Contains(W.Name.ToLower()))
            {
                if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo))
                {
                    if (Player.Instance.HasBuff("RivenFengShuiEngine") && R.IsReady() &&
                        combo["AlwaysR"].Cast<CheckBox>().CurrentValue)
                    {
                        var target2 = TargetSelector.GetTarget(R.Range, DamageType.Physical);
                        if (target2 != null &&
                            (target2.Distance(Player.Instance) < W.Range &&
                             target2.Health >
                             Player.Instance.CalculateDamageOnUnit(target2, DamageType.Physical, DamageHandler.WDamage()) ||
                             target2.Distance(Player.Instance) > W.Range) &&
                            Player.Instance.CalculateDamageOnUnit(target2, DamageType.Physical,
                                DamageHandler.RDamage(target2) + DamageHandler.WDamage()) > target2.Health)
                        {
                            R.Cast(target2);
                        }
                    }
                }

                target = (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo) ||
                          Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Harass))
                    ? TargetSelector.GetTarget(E.Range + W.Range, DamageType.Physical)
                    : (Obj_AI_Base)Orbwalker.LastTarget;
                if (Q.IsReady() && Orbwalker.ActiveModesFlags != Orbwalker.ActiveModes.None && target != null)
                {
                    Player.CastSpell(SpellSlot.Q, target.Position);
                    return;
                }
                return;
            }

            //E
            if (args.SData.Name.ToLower().Contains(E.Name.ToLower()))
            {
                if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo))
                {
                    if (Player.Instance.HasBuff("RivenFengShuiEngine") && R.IsReady() &&
                        combo["AlwaysR"].Cast<CheckBox>().CurrentValue)
                    {
                        var target2 = TargetSelector.GetTarget(R.Range, DamageType.Physical);
                        if (target2 != null &&
                            Player.Instance.CalculateDamageOnUnit(target2, DamageType.Physical,
                                (DamageHandler.RDamage(target2))) > target2.Health)
                        {
                            R.Cast(target2);
                            return;
                        }
                    }
                    if ((R.Name != IsFirstR) && R.IsReady() &&
                        !Player.Instance.HasBuff("RivenFengShuiEngine") &&
                        combo["AlwaysR"].Cast<CheckBox>().CurrentValue)
                    {
                        Player.CastSpell(SpellSlot.R);
                    }
                    target = TargetSelector.GetTarget(W.Range, DamageType.Physical);
                    if (target != null && Player.Instance.Distance(target) < W.Range)
                    {
                        Player.CastSpell(SpellSlot.W);
                        return;
                    }
                }
            }

            //Q
            if (args.SData.Name.ToLower().Contains(Q.Name.ToLower()))
            {
                if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo))
                {
                    if (Player.Instance.HasBuff("RivenFengShuiEngine") && R.IsReady() &&
                        combo["AlwaysR"].Cast<CheckBox>().CurrentValue)
                    {
                        var target2 = TargetSelector.GetTarget(R.Range, DamageType.Physical);
                        if (target2 != null &&
                            (target2.Distance(Player.Instance) < 300 &&
                             target2.Health >
                             Player.Instance.CalculateDamageOnUnit(target2, DamageType.Physical, DamageHandler.QDamage()) ||
                             target2.Distance(Player.Instance) > 300) &&
                            Player.Instance.CalculateDamageOnUnit(target2, DamageType.Physical,
                                DamageHandler.RDamage(target2) + DamageHandler.QDamage()) > target2.Health)
                        {
                            R.Cast(target2);
                        }
                    }
                }
                return;
            }

            if (args.SData.IsAutoAttack() && target != null)
            {
               
                    if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo))
                    {
                        ComboAfterAa(target);
                    }

                    if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Harass))
                    {
                        HarassAfterAa(target);
                    }

                    if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LaneClear) && target.IsMinion())
                    {
                        LaneClearAfterAa(target);
                    }
                                           
            }
        }

        private static void OnMenuLoad()
        {
            menu = MainMenu.AddMenu("Blessed Riven", "blessedriven");

            combo = menu.AddSubMenu("Combo Settings", "combo");
            combo.Add("ComboW", new CheckBox("Use W"));
            combo.Add("NormalCombo", new KeyBind("Normal Combo", false, KeyBind.BindTypes.HoldActive, ' '));
            combo.Add("UseFastCombo", new KeyBind("Fast Combo", false, KeyBind.BindTypes.PressToggle, 'L'));
            combo.Add("AlwaysR", new KeyBind("Forced R", false, KeyBind.BindTypes.PressToggle, 'G'));
            combo.Add("RKillable", new CheckBox("Cast R to Enemy Killable"));
            combo.Add("Alive.Q", new CheckBox("Q TEST"));

            harass = menu.AddSubMenu("Harass Settings", "harass");
            harass.Add("HarassQ", new CheckBox("Use Q"));
            harass.Add("HarassW", new CheckBox("Use W"));
            harass.Add("HarassE", new CheckBox("Use E"));
            harass.Add("fastharass", new KeyBind("Fast Harass", false, KeyBind.BindTypes.HoldActive, 'X'));
            harass.Add("burst", new KeyBind("Flash Burst", false, KeyBind.BindTypes.HoldActive, 'T'));

            clear = menu.AddSubMenu("Clear Settings", "clear");
            clear.Add("LaneQ", new CheckBox("Use Q LaneClear"));
            clear.Add("LaneW", new Slider("Use W on {0} Minion", 3, 0, 5));
            clear.Add("LaneE", new CheckBox("Use E Laneclear",false));
            clear.AddSeparator();
            clear.Add("JungleQ", new CheckBox("Use Q JungleClear"));
            clear.Add("JungleW", new CheckBox("Use W JungleClear"));
            clear.Add("JungleE", new CheckBox("Use E JungleClear"));

            misc = menu.AddSubMenu("Misc Settings", "misc");
            misc.Add("youmu", new CheckBox("Use Youmu"));
            misc.Add("FirstHydra", new CheckBox("Flash Burst Hydra Cast before W", false));
            misc.Add("Qstrange", new CheckBox("AA Reset for Q"));
            misc.Add("Winterrupt", new CheckBox("W Interrupt"));
            misc.Add("AutoW", new Slider("Auto W on {0} Minion", 0, 0, 5));
            misc.Add("RMaxDam", new CheckBox("Use Second R Max Damage"));
            misc.Add("killstealw", new CheckBox("Killsteal W"));
            misc.Add("killstealr", new CheckBox("Killsteal Second R"));
            misc.Add("AutoShield", new CheckBox("Incoming Damage to Auto Cast E"));
            misc.Add("Shield", new CheckBox("Auto Cast E While LastHit"));
            misc.Add("KeepQ", new CheckBox("Keep Q Alive",false));
            misc.Add("QD", new Slider("First,Second Q Delay {0}", 29, 23, 43));
            misc.Add("QLD", new Slider("Third Q Delay {0}", 39, 36, 53));

            Skin = menu.AddSubMenu("Skin Changer", "SkinChanger");
            Skin.Add("checkSkin", new CheckBox("Use Skin Changer"));
            Skin.Add("skin.Id", new Slider("Skin", 4, 0, 6));

            draw = menu.AddSubMenu("Draw Settings", "draw");
            draw.Add("DrawAlwaysR", new CheckBox("Forced R Status"));
            draw.Add("DrawUseFastCombo", new CheckBox("Fast Combo Status"));
            draw.Add("Dind", new CheckBox("Damage Indicator"));
            draw.Add("DrawCB", new CheckBox("Combo Engage Range"));
            draw.Add("DrawBT", new CheckBox("Burst Engage Range", false));
            draw.Add("DrawFH", new CheckBox("FastHarass Engage Range"));
            draw.Add("DrawHS", new CheckBox("Harass Engage Range",false));
                      
        }

        private static void Interrupt(AIHeroClient sender, EventArgs args)
        {
            if (sender.IsEnemy && W.IsReady() && sender.IsValidTarget() && !sender.IsZombie && WInterrupt)
            {
                if (sender.IsValidTarget(125 + _Player.BoundingRadius + sender.BoundingRadius)) W.Cast();
            }
        }

        private static void AutoUseW()
        {
            if (AutoW > 0)
            {
                if (_Player.CountEnemiesInRange(WRange) >= AutoW)
                {
                    ForceW();
                }
            }
        }

        private static int TickLimiter = 1;
        private static int LastGameTick = 0;

        private static void OnTick(EventArgs args)
        {
            if (_Player.IsDead)
                return;

            ForceSkill();
            UseRMaxDam();
            AutoUseW();
            Killsteal();
            if (LastQ + 3600 < Environment.TickCount)
            {
                QStack = 0;
            }
            if (BurstCombo) Burst();
            if (FastHarassCombo) FastHarass();
            
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo)) Combo();
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LaneClear)) Jungleclear(); 
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Harass)) Harass();                 
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Flee)) Flee();
            if (combo["Alive.Q"].Cast<CheckBox>().CurrentValue && !Player.Instance.IsRecalling() && QStack < 3 && LastQ + 3480 < Environment.TickCount && Player.Instance.HasBuff("RivenTriCleaveBuff"))
            {
                Player.CastSpell(SpellSlot.Q,
                    Orbwalker.LastTarget != null && Orbwalker.LastAutoAttack - Environment.TickCount < 3000
                        ? Orbwalker.LastTarget.Position
                        : Game.CursorPos);
            }
        }

        private static void Killsteal()
        {
            if (KillstealW && W.IsReady())
            {
                var targets = EntityManager.Heroes.Enemies.Where(x => x.IsValidTarget(R.Range) && !x.IsZombie);
                foreach (var target in targets)
                {
                    if (target.Health < _Player.GetSpellDamage(target, SpellSlot.W) && InWRange(target))
                        W.Cast();
                }
            }
            if (KillstealR && R.IsReady() && R.Name == IsSecondR)
            {
                var targets = EntityManager.Heroes.Enemies.Where(x => x.IsValidTarget(R.Range) && !x.IsZombie);
                foreach (var target in targets)
                {
                    if (target.Health < Rdame(target, target.Health) &&
                        (!target.HasBuff("kindrednodeathbuff") && !target.HasBuff("Undying Rage") &&
                         !target.HasBuff("JudicatorIntervention")))
                        R.Cast(target.Position);
                }
            }
        }

        private static void UseRMaxDam()
        {
            if (RMaxDam && R.IsReady() && R.Name == IsSecondR)
            {
                var targets = EntityManager.Heroes.Enemies.Where(x => x.IsValidTarget(R.Range) && !x.IsZombie);
                foreach (var target in targets)
                {
                    if (target.HealthPercent <= 0.25 &&
                        (!target.HasBuff("kindrednodeathbuff") || !target.HasBuff("Undying Rage") ||
                         !target.HasBuff("JudicatorIntervention")))
                        R.Cast(target.Position);
                }
            }
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            //temp
            if (_Player.IsDead)
                return;
            var heropos = Drawing.WorldToScreen(ObjectManager.Player.Position);

            var green = Color.LimeGreen;
            var red = Color.IndianRed;
            if (DrawCB)
                Circle.Draw(E.IsReady() ? green : red, 250 + _Player.AttackRange + 70, ObjectManager.Player.Position);
            if (DrawBT && Flash != null && Flash.Slot != SpellSlot.Unknown)
                Circle.Draw(R.IsReady() && Flash.IsReady ? green : red, 800, ObjectManager.Player.Position);
            if (DrawFH)
                Circle.Draw(E.IsReady() && Q.IsReady() ? green : red, 450 + _Player.AttackRange + 70,
                    ObjectManager.Player.Position);
            if (DrawHS)
                Circle.Draw(Q.IsReady() && W.IsReady() ? green : red, 400, ObjectManager.Player.Position);
            if (DrawAlwaysR)
            {
                Drawing.DrawText(heropos.X - 40, heropos.Y + 20, System.Drawing.Color.Black, "Always R = ");
                Drawing.DrawText(heropos.X + 32, heropos.Y + 20,
                    AlwaysR ? System.Drawing.Color.IndianRed : System.Drawing.Color.LightGreen,
                    AlwaysR ? "On" : "Off");
            }
            if (DrawUseFastCombo)
            {
                Drawing.DrawText(heropos.X - 40, heropos.Y + 33, System.Drawing.Color.Black, "Fast Combo = ");
                Drawing.DrawText(heropos.X + 50, heropos.Y + 33,
                    UseFastCombo ? System.Drawing.Color.IndianRed : System.Drawing.Color.LightGreen,
                    UseFastCombo ? "On" : "Off");
            }
            
        }

        private static void Jungleclear()
        {
            //temp
            var Mobs =
                EntityManager.MinionsAndMonsters.Monsters.Where(
                    m => m.Distance(ObjectManager.Player) < 250 + _Player.AttackRange + 70).OrderBy(m => m.MaxHealth);

            if (!Mobs.Any())
                return;
            var mob = Mobs.First();
            if (W.IsReady(200) && E.IsReady(1) && ObjectManager.Player.IsInAutoAttackRange(mob))
            {
                Player.CastSpell(SpellSlot.E, mob.Position);
                Utils.DelayAction(ForceItem, 1);
                Utils.DelayAction(ForceW, 1);

            }
            
            
            
            
        }

        private static void Combo()
        {
            EnableR = false;
            var targetR = TargetSelector.GetTarget(250 + _Player.AttackRange + 70, DamageType.Physical);
            if (targetR == null || !targetR.IsValidTarget()) return;
            if (R.IsReady() && R.Name == IsFirstR && ObjectManager.Player.IsInAutoAttackRange(targetR) && AlwaysR)
            {
                ForceR();
                EnableR = true;
                //CastR1();
            }
                
            if (R.IsReady() && R.Name == IsFirstR && W.IsReady() && InWRange(targetR) && ComboW && AlwaysR)
            {
                ForceR();
                //CastR1();
                EnableR = true;
                Utils.DelayAction(ForceW, 1);
            }
            if (W.IsReady() && InWRange(targetR) && ComboW) W.Cast();
            if (UseFastCombo && R.IsReady() && R.Name == IsFirstR && W.IsReady() && E.IsReady() &&
                targetR.IsValidTarget() && !targetR.IsZombie && (IsKillableR(targetR) || AlwaysR))
            {
                if (!InWRange(targetR))
                {
                    Player.CastSpell(SpellSlot.E, targetR.Position);
                    ForceR();
                    Utils.DelayAction(ForceW, 1);
                    Utils.DelayAction(() => ForceCastQ(targetR), 220);
                }
            }
            else if (!UseFastCombo && R.IsReady() && R.Name == IsFirstR && W.IsReady() &&
                     E.IsReady() && targetR.IsValidTarget() && !targetR.IsZombie &&
                     (IsKillableR(targetR) || AlwaysR))
            {
                if (!InWRange(targetR))
                {
                    Player.CastSpell(SpellSlot.E, targetR.Position);
                    ForceR();
                    Utils.DelayAction(ForceW, 1);
                }
            }
            else if (UseFastCombo && W.IsReady() && E.IsReady())
            {
                if (targetR.IsValidTarget() && !targetR.IsZombie && !InWRange(targetR))
                {
                    Player.CastSpell(SpellSlot.E, targetR.Position);
                    Utils.DelayAction(CastYoumoo, 1);
                    Utils.DelayAction(ForceItem, 1);
                    Utils.DelayAction(ForceW, 1);
                    Utils.DelayAction(() => ForceCastQ(targetR), 220);
                }
            }
            else if (!UseFastCombo && W.IsReady() && E.IsReady())
            {
                if (targetR.IsValidTarget() && !targetR.IsZombie && !InWRange(targetR))
                {
                    Player.CastSpell(SpellSlot.E, targetR.Position);
                    Utils.DelayAction(CastYoumoo, 1);
                    Utils.DelayAction(ForceItem, 1);
                    Utils.DelayAction(ForceW, 1);
                }
            }
            else if (E.IsReady())
            {
                if (targetR.IsValidTarget() && !targetR.IsZombie && !InWRange(targetR))
                {
                    Player.CastSpell(SpellSlot.E, targetR.Position);
                }
            }
            if (Q.IsReady())
            {

                if (!targetR.IsValidTarget() || targetR.IsZombie) return;
                Utils.DelayAction(ForceItem, 1);
                Utils.DelayAction(() => ForceCastQ(targetR), 220);
            }
        }

        private static void Burst()
        {
            var target = TargetSelector.SelectedTarget;
            if (target == null || !target.IsValidTarget()) return;
            Orbwalker.ForcedTarget = target;
            Orbwalker.OrbwalkTo(target.ServerPosition);
            if (target.IsValidTarget() && !target.IsZombie)
            {
                if (R.IsReady() && R.Name == IsFirstR && W.IsReady() && E.IsReady() &&
                    _Player.Distance(target.Position) <= 250 + 70 + _Player.AttackRange)
                {
                    Player.CastSpell(SpellSlot.E, target.Position);
                    CastYoumoo();
                    ForceR();
                    Utils.DelayAction(ForceW, 1);
                }
                else if (R.IsReady() && R.Name == IsFirstR && E.IsReady() && W.IsReady() && Q.IsReady() &&
                         _Player.Distance(target.Position) <= 400 + 70 + _Player.AttackRange)
                {
                    CastYoumoo();
                    Player.CastSpell(SpellSlot.E, target.Position);
                    ForceR();
                    Utils.DelayAction(() => ForceCastQ(target), 220);
                    Utils.DelayAction(ForceW, 1);
                }
                else if (Flash.IsReady
                         && R.IsReady() && R.Name == IsFirstR && (_Player.Distance(target.Position) <= 800) &&
                         (!FirstHydra || HasHydra() == null || !HasHydra().IsReady() ))
                    
                {
                    Player.CastSpell(SpellSlot.E, target.Position);
                    CastYoumoo();
                    ForceR();
                    Utils.DelayAction(FlashW, 1);
                }
                else if (Flash.IsReady
                         && R.IsReady() && E.IsReady() && W.IsReady() && R.Name == IsFirstR &&
                         (_Player.Distance(target.Position) <= 800) && FirstHydra && HasHydra() != null)
                {
                    Player.CastSpell(SpellSlot.E, target.Position);
                    ForceR();
                    Utils.DelayAction(ForceItem, 1);
                    Utils.DelayAction(FlashW, 1);
                }
            }
        }

        private static void FastHarass()
        {
            var target = TargetSelector.SelectedTarget;

            if (target == null || !target.IsValidTarget()) target = TargetSelector.GetTarget(450 + _Player.AttackRange + 70, DamageType.Physical);
            if (target == null || !target.IsValidTarget()) return;
            Orbwalker.ForcedTarget = target;
            Orbwalker.OrbwalkTo(target.ServerPosition);
            if (Q.IsReady() && E.IsReady())
            {
                
                if (!target.IsValidTarget() || target.IsZombie) return;
                if (!ObjectManager.Player.IsInAutoAttackRange(target) && !InWRange(target)) Player.CastSpell(SpellSlot.E, target.Position);
                Utils.DelayAction(ForceItem, 10);
                Utils.DelayAction(() => ForceCastQ(target), 220);
            }
        }

        private static void Harass()
        {
            var target = TargetSelector.GetTarget(400, DamageType.Physical);
            if (target == null || !target.IsValidTarget()) return;
            if (Q.IsReady() && W.IsReady() && E.IsReady() && QStack == 1)
            {
                if (target.IsValidTarget() && !target.IsZombie)
                {
                    ForceCastQ(target);
                    Utils.DelayAction(ForceW, 1);
                }
            }
            if (Q.IsReady() && E.IsReady() && QStack == 3 && !Orbwalker.CanAutoAttack && Orbwalker.CanMove)
            {
                var epos = _Player.ServerPosition +
                           (_Player.ServerPosition - target.ServerPosition).Normalized()*300;
                Player.CastSpell(SpellSlot.E, epos);
                Utils.DelayAction(() => Player.CastSpell(SpellSlot.Q, epos), 220);
            }
        }

        private static void Flee()
        {
            var enemy =
                EntityManager.Heroes.Enemies.Where(
                    hero =>
                        hero.IsValidTarget(WRange) && W.IsReady());
            var x = _Player.Position.Extend(Game.CursorPos, 300);
            if (W.IsReady() && enemy.Any()) W.Cast();
            if (Q.IsReady() && !_Player.IsDashing()) Player.CastSpell(SpellSlot.Q, Game.CursorPos);
            if (E.IsReady() && !_Player.IsDashing()) Player.CastSpell(SpellSlot.E, x.To3D());
        }

        private static void OnPlay(Obj_AI_Base sender, GameObjectPlayAnimationEventArgs args)
        {
            if (_Player.IsDead)
                return;
            if (!sender.IsMe) return;

            switch (args.Animation)
            {
                case "Spell1a":
                    LastQ = Environment.TickCount;
                    if (Qstrange && (Orbwalker.ActiveModesFlags != Orbwalker.ActiveModes.None)) Orbwalker.ResetAutoAttack();  Utils.DelayAction(Reset, 291 - Game.Ping);
                    QStack = 2;
                    if (BurstCombo || Orbwalker.ActiveModesFlags != Orbwalker.ActiveModes.None &&
                        Orbwalker.ActiveModesFlags != Orbwalker.ActiveModes.LastHit &&
                        Orbwalker.ActiveModesFlags != Orbwalker.ActiveModes.Flee)
                        Orbwalker.ResetAutoAttack();
                    Utils.DelayAction(Reset, 291 - Game.Ping);
                    break;
                case "Spell1b":
                    LastQ = Environment.TickCount;
                    if (Qstrange && (Orbwalker.ActiveModesFlags != Orbwalker.ActiveModes.None)) Orbwalker.ResetAutoAttack(); Utils.DelayAction(Reset, 291 - Game.Ping);
                    QStack = 3;
                    if (BurstCombo || Orbwalker.ActiveModesFlags != Orbwalker.ActiveModes.None &&
                        Orbwalker.ActiveModesFlags != Orbwalker.ActiveModes.LastHit &&
                        Orbwalker.ActiveModesFlags != Orbwalker.ActiveModes.Flee)
                        Orbwalker.ResetAutoAttack();
                    Utils.DelayAction(Reset, 291 - Game.Ping);
                    break;
                case "Spell1c":
                    LastQ = Environment.TickCount;
                    if (Qstrange && (Orbwalker.ActiveModesFlags != Orbwalker.ActiveModes.None )) Orbwalker.ResetAutoAttack(); Utils.DelayAction(Reset, 393 - Game.Ping);
                    QStack = 1;
                    if (BurstCombo || Orbwalker.ActiveModesFlags != Orbwalker.ActiveModes.None &&
                        Orbwalker.ActiveModesFlags != Orbwalker.ActiveModes.LastHit &&
                        Orbwalker.ActiveModesFlags != Orbwalker.ActiveModes.Flee)
                        Orbwalker.ResetAutoAttack();
                    Utils.DelayAction(Reset, 393 - Game.Ping);
                    break;
                case "Spell3":
                    if ((BurstCombo ||//Orbwalker.ActiveModesFlags == Orbwalker.ActiveModes.Burst ||
                         Orbwalker.ActiveModesFlags == Orbwalker.ActiveModes.Combo ||
                         //Orbwalker.ActiveModesFlags == Orbwalker.ActiveModes.FastHarass ||
                         Orbwalker.ActiveModesFlags == Orbwalker.ActiveModes.Flee) && Youmu) CastYoumoo();
                    break;
                case "Spell4a":
                    LastR = Environment.TickCount;
                    break;
                case "Spell4b":
                    var target = TargetSelector.SelectedTarget;

                    if (target == null || !target.IsValidTarget()) target = TargetSelector.GetTarget(450 + _Player.AttackRange + 70, DamageType.Physical);
                    if (target == null || !target.IsValidTarget()) return;
                    if (Q.IsReady() && target.IsValidTarget()) ForceCastQ(target);
                    break;
                
            }
        }

        private static void OnCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (_Player.IsDead)
                return;

            if (args.SData.Name.Contains("ItemTiamatCleave")) forceItem = false;
            if (args.SData.Name.Contains("RivenTriCleave")) forceQ = false;
            if (args.SData.Name.Contains("RivenMartyr")) forceW = false;
            if (args.SData.Name == IsFirstR) forceR = false;
            if (args.SData.Name == IsSecondR) forceR2 = false;

            if (!sender.IsMe) return;
            var target = args.Target as Obj_AI_Base;

            // Hydra
            if (args.SData.Name.ToLower().Contains("itemtiamatcleave"))
            {
                Orbwalker.ResetAutoAttack();
                if (W.IsReady())
                {
                    var target2 = TargetSelector.GetTarget(W.Range, DamageType.Physical);
                    if (target2 != null || Orbwalker.ActiveModesFlags != Orbwalker.ActiveModes.None)
                    {
                        Player.CastSpell(SpellSlot.W);
                    }
                }
                return;
            }
            //W
            if (args.SData.Name.ToLower().Contains(W.Name.ToLower()))
            {
                if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo))
                {
                    if (Player.Instance.HasBuff("RivenFengShuiEngine") && R.IsReady() &&
                        combo["AlwaysR"].Cast<CheckBox>().CurrentValue)
                    {
                        var target2 = TargetSelector.GetTarget(R.Range, DamageType.Physical);
                        if (target2 != null &&
                            (target2.Distance(Player.Instance) < W.Range &&
                             target2.Health >
                             Player.Instance.CalculateDamageOnUnit(target2, DamageType.Physical, DamageHandler.WDamage()) ||
                             target2.Distance(Player.Instance) > W.Range) &&
                            Player.Instance.CalculateDamageOnUnit(target2, DamageType.Physical,
                                DamageHandler.RDamage(target2) + DamageHandler.WDamage()) > target2.Health)
                        {
                            R.Cast(target2);
                        }
                    }
                }

                target = (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo) ||
                          Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Harass))
                    ? TargetSelector.GetTarget(E.Range + W.Range, DamageType.Physical)
                    : (Obj_AI_Base)Orbwalker.LastTarget;
                if (Q.IsReady() && Orbwalker.ActiveModesFlags != Orbwalker.ActiveModes.None && target != null)
                {
                    Player.CastSpell(SpellSlot.Q, target.Position);
                    return;
                }
                return;
            }

            //E
            if (args.SData.Name.ToLower().Contains(E.Name.ToLower()))
            {
                if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo))
                {
                    if (Player.Instance.HasBuff("RivenFengShuiEngine") && R.IsReady() &&
                        combo["AlwaysR"].Cast<CheckBox>().CurrentValue)
                    {
                        var target2 = TargetSelector.GetTarget(R.Range, DamageType.Physical);
                        if (target2 != null &&
                            Player.Instance.CalculateDamageOnUnit(target2, DamageType.Physical,
                                (DamageHandler.RDamage(target2))) > target2.Health)
                        {
                            R.Cast(target2);
                            return;
                        }
                    }
                    if ((forceR && R.Name == IsFirstR) && R.IsReady() &&
                        !Player.Instance.HasBuff("RivenFengShuiEngine") &&
                        combo["AlwaysR"].Cast<CheckBox>().CurrentValue)
                    {
                        Player.CastSpell(SpellSlot.R);
                    }
                    target = TargetSelector.GetTarget(W.Range, DamageType.Physical);
                    if (target != null && Player.Instance.Distance(target) < W.Range)
                    {
                        Player.CastSpell(SpellSlot.W);
                        return;
                    }
                }
                if (!sender.IsEnemy || sender.Type != _Player.Type ||
                (!AutoShield && (!Shield || Orbwalker.ActiveModesFlags != Orbwalker.ActiveModes.LastHit)))
                    return;
                var epos = _Player.ServerPosition +
                           (_Player.ServerPosition - sender.ServerPosition).Normalized() * 300;

                if (!sender.IsMe) return;

                if (args.SData.Name.ToLower().Contains(W.Name.ToLower()))
                {
                    LastW = Environment.TickCount;
                    return;
                }
                if (args.SData.Name.ToLower().Contains(Q.Name.ToLower()))
                {
                    LastQ = Environment.TickCount;
                    if (!combo["Alive.Q"].Cast<CheckBox>().CurrentValue) return;
                    Core.DelayAction(() =>
                    {
                        if (!Player.Instance.IsRecalling() && QStack < 2)
                        {
                            Player.CastSpell(SpellSlot.Q,
                                Orbwalker.LastTarget != null && Orbwalker.LastAutoAttack - Environment.TickCount < 3000
                                    ? Orbwalker.LastTarget.Position
                                    : Game.CursorPos);
                        }
                    }, 3480);
                    return;
                }

                if (!(_Player.Distance(sender.ServerPosition) <= args.SData.CastRange)) return;
                switch (args.SData.TargettingType)
                {
                    case SpellDataTargetType.Unit:

                        if (args.Target.NetworkId == _Player.NetworkId)
                        {
                            if (Orbwalker.ActiveModesFlags == Orbwalker.ActiveModes.LastHit &&
                                !args.SData.Name.Contains("NasusW"))
                            {
                                if (E.IsReady()) Player.CastSpell(SpellSlot.E, epos);
                            }
                        }

                        break;
                    case SpellDataTargetType.SelfAoe:

                        if (Orbwalker.ActiveModesFlags == Orbwalker.ActiveModes.LastHit)
                        {
                            if (E.IsReady()) Player.CastSpell(SpellSlot.E, epos);
                        }

                        break;
                }
                if (args.SData.Name.Contains("IreliaEquilibriumStrike"))
                {
                    if (args.Target.NetworkId == _Player.NetworkId)
                    {
                        if (W.IsReady() && InWRange(sender)) W.Cast();
                        else if (E.IsReady()) Player.CastSpell(SpellSlot.E, epos);
                    }
                }
                if (args.SData.Name.Contains("TalonCutthroat"))
                {
                    if (args.Target.NetworkId == _Player.NetworkId)
                    {
                        if (W.IsReady()) W.Cast();
                    }
                }
                if (args.SData.Name.Contains("RenektonPreExecute"))
                {
                    if (args.Target.NetworkId == _Player.NetworkId)
                    {
                        if (W.IsReady()) W.Cast();
                    }
                }
                if (args.SData.Name.Contains("GarenRPreCast"))
                {
                    if (args.Target.NetworkId == _Player.NetworkId)
                    {
                        if (E.IsReady()) Player.CastSpell(SpellSlot.E, epos);
                    }
                }
                if (args.SData.Name.Contains("GarenQAttack"))
                {
                    if (args.Target.NetworkId == _Player.NetworkId)
                    {
                        if (E.IsReady()) Player.CastSpell(SpellSlot.E, Game.CursorPos);
                    }
                }
                if (args.SData.Name.Contains("XenZhaoThrust3"))
                {
                    if (args.Target.NetworkId == _Player.NetworkId)
                    {
                        if (W.IsReady()) W.Cast();
                    }
                }
                if (args.SData.Name.Contains("RengarQ"))
                {
                    if (args.Target.NetworkId == _Player.NetworkId)
                    {
                        if (E.IsReady()) Player.CastSpell(SpellSlot.E, Game.CursorPos);
                    }
                }
                if (args.SData.Name.Contains("RengarPassiveBuffDash"))
                {
                    if (args.Target.NetworkId == _Player.NetworkId)
                    {
                        if (E.IsReady()) Player.CastSpell(SpellSlot.E, Game.CursorPos);
                    }
                }
                if (args.SData.Name.Contains("RengarPassiveBuffDashAADummy"))
                {
                    if (args.Target.NetworkId == _Player.NetworkId)
                    {
                        if (E.IsReady()) Player.CastSpell(SpellSlot.E, Game.CursorPos);
                    }
                }
                if (args.SData.Name.Contains("TwitchEParticle"))
                {
                    if (args.Target.NetworkId == _Player.NetworkId)
                    {
                        if (E.IsReady()) Player.CastSpell(SpellSlot.E, Game.CursorPos);
                    }
                }
                if (args.SData.Name.Contains("FizzPiercingStrike"))
                {
                    if (args.Target.NetworkId == _Player.NetworkId)
                    {
                        if (E.IsReady()) Player.CastSpell(SpellSlot.E, Game.CursorPos);
                    }
                }
                if (args.SData.Name.Contains("HungeringStrike"))
                {
                    if (args.Target.NetworkId == _Player.NetworkId)
                    {
                        if (E.IsReady()) Player.CastSpell(SpellSlot.E, Game.CursorPos);
                    }
                }
                if (args.SData.Name.Contains("YasuoDash"))
                {
                    if (args.Target.NetworkId == _Player.NetworkId)
                    {
                        if (E.IsReady()) Player.CastSpell(SpellSlot.E, Game.CursorPos);
                    }
                }
                if (args.SData.Name.Contains("KatarinaRTrigger"))
                {
                    if (args.Target.NetworkId == _Player.NetworkId)
                    {
                        if (W.IsReady() && InWRange(sender)) W.Cast();
                        else if (E.IsReady()) Player.CastSpell(SpellSlot.E, Game.CursorPos);
                    }
                }
                if (args.SData.Name.Contains("YasuoDash"))
                {
                    if (args.Target.NetworkId == _Player.NetworkId)
                    {
                        if (E.IsReady()) Player.CastSpell(SpellSlot.E, Game.CursorPos);
                    }
                }
                if (args.SData.Name.Contains("KatarinaE"))
                {
                    if (args.Target.NetworkId == _Player.NetworkId)
                    {
                        if (W.IsReady()) W.Cast();
                    }
                }
                if (args.SData.Name.Contains("MonkeyKingQAttack"))
                {
                    if (args.Target.NetworkId == _Player.NetworkId)
                    {
                        if (E.IsReady()) Player.CastSpell(SpellSlot.E, Game.CursorPos);
                    }
                }
                if (args.SData.Name.Contains("MonkeyKingSpinToWin"))
                {
                    if (args.Target.NetworkId == _Player.NetworkId)
                    {
                        if (E.IsReady()) Player.CastSpell(SpellSlot.E, Game.CursorPos);
                        else if (W.IsReady()) W.Cast();
                    }
                }
                if (args.SData.Name.Contains("MonkeyKingQAttack"))
                {
                    if (args.Target.NetworkId == _Player.NetworkId)
                    {
                        if (E.IsReady()) Player.CastSpell(SpellSlot.E, Game.CursorPos);
                    }
                }
                if (args.SData.Name.Contains("MonkeyKingQAttack"))
                {
                    if (args.Target.NetworkId == _Player.NetworkId)
                    {
                        if (E.IsReady()) Player.CastSpell(SpellSlot.E, Game.CursorPos);
                    }
                }
                if (args.SData.Name.Contains("MonkeyKingQAttack"))
                {
                    if (args.Target.NetworkId == _Player.NetworkId)
                    {
                        if (E.IsReady()) Player.CastSpell(SpellSlot.E, Game.CursorPos);
                    }
                }
            }

            //Q
            if (args.SData.Name.ToLower().Contains(Q.Name.ToLower()))
            {
                if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo))
                {
                    if (Player.Instance.HasBuff("RivenFengShuiEngine") && R.IsReady() &&
                        combo["AlwaysR"].Cast<CheckBox>().CurrentValue)
                    {
                        var target2 = TargetSelector.GetTarget(R.Range, DamageType.Physical);
                        if (target2 != null &&
                            (target2.Distance(Player.Instance) < 300 &&
                             target2.Health >
                             Player.Instance.CalculateDamageOnUnit(target2, DamageType.Physical, DamageHandler.QDamage()) ||
                             target2.Distance(Player.Instance) > 300) &&
                            Player.Instance.CalculateDamageOnUnit(target2, DamageType.Physical,
                                DamageHandler.RDamage(target2) + DamageHandler.QDamage()) > target2.Health)
                        {
                            R.Cast(target2);
                        }
                    }
                }
                return;
            }

            if (args.SData.IsAutoAttack() && target != null)
            {
                
                    if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo))
                    {
                        ComboAfterAa(target);
                    }

                    if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Harass))
                    {
                        HarassAfterAa(target);
                    }

                    if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LaneClear) && target.IsMinion())
                    {
                        LaneClearAfterAa(target);
                    }
                
            }
            }

        public static void HarassAfterAa(Obj_AI_Base target)
        {
            if (harass["HarassW"].Cast<CheckBox>().CurrentValue && W.IsReady() &&
                W.IsInRange(target))
            {
                if (Hydra != null && Hydra.IsReady())
                {
                    Hydra.Cast();
                    return;
                }
                Player.CastSpell(SpellSlot.W);
                return;
            }
            if (harass["HarassQ"].Cast<CheckBox>().CurrentValue && Q.IsReady())
            {
                Player.CastSpell(SpellSlot.Q, target.Position);
                return;
            }
            if (Hydra != null && Hydra.IsReady())
            {
                Hydra.Cast();
            }
        }

        private static void Reset()
        {           
            Player.DoEmote(Emote.Dance);
            Orbwalker.ResetAutoAttack();
        }

        private static bool InWRange(GameObject target)
        {
            if (target == null || !target.IsValid) return false;
            return (_Player.HasBuff("RivenFengShuiEngine"))
            ? 330 >= _Player.Distance(target.Position)
            : 265 >= _Player.Distance(target.Position);
            
        }


        private static void ForceSkill()
        {
            if (QTarget == null || !QTarget.IsValidTarget()) return;
            if (forceR && R.Name == IsFirstR)
            {
                Player.CastSpell(SpellSlot.R);
                return;
            }
            if (forceQ && QTarget != null && QTarget.IsValidTarget(E.Range + _Player.BoundingRadius + 70) && Q.IsReady())
                Player.CastSpell(SpellSlot.Q, ((Obj_AI_Base)QTarget).ServerPosition);
            if (forceW) W.Cast();
            
            var hydra = HasHydra();
            if (forceItem && hydra != null && hydra.IsReady()) hydra.Cast();
            if (forceR2 && R.Name == IsSecondR)
            {
                var target = TargetSelector.SelectedTarget;

                if (target == null || !target.IsValidTarget()) target = TargetSelector.GetTarget(450 + _Player.AttackRange + 70, DamageType.Physical);
                if (target == null || !target.IsValidTarget()) return;
                R.Cast(target);
            }
        }

        private static void CastR1(int delay = 0)
        {
            Player.CastSpell(SpellSlot.R);
        }
        private static void ForceItem()
        {
            var hydra = HasHydra();
            if (hydra != null && hydra.IsReady()) forceItem = true;
            Utils.DelayAction(() => forceItem = false, 1);
        }

        private static void ForceR()
        {
            forceR = (R.IsReady() && R.Name == IsFirstR);
            Utils.DelayAction(() => forceR = false, 1);
        }

        private static void ForceR2()
        {
            forceR2 = R.IsReady() && R.Name == IsSecondR;
            Utils.DelayAction(() => forceR2 = false, 1);
        }

        private static void ForceW()
        {
            forceW = W.IsReady();
            Utils.DelayAction(() => forceW = false, 1);
        }

        private static void ForceCastQ(AttackableUnit target)
        {
            forceQ = true;
            QTarget = target;
        }


        private static void FlashW()
        {
            var target = TargetSelector.SelectedTarget;
            if (target != null && target.IsValidTarget() && !target.IsZombie)
            {
                Utils.DelayAction(() => _Player.Spellbook.CastSpell(Flash.Slot, target.Position), 1);
                W.Cast();              
            }
        }


        private static void CastYoumoo()
        {
            var youmu = ObjectManager.Player.InventoryItems.FirstOrDefault(it => it.Id == ItemId.Youmuus_Ghostblade);
       
            if (youmu != null && youmu.CanUseItem()) youmu.Cast();
        }

        private static double basicdmg(Obj_AI_Base target)
        {
            if (target != null)
            {
                double dmg = 0;
                double passivenhan;
                if (_Player.Level >= 18)
                {
                    passivenhan = 0.5;
                }
                else if (_Player.Level >= 15)
                {
                    passivenhan = 0.45;
                }
                else if (_Player.Level >= 12)
                {
                    passivenhan = 0.4;
                }
                else if (_Player.Level >= 9)
                {
                    passivenhan = 0.35;
                }
                else if (_Player.Level >= 6)
                {
                    passivenhan = 0.3;
                }
                else if (_Player.Level >= 3)
                {
                    passivenhan = 0.25;
                }
                else
                {
                    passivenhan = 0.2;
                }
                if (HasHydra()!=null) dmg = dmg + _Player.GetAutoAttackDamage(target)*0.7;
                if (W.IsReady()) dmg = dmg + _Player.GetSpellDamage(target, SpellSlot.W);
                if (Q.IsReady())
                {
                    var qnhan = 4 - QStack;
                    
                    dmg = dmg + ObjectManager.Player.GetSpellDamage(target, SpellSlot.Q)*qnhan + _Player.GetAutoAttackDamage(target)*qnhan*(1 + passivenhan);
                }
                dmg = dmg + _Player.GetAutoAttackDamage(target)*(1 + passivenhan);
                return dmg;
            }
            return 0;
        }


        private static float getComboDamage(Obj_AI_Base enemy)
        {
            if (enemy != null)
            {
                float damage = 0;
                float passivenhan;
                if (_Player.Level >= 18)
                {
                    passivenhan = 0.5f;
                }
                else if (_Player.Level >= 15)
                {
                    passivenhan = 0.45f;
                }
                else if (_Player.Level >= 12)
                {
                    passivenhan = 0.4f;
                }
                else if (_Player.Level >= 9)
                {
                    passivenhan = 0.35f;
                }
                else if (_Player.Level >= 6)
                {
                    passivenhan = 0.3f;
                }
                else if (_Player.Level >= 3)
                {
                    passivenhan = 0.25f;
                }
                else
                {
                    passivenhan = 0.2f;
                }
                if (HasHydra() != null) damage = damage + _Player.GetAutoAttackDamage(enemy)*0.7f;
                if (W.IsReady()) damage = damage + ObjectManager.Player.GetSpellDamage(enemy, SpellSlot.W);
                if (Q.IsReady())
                {
                    var qnhan = 4 - QStack;
                    damage = damage + ObjectManager.Player.GetSpellDamage(enemy, SpellSlot.Q)*qnhan +
                             _Player.GetAutoAttackDamage(enemy)*qnhan*(1 + passivenhan);
                }
                damage = damage + _Player.GetAutoAttackDamage(enemy)*(1 + passivenhan);
                if (R.IsReady())
                {
                    return damage*1.2f + ObjectManager.Player.GetSpellDamage(enemy, SpellSlot.R);
                }

                return damage;
            }
            return 0;
        }

        public static bool IsKillableR(AIHeroClient target)
        {
            if (RKillable && target.IsValidTarget() && (totaldame(target) >= target.Health
                                                        && basicdmg(target) <= target.Health) ||
                _Player.CountEnemiesInRange(900) >= 2 &&
                (!target.HasBuff("kindrednodeathbuff") && !target.HasBuff("Undying Rage") &&
                 !target.HasBuff("JudicatorIntervention")))
            {
                return true;
            }
            return false;
        }

        private static double totaldame(Obj_AI_Base target)
        {
            if (target != null)
            {
                float dmg = 0;
                float passivenhan;
                if (_Player.Level >= 18)
                {
                    passivenhan = 0.5f;
                }
                else if (_Player.Level >= 15)
                {
                    passivenhan = 0.45f;
                }
                else if (_Player.Level >= 12)
                {
                    passivenhan = 0.4f;
                }
                else if (_Player.Level >= 9)
                {
                    passivenhan = 0.35f;
                }
                else if (_Player.Level >= 6)
                {
                    passivenhan = 0.3f;
                }
                else if (_Player.Level >= 3)
                {
                    passivenhan = 0.25f;
                }
                else
                {
                    passivenhan = 0.2f;
                }
                if (HasHydra() != null) dmg = dmg + _Player.GetAutoAttackDamage(target)*0.7f;
                if (W.IsReady()) dmg = dmg + _Player.GetSpellDamage(target, SpellSlot.W);
                if (Q.IsReady())
                {
                    var qnhan = 4 - QStack;
                    dmg = dmg + ObjectManager.Player.GetSpellDamage(target, SpellSlot.Q)*qnhan + _Player.GetAutoAttackDamage(target)*qnhan*(1 + passivenhan);
                }
                dmg = dmg + _Player.GetAutoAttackDamage(target)*(1 + passivenhan);
                if (R.IsReady())
                {
                    var rdmg = Rdame(target, target.Health - dmg*1.2f);
                    return dmg*1.2 + rdmg;
                }
                return dmg;
            }
            return 0;
        }

        private static double Rdame(Obj_AI_Base target, float health)
        {
            if (target != null)
            {
                float missinghealth = (target.MaxHealth - health)/target.MaxHealth > 0.75f
                    ? 0.75f
                    : (target.MaxHealth - health)/target.MaxHealth;
                float pluspercent = missinghealth * (2.666667F); // 8/3
                float rawdmg = new float[] {80, 120, 160}[R.Level - 1] + 0.6f*_Player.FlatPhysicalDamageMod;
                return ObjectManager.Player.CalculateDamageOnUnit(target, DamageType.Physical, rawdmg*(1 + pluspercent));
            }
            return 0;
        }
    }
}