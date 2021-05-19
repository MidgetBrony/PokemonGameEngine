﻿using Kermalis.PokemonBattleEngine.Data;
using Kermalis.PokemonGameEngine.Core;
using Kermalis.PokemonGameEngine.GUI.Transition;
using Kermalis.PokemonGameEngine.Input;
using Kermalis.PokemonGameEngine.Pkmn;
using Kermalis.PokemonGameEngine.Pkmn.Pokedata;
using Kermalis.PokemonGameEngine.Render;
using Kermalis.PokemonGameEngine.UI;
using Kermalis.PokemonGameEngine.Util;
using Kermalis.PokemonGameEngine.World;
using System;

namespace Kermalis.PokemonGameEngine.GUI.Pkmn
{
    // TODO: Eggs
    // TODO: Different modes (like select a move)
    // TODO: Box mon, battle mon
    // TODO: Up/down
    internal sealed class SummaryGUI
    {
        public enum Mode : byte
        {
            JustView
        }
        private enum Page : byte
        {
            Info,
            Personal,
            Stats,
            Moves,
            Ribbons
        }

        private readonly Mode _mode;
        private Page _page;
        private readonly PartyPokemon _currentPkmn;

        private FadeColorTransition _fadeTransition;
        private Action _onClosed;

        private AnimatedImage _pkmnImage;
        private readonly Image _pageImage;
        private const float PageImageWidth = 0.55f;
        private const float PageImageHeight = 0.95f;

        #region Open & Close GUI

        public unsafe SummaryGUI(PartyPokemon pkmn, Mode mode, Action onClosed)
        {
            _mode = mode;
            _page = Page.Info;

            _pageImage = new Image((int)(Program.RenderWidth * PageImageWidth), (int)(Program.RenderHeight * PageImageHeight));

            _currentPkmn = pkmn;
            LoadPkmnImage();
            UpdatePageImage();

            _onClosed = onClosed;
            _fadeTransition = new FadeFromColorTransition(500, 0);
            Game.Instance.SetCallback(CB_FadeInSummary);
            Game.Instance.SetRCallback(RCB_Fading);
        }

        private unsafe void CloseSummaryMenu()
        {
            _fadeTransition = new FadeToColorTransition(500, 0);
            Game.Instance.SetCallback(CB_FadeOutSummary);
            Game.Instance.SetRCallback(RCB_Fading);
        }

        private unsafe void CB_FadeInSummary()
        {
            if (_fadeTransition.IsDone)
            {
                _fadeTransition = null;
                Game.Instance.SetCallback(CB_InfoPage);
                Game.Instance.SetRCallback(RCB_RenderTick);
            }
        }
        private unsafe void CB_FadeOutSummary()
        {
            if (_fadeTransition.IsDone)
            {
                _fadeTransition = null;
                _onClosed.Invoke();
                _onClosed = null;
            }
        }

        private unsafe void RCB_Fading(uint* bmpAddress, int bmpWidth, int bmpHeight)
        {
            RCB_RenderTick(bmpAddress, bmpWidth, bmpHeight);
            _fadeTransition.RenderTick(bmpAddress, bmpWidth, bmpHeight);
        }

        #endregion

        private void LoadPkmnImage()
        {
            PBESpecies species = _currentPkmn.Species;
            PBEForm form = _currentPkmn.Form;
            PBEGender gender = _currentPkmn.Gender;
            bool shiny = _currentPkmn.Shiny;
            uint pid = _currentPkmn.PID;
            bool egg = _currentPkmn.IsEgg;
            _pkmnImage = PokemonImageUtils.GetPokemonImage(species, form, gender, shiny, false, false, pid, egg);
        }
        private void SwapPage(Page newPage)
        {
            _page = newPage;
            UpdatePageImage();
            Game.MainCallback cb;
            switch (newPage)
            {
                case Page.Info: cb = CB_InfoPage; break;
                case Page.Personal: cb = CB_PersonalPage; break;
                case Page.Stats: cb = CB_StatsPage; break;
                default: throw new Exception();
            }
            Game.Instance.SetCallback(cb);
        }

