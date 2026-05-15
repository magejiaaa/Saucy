using Dalamud.Interface;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Components;
using ECommons.DalamudServices;
using ECommons.ImGuiMethods;
using FFTriadBuddy;
using ImGuiNET;
using PunishLib.ImGuiMethods;
using Saucy.CuffACur;
using Saucy.OtherGames;
using Saucy.TripleTriad;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using TriadBuddyPlugin;


namespace Saucy
{
    // It is good to have this be disposable in general, in case you ever need it
    // to do any cleanup
    public unsafe class PluginUI : IDisposable
    {
        private Configuration configuration;

        // this extra bool exists for ImGui, since you can't ref a property
        private bool visible = false;
        public bool Visible
        {
            get { return visible; }
            set { visible = value; }
        }

        private bool settingsVisible = false;
        private GameNpcInfo currentNPC;

        public bool SettingsVisible
        {
            get { return settingsVisible; }
            set { settingsVisible = value; }
        }

        public GameNpcInfo CurrentNPC
        {
            get => currentNPC;
            set
            {
                if (currentNPC != value)
                {
                    TriadAutomater.TempCardsWonList.Clear();
                    currentNPC = value;
                }
            }
        }

        public PluginUI(Configuration configuration)
        {
            this.configuration = Saucy.Config;
        }

        public void Dispose()
        {
        }

        public bool Enabled { get; set; } = false;

        public void Draw()
        {
            DrawMainWindow();
        }

        public void DrawMainWindow()
        {
            if (!Visible)
            {
                return;
            }

            ImGui.SetNextWindowSize(new Vector2(520, 420), ImGuiCond.FirstUseEver);
            //ImGui.SetNextWindowSizeConstraints(new Vector2(520, 420), new Vector2(float.MaxValue, float.MaxValue));
            if (ImGui.Begin("Saucy 設定", ref visible))
            {
                if (ImGui.BeginTabBar("###Games", ImGuiTabBarFlags.Reorderable))
                {
                    if (ImGui.BeginTabItem("重擊伽美什"))
                    {
                        DrawCufTab();
                        ImGui.EndTabItem();
                    }

                    if (Saucy.openTT)
                    {
                        Saucy.openTT = false;
                        if (ImGuiEx.BeginTabItem("九宮幻卡", ImGuiTabItemFlags.SetSelected))
                        {
                            DrawTriadTab();
                            ImGui.EndTabItem();
                        }
                    }
                    else
                    {
                        if (ImGui.BeginTabItem("九宮幻卡"))
                        {
                            DrawTriadTab();
                            ImGui.EndTabItem();
                        }
                    }

                    if (ImGui.BeginTabItem("孤樹無援"))
                    {
                        if (ImGui.BeginTabBar($"LimbTab"))
                        {
                            if (ImGui.BeginTabItem("主要"))
                            {
                                Saucy.P.LimbManager.DrawSettings();
                                ImGui.EndTabItem();
                            }
                            if (ImGui.BeginTabItem($"除錯"))
                            {
                                Saucy.P.LimbManager.DrawDebug();
                                ImGui.EndTabItem();
                            }

                            ImGui.EndTabBar();
                        }
                       
                        ImGui.EndTabItem();
                    }

                    if (ImGui.BeginTabItem("其他遊戲"))
                    {
                        DrawOtherGamesTab();
                        ImGui.EndTabItem();
                    }


                    if (ImGui.BeginTabItem("統計"))
                    {
                        DrawStatsTab();
                        ImGui.EndTabItem();
                    }

                    if (ImGui.BeginTabItem("關於"))
                    {
                        AboutTab.Draw("Saucy");
                        ImGui.EndTabItem();
                    }

                    ImGui.EndTabBar();
                }
            }
            ImGui.End();
        }

        private void DrawOtherGamesTab()
        {
            //ImGui.Checkbox("Enable Air Force One Module", ref AirForceOneModule.ModuleEnabled);

            var sliceIsRightEnabled = SliceIsRightModule.ModuleEnabled;
            if (ImGui.Checkbox("啟用揮刀斬魔模組", ref sliceIsRightEnabled))
            {
                SliceIsRightModule.ModuleEnabled = sliceIsRightEnabled;
                Saucy.Config.Save();
            }

            if (ImGui.Checkbox("啟用自動迷你仙人彩", ref Saucy.Config.EnableAutoMiniCactpot))
                Saucy.Config.Save();
        }

