using System.Numerics;
using ImGuiNET;

namespace ShorNet
{
    internal sealed class ShorNetWindow
    {
        public float Scale { get; set; } = 1f;

        public void Toggle() { _visible = !_visible; }
        public void Show()   { _visible = true; }
        public void Hide()   { _visible = false; }

        private bool _visible     = false;
        private bool _initialized = false;
        private readonly SettingsTab _settings = new SettingsTab();

        // ── Colors ────────────────────────────────────────────────────────────

        internal static uint Col(byte r, byte g, byte b, byte a = 255)
            => (uint)((a << 24) | (b << 16) | (g << 8) | r);

        internal static readonly uint C_WindowBg  = Col(0x0F, 0x10, 0x14, 245);
        internal static readonly uint C_TitleBg   = Col(0x1A, 0x1D, 0x23, 255);
        internal static readonly uint C_PanelBg   = Col(0x13, 0x16, 0x1B, 255);
        internal static readonly uint C_Border     = Col(0x2D, 0x31, 0x39, 255);
        internal static readonly uint C_AccentBlue = Col(0x00, 0x8D, 0xFD, 255);
        internal static readonly uint C_BtnNormal  = Col(0x00, 0x00, 0x00, 255);
        internal static readonly uint C_BtnHover   = Col(0x1A, 0x3A, 0x5C, 255);
        internal static readonly uint C_BtnActive  = Col(0x0D, 0x24, 0x40, 255);
        internal static readonly uint C_TextPri    = Col(0xF1, 0xF5, 0xF9, 255);
        internal static readonly uint C_TextMuted  = Col(0x64, 0x74, 0x8B, 255);
        internal static readonly uint C_InputBg    = Col(0x0A, 0x0C, 0x10, 255);

        internal static Vector4 V4WindowBg   => ImGui.ColorConvertU32ToFloat4(C_WindowBg);
        internal static Vector4 V4TitleBg    => ImGui.ColorConvertU32ToFloat4(C_TitleBg);
        internal static Vector4 V4PanelBg    => ImGui.ColorConvertU32ToFloat4(C_PanelBg);
        internal static Vector4 V4Border     => ImGui.ColorConvertU32ToFloat4(C_Border);
        internal static Vector4 V4AccentBlue => ImGui.ColorConvertU32ToFloat4(C_AccentBlue);
        internal static Vector4 V4BtnNormal  => ImGui.ColorConvertU32ToFloat4(C_BtnNormal);
        internal static Vector4 V4BtnHover   => ImGui.ColorConvertU32ToFloat4(C_BtnHover);
        internal static Vector4 V4BtnActive  => ImGui.ColorConvertU32ToFloat4(C_BtnActive);
        internal static Vector4 V4TextPri    => ImGui.ColorConvertU32ToFloat4(C_TextPri);
        internal static Vector4 V4TextMuted  => ImGui.ColorConvertU32ToFloat4(C_TextMuted);
        internal static Vector4 V4InputBg    => ImGui.ColorConvertU32ToFloat4(C_InputBg);

        // ── Draw entry point ─────────────────────────────────────────────────

        public void Draw()
        {
            if (!_visible) return;

            float s = Scale;

            ImGui.SetNextWindowSize(new Vector2(420f * s, 220f * s), ImGuiCond.FirstUseEver);
            ImGui.SetNextWindowSizeConstraints(
                new Vector2(300f * s, 160f * s),
                new Vector2(700f * s, 500f * s));
            ImGui.SetNextWindowPos(
                new Vector2(ImGui.GetIO().DisplaySize.X * 0.5f, ImGui.GetIO().DisplaySize.Y * 0.5f),
                ImGuiCond.FirstUseEver,
                new Vector2(0.5f, 0.5f));

            PushWindowStyle();

            bool open = _visible;
            bool expanded = ImGui.Begin("##shornet_settings", ref open,
                ImGuiWindowFlags.NoCollapse |
                ImGuiWindowFlags.NoScrollbar |
                ImGuiWindowFlags.NoScrollWithMouse);

            PopWindowStyle();

            if (!open)
            {
                _visible = false;
                if (expanded) ImGui.End();
                return;
            }

            if (expanded)
            {
                if (!_initialized)
                {
                    _initialized = true;
                    _settings.OnShow();
                }

                DrawTitle();
                _settings.Draw(s);
            }

            ImGui.End();
        }