        private unsafe void UpdatePageImage()
        {
            _pageImage.Draw(DrawPage);
        }
        private unsafe void DrawPage(uint* bmpAddress, int bmpWidth, int bmpHeight)
        {
            RenderUtils.OverwriteRectangle(bmpAddress, bmpWidth, bmpHeight, RenderUtils.Color(0, 0, 0, 0));
            Font.Default.DrawStringScaled(bmpAddress, bmpWidth, bmpHeight, 0, 0, 2, _page.ToString(), Font.DefaultBlack_I);

            switch (_page)
            {
                case Page.Info: DrawInfoPage(bmpAddress, bmpWidth, bmpHeight); break;
                case Page.Personal: DrawPersonalPage(bmpAddress, bmpWidth, bmpHeight); break;
                case Page.Stats: DrawStatsPage(bmpAddress, bmpWidth, bmpHeight); break;
            }
        }
        private unsafe void DrawInfoPage(uint* bmpAddress, int bmpWidth, int bmpHeight)
        {
            const float winX = 0.03f;
            const float winY = 0.15f;
            const float winW = 0.97f - winX;
            const float winH = 0.85f - winY;
            const float leftColX = winX + 0.02f;
            const float rightColX = winX + 0.52f;
            const float rightColY = winY + 0.03f;
            const float rightColW = 0.95f - rightColX;
            const float rightColH = 0.82f - rightColY;
            const float rightColCenterX = rightColX + (rightColW / 2f);
            const float textStartY = rightColY + 0.02f;
            const float textSpacingY = 0.1f;
            int xpW = (int)(bmpWidth * 0.3f);
            int xpX = RenderUtils.GetCoordinatesForCentering(bmpWidth, xpW, rightColCenterX);
            int xpY = (int)(bmpHeight * (rightColY + 0.61f));
            RenderUtils.FillRoundedRectangle(bmpAddress, bmpWidth, bmpHeight, winX, winY, winX + winW, winY + winH, 15, RenderUtils.Color(128, 215, 135, 255));
            RenderUtils.FillRoundedRectangle(bmpAddress, bmpWidth, bmpHeight, rightColX, rightColY, rightColX + rightColW, rightColY + rightColH, 8, RenderUtils.Color(210, 210, 210, 255));

            Font leftColFont = Font.Default;
            uint[] leftColColors = Font.DefaultWhite2_I;
            Font rightColFont = Font.Default;
            uint[] rightColColors = Font.DefaultBlack_I;

            void PlaceLeftCol(int i, string leftColStr)
            {
                float y = textStartY + (i * textSpacingY);
                leftColFont.DrawString(bmpAddress, bmpWidth, bmpHeight, leftColX, y, leftColStr, leftColColors);
            }
            void PlaceRightCol(int i, string rightColStr, uint[] colors)
            {
                float y = textStartY + (i * textSpacingY);
                rightColFont.MeasureString(rightColStr, out int strW, out _);
                rightColFont.DrawString(bmpAddress, bmpWidth, bmpHeight,
                    RenderUtils.GetCoordinatesForCentering(bmpWidth, strW, rightColCenterX), (int)(bmpHeight * y), rightColStr, colors);
            }

            PlaceLeftCol(0, "Species");
            PlaceLeftCol(1, "Type(s)");
            PlaceLeftCol(2, "OT");
            PlaceLeftCol(3, "OT ID");
            PlaceLeftCol(4, "Exp. Points");
            PlaceLeftCol(5, "Exp. To Next Level");

            PBESpecies species = _currentPkmn.Species;
            PBEForm form = _currentPkmn.Form;
            var bs = new BaseStats(species, form);
            OTInfo ot = _currentPkmn.OT;
            byte level = _currentPkmn.Level;
            uint exp = _currentPkmn.EXP;
            uint toNextLvl;
            if (level >= PkmnConstants.MaxLevel)
            {
                toNextLvl = 0;
                RenderUtils.EXP_SingleLine(bmpAddress, bmpWidth, bmpHeight, xpX, xpY, xpW, 0);
            }
            else
            {
                PBEGrowthRate gr = bs.GrowthRate;
                toNextLvl = PBEEXPTables.GetEXPRequired(gr, (byte)(level + 1)) - exp;
                RenderUtils.EXP_SingleLine(bmpAddress, bmpWidth, bmpHeight, xpX, xpY, xpW, exp, level, gr);
            }

            // Species
            string str = PBELocalizedString.GetSpeciesName(species).English;
            PlaceRightCol(0, str, rightColColors);
            // Types
            str = PBELocalizedString.GetTypeName(bs.Type1).English;
            if (bs.Type2 != PBEType.None)
            {
                str += ' ' + PBELocalizedString.GetTypeName(bs.Type2).English;
            }
            PlaceRightCol(1, str, rightColColors);
            // OT
            str = ot.TrainerName;
            PlaceRightCol(2, str, ot.TrainerIsFemale ? Font.DefaultRed_I : Font.DefaultBlue_I);
            // OT ID
            str = ot.TrainerID.ToString();
            PlaceRightCol(3, str, rightColColors);
            // Exp
            str = exp.ToString();
            PlaceRightCol(4, str, rightColColors);
            // To next level
            str = toNextLvl.ToString();
            PlaceRightCol(5, str, rightColColors);
        }
        private unsafe void DrawPersonalPage(uint* bmpAddress, int bmpWidth, int bmpHeight)
        {
            const float winX = 0.08f;
            const float winY = 0.15f;
            const float winW = 0.75f - winX;
            const float winH = 0.93f - winY;
            const float leftColX = winX + 0.03f;
            const float textStartY = winY + 0.05f;
            const float textSpacingY = 0.1f;
            RenderUtils.FillRoundedRectangle(bmpAddress, bmpWidth, bmpHeight, winX, winY, winX + winW, winY + winH, 15, RenderUtils.Color(145, 225, 225, 255));

            Font leftColFont = Font.Default;
            uint[] leftColColors = Font.DefaultBlack_I;
            uint[] highlightColors = Font.DefaultRed_I;

            void Place(int i, int xOff, string leftColStr, uint[] colors)
            {
                float y = textStartY + (i * textSpacingY);
                leftColFont.DrawString(bmpAddress, bmpWidth, bmpHeight, (int)(bmpWidth * leftColX) + xOff, (int)(bmpHeight * y), leftColStr, colors);
            }

            PBENature nature = _currentPkmn.Nature;
            DateTime met = _currentPkmn.MetDate;
            MapSection loc = _currentPkmn.MetLocation;
            byte metLvl = _currentPkmn.MetLevel;
            string characteristic = Characteristic.GetCharacteristic(_currentPkmn.PID, _currentPkmn.IndividualValues) + '.';
            PBEFlavor? flavor = PBEDataUtils.GetLikedFlavor(nature);

            // Nature
            string str = PBELocalizedString.GetNatureName(nature).English + ' ';
            Place(0, 0, str, highlightColors);
            leftColFont.MeasureString(str, out int strW, out _);
            str = "nature.";
            Place(0, strW, str, leftColColors);
            // Met date
            str = met.ToString("MMMM dd, yyyy");
            Place(1, 0, str, leftColColors);
            // Met location
            str = loc.ToString();
            Place(2, 0, str, highlightColors);
            // Met level
            str = string.Format("Met at Level {0}.", metLvl);
            Place(3, 0, str, leftColColors);
            // Characteristic
            str = characteristic;
            Place(5, 0, str, leftColColors);
            // Flavor
            if (flavor.HasValue)
            {
                str = "Likes ";
                Place(6, 0, str, leftColColors);
                leftColFont.MeasureString(str, out strW, out _);
                str = flavor.Value.ToString() + ' ';
                Place(6, strW, str, highlightColors);
                leftColFont.MeasureString(str, out int strW2, out _);
                str = "food.";
                Place(6, strW + strW2, str, leftColColors);
            }
            else
            {
                str = "Likes all food.";
                Place(6, 0, str, leftColColors);
            }
        }
        private unsafe void DrawStatsPage(uint* bmpAddress, int bmpWidth, int bmpHeight)
        {
            const float winX = 0.03f;
            const float winY = 0.15f;
            const float winW = 0.97f - winX;
            const float winH = 0.995f - winY;
            const float leftColX = winX + 0.02f;
            const float rightColX = winX + 0.52f;
            const float rightColY = winY + 0.02f;
            const float rightColW = 0.95f - rightColX;
            const float rightColH = 0.535f;
            const float rightColCenterX = rightColX + (rightColW / 2f);
            const float textStartY = rightColY + 0.01f;
            const float textStart2Y = rightColY + 0.13f;
            const float textSpacingY = 0.08f;
            const float abilTextY = textStart2Y + (5.5f * textSpacingY);
            const float abilDescX = leftColX + 0.03f;
            const float abilDescY = textStart2Y + (6.6f * textSpacingY);
            const float abilX = winX + 0.18f;
            const float abilTextX = abilX + 0.03f;
            const float abilY = abilTextY;
            const float abilW = 0.95f - abilX;
            const float abilH = 0.075f;
            int hpW = (int)(bmpWidth * 0.3f);
            int hpX = RenderUtils.GetCoordinatesForCentering(bmpWidth, hpW, rightColCenterX);
            int hpY = (int)(bmpHeight * (rightColY + 0.09f));
            RenderUtils.FillRoundedRectangle(bmpAddress, bmpWidth, bmpHeight, winX, winY, winX + winW, winY + winH, 12, RenderUtils.Color(135, 145, 250, 255));
            // Stats
            RenderUtils.FillRoundedRectangle(bmpAddress, bmpWidth, bmpHeight, rightColX, rightColY, rightColX + rightColW, rightColY + rightColH, 8, RenderUtils.Color(210, 210, 210, 255));
            // Abil
            RenderUtils.FillRoundedRectangle(bmpAddress, bmpWidth, bmpHeight, abilX, abilY, abilX + abilW, abilY + abilH, 5, RenderUtils.Color(210, 210, 210, 255));
            // Abil desc
            RenderUtils.FillRoundedRectangle(bmpAddress, bmpWidth, bmpHeight, leftColX, abilDescY, 0.95f, 0.98f, 5, RenderUtils.Color(210, 210, 210, 255));

            Font leftColFont = Font.Default;
            uint[] leftColColors = Font.DefaultWhite2_I;
            Font rightColFont = Font.Default;
            uint[] rightColColors = Font.DefaultBlack_I;

            void PlaceLeftCol(int i, string leftColStr)
            {
                float y;
                if (i == -1)
                {
                    y = abilTextY;
                }
                else if (i == -2)
                {
                    y = textStartY;
                }
                else
                {
                    y = textStart2Y + (i * textSpacingY);
                }
                leftColFont.DrawString(bmpAddress, bmpWidth, bmpHeight, leftColX, y, leftColStr, leftColColors);
            }
            void PlaceRightCol(int i, string rightColStr, uint[] colors)
            {
                float y = i == -2 ? textStartY : textStart2Y + (i * textSpacingY);
                rightColFont.MeasureString(rightColStr, out int strW, out _);
                rightColFont.DrawString(bmpAddress, bmpWidth, bmpHeight,
                    RenderUtils.GetCoordinatesForCentering(bmpWidth, strW, rightColCenterX), (int)(bmpHeight * y), rightColStr, colors);
            }

            PlaceLeftCol(-2, "HP");
            PlaceLeftCol(0, "Attack");
            PlaceLeftCol(1, "Defense");
            PlaceLeftCol(2, "Special Attack");
            PlaceLeftCol(3, "Special Defense");
            PlaceLeftCol(4, "Speed");
            PlaceLeftCol(-1, "Ability");

            ushort hp = _currentPkmn.HP;
            ushort maxHP = _currentPkmn.MaxHP;
            ushort atk = _currentPkmn.Attack;
            ushort def = _currentPkmn.Defense;
            ushort spAtk = _currentPkmn.SpAttack;
            ushort spDef = _currentPkmn.SpDefense;
            ushort speed = _currentPkmn.Speed;
            PBEAbility abil = _currentPkmn.Ability;

            // HP
            string str = string.Format("{0}/{1}", hp, maxHP);
            PlaceRightCol(-2, str, rightColColors);
            double percent = (double)hp / maxHP;
            RenderUtils.HP_TripleLine(bmpAddress, bmpWidth, bmpHeight, hpX, hpY, hpW, percent);
            // Attack
            str = atk.ToString();
            PlaceRightCol(0, str, rightColColors);
            // Defense
            str = def.ToString();
            PlaceRightCol(1, str, rightColColors);
            // Sp. Attack
            str = spAtk.ToString();
            PlaceRightCol(2, str, rightColColors);
            // Sp. Defense
            str = spDef.ToString();
            PlaceRightCol(3, str, rightColColors);
            // Speed
            str = speed.ToString();
            PlaceRightCol(4, str, rightColColors);
            // Ability
            str = PBELocalizedString.GetAbilityName(abil).English;
            rightColFont.DrawString(bmpAddress, bmpWidth, bmpHeight, abilTextX, abilTextY, str, rightColColors);
            // Ability desc
            str = PBELocalizedString.GetAbilityDescription(abil).English;
            leftColFont.DrawString(bmpAddress, bmpWidth, bmpHeight, abilDescX, abilDescY, str, rightColColors);
        }