        private void DrawStatsTab()
        {
            if (ImGui.BeginTabBar("Stats"))
            {
                if (ImGui.BeginTabItem("累計"))
                {
                    this.DrawStatsTab(Saucy.Config.Stats, out bool reset);

                    if (reset)
                    {
                        Saucy.Config.Stats = new();
                        Saucy.Config.Save();
                    }

                    ImGui.EndTabItem();
                }
                if (ImGui.BeginTabItem("本次"))
                {
                    this.DrawStatsTab(Saucy.Config.SessionStats, out bool reset);
                    if (reset)
                        Saucy.Config.SessionStats = new();
                    ImGui.EndTabItem();
                }

                ImGui.EndTabBar();
            }
        }

        private void DrawStatsTab(Stats stat, out bool reset)
        {
            if (ImGui.BeginTabBar("Games"))
            {
                if (ImGui.BeginTabItem("重擊伽美什"))
                {
                    DrawCuffStats(stat);
                    ImGui.EndTabItem();
                }

                if (ImGui.BeginTabItem("九宮幻卡"))
                {
                    DrawTTStats(stat);
                    ImGui.EndTabItem();
                }

                if (ImGui.BeginTabItem($"孤樹無援"))
                {
                    DrawLimbStats(stat);
                    ImGui.EndTabItem();
                }

                ImGui.EndTabBar();
            }

            ImGui.PushItemWidth(ImGui.GetContentRegionAvail().X);
            reset = ImGui.Button("重設統計（按住 Ctrl）", new Vector2(ImGui.GetContentRegionAvail().X, ImGui.GetContentRegionAvail().Y)) && ImGui.GetIO().KeyCtrl;
        }

        private void DrawLimbStats(Stats stat)
        {
            ImGui.BeginChild("Limb Stats", new Vector2(0, ImGui.GetContentRegionAvail().Y - 30f), true);
            ImGui.Columns(3, null, false);
            ImGui.NextColumn();
            ImGuiEx.CenterColumnText(ImGuiColors.DalamudRed, "孤樹無援", true);
            ImGuiHelpers.ScaledDummy(10f);
            ImGui.Columns(2, null, false);
            ImGui.NextColumn();
            ImGui.NextColumn();
            ImGuiEx.CenterColumnText("遊玩次數", true);
            ImGui.NextColumn();
            ImGuiEx.CenterColumnText("獲得 MGP", true);
            ImGui.NextColumn();
            ImGuiEx.CenterColumnText($"{stat.LimbGamesPlayed.ToString("N0")}");
            ImGui.NextColumn();
            ImGuiEx.CenterColumnText($"{stat.LimbMGP.ToString("N0")}");

            ImGui.EndChild();
        }

        private void DrawCuffStats(Stats stat)
        {
            ImGui.BeginChild("Cuff Stats", new Vector2(0, ImGui.GetContentRegionAvail().Y - 30f), true);
            ImGui.Columns(3, null, false);
            ImGui.NextColumn();
            ImGuiEx.CenterColumnText(ImGuiColors.DalamudRed, "重擊伽美什", true);
            ImGuiHelpers.ScaledDummy(10f);
            ImGui.NextColumn();
            ImGui.NextColumn();
            ImGui.NextColumn();
            ImGuiEx.CenterColumnText("遊玩次數", true);
            ImGui.NextColumn();
            ImGui.NextColumn();
            ImGui.NextColumn();
            ImGuiEx.CenterColumnText($"{stat.CuffGamesPlayed.ToString("N0")}");
            ImGui.NextColumn();
            ImGui.NextColumn();
            ImGui.Spacing();
            ImGuiEx.CenterColumnText("重擊！", true);
            ImGui.NextColumn();
            ImGuiEx.CenterColumnText("痛擊！！", true);
            ImGui.NextColumn();
            ImGuiEx.CenterColumnText("猛擊！！！！", true);
            ImGui.NextColumn();
            ImGuiEx.CenterColumnText($"{stat.CuffBruisings.ToString("N0")}");
            ImGui.NextColumn();
            ImGuiEx.CenterColumnText($"{stat.CuffPunishings.ToString("N0")}");
            ImGui.NextColumn();
            ImGuiEx.CenterColumnText($"{stat.CuffBrutals.ToString("N0")}");
            ImGui.NextColumn();
            ImGui.NextColumn();
            ImGuiEx.CenterColumnText("獲得 MGP", true);
            ImGui.NextColumn();
            ImGui.NextColumn();
            ImGui.NextColumn();
            ImGuiEx.CenterColumnText($"{stat.CuffMGP.ToString("N0")}");

            ImGui.EndChild();
        }