        private void DrawTitle()
        {
            var dl     = ImGui.GetForegroundDrawList();
            var winPos = ImGui.GetWindowPos();
            float titleH = ImGui.GetFrameHeight();
            float textH  = ImGui.GetTextLineHeight();
            float y      = winPos.Y + (titleH - textH) * 0.5f;
            float x      = winPos.X + 8f * Scale;

            uint colShor    = Col(0xF1, 0xF5, 0xF9);
            uint colNet     = Col(0x4A, 0x90, 0xD9);

            dl.AddText(new Vector2(x, y), colShor, "Shor");
            float shorW = ImGui.CalcTextSize("Shor").X;
            dl.AddText(new Vector2(x + shorW, y), colNet, "Net");
            float netW = ImGui.CalcTextSize("Net").X;
            dl.AddText(new Vector2(x + shorW + netW + 4f * Scale, y), colShor, "Settings");
        }

        // ── Style helpers ─────────────────────────────────────────────────────

        private void PushWindowStyle()
        {
            ImGui.PushStyleColor(ImGuiCol.WindowBg,       V4WindowBg);
            ImGui.PushStyleColor(ImGuiCol.TitleBg,        V4TitleBg);
            ImGui.PushStyleColor(ImGuiCol.TitleBgActive,  V4TitleBg);
            ImGui.PushStyleColor(ImGuiCol.Border,         V4Border);
            ImGui.PushStyleColor(ImGuiCol.Text,           V4TextPri);
            ImGui.PushStyleColor(ImGuiCol.Separator,      V4Border);

            ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding,  new Vector2(10f * Scale, 8f * Scale));
            ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing,    new Vector2(6f  * Scale, 4f * Scale));
            ImGui.PushStyleVar(ImGuiStyleVar.WindowRounding, 3f * Scale);
            ImGui.PushStyleVar(ImGuiStyleVar.FrameRounding,  2f * Scale);
        }

        private void PopWindowStyle()
        {
            ImGui.PopStyleVar(4);
            ImGui.PopStyleColor(6);
        }

        internal static void PushWidgetStyle()
        {
            ImGui.PushStyleColor(ImGuiCol.Button,               V4BtnNormal);
            ImGui.PushStyleColor(ImGuiCol.ButtonHovered,        V4BtnHover);
            ImGui.PushStyleColor(ImGuiCol.ButtonActive,         V4BtnActive);
            ImGui.PushStyleColor(ImGuiCol.FrameBg,              V4InputBg);
            ImGui.PushStyleColor(ImGuiCol.FrameBgHovered,       V4BtnHover);
            ImGui.PushStyleColor(ImGuiCol.FrameBgActive,        V4BtnActive);
            ImGui.PushStyleColor(ImGuiCol.Header,               V4BtnNormal);
            ImGui.PushStyleColor(ImGuiCol.HeaderHovered,        V4BtnHover);
            ImGui.PushStyleColor(ImGuiCol.HeaderActive,         V4BtnActive);
            ImGui.PushStyleColor(ImGuiCol.ScrollbarBg,          V4InputBg);
            ImGui.PushStyleColor(ImGuiCol.ScrollbarGrab,        V4BtnHover);
            ImGui.PushStyleColor(ImGuiCol.ScrollbarGrabHovered, V4AccentBlue);
            ImGui.PushStyleColor(ImGuiCol.ScrollbarGrabActive,  V4AccentBlue);
            ImGui.PushStyleColor(ImGuiCol.PopupBg,              V4PanelBg);
            ImGui.PushStyleColor(ImGuiCol.Text,                 V4TextPri);
            ImGui.PushStyleColor(ImGuiCol.Border,               V4Border);
            ImGui.PushStyleVar(ImGuiStyleVar.FrameBorderSize, 1f);
        }

        internal static void PopWidgetStyle()
        {
            ImGui.PopStyleVar(1);
            ImGui.PopStyleColor(16);
        }

        internal static void SectionHeader(string text)
        {
            ImGui.PushStyleColor(ImGuiCol.Text, V4TextMuted);
            ImGui.TextUnformatted(text.ToUpper());
            ImGui.PopStyleColor();
            ImGui.Separator();
        }
    }
}