        private void CB_InfoPage()
        {
            if (InputManager.IsPressed(Key.B))
            {
                CloseSummaryMenu();
                return;
            }
            if (InputManager.IsPressed(Key.Right))
            {
                SwapPage(Page.Personal);
                return;
            }
        }
        private void CB_PersonalPage()
        {
            if (InputManager.IsPressed(Key.B))
            {
                CloseSummaryMenu();
                return;
            }
            if (InputManager.IsPressed(Key.Left))
            {
                SwapPage(Page.Info);
                return;
            }
            if (InputManager.IsPressed(Key.Right))
            {
                SwapPage(Page.Stats);
                return;
            }
        }
        private void CB_StatsPage()
        {
            if (InputManager.IsPressed(Key.B))
            {
                CloseSummaryMenu();
                return;
            }
            if (InputManager.IsPressed(Key.Left))
            {
                SwapPage(Page.Personal);
                return;
            }
        }

        private unsafe void RCB_RenderTick(uint* bmpAddress, int bmpWidth, int bmpHeight)
        {
            RenderUtils.ThreeColorBackground(bmpAddress, bmpWidth, bmpHeight, RenderUtils.Color(215, 231, 230, 255), RenderUtils.Color(231, 163, 0, 255), RenderUtils.Color(242, 182, 32, 255));

            AnimatedImage.UpdateCurrentFrameForAll();
            _pkmnImage.DrawOn(bmpAddress, bmpWidth, bmpHeight,
                RenderUtils.GetCoordinatesForCentering(bmpWidth, _pkmnImage.Width, 0.2f), RenderUtils.GetCoordinatesForEndAlign(bmpHeight, _pkmnImage.Height, 0.6f));

            _pageImage.DrawOn(bmpAddress, bmpWidth, bmpHeight, 1f - PageImageWidth, 1f - PageImageHeight);
        }
    }
}