        private void DrawTTStats(Stats stat)
        {
            ImGui.BeginChild("TT Stats", new Vector2(0, ImGui.GetContentRegionAvail().Y - 30f), true);
            ImGui.Columns(3, null, false);
            ImGui.NextColumn();
            ImGuiEx.CenterColumnText(ImGuiColors.DalamudRed, "九宮幻卡", true);
            ImGuiHelpers.ScaledDummy(10f);
            ImGui.NextColumn();
            ImGui.NextColumn();
            ImGui.NextColumn();
            ImGuiEx.CenterColumnText("遊玩次數", true);
            ImGui.NextColumn();
            ImGui.NextColumn();
            ImGui.NextColumn();
            ImGuiEx.CenterColumnText($"{stat.GamesPlayedWithSaucy.ToString("N0")}");
            ImGui.NextColumn();
            ImGui.NextColumn();
            ImGui.Spacing();
            ImGuiEx.CenterColumnText("勝利", true);
            ImGui.NextColumn();
            ImGuiEx.CenterColumnText("敗北", true);
            ImGui.NextColumn();
            ImGuiEx.CenterColumnText("平手", true);
            ImGui.NextColumn();
            ImGuiEx.CenterColumnText($"{stat.GamesWonWithSaucy.ToString("N0")}");
            ImGui.NextColumn();
            ImGuiEx.CenterColumnText($"{stat.GamesLostWithSaucy.ToString("N0")}");
            ImGui.NextColumn();
            ImGuiEx.CenterColumnText($"{stat.GamesDrawnWithSaucy.ToString("N0")}");
            ImGui.NextColumn();
            ImGuiEx.CenterColumnText("勝率", true);
            ImGui.NextColumn();
            ImGuiEx.CenterColumnText("獲得卡片", true);
            ImGui.NextColumn();
            if (stat.NPCsPlayed.Count > 0)
            {
                ImGuiEx.CenterColumnText("最常對戰 NPC", true);
                ImGui.NextColumn();
            }
            else
            {
                ImGui.NextColumn();
            }

            if (stat.GamesPlayedWithSaucy > 0)
            {
                ImGuiEx.CenterColumnText($"{Math.Round(((double)stat.GamesWonWithSaucy / (double)stat.GamesPlayedWithSaucy) * 100, 2)}%");
            }
            else
            {
                ImGuiEx.CenterColumnText("");
            }
            ImGui.NextColumn();
            ImGuiEx.CenterColumnText($"{stat.CardsDroppedWithSaucy.ToString("N0")}");
            ImGui.NextColumn();

            if (stat.NPCsPlayed.Count > 0)
            {
                ImGuiEx.CenterColumnText($"{stat.NPCsPlayed.OrderByDescending(x => x.Value).First().Key}");
                ImGuiEx.CenterColumnText($"{stat.NPCsPlayed.OrderByDescending(x => x.Value).First().Value.ToString("N0")} 次");
                ImGui.NextColumn();
                ImGui.NextColumn();
                ImGui.NextColumn();
            }

            ImGui.NextColumn();
            ImGuiEx.CenterColumnText("獲得 MGP", true);
            ImGui.NextColumn();
            ImGuiEx.CenterColumnText("卡片掉落總價值", true);
            ImGui.NextColumn();
            if (stat.CardsWon.Count > 0)
            {
                ImGuiEx.CenterColumnText("最多獲得卡片", true);
            }
            ImGui.NextColumn();
            ImGuiEx.CenterColumnText($"{stat.MGPWon.ToString("N0")} MGP");
            ImGui.NextColumn();
            ImGuiEx.CenterColumnText($"{GetDroppedCardValues(stat).ToString("N0")} MGP");
            ImGui.NextColumn();
            if (stat.CardsWon.Count > 0)
            {
                ImGuiEx.CenterColumnText($"{TriadCardDB.Get().FindById((int)stat.CardsWon.OrderByDescending(x => x.Value).First().Key).Name.GetLocalized()}");
                ImGui.NextColumn();
                ImGui.NextColumn();
                ImGui.NextColumn();
                ImGuiEx.CenterColumnText($"{stat.CardsWon.OrderByDescending(x => x.Value).First().Value.ToString("N0")} 次");
            }

            ImGui.Columns(1);
            ImGui.EndChild();
        }

