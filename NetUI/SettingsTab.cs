using System.Collections.Generic;
using ImGuiNET;
using UnityEngine;
using Vector2 = System.Numerics.Vector2;

namespace ShorNet
{
    internal sealed class SettingsTab
    {
        private int          _chatWindowIdx;
        private int          _chatTabIdx;
        private List<IDLog>  _chatWindows     = new List<IDLog>();
        private List<string> _chatWindowNames = new List<string>();
        private List<string> _chatTabNames    = new List<string>();

        public void OnShow()
        {
            RefreshChatWindows();
        }

        public void Draw(float scale)
        {
            ShorNetWindow.PushWidgetStyle();
            ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(6f * scale, 6f * scale));

            ImGui.BeginChild("##settings_scroll", Vector2.Zero, false, ImGuiWindowFlags.None);

            DrawChatOutputSection(scale);

            ImGui.EndChild();

            ImGui.PopStyleVar();
            ShorNetWindow.PopWidgetStyle();
        }

        private void DrawChatOutputSection(float s)
        {
            ShorNetWindow.SectionHeader("Chat Output");

            float labelW = 80f * s;

            // Window picker
            ImGui.PushStyleColor(ImGuiCol.Text, ShorNetWindow.V4TextMuted);
            ImGui.TextUnformatted("Window:");
            ImGui.PopStyleColor();
            ImGui.SameLine(labelW);
            ImGui.SetNextItemWidth(-1f);

            if (_chatWindowNames.Count > 0)
            {
                if (ImGui.Combo("##sn_chat_win", ref _chatWindowIdx,
                    _chatWindowNames.ToArray(), _chatWindowNames.Count))
                {
                    if (_chatWindowIdx < _chatWindows.Count)
                    {
                        ShorNetSettings.ChatOutputWindow = _chatWindows[_chatWindowIdx].WindowName;
                        RefreshChatTabs(_chatWindows[_chatWindowIdx]);
                        ChatFilterInjector.ApplyChatMask();
                        ShorNetSettings.Save();
                    }
                }
            }
            else
            {
                ImGui.TextDisabled("(no chat windows found)");
            }

            ImGui.Spacing();

            // Tab picker
            ImGui.PushStyleColor(ImGuiCol.Text, ShorNetWindow.V4TextMuted);
            ImGui.TextUnformatted("Tab:");
            ImGui.PopStyleColor();
            ImGui.SameLine(labelW);
            ImGui.SetNextItemWidth(-1f);

            if (_chatTabNames.Count > 0)
            {
                if (ImGui.Combo("##sn_chat_tab", ref _chatTabIdx,
                    _chatTabNames.ToArray(), _chatTabNames.Count))
                {
                    ShorNetSettings.ChatOutputTab = _chatTabIdx;
                    ChatFilterInjector.ApplyChatMask();
                    ShorNetSettings.Save();
                }
            }
            else
            {
                ImGui.TextDisabled("(no tabs)");
            }
        }

        private void RefreshChatWindows()
        {
            _chatWindows.Clear();
            _chatWindowNames.Clear();

            foreach (var win in UpdateSocialLog.ChatWindows)
            {
                _chatWindows.Add(win);
                _chatWindowNames.Add(string.IsNullOrEmpty(win.WindowName) ? "(unnamed)" : win.WindowName);
            }

            _chatWindowIdx = 0;
            for (int i = 0; i < _chatWindows.Count; i++)
            {
                if (_chatWindows[i].WindowName == ShorNetSettings.ChatOutputWindow)
                {
                    _chatWindowIdx = i;
                    break;
                }
            }

            if (_chatWindowIdx < _chatWindows.Count)
                RefreshChatTabs(_chatWindows[_chatWindowIdx]);
        }

        private void RefreshChatTabs(IDLog win)
        {
            _chatTabNames.Clear();
            if (win == null) return;

            int count = Mathf.Clamp(win.activeTabs, 1, win.TabDisplayName.Length);
            for (int i = 0; i < count; i++)
            {
                string name = win.TabDisplayName[i];
                _chatTabNames.Add(string.IsNullOrEmpty(name) ? $"Tab {i + 1}" : name);
            }

            _chatTabIdx = Mathf.Clamp(ShorNetSettings.ChatOutputTab, 0, _chatTabNames.Count - 1);
        }
    }
}