        private int GetDroppedCardValues(Stats stat)
        {
            int output = 0;
            foreach (var card in stat.CardsWon)
                output += GameCardDB.Get().FindById((int)card.Key).SaleValue * stat.CardsWon[card.Key];

            return output;
        }

        public void DrawTriadTab()
        {
            bool enabled = TriadAutomater.ModuleEnabled;

            ImGui.TextWrapped(@"使用方式：向想對戰的 NPC 發起九宮幻卡挑戰。進入挑戰後，勾選「啟用九宮幻卡模組」。");
            ImGui.Separator();

            if (ImGui.Checkbox("啟用九宮幻卡模組", ref enabled))
            {
                TriadAutomater.ModuleEnabled = enabled;

                if (enabled)
                    CufModule.ModuleEnabled = false;
            }

            bool autoOpen = configuration.OpenAutomatically;

            if (ImGui.Checkbox("挑戰 NPC 時自動開啟 Saucy", ref autoOpen))
            {
                configuration.OpenAutomatically = autoOpen;
                configuration.Save();
            }

            int selectedDeck = configuration.SelectedDeckIndex;

            if (Saucy.TTSolver.profileGS.GetPlayerDecks().Count() > 0)
            {
                bool useAutoDeck = Saucy.Config.UseRecommendedDeck;
                if (ImGui.Checkbox("自動選擇勝率最高的牌組", ref useAutoDeck))
                {
                    Saucy.Config.UseRecommendedDeck = useAutoDeck;
                    Saucy.Config.Save();
                }

                if (!Saucy.Config.UseRecommendedDeck)
                {
                    ImGui.PushItemWidth(200);
                    string preview = "";

                    if (selectedDeck == -1 || Saucy.TTSolver.profileGS.GetPlayerDecks()[selectedDeck] is null)
                    {
                        preview = "";
                    }
                    else
                    {
                        preview = selectedDeck >= 0 ? Saucy.TTSolver.profileGS.GetPlayerDecks()[selectedDeck].name : string.Empty;
                    }

                    if (ImGui.BeginCombo("選擇牌組", preview))
                    {
                        if (ImGui.Selectable(""))
                        {
                            configuration.SelectedDeckIndex = -1;
                        }

                        foreach (var deck in Saucy.TTSolver.profileGS.GetPlayerDecks())
                        {
                            if (deck is null) continue;
                            var index = deck.id;
                            //var index = Saucy.TTSolver.preGameDecks.Where(x => x.Value == deck).First().Key;
                            if (ImGui.Selectable(deck.name, index == selectedDeck))
                            {
                                configuration.SelectedDeckIndex = index;
                                configuration.Save();
                            }
                        }

                        ImGui.EndCombo();
                    }

                }
            }
            else
            {
                ImGui.TextWrapped("請先向 NPC 發起挑戰，讓牌組清單載入。");
            }

            if (ImGui.Checkbox("遊玩指定次數", ref TriadAutomater.PlayXTimes) && (TriadAutomater.NumberOfTimes <= 0 || TriadAutomater.PlayUntilCardDrops || TriadAutomater.PlayUntilAllCardsDropOnce))
            {
                TriadAutomater.NumberOfTimes = 1;
                TriadAutomater.PlayUntilCardDrops = false;
                TriadAutomater.PlayUntilAllCardsDropOnce = false;
            }

            if (ImGui.Checkbox("遊玩直到任一卡片掉落", ref TriadAutomater.PlayUntilCardDrops) && (TriadAutomater.NumberOfTimes <= 0 || TriadAutomater.PlayXTimes || TriadAutomater.PlayUntilAllCardsDropOnce))
            {
                TriadAutomater.NumberOfTimes = 1;
                TriadAutomater.PlayXTimes = false;
                TriadAutomater.PlayUntilAllCardsDropOnce = false;
            }


            if (GameNpcDB.Get().mapNpcs.TryGetValue(Saucy.TTSolver.preGameNpc?.Id ?? -1, out var npcInfo))
            {
                CurrentNPC = npcInfo;
            }
            else
            {
                CurrentNPC = null;
            }

            if (ImGui.Checkbox($"遊玩直到此 NPC 的所有卡片至少掉落指定次數 {(CurrentNPC is null ? "" : $"({TriadNpcDB.Get().FindByID(CurrentNPC.npcId).Name.GetLocalized()})")}", ref TriadAutomater.PlayUntilAllCardsDropOnce))
            {
                TriadAutomater.TempCardsWonList.Clear();
                TriadAutomater.PlayUntilCardDrops = false;
                TriadAutomater.PlayXTimes = false;
                TriadAutomater.NumberOfTimes = 1;
            }

            bool onlyUnobtained = Saucy.Config.OnlyUnobtainedCards;

            if (TriadAutomater.PlayUntilAllCardsDropOnce)
            {
                ImGui.SameLine();
                if (ImGui.Checkbox("只計算尚未取得的卡片", ref onlyUnobtained))
                {
                    TriadAutomater.TempCardsWonList.Clear();
                    Saucy.Config.OnlyUnobtainedCards = onlyUnobtained;
                    Saucy.Config.Save();
                }
            }

            if (TriadAutomater.PlayUntilAllCardsDropOnce && CurrentNPC != null)
            {
                ImGui.Indent();
                GameCardDB.Get().Refresh();
                foreach (var card in CurrentNPC.rewardCards)
                {
                    if ((Saucy.Config.OnlyUnobtainedCards && !GameCardDB.Get().FindById(card).IsOwned) || !Saucy.Config.OnlyUnobtainedCards)
                    {
                        TriadAutomater.TempCardsWonList.TryAdd((uint)card, 0);
                        ImGui.Text($"- {TriadCardDB.Get().FindById((int)GameCardDB.Get().FindById(card).CardId).Name.GetLocalized()} {TriadAutomater.TempCardsWonList[(uint)card]}/{TriadAutomater.NumberOfTimes}");
                    }

                }

                if (Saucy.Config.OnlyUnobtainedCards && TriadAutomater.TempCardsWonList.Count == 0)
                {
                    ImGui.PushStyleColor(ImGuiCol.Text, ImGuiColors.DalamudRed);
                    ImGui.TextWrapped($@"你已擁有此 NPC 的所有卡片。請取消勾選「只計算尚未取得的卡片」，或改選其他 NPC。");
                    ImGui.PopStyleColor();
                }
                ImGui.Unindent();
            }


            if (TriadAutomater.PlayXTimes || TriadAutomater.PlayUntilCardDrops || TriadAutomater.PlayUntilAllCardsDropOnce)
            {
                ImGui.PushItemWidth(150f);
                ImGui.Text("次數：");
                ImGui.SameLine();

                if (ImGui.InputInt("###NumberOfTimes", ref TriadAutomater.NumberOfTimes))
                {
                    if (TriadAutomater.NumberOfTimes <= 0)
                        TriadAutomater.NumberOfTimes = 1;
                }

                ImGui.Checkbox("完成後登出", ref TriadAutomater.LogOutAfterCompletion);

                bool playSound = Saucy.Config.PlaySound;

                ImGui.Columns(2, null, false);
                if (ImGui.Checkbox("完成後播放音效", ref playSound))
                {
                    Saucy.Config.PlaySound = playSound;
                    Saucy.Config.Save();
                }

                if (playSound)
                {
                    ImGui.NextColumn();
                    ImGui.Text("選擇音效");
                    if (ImGui.BeginCombo("###SelectSound", Saucy.Config.SelectedSound))
                    {
                        string path = Path.Combine(Svc.PluginInterface.AssemblyLocation.Directory.FullName, "Sounds");
                        foreach (var file in new DirectoryInfo(path).GetFiles())
                        {
                            if (ImGui.Selectable($"{Path.GetFileNameWithoutExtension(file.FullName)}", Saucy.Config.SelectedSound == Path.GetFileNameWithoutExtension(file.FullName)))
                            {
                                Saucy.Config.SelectedSound = Path.GetFileNameWithoutExtension(file.FullName);
                                Saucy.Config.Save();
                            }
                        }

                        ImGui.EndCombo();
                    }

                    if (ImGui.Button("開啟音效資料夾"))
                    {
                        Process.Start("explorer.exe", @$"{Path.Combine(Svc.PluginInterface.AssemblyLocation.Directory.FullName, "Sounds")}");
                    }
                    ImGuiComponents.HelpMarker("將 MP3 檔案放入音效資料夾，即可加入自訂音效。");
                }
                ImGui.Columns(1);
            }
        }

        public unsafe void DrawCufTab()
        {
            bool enabled = CufModule.ModuleEnabled;

            ImGui.TextWrapped(@"使用方式：勾選「啟用重擊伽美什模組」，然後走到重擊伽美什機台前。");
            ImGui.Separator();

            if (ImGui.Checkbox("啟用重擊伽美什模組", ref enabled))
            {
                CufModule.ModuleEnabled = enabled;
                if (enabled && TriadAutomater.ModuleEnabled)
                    TriadAutomater.ModuleEnabled = false;
            }

            if (ImGui.Checkbox("遊玩指定次數", ref TriadAutomater.PlayXTimes) && TriadAutomater.NumberOfTimes <= 0)
            {
                TriadAutomater.NumberOfTimes = 1;
            }

            if (TriadAutomater.PlayXTimes)
            {
                ImGui.PushItemWidth(150f);
                ImGui.Text("次數：");
                ImGui.SameLine();

                if (ImGui.InputInt("###NumberOfTimes", ref TriadAutomater.NumberOfTimes))
                {
                    if (TriadAutomater.NumberOfTimes <= 0)
                        TriadAutomater.NumberOfTimes = 1;
                }

                ImGui.Checkbox("完成後登出", ref TriadAutomater.LogOutAfterCompletion);

                bool playSound = Saucy.Config.PlaySound;

                ImGui.Columns(2, null, false);
                if (ImGui.Checkbox("完成後播放音效", ref playSound))
                {
                    Saucy.Config.PlaySound = playSound;
                    Saucy.Config.Save();
                }

                if (playSound)
                {
                    ImGui.NextColumn();
                    ImGui.Text("選擇音效");
                    if (ImGui.BeginCombo("###SelectSound", Saucy.Config.SelectedSound))
                    {
                        string path = Path.Combine(Svc.PluginInterface.AssemblyLocation.Directory.FullName, "Sounds");
                        foreach (var file in new DirectoryInfo(path).GetFiles())
                        {
                            if (ImGui.Selectable($"{Path.GetFileNameWithoutExtension(file.FullName)}", Saucy.Config.SelectedSound == Path.GetFileNameWithoutExtension(file.FullName)))
                            {
                                Saucy.Config.SelectedSound = Path.GetFileNameWithoutExtension(file.FullName);
                                Saucy.Config.Save();
                            }
                        }

                        ImGui.EndCombo();
                    }

                    if (ImGui.Button("開啟音效資料夾"))
                    {
                        Process.Start("explorer.exe", @$"{Path.Combine(Svc.PluginInterface.AssemblyLocation.Directory.FullName, "Sounds")}");
                    }
                    ImGuiComponents.HelpMarker("將 MP3 檔案放入音效資料夾，即可加入自訂音效。");
                }
                ImGui.Columns(1);
            }
        }
    }
